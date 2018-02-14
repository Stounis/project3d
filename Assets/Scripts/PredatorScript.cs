using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

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
	int rotateSpeed = 75;
	bool spotted = false;
	Rotation rotation;


	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		agent = GetComponent<NavMeshAgent> ();
		rotation = GetComponent<Rotation> ();
		currentState = State.Seek;
		initialAngle = transform.rotation.y;        
    }
	
	// Update is called once per frame
	void Update () {

		if (agent.enabled && Vector3.Distance(transform.position, destination) <= 1f) {
			agent.destination = transform.position;
			agent.enabled = false;
		}

		switch(currentState){
		case State.Idle:
			rest (0.01f);
			break;
		case State.Seek:
			consumeStamina (0.01f);
			break;
		case State.Attack:
			consumeStamina (0.3f);
			break;
		}

		//rotate ();
		changeFieldOfView ();

		if (Input.GetKey(KeyCode.N)){
			eat ();
		}

		if(Input.GetKey(KeyCode.M)){
			lookAtSound ();			
		}

		// starved to death, rip
		if (hunger < 0) {
			Dead ();
		}
		
	} // end of Update

	/*
	 * Updates the Physics
	 */
	void FixedUpdate() {
        // reducing the speed according to the rotation of the object
        if (agent.enabled)
        {
            if (agent.enabled && Vector3.Distance(transform.position, destination) <= 1f)
            {
                agent.destination = transform.position;
                agent.enabled = false;
            }
        }
        else
        {
            if (moving)
            {
                reducedSpeed = moveSpeed - ((moveSpeed / 2) * Mathf.Abs((initialAngle - transform.rotation.y) % 180));
                rigidbody.velocity = movingDir * reducedSpeed;
            }
            else
            {
                rigidbody.velocity = transform.forward * 0;
            }
        }
    } // end of Fixed-Update

    /*
     * returns true if agent is dead
     */
    public override bool isDead()
    {
        return State.Dead == currentState;
    } // end of isdead

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
					Destroy (g);	
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
        initialAngle = transform.rotation.y;
        moving = true;
    }

    /*
	 * Action MoveBack
	 */
    void moveBackward() {
        agent.enabled = false;
        movingDir = -transform.forward;
        initialAngle = Quaternion.Inverse(transform.rotation).y;
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
			rotation.lookAtObject(origin);
			//agent.enabled = true;
			//destination = origin;
			//agent.SetDestination (origin);
		}
	}

	/*
	 * Selects the next action
	 */
	void selectAction(int a){
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
			//agent follow
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
	public bool[] getAvailableActions(int s){
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

} // end of PredatorScript