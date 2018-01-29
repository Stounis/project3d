using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour {

	CapsuleCollider collider;

	// Use this for initialization
	void Start () {
		collider = GetComponent<CapsuleCollider> ();
	}
	
	// Update is called once per frame
	/*
	 * changing the radius of the sound the object is making
	 * according to its movement speed
	 */
	void Update () {
		bool moving = GetComponentInParent<PreyScript> ().moving;
		float speed = GetComponentInParent<PreyScript> ().reducedSpeed;
		if (moving) {
			collider.radius = speed;
		} else {
			collider.radius = 0.5f;
		}
	} // end of Update

	/*
	 * Adds itself to the collider's soundlist when it enters the collision 
	 */
	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Predator" || other.gameObject.tag=="Prey") {
			Controller c = other.gameObject.GetComponent<Controller> ();
			c.addSoundObject (this.gameObject.transform);
			Debug.Log ("SOUND");
		}
	} // end of OnTriggerEnter


	/*
	 * Updates the position of the sound origin in the controller SoundList
	 * while it still colides with the object
	 */
	void OnTriggerStay(Collider other){
		if (other.gameObject.tag == "Predator" || other.gameObject.tag=="Prey") {
			Controller c = other.gameObject.GetComponent<Controller> ();
			c.addSoundObject (this.gameObject.transform);
		}
	} // end of OnTriggerStay

} // end of Sound class
