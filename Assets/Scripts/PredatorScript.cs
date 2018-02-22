using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PredatorScript : Controller {

	ArrayList eatPreyList = new ArrayList(); //List that holds all prey objects that are within the second collider

	// states and actions
	public enum State {Idle, Seek, Attack, Dead};
    public int stateSize = System.Enum.GetValues(typeof(State)).Length;       
	public enum Action {None, RotateLeft, RotateRight, LookAtObject, Move, Stop, FollowObject, Eat, Idle, Seek, Attack};
    public int actionSize = System.Enum.GetValues(typeof(Action)).Length;
	State currentState;
	Action currentAction;
    ObjectState oldState = null;

	//speed
	public float SeekMoveSpeed = 4.5f;
	public float AttackMoveSpeed = 6.5f;

    //movement and direction
    Vector3 destination;
	Vector3 movingDir;
	float initialAngle;

	//rotate variables
	bool spotted = false;
	Rotation rotation;


	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        initialAngle = transform.rotation.eulerAngles.y; // the object only rotates on the y axis
        rotation = GetComponent<Rotation>();
        currentState = State.Seek;

        if (dummy) {
            GameObject[] ways = GameObject.FindGameObjectsWithTag("Waypoint");
            points = new Transform[ways.Length];
            for (int i = 0; i < ways.Length; i++) {
                points[i] = ways[i].transform;
            }
            agent.autoBraking = false;
            GotoNextPoint();
        }
    }
	
	// Update is called once per frame
	void Update () {

        if (currentState == State.Dead)
            return;

        if (dummy) {
            dummyBehaviour();
            return;
        }
        hunger -= 0.01f;

        if (stamina < 25 || hunger > 75) {
            moveSpeed = 2f;
        }

        if (hunger < 0) { // starved to death or sploded from too much food
            Dead();
        }

        switch (currentState){
		case State.Idle:
			rest (0.01f);
			break;
		case State.Seek:
			consumeStamina (0.01f);
            moveSpeed = SeekMoveSpeed;
			break;
		case State.Attack:
			consumeStamina (0.3f);
            moveSpeed = AttackMoveSpeed;     
			break;
		}

        int[] states = generateStates();
        ObjectState curState = stateArray.findState(states);
        if (curState == null)   // check that state exist 
            curState = stateArray.addState(states);

        // debug
        bool moveToNewState = false;
        if (oldState != null) {
            if (oldState.getId() != curState.getId()) {
                //Debug.Log("current state: " + curState.getId());

                moveToNewState = true;
            }
        }

        //Calculate Q using the previous objstate,action and the current objstate
        if (oldState != null && moveToNewState) {
            qAlgorithm.rl(oldState, (int)currentAction, curState);
        }

        bool[] availableActions = getAvailableActions((int)currentState); // get the available actions

        if (keyboard) {// if boolean variable is true use the keyboard commands
            Action action = keyboardActions(availableActions);
            selectAction((int)action);
        }
        else /*if (moveToNewState)*/ {
            Action action = intToAction(qAlgorithm.nextAction(curState));// get best action according to q table
            //Debug.Log("selected action: " + action);
            selectAction((int)action);
        }

        oldState = curState;

        changeFieldOfView();		
	} // end of Update

	/*
	 * Updates the Physics
	 */
	void FixedUpdate() {
        // reducing the speed according to the rotation of the object
        if (agent.enabled) {
            if (agent.enabled && Vector3.Distance(transform.position, destination) <= 1f) {
                agent.destination = transform.position;
                agent.enabled = false;
            }
        }
        else {
            if (moving) {
                reducedSpeed = moveSpeed - ((moveSpeed / 2) * Mathf.Abs((initialAngle - transform.rotation.eulerAngles.y) % 360) / 360);
                rigidbody.velocity = movingDir * reducedSpeed;
            }
            else {
                rigidbody.velocity = transform.forward * 0;
            }
        }
    } // end of Fixed-Update

	void changeFieldOfView(){
		float angle = GetComponent<FieldOfView>().viewAngle;
		float radius = GetComponent<FieldOfView>().viewRadius;
		if (spotted) {
			if (GetComponent<FieldOfView>().viewAngle > fowSpotedAngle)
				GetComponent<FieldOfView>().viewAngle -= fowTransitionAngle;
			if (GetComponent<FieldOfView>().viewRadius < fowSpotedRadius)
				GetComponent<FieldOfView>().viewRadius += fowTransitionRadius;
		} else {
			if (GetComponent<FieldOfView>().viewAngle < fowSeekAngle)
				GetComponent<FieldOfView>().viewAngle += fowTransitionAngle;
			if (GetComponent<FieldOfView>().viewRadius > fowSeekRadius)
				GetComponent<FieldOfView>().viewRadius -= fowTransitionRadius;
		}
	}

	void eat(){
		if (eatPreyList.Count > 0) {
			GameObject g = (GameObject)eatPreyList [0];
			if (GetComponent<FieldOfView> ().visibleTargets.Count > 0) {
				removePreyFromList (g);
				if (g != null) {
					hunger += 400;
					GetComponent<FieldOfView> ().visibleTargets.Clear ();
                    g.GetComponent<PreyScript>().damage();
				}
			}
		}
	} // end of eat

    /*
	 * Action Move towards the movingDir (direction)
	 * initialAngle is to reduce the speed according to how much the rotation of the object has altered during that initial move action
	 */
    void moveForward() {
        agent.enabled = false;
        movingDir = transform.forward;
        initialAngle = transform.rotation.eulerAngles.y;
        moving = true;
    }

    /*
	 * Action MoveBack
	 */
    void moveBackward() {
        agent.enabled = false;
        movingDir = -transform.forward;
        initialAngle = (transform.rotation.eulerAngles.y + 180) % 360;
        moving = true;
    }

    /*
	 * move towards an eadible object
	 */
    void moveToTarget() {
        if(!agent.enabled)
            stop();
        agent.enabled = true;
        if (GetComponent<FieldOfView>().visibleTargets.Count > 0) {
            Transform eadible = (Transform)GetComponent<FieldOfView>().visibleTargets[0];
            Vector3 targetPosition = eadible.position;
            agent.SetDestination(targetPosition);
            destination = targetPosition;
        }
    }

    /*
	 * Action Stop. stops the movement of the object
	 */
    void stop() {
        agent.enabled = false;
        moving = false;
        rigidbody.velocity = transform.forward * 0;
    }

    /*
	 * Look At sound Source
	 */
    void lookAtSound(){
		if (soundList.Count > 0) {
			Vector3 origin = soundList[0];
			rotation.lookAtObject(origin,true);
			/*agent.enabled = true;
			destination = origin;
			agent.SetDestination (origin); */
		}
	}

    void goToSound() {
        if(soundList.Count > 0) {
            Vector3 origin = soundList[0];
            agent.SetDestination(origin);
            destination = origin;
        }
    }

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
		case Action.Move:
			moveForward ();
			break;
		case Action.Stop:
			stop ();
			break;
		case Action.FollowObject:
            moveToTarget();
			break;
        case Action.Eat:
            eat();
            break;
		case Action.Idle:
			Idle ();
			break;
		case Action.Seek:
			Seek ();
			break;
		case Action.Attack:
			Attack ();
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
				Action.Attack,
				Action.Seek
			};
			break;
		case State.Seek:
			chooseActions = new Action[] {
				Action.RotateLeft,
				Action.RotateRight,
				Action.LookAtObject,
				Action.Move,
				Action.Stop,
				Action.Attack,
				Action.Idle
			};
                if (GetComponent<FieldOfView>().visibleTargets.Count > 0)
                    availableActions[(int)Action.FollowObject] = true;
			break;
		case State.Attack:
			chooseActions = new Action[]{
				Action.RotateLeft, 
				Action.RotateRight, 
				Action.LookAtObject, 
				Action.Move, 
				Action.Stop, 
				Action.Seek, 
				Action.Idle
			};
                if (GetComponent<FieldOfView>().visibleTargets.Count > 0)
                    availableActions[(int)Action.FollowObject] = true;
                if (eatPreyList.Count > 0)
				availableActions [(int)Action.Eat] = true;
			break;
		case State.Dead:
			chooseActions = null;
			break;
		}

		if (soundList.Count > 0 && currentState!=State.Dead) // check if there is a sound origin that the o can check out
			availableActions [(int)Action.LookAtObject] = true;
		
		if (chooseActions != null) {
			for (int i = 0; i < chooseActions.Length; i++) {
				availableActions [(int)chooseActions [i]]=true;
			}
		}
		return availableActions;
	} // end of getAvailableActions 

    /*
     * generate an array with states
     */
    int[] generateStates() {
        int[] stateArray = new int[9];
        stateArray[0] = (int)currentState; // current state
        stateArray[1] = (int)System.Convert.ToSingle(moving); // is it moving
        stateArray[2] = (int)System.Convert.ToSingle(agent.enabled);
        stateArray[3] = (int)System.Convert.ToSingle(soundList.Count > 0); // length of soundlist
        stateArray[4] = (int)System.Convert.ToSingle(GetComponent<FieldOfView>().visibleTargets.Count > 0); // size of visible targets
        stateArray[5] = (int)System.Convert.ToSingle(GetComponent<FieldOfView>().visibleEadibles.Count > 0); // size of visible targets
        stateArray[6] = (int)System.Convert.ToSingle(eadibleList.Count > 0); // contact with eadibles
        stateArray[7] = staminaLevel(); // stamina level
        stateArray[8] = hungerLevel(); // hunger level

        return stateArray;
    } // end of generateStates

	/*
	 * changes the state to Idle
	 */
	void Idle(){
		currentState = State.Idle;
	}

	/*
	 * changes the state to Seek
	 */
	void Seek(){
		currentState = State.Seek;
	}

	/*
	 * changes the state to Attack
	 */
	void Attack(){
		currentState = State.Attack;
	}

	/*
	 * changes the state to Dead
	 */
	void Dead(){
		currentState = State.Dead;
	}

    /*
	 * returns the reward according to the state and action 
	 */
    public override int reward(int state, int action) {
        int reward = 0;
        if (intToState(state) != PredatorScript.State.Dead) {
            if (action == (int)PredatorScript.Action.Eat)
                reward = 100;
            else
                reward = 0;
        }
        else
            reward = -100;
        return reward;
    }

    /*
	 * adds prey to the list when the prey collides with the secondary game object
	 */
    public void addPreyToList(GameObject g){
		eatPreyList.Add (g);
	}

	/*
	 * removes prey from the list when it exits the collision
	 */
	public void removePreyFromList(GameObject g){
		for (int i = 0; i < eatPreyList.Count; i++) {
			if (eatPreyList [i] == g) {
				eatPreyList.RemoveAt (i);
			}
		}			
	}

	/*
	 * converts an integer to a state
	 */
	PredatorScript.State intToState(int i){
		if (i == (int)PredatorScript.State.Idle)
			return PredatorScript.State.Idle;
		else if (i == (int)PredatorScript.State.Seek)
			return PredatorScript.State.Seek;
		else if (i == (int)PredatorScript.State.Attack)
			return PredatorScript.State.Attack;
		else 
			return PredatorScript.State.Dead;
	} // end of intToState

    /*
    * converts int to an action
    */
    PredatorScript.Action intToAction(int a) {
        foreach (Action action in System.Enum.GetValues(typeof(Action))) {
            if ((int)action == a)
                return action;
        }
        return Action.None;
    } // end of intToAction

    PredatorScript.Action keyboardActions(bool[] availableActions) {
        Action action = Action.None;
        ////// ACTIONS CONTROLLED BY THE KEYBOARD ////// 
        if (Input.GetKey(KeyCode.E) && availableActions[(int)Action.Eat]) {
            Debug.Log("Prey Eating!");
            //eat ();
            action = Action.Eat;
        }

        if (Input.GetKey(KeyCode.A) && availableActions[(int)Action.Move]) {
            //rigidbody.velocity = transform.forward * moveSpeed;
            //move();
            action = Action.Move;
        }
        if (Input.GetKey(KeyCode.Z) && availableActions[(int)Action.FollowObject]) {
            action = Action.FollowObject;
        }
        if (Input.GetKey(KeyCode.D) && availableActions[(int)Action.Stop]) {
            //rigidbody.velocity = transform.forward * 0;
            //stop();
            action = Action.Stop;
        }
        if (Input.GetKey(KeyCode.X) && availableActions[(int)Action.LookAtObject]) {
            action = Action.LookAtObject;
        }

        //switch state test
        if (Input.GetKey(KeyCode.Alpha1) && availableActions[(int)Action.Idle]) { // Idle
                                                                                  //Idle ();
            action = Action.Idle;
        }
        if (Input.GetKey(KeyCode.Alpha2) && availableActions[(int)Action.Seek]) { // Seek
                                                                                  //Seek ();
            action = Action.Seek;
        }
        if (Input.GetKey(KeyCode.Alpha3) && availableActions[(int)Action.Attack]) { // Flee
                                                                                  //Flee ();
            action = Action.Attack;
        }

        if (Input.GetKey(KeyCode.Q) && availableActions[(int)Action.RotateLeft]) { // 1 on keyboard to rotate left
                                                                                   //rotation.RotateLeft (); // method will be called by controller
            action = Action.RotateLeft;
        }
        if (Input.GetKey(KeyCode.W) && availableActions[(int)Action.RotateRight]) { // 2 on keyboard to rotate right
                                                                                    //rotation.RotateRight (); // method will be called by controller
            action = Action.RotateRight;
        }
        return action;
    }// end of keyboard

    /*
     * follows waypoints and flees to the sight of the enemy
     */
    void dummyBehaviour() {
        if (eatPreyList.Count > 0)
            eat();
        else if (GetComponent<FieldOfView>().visibleTargets.Count > 0) {
            Transform target = (Transform)GetComponent<FieldOfView>().visibleTargets[0].transform;
            Vector3 targetPosition = target.position;
            agent.speed = AttackMoveSpeed;
            agent.destination = targetPosition;
        }
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GotoNextPoint();
    } // end of dummy behaviour

    void GotoNextPoint() {
        // Returns if no points have been set up
        if (points.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        agent.destination = points[destPoint].position;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % points.Length;
    } // end of gotonextpoint

} // end of PredatorScript