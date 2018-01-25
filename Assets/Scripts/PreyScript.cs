using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PreyScript : Controller {

	//states and actions
	enum State {Idle, Seek, Flee, Dead};
	enum Action {RotateLeft, RotateRight, LookAtObject, Move, Stop, FollowObject, Eat, Idle, Seek, Flee}; // possible actions. Maybe NoAction as another option?
	State currentState;
	Action currentAction;

	bool flee = false;
	Vector3 destination;

	Vector3 movingDir;
	bool moving = false;

	// rotation reduction speed
	float initialAngle;
	public float reducedSpeed; // testing
	Rotation rotation; // class which controls the rotation of the object

	// init method
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		//viewCamera = Camera.main;
		agent = GetComponent<NavMeshAgent> ();
		initialAngle = transform.rotation.y; // the object only rotates on the y axis
		currentState = State.Idle;
		rotation = GetComponent<Rotation> ();
	} // end of Start

	void Update(){
		if (alive) {
			agent.enabled = true;
			if (!flee) {
				agent.destination = transform.position;
				agent.enabled = false;
				//Vector3 mousePos = viewCamera.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
				//transform.LookAt (mousePos + Vector3.up * transform.position.y);
				//velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * moveSpeed;

				switch (currentState) {
				case State.Idle:
					rest (0.1f);
					break;
				case State.Seek:
					consumeStamina (0.01f);
					break;
				case State.Flee:
					consumeStamina (0.1f);
					break;
				}

				bool [] availableActions = getAvailableActions(); // get the available actions

				if (Input.GetKey (KeyCode.E) && availableActions[(int)Action.Eat]) {
					Debug.Log ("Eating!");
					eat ();
				}

				if (Input.GetKey (KeyCode.F) && availableActions[(int)Action.Move]) {
					//rigidbody.velocity = transform.forward * moveSpeed;
					move();
				}

				if (Input.GetKey (KeyCode.G) && availableActions[(int)Action.Stop]) {
					//rigidbody.velocity = transform.forward * 0;
					stop();
				}

				//switch state test
				if (Input.GetKey (KeyCode.Alpha3) && availableActions[(int)Action.Idle]) // Idle
					Idle();
				if (Input.GetKey (KeyCode.Alpha4) && availableActions[(int)Action.Seek]) // Seek
					Seek ();
				if (Input.GetKey (KeyCode.Alpha5) && availableActions[(int)Action.Flee]) // Flee
					Flee ();


				// reducing the speed according to the rotation of the object
				if (moving) {
					reducedSpeed = moveSpeed - ((moveSpeed/2)*Mathf.Abs((initialAngle-transform.rotation.y)%180));
					rigidbody.velocity = movingDir * reducedSpeed;
				} else {
					rigidbody.velocity = transform.forward * 0;
				}
					
				if (stamina < 0) {
					alive = false;
				}
			} else { // agent stuff
				if (Vector3.Distance(transform.position, destination) <= 1f) {
					flee = false;
					agent.destination = transform.position;
				}
			}
		}    
	}

	/*
	 * prey rests to regenerate stamina
	 */ 
	void rest(float s){
		if (stamina + s <= 100) {
			stamina += s;
		}
	}

	/*
	 * prey consumes stamina s from its stamina level
	 */
	void consumeStamina(float s){
		if (stamina - s > 0) {
			stamina -= s;
		}
	}


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

	void FixedUpdate(){
		 /*if (alive) {
			Vector3 stationary = new Vector3 (0, 0, 0);
			if (velocity != stationary) {	
				stamina -= 0.01f * moveSpeed;
			} else {
				stamina -= 0.01f;
			}
			rigidbody.MovePosition (rigidbody.position + velocity * Time.fixedDeltaTime);
			stamina -= 0.01f;
		} */
	}
		
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
			stamina += 30;
		}
	}

	/*
	 * returns a boolean array with all the available actions of the object.
	 * the actions are selected according to the object's state and other parameters.
	 */
	bool[] getAvailableActions(){
		bool[] availableActions = new bool[10];
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
}
