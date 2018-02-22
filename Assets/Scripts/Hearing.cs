using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hearing : MonoBehaviour {

	CapsuleCollider collider;
	public float hearingRadius = 6;

	// Use this for initialization
	void Start () {
		collider = GetComponent<CapsuleCollider> ();
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = this.GetComponentInParent<Transform> ().rotation;

		bool moving = GetComponentInParent<Controller> ().moving;
        bool agent = GetComponentInParent<Controller>().agentEnabled();
        float speed = GetComponentInParent<Controller> ().reducedSpeed;

        if ((moving || agent) && speed > 2f) {
			Controller c = GetComponentInParent<Controller> ();
			collider.radius = hearingRadius/(c.moveSpeed - c.moveSpeed/speed);
		} else {
			collider.radius = hearingRadius;
		}
	} // end of Update

	/*
	 * Adds itself to the collider's soundlist when it enters the collision 
	 */
	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Sound" && !other.GetComponent<Sound>().silent) {
			Controller c = this.GetComponentInParent<Controller> ();

			//Debug.Log (this.GetComponentInParent<Transform>().parent.tag + " heared " + other.GetComponentInParent<Transform>().parent.tag);
		
			Vector3 point = (other.gameObject.transform.position + transform.position) / 2;
			c.addSoundObject (point);
		}
	} // end of OnTriggerEnter 

	/*
	 * changing the radius of the sound the object is making
	 * according to its movement speed
	 */
	void OnTriggerStay(Collider other){
		if (other.gameObject.tag == "Sound" && !other.GetComponent<Sound>().silent) {
			Controller c = this.GetComponentInParent<Controller> ();

			//Debug.Log (this.GetComponentInParent<Transform>().parent.tag + " heared " + other.GetComponentInParent<Transform>().parent.tag);

			Vector3 point = (other.gameObject.transform.position + transform.position) / 2;
			c.addSoundObject (point);
		}
	} // end of OnTriggerStay 
} // end of hearing class
