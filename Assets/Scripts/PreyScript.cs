using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PreyScript : Controller {

	//states and actions
	enum State {Idle, Seek, Flee, Dead}; // possible states
	enum Action {None, RotateLeft, RotateRight, LookAtObject, Move, Stop, FollowObject, Eat, Idle, Seek, Flee}; // possible actions
	State currentState;
	Action currentAction;

	//speed
	public float FleeMovepSpeed = 6;
	public float SeekMoveSpeed = 4;

	bool flee = false;
	Vector3 destination;

	Vector3 movingDir;
	public bool moving = false;

	// rotation reduction speed
	float initialAngle;
	public float reducedSpeed; // testing
	Rotation rotation; // class which controls the rotation of the object

	// init method
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		agent = GetComponent<NavMeshAgent> ();
		initialAngle = transform.rotation.y; // the object only rotates on the y axis
		rotation = GetComponent<Rotation> ();
		currentState = State.Idle;
	} // end of Start

	/*
	 * Updates every frame
	 */
	void Update(){
		
		agent.enabled = true;
		if (!flee) {
			agent.destination = transform.position;
			agent.enabled = false;
			switch (currentState) {
			case State.Idle:
				rest (0.1f);
				stop ();
				break;
			case State.Seek:
				consumeStamina (0.01f);
				moveSpeed = SeekMoveSpeed;
				break;
			case State.Flee:
				consumeStamina (0.1f);
				moveSpeed = FleeMovepSpeed;
				break;
			}
			hunger -= 0.01f;

			bool[] availableActions = getAvailableActions (); // get the available actions
			Action action = Action.None; // testing actions with keyboard

			////// ACTIONS CONTROLLED BY THE KEYBOARD ////// 
			if (Input.GetKey (KeyCode.E) && availableActions [(int)Action.Eat]) {
				Debug.Log ("Eating!");
				//eat ();
				action = Action.Eat;
			}

			if (Input.GetKey (KeyCode.F) && availableActions [(int)Action.Move]) {
				//rigidbody.velocity = transform.forward * moveSpeed;
				//move();
				action = Action.Move;
			}
			if (Input.GetKey (KeyCode.G) && availableActions [(int)Action.Stop]) {
				//rigidbody.velocity = transform.forward * 0;
				//stop();
				action = Action.Stop;
			}

			//switch state test
			if (Input.GetKey (KeyCode.Alpha3) && availableActions [(int)Action.Idle]){ // Idle
				//Idle ();
				action = Action.Idle;
			}
			if (Input.GetKey (KeyCode.Alpha4) && availableActions [(int)Action.Seek]) { // Seek
				//Seek ();
				action = Action.Seek;
			}
			if (Input.GetKey (KeyCode.Alpha5) && availableActions[(int)Action.Flee]){ // Flee
				//Flee ();
				action = Action.Flee;
			}

			if (Input.GetKey (KeyCode.Alpha1) && availableActions[(int)Action.RotateLeft]){ // 1 on keyboard to rotate left
				//rotation.RotateLeft (); // method will be called by controller
				action = Action.RotateLeft;
			}
			if (Input.GetKey (KeyCode.Alpha2) && availableActions[(int)Action.RotateRight]){ // 2 on keyboard to rotate right
				//rotation.RotateRight (); // method will be called by controller
				action = Action.RotateRight;
			}

			selectAction (action); 

			// reducing the speed according to the rotation of the object
			if (moving) {
				reducedSpeed = moveSpeed - ((moveSpeed/2)*Mathf.Abs((initialAngle-transform.rotation.y)%180));
				rigidbody.velocity = movingDir * reducedSpeed;
			} else {
				rigidbody.velocity = transform.forward * 0;
			}
				
			if (hunger < 0) {
				Dead ();
			}

		} else { // agent stuff
			if (Vector3.Distance(transform.position, destination) <= 1f) {
				flee = false;
				agent.destination = transform.position;
			}
		}		  
	} // end of Update

	/*
	 * Updates Physics 
	 */
	void FixedUpdate(){
	} // end of Fixed-Update

	/*
	 *  NOT USED. rotation moved to different class
	 */
	void rotate(){
		if (GetComponent<FieldOfView>().visibleTargets.Count>0){
			flee = true;
			Transform target = (Transform)GetComponent<FieldOfView> ().visibleTargets [0];
			float distance = Vector2.Distance (new Vector2(transform.position.x, transform.position.z), new Vector2(target.position.x, target.position.z));
			float x = transform.position.x + (transform.position.x - target.position.x);
			float z = transform.position.z + (transform.position.z - target.position.z);
			destination = new Vector3 (x, transform.position.y, z);
			agent.SetDestination (destination);
			transform.LookAt (destination);
		}
	} // end of rotate
		
	/*
	 * Action Move towards the movingDir (direction)
	 * initialAngle is to reduce the speed according to how much the rotation of the object has altered during that initial move action
	 */ 
	void move(){
		moving = true;
		movingDir = transform.forward;
		initialAngle = transform.rotation.y;
	}

	/*
	 * Action Stop. stops the movement of the object
	 */
	void stop(){
		moving = false;
	}

	/*
	 * Action Eat. eats an eadible object if it collides with it
	 */
	void eat(){
		if (eadibleList.Count > 0) {
			GameObject e = (GameObject)eadibleList [0];
			eadibleList.RemoveAt (0);
			Destroy (e);
			hunger += 30;
		}
	} // end of eat

	/*
	 * Selects the next action
	 */
	void selectAction(Action action){
		currentAction = action;
		switch (currentAction) {
		case Action.RotateLeft:
			rotation.RotateLeft ();
			break;
		case Action.RotateRight:
			rotation.RotateRight ();
			break;
		case Action.LookAtObject:
			// rotation look at
			break;
		case Action.Move:
			move ();
			break;
		case Action.Stop:
			stop ();
			break;
		case Action.FollowObject:
			//agent follow
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
	bool[] getAvailableActions(){
		bool[] availableActions = new bool[11];
		Action[] chooseActions = null;
		switch (currentState) {
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
				Action.Move,
				Action.Stop,
				Action.Flee,
				Action.Idle
			};
			if (eadibleList.Count > 0 && !moving)
				availableActions [(int)Action.Eat] = true;
			break;
		case State.Flee:
			chooseActions = new Action[]{
				Action.RotateLeft, 
				Action.RotateRight, 
				Action.LookAtObject, 
				Action.Move, 
				Action.Stop, 
				Action.Seek, 
				Action.Idle
			};
			break;
		case State.Dead:
			chooseActions = null;
			break;
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

} // end of PreyScript Class
