using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PreyScript : Controller {

	//states and actions
	enum State {Idle, Seek, Flee, Dead}; // possible states
	public int stateSize = System.Enum.GetValues(typeof(State)).Length;
	enum Action {None, RotateLeft, RotateRight, LookAtObject, MoveForward, MoveBackwards, Stop, FollowObject, Eat, Idle, Seek, Flee}; // possible actions
	public int actionSize = System.Enum.GetValues(typeof(Action)).Length;
	State currentState;
	Action currentAction;
    ObjectState oldState = null;

	//speed
	public float FleeMovepSpeed = 6;
	public float SeekMoveSpeed = 4;

	Vector3 destination; // agent destination
	Vector3 movingDir; // moving direction

	// rotation reduction speed
	float initialAngle;
	Rotation rotation; // class which controls the rotation of the object

	// init method
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		agent = GetComponent<NavMeshAgent> ();
		initialAngle = transform.rotation.y; // the object only rotates on the y axis
		rotation = GetComponent<Rotation> ();
		currentState = State.Seek;
	} // end of Start

    /*
	 * Updates every frame
	 */
    void Update() {
        if (currentState == State.Dead)
            return;
        hunger -= 0.01f;

        if (stamina < 25 || hunger > 75)
        {
            moveSpeed = 2f;
        }

        if (hunger < 0 || hunger > 115)
        { // starved to death or sploded from too much food
            Dead();
        }

        switch (currentState) {
            case State.Idle:
                rest(0.1f);
                stop();
                break;
            case State.Seek:
                consumeStamina(0.01f);
                moveSpeed = SeekMoveSpeed;
                break;
            case State.Flee:
                consumeStamina(0.1f);
                moveSpeed = FleeMovepSpeed;
                break;
        }

        int[] states = generateStates();
        ObjectState curState = stateArray.findState(states);
        if (curState == null)   // check that state exist 
            curState = stateArray.addState(states);

        // debug
        bool moveToNewState = true;
        if (oldState != null) {
            if (oldState.getId() != curState.getId())
            {
                Debug.Log("current state: " + curState.getId());
                moveToNewState = true;
            }
        }

        //Calculate Q using the previous objstate,action and the current objstate
        if (oldState != null && moveToNewState)
        {
            qAlgorithm.rl(oldState, (int)currentAction, curState);
            //qAlgorithm.printQTable();
        }


        bool[] availableActions = getAvailableActions((int)currentState); // get the available actions
        Action action = Action.None; // testing actions with keyboard

        if (keyboard)
        {// if boolean variable is true use the keyboard commands
            action = keyboardActions(availableActions);
            selectAction((int)action);
        }
        else if (moveToNewState) { 
             action = intToAction(qAlgorithm.bestAction(curState));// get best action according to q table
            selectAction((int)action);
        }

        //Debug.Log("selected Action: " + action);
		selectAction ((int)action);
        oldState = curState;
	} // end of Update

	/*
	 * Updates Physics 
	 */
	void FixedUpdate(){
		// reducing the speed according to the rotation of the object
		if (agent.enabled) {
			if (agent.enabled && Vector3.Distance(transform.position, destination) <= 1f) {
				agent.destination = transform.position;
				agent.enabled = false;
			}
		} else {
			if (moving) {
				reducedSpeed = moveSpeed - ((moveSpeed / 2) * Mathf.Abs ((initialAngle - transform.rotation.y) % 180));
				rigidbody.velocity = movingDir * reducedSpeed;
			} else {
				rigidbody.velocity = transform.forward * 0;
			}
		}
	} // end of Fixed-Update

    /*
     * returns true if the agent is dead, false if its not
     */
    public override bool isDead()
    {
        return State.Dead == currentState;
    } // end of is dead;

    /*
	 * Look At sound Source
	 */
    void lookAtSound(){
		/*if (soundList.Count > 0) {
			Vector3 origin = soundList[0];
			rotation.lookAtObject(origin);
		} */

		//testing
		if(GetComponent<FieldOfView>().visibleEadibles.Count>0){
			Vector3 eadiblePosition = GetComponent<FieldOfView> ().visibleEadibles [0].position;
			rotation.lookAtObject (eadiblePosition);
		}
	}

	/*
	 * Action Move towards the movingDir (direction)
	 * initialAngle is to reduce the speed according to how much the rotation of the object has altered during that initial move action
	 */ 
	void moveForward(){
		agent.enabled = false;
		movingDir = transform.forward;
		initialAngle = transform.rotation.y;
		moving = true;
	}

	/*
	 * Action MoveBack
	 */
	void moveBackward(){
		agent.enabled = false;
		movingDir = -transform.forward;
		initialAngle = Quaternion.Inverse(transform.rotation).y;
		moving = true;
	}

	/*
	 * move towards an eadible object
	 */
	void moveToEadible(){
		stop ();
		agent.enabled = true;
		if (GetComponent<FieldOfView>().visibleEadibles.Count > 0) {
			Transform eadible = (Transform)GetComponent<FieldOfView>().visibleEadibles [0];
			Vector3 eadiblePosition = eadible.position;
			agent.SetDestination (eadiblePosition);
			destination = eadiblePosition;
		}
	}
	              
	/*
	 * Action Stop. stops the movement of the object
	 */
	void stop(){
		agent.enabled = false;
		moving = false;
		rigidbody.velocity = transform.forward * 0;
	}

	/*
	 * Action Eat. eats an eadible object if it collides with it
	 */
	void eat(){
		if (eadibleList.Count > 0) {
			GameObject e = (GameObject)eadibleList [0];
			eadibleList.RemoveAt (0);
			Destroy (e);
			hunger += 15;
		}
	} // end of eat

	/*
	 * Selects the next action
	 */
	public override void selectAction(int a){
        Action action = intToAction(a);
		currentAction = action;
		switch (currentAction) {
		case Action.RotateLeft:
			rotation.RotateLeft ();
			break;
		case Action.RotateRight:
			rotation.RotateRight ();
			break;
		case Action.LookAtObject:
			lookAtSound ();
			break;
		case Action.MoveForward:
			moveForward ();
			break;
		case Action.MoveBackwards:
			moveBackward ();
			break;
		case Action.Stop:
			stop ();
			break;
		case Action.FollowObject:
			moveToEadible ();
			break;
		case Action.Eat:
			eat ();
			break;
		case Action.Idle:
			Idle ();
			break;
		case Action.Seek:
			Seek ();
			break;
		case Action.Flee:
			Flee ();
			break;
		}
	} // end of selectAction

	/*
	 * returns a boolean array with all the available actions of the object.
	 * the actions are selected according to the object's state and other parameters.
	 */
	public override bool[] getAvailableActions(int s){
		State state = intToState (s);
		bool[] availableActions = new bool[actionSize];
		Action[] chooseActions = null;
		switch (state) {
		case State.Idle:
			chooseActions = new Action[]{
				Action.RotateLeft, 
				Action.RotateRight, 
				Action.Flee,
				Action.Seek
			};
			break;
		case State.Seek:
			chooseActions = new Action[] {
				Action.RotateLeft,
				Action.RotateRight,
				Action.LookAtObject,
				Action.MoveForward,
				Action.MoveBackwards,
				Action.Stop,
				Action.Flee,
				Action.Idle
			};
			if (eadibleList.Count > 0 && !moving) // eat rule
				availableActions [(int)Action.Eat] = true;
			if (GetComponent<FieldOfView>().visibleEadibles.Count > 0) // move towards object rule
				availableActions [(int)Action.FollowObject] = true;
			break;
		case State.Flee:
			chooseActions = new Action[]{
				Action.RotateLeft, 
				Action.RotateRight, 
				Action.LookAtObject, 
				Action.MoveForward, 
				Action.MoveBackwards,
				Action.Stop, 
				Action.Seek, 
				Action.Idle
			};
			break;
		case State.Dead:
			chooseActions = null;
            return availableActions;
			break;
		}
		if (GetComponent<FieldOfView>().visibleEadibles.Count>0){ // test, replace eadible with sound source
			availableActions [(int)Action.LookAtObject] = true;
		}
		if (chooseActions != null) {
			for (int i = 0; i < chooseActions.Length; i++) {
				availableActions [(int)chooseActions [i]]=true;
			}
		}
		return availableActions;
	} // end of getAvailableActions 


	/*
	 * State Change to dead.
	 * When the object dies from hunger or is eaten by the predator.
	 */
	public void Dead(){
		currentState = State.Dead;
	}

	/*
	 * State Change to Idle.
	 * Only available from the other 2 states.
	 */
	void Idle(){
		currentState = State.Idle;
	}

	/*
	 * State Change to Seek.
	 * Only available from the other 2 states.
	 */
	void Seek(){
		currentState = State.Seek;
	}

	/*
	 * State Change to Flee.
	 * Only available from the other 2 states.
	 */
	void Flee(){
		currentState = State.Flee;
	}

	/*
	 * generate states
	 */
	int[] generateStates(){

		int[] stateArray  = new int[9];
		stateArray [0] = (int)currentState; // current state
		stateArray [1] = (int)currentAction; // current action
		stateArray [2] = (int)System.Convert.ToSingle (moving); // is it moving
		stateArray [3] = (int)System.Convert.ToSingle (soundList.Count>0); // length of soundlist
		stateArray [4] = (int)System.Convert.ToSingle (GetComponent<FieldOfView>().visibleTargets.Count>0); // size of visible targets
		stateArray [5] = (int)System.Convert.ToSingle (GetComponent<FieldOfView>().visibleEadibles.Count>0); // size of visible targets
        stateArray [6] = (int)System.Convert.ToSingle(eadibleList.Count > 0); // contact with eadibles
        stateArray [7] = staminaLevel(); // stamina level
		stateArray [8] = hungerLevel (); // hunger level
		return stateArray;
	} // end of generateStates

	/*
	 * sets stamina as a continuous variable
	 * that is split into 4 different categories
	 */
	int staminaLevel(){
		int staminaGroup = 0;
		if (stamina >= 75)
			staminaGroup = 3;
		else if (stamina >= 50 && stamina < 75)
			staminaGroup = 2;
		else if (stamina >= 25 && stamina < 50)
			staminaGroup = 1;
		else
			staminaGroup = 0;

		return staminaGroup;
	} // end of staminaLevel

	/*
	 * sets hunger level as a continuous variable
	 * that is split into 4 different categories
	 */
	int hungerLevel(){
		int hungerGroup = 0;

        if (hunger > 100)
            hungerGroup = 4;
		else if (hunger >= 75 && hunger <= 100)
			hungerGroup = 3;
		else if (hunger >= 50 && hunger < 75)
			hungerGroup = 2;
		else if (hunger >= 25 && hunger < 50)
			hungerGroup = 1;
		else
			hungerGroup = 0;

		return hungerGroup;
	} // end of hungerLevel

    /*
	 * returns the reward according to the state and action 
	 */
    public override float reward(int state, int action)
    {
        float reward = 0f;
        if (intToState(state) != PreyScript.State.Dead)
        {
            if (action == (int)PreyScript.Action.Eat)
                reward = 20f;
            else
                reward = -1f;
        }
        else
        {
            reward = -100f;
        }
        return reward;
    } // end of R

    /*
	 * converts an integer to a state
	 */
    PreyScript.State intToState(int i){
		if (i == (int)PreyScript.State.Idle)
			return PreyScript.State.Idle;
		else if (i == (int)PreyScript.State.Seek)
			return PreyScript.State.Seek;
		else if (i == (int)PreyScript.State.Flee)
			return PreyScript.State.Flee;
		else 
			return PreyScript.State.Dead;
	} // end of intToState

    /*
     * converts int to an action
     */
    PreyScript.Action intToAction(int a) {
        foreach (Action action in System.Enum.GetValues(typeof(Action))) {
            if ((int)action == a)
                return action;
        }
        return Action.None;
    } // end of intToAction

    PreyScript.Action keyboardActions(bool[] availableActions) {
        Action action = Action.None;
        ////// ACTIONS CONTROLLED BY THE KEYBOARD ////// 
        if (Input.GetKey(KeyCode.E) && availableActions[(int)Action.Eat])
        {
            Debug.Log("Prey Eating!");
            //eat ();
            action = Action.Eat;
        }

        if (Input.GetKey(KeyCode.A) && availableActions[(int)Action.MoveForward])
        {
            //rigidbody.velocity = transform.forward * moveSpeed;
            //move();
            action = Action.MoveForward;
        }
        if (Input.GetKey(KeyCode.S) && availableActions[(int)Action.MoveBackwards])
        {
            action = Action.MoveBackwards;
        }
        if (Input.GetKey(KeyCode.Z) && availableActions[(int)Action.FollowObject])
        {
            action = Action.FollowObject;
        }
        if (Input.GetKey(KeyCode.D) && availableActions[(int)Action.Stop])
        {
            //rigidbody.velocity = transform.forward * 0;
            //stop();
            action = Action.Stop;
        }
        if (Input.GetKey(KeyCode.X) && availableActions[(int)Action.LookAtObject])
        {
            action = Action.LookAtObject;
        }

        //switch state test
        if (Input.GetKey(KeyCode.Alpha1) && availableActions[(int)Action.Idle])
        { // Idle
          //Idle ();
            action = Action.Idle;
        }
        if (Input.GetKey(KeyCode.Alpha2) && availableActions[(int)Action.Seek])
        { // Seek
          //Seek ();
            action = Action.Seek;
        }
        if (Input.GetKey(KeyCode.Alpha3) && availableActions[(int)Action.Flee])
        { // Flee
          //Flee ();
            action = Action.Flee;
        }

        if (Input.GetKey(KeyCode.Q) && availableActions[(int)Action.RotateLeft])
        { // 1 on keyboard to rotate left
          //rotation.RotateLeft (); // method will be called by controller
            action = Action.RotateLeft;
        }
        if (Input.GetKey(KeyCode.W) && availableActions[(int)Action.RotateRight])
        { // 2 on keyboard to rotate right
          //rotation.RotateRight (); // method will be called by controller
            action = Action.RotateRight;
        }
        return action;
    } //end of keyboard actions
} // end of PreyScript Class
