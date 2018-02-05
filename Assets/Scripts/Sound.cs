using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour {

	CapsuleCollider collider;
	public bool silent = false;
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
		transform.rotation = this.GetComponentInParent<Transform> ().rotation;

		bool moving = GetComponentInParent<Controller> ().moving;
		float speed = GetComponentInParent<Controller> ().reducedSpeed;
		if (moving && speed>2) {
			collider.radius = speed;
			silent = false;
		} else {
			collider.radius = 0.0f;
			silent = true;
		}
	} // end of Update

} // end of Sound class
