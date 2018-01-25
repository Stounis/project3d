using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

	public GameObject prey;
	public GameObject predator;

	bool followPrey = false;
	bool followPredator = false;

	Vector3 initialPosition;
	float initialSize;
	Camera camera;

	// Use this for initialization
	void Start () {
		camera = GetComponent<Camera>();
		initialPosition = transform.position;
		initialSize = camera.orthographicSize;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (Input.GetAxis ("Mouse ScrollWheel") > 0f) { // scroll up
			camera.orthographicSize -= 0.5f;
		} else if (Input.GetAxis ("Mouse ScrollWheel") < 0f) { // scroll down
			camera.orthographicSize += 0.5f;
		} 

		if (Input.GetKey (KeyCode.I)) { // follow prey
			followPrey = true;
			followPredator = false;
		}

		if (Input.GetKey (KeyCode.O)) { // follow predator
			followPrey = false;
			followPredator = true;
		}

		if (Input.GetKey (KeyCode.P)) { // stop following
			followPrey = false;
			followPredator = false;
			transform.position = initialPosition;
			camera.orthographicSize = initialSize;
		}

		if (followPrey) {
			if (prey != null) {
				Vector3 follow = new Vector3 (prey.transform.position.x, initialPosition.y, prey.transform.position.z);
				transform.position = follow;
			}
		} else if (followPredator) {
			if (predator != null) {
				Vector3 follow = new Vector3 (predator.transform.position.x, initialPosition.y, predator.transform.position.z);
				transform.position = follow;
			}
		} 
	}
}
