using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PredatorScript : Controller {

	ArrayList eatPreyList = new ArrayList(); //List that holds all prey objects that are within the second collider

	//NavMeshAgent agent;

	//rotate variables
	int rotateSpeed = 75;
	bool spotted = false;

	// Field Of View Variables
	float fowSpotedAngle = 30;
	float fowSpotedRadius = 8;     
	float fowSeekAngle = 75;
	float fowSeekRadius = 4;
	float fowTransitionAngle = 0.3f;
	float fowTransitionRadius = 0.01f;

	// Use this for initialization
	/*void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		viewCamera = Camera.main;
		agent = GetComponent<NavMeshAgent> ();
	}*/
	
	// Update is called once per frame
	void Update () {
		if (alive) {

			// movement from keyboard
			//velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * moveSpeed;

			/*// rotate from Q and E
			if (Input.GetKey (KeyCode.Q)) {
				transform.Rotate (Vector3.up, (-50) * Time.deltaTime); // rotate left
			}
			else if(Input.GetKey(KeyCode.E)){
				transform.Rotate (Vector3.up, (50) * Time.deltaTime); // rotate right
			} */



			rotate ();
			changeFieldOfView ();
			//eatPrey ();

			if (Input.GetKey(KeyCode.N)){
				eatPrey ();
			}

			// starved to death, rip
			if (stamina < 0) {
				alive = false;
			}
		}	
	} // end of Update

	void FixedUpdate() {
		if (alive) {
			Vector3 stationary = new Vector3 (0, 0, 0);
			// check if object moves
			if (velocity != stationary) {	
				stamina -= 0.01f * agent.speed;
			} else {
				stamina -= 0.01f;

			}
			//rigidbody.MovePosition (rigidbody.position + velocity * Time.fixedDeltaTime);
			stamina -= 0.01f;
		}
	}

	void rotate(){
		spotted = false;
		transform.Rotate (Vector3.up, (rotateSpeed) * Time.deltaTime); // rotate right

		// if prey spotted lock onto it
		if (this.GetComponent<FieldOfView> ().visibleTargets.Count > 0) {
			// lock on first prey
			// maybe change later to closest if multiple prey?
			if (this.GetComponent<FieldOfView> ().visibleTargets [0].transform != null) {
				spotted = true;
				transform.LookAt (this.GetComponent<FieldOfView> ().visibleTargets [0].transform);
				agent.SetDestination (this.GetComponent<FieldOfView>().visibleTargets[0].transform.position);
			}
		}
	}

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

	void eatPrey(){
		if (eatPreyList.Count > 0) {
			GameObject g = (GameObject)eatPreyList [0];
			if (GetComponent<FieldOfView> ().visibleTargets.Count > 0) {
				removePreyFromList (g);
				if (g != null) {
					stamina += 400;
					GetComponent<FieldOfView> ().visibleTargets.Clear ();
					Destroy (g);	
				}
			}
		}
	}

	public void addPreyToList(GameObject g){
		eatPreyList.Add (g);
	}

	public void removePreyFromList(GameObject g){
		for (int i = 0; i < eatPreyList.Count; i++) {
			if (eatPreyList [i] == g) {
				eatPreyList.RemoveAt (i);
			}
		}			
	}

} // end of PredatorScript
