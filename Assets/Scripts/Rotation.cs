using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour {
	Transform transform;

	int rotateSpeed = 100;
	public float maxRotation = 15f;
	public float rotation = 0;
	public bool lookLeft = false;
	public bool lookRight = false;
	public bool lookObject = false;
    bool lookSound = false;

	Vector3 objectPosition;

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
				rotation = 0f;
			} else {
				rotateLeft ();
				rotation -= 1f;
			}
		}

		if (lookRight) {
			if (rotation > maxRotation) {
				lookRight = false;
				rotation = 0f;
			} else {
				rotateRight ();
				rotation += 1f;
			}
		}

		if (lookObject) {
			float speed = 3f;
			Vector3 objectDir = (objectPosition - transform.position).normalized;
			objectDir.y = 0; // only rotate on y axis
			Quaternion lookRotation = Quaternion.LookRotation (objectDir);
			float angle = Quaternion.Angle (transform.rotation, lookRotation);
			if (angle > 4f) { // 3 to 4 is the most optimal
				transform.rotation = Quaternion.Slerp (transform.rotation, lookRotation, Time.deltaTime * speed);
			} else {
				lookObject = false;
                if (lookSound) {
                    GetComponent<Controller>().soundList.RemoveAt(0);
                    lookSound = false;
                }
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
	public void lookAtObject(Vector3 origin, bool sound) {
		//transform.LookAt (origin);

		lookLeft = false;
		lookRight = false;
		lookObject = true;

        lookSound = sound;
		objectPosition = origin;
	}

	/*
	 * Accessed by the Controller to make the object rotate left
	 */
	public void RotateLeft(){
		lookLeft = true;
		lookRight = false;
		lookObject = false;
		rotation = 0f;
	}


	/*
	 * Accessed by the Controller to make the object rotate right
	 */
	public void RotateRight(){
		lookLeft = false;
		lookRight = true;
		lookObject = false;
		rotation = 0f;
	}
}
