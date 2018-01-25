using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PreyScript : Controller {

	//states
	enum State {Idle, Seek, Flee, Dead};
	State currentState;

	bool flee = false;
	Vector3 destination;

	Vector3 movingDir;
	bool moving = false;

	// rotation reduction speed
	float initialAngle;
	public float reducedSpeed; // testing

	// init method
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		//viewCamera = Camera.main;
		agent = GetComponent<NavMeshAgent> ();
		initialAngle = transform.rotation.y; // the object only rotates on the y axis
		currentState = State.Idle;
	}

	void Update(){
		if (alive) {
			agent.enabled = true;
			if (!flee) {
				agent.destination = transform.position;
				agent.enabled = false;
				//Vector3 mousePos = viewCamera.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
				//transform.LookAt (mousePos + Vector3.up * transform.position.y);
				//velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * moveSpeed;

				if (Input.GetKey (KeyCode.E)) {
					eat ();
				}

				if (Input.GetKey (KeyCode.F)) {
					//rigidbody.velocity = transform.forward * moveSpeed;
					moving = true;
					movingDir = transform.forward;
					initialAngle = transform.rotation.y;
				}

				if (Input.GetKey (KeyCode.G)) {
					//rigidbody.velocity = transform.forward * 0;
					moving = false;
				}

				if (moving) {
					reducedSpeed = moveSpeed - ((moveSpeed/2)*Mathf.Abs((initialAngle-transform.rotation.y)%180));
					rigidbody.velocity = movingDir * reducedSpeed;
				} else {
					rigidbody.velocity = transform.forward * 0;
				}

				//rotate ();

				if (stamina < 0) {
					alive = false;
				}
			} else {
				if (Vector3.Distance(transform.position, destination) <= 1f) {
					flee = false;
					agent.destination = transform.position;
				}
			}
		}    
	}

	void updatetest(){
		if (alive) {
			switch (currentState) {
			case State.Idle:
				rest (0.01f);	
				break;
			case State.Seek:
				consumeStamina (0.05f);
				break;
			case State.Flee:
				consumeStamina (0.1f);
				break;
			case State.Dead:
				break;
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
	}

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
		

	void move(){
		
	}

	void eat(){
		if (eadibleList.Count > 0) {
			GameObject e = (GameObject)eadibleList [0];
			eadibleList.RemoveAt (0);
			Destroy (e);
			stamina += 30;
		}
	}
}
