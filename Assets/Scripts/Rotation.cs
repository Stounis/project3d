using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour {
	Transform transform;
	int rotateSpeed = 100;
	public float maxRotation = 15f;
	public float rotation = 0;
	bool rotating = false;
	bool lookLeft = false;
	bool lookRight = false;
	bool lookStraight = false;
	// Use this for initialization
	void Start () {
		transform = GetComponent<Transform> ();
	}
	
	// Update is called once per frame
	// not used!
	void UpdateT () {
		if (!rotating) {
			float random = Random.Range (0.0f, 1.0f);
			if (random < 0.35f) {
				lookLeft = true;
				rotateLeft ();
			} else if (random > 0.35f && random < 0.7f) {
				lookRight = true;
				rotateRight ();
			} else {
				lookStraight = true;
			}
			rotating = true;
		} else {

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

			if (lookStraight) {
				if (rotation > maxRotation) {
					lookStraight = false;
					rotating = false;
					rotation = 0f;
				} else {
					rotation += 1f;
				}
			}
		}
	} // end of Update


	/*
	 *  Updates every frame
	 */
	void Update(){

		if (Input.GetKey (KeyCode.Alpha1)) // 1 on keyboard to rotate left
			RotateLeft (); // method will be called by controller

		if (Input.GetKey (KeyCode.Alpha2)) // 2 on keyboard to rotate right
			RotateRight (); // method will be called by controller

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
	void lookAtObject(GameObject enemy) {
		transform.LookAt (enemy.transform);
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
