﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour {
	Transform transform;
	int rotateSpeed = 100;
	public float maxRotation = 15f;
	public float rotation = 0;
	bool rotating = false; // not used?
	bool lookLeft = false;
	bool lookRight = false;
	bool lookStraight = false;

	// Use this for initialization
	void Start () {
		transform = GetComponent<Transform> ();
	}

	/*
	 *  Updates every frame
	 */
	void Update(){
		if (lookLeft) {
			if (rotation < -maxRotation) {
				lookLeft = false;
				rotating = false;
				rotation = 0f;
			} else {
				rotateLeft ();
				rotation -= 1f;
			}
		}

		if (lookRight) {
			if (rotation > maxRotation) {
				lookRight = false;
				rotating = false;
				rotation = 0f;
			} else {
				rotateRight ();
				rotation += 1f;
			}
		}
	} // end of Update

	void rotateLeft() {
		transform.Rotate (Vector3.up, (-rotateSpeed) * Time.deltaTime); 
	}

	void rotateRight() {
		transform.Rotate (Vector3.up, (rotateSpeed) * Time.deltaTime); // rotate right
	}


	/*
	 * Might not be used
	 */ 
	public void lookAtObject(Vector3 origin) {
		transform.LookAt (origin);
	}

	/*
	 * Accessed by the Controller to make the object rotate left
	 */
	public void RotateLeft(){
		lookLeft = true;
		lookRight = false;
		rotation = 0f;
		rotating = true;
	}


	/*
	 * Accessed by the Controller to make the object rotate right
	 */
	public void RotateRight(){
		lookLeft = false;
		lookRight = true;
		rotation = 0f;
		rotating = true;
	}
}
