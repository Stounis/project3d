using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class Controller : MonoBehaviour {

	public float moveSpeed = 6;
	public float weight = 50;
	public float stamina = 100; // used for walk, run etc
	public float hunger = 50;
	public string id;

	protected bool alive = true;

	public ArrayList eadibleList = new ArrayList(); // list that holds all the eadible objects that the controller collides with
	public List<Vector3> soundList =  new List<Vector3>(); // holds vector3 points when the sound capsule collides with the controller

	protected Rigidbody rigidbody;
	protected Camera viewCamera;
	protected Vector3 velocity;
	protected NavMeshAgent agent;

	// Field Of View Variables
	/*float fowSpotedAngle = 30;
	float fowSpotedRadius = 8;     
	float fowSeekAngle = 75;
	float fowSeekRadius = 4;
	float fowTransitionAngle = 0.3f;
	float fowTransitionRadius = 0.01f;*/

	void Start () {
		//rigidbody = GetComponent<Rigidbody> ();
		//viewCamera = Camera.main;
		//agent = GetComponent<NavMeshAgent> ();
	}
		
	void Update () {
		
	}

	void FixedUpdate() {
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

	void changeFieldOfView(){
		/*float angle = GetComponent<FieldOfView>().viewAngle;
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
		} */
	}
		
	public string getId(){
		return id;
	}


	/*
	 * controller rests to regenerate stamina
	 */ 
	protected void rest(float s){
		if (stamina + s <= 100) {
			stamina += s;
		}
	}

	/*
	 * controller consumes stamina s from its stamina level
	 */
	protected void consumeStamina(float s){
		if (stamina - s > 0) {
			stamina -= s;
		}
	}

	/*
	 * adds the eadible object that collided with the controller in the list
	 */
	public void addEadibleObject(GameObject g){
		eadibleList.Add (g);
	}

	/*
	 * removes the eadible object when the controller exits the collision with it
	 */
	public void removeEadibleObject(GameObject g){
		for (int i = 0; i < eadibleList.Count; i++) {
			if (g == eadibleList [i]) {
				eadibleList.RemoveAt(i);
			}
		}
	} // end of removeEadibleObject

	public void addSoundObject(Transform soundOrigin) {
		if (soundList.Count > 0)
			soundList.RemoveAt (0);
		soundList.Add (soundOrigin.transform.position);
	}

} // end of Controller Class