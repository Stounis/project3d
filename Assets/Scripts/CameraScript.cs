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
            if (GameObject.FindGameObjectsWithTag("Prey").Length>0) {
                prey = GameObject.FindGameObjectWithTag("Prey");
                observePrey();
            }            
		}

		if (Input.GetKey (KeyCode.O)) { // follow predator
            if (GameObject.FindGameObjectsWithTag("Predator").Length > 0) {
                predator = GameObject.FindGameObjectWithTag("Predator");
                observePredator();
            }            
		}

		if (Input.GetKey (KeyCode.P)) { // stop following
            initCamera();
		}

		if (followPrey) {
            if (prey != null) {
                Vector3 follow = new Vector3(prey.transform.position.x, initialPosition.y, prey.transform.position.z);
                transform.position = follow;
            } else {
                initCamera();
            }
		} else if (followPredator) {
            if (predator != null) {
                Vector3 follow = new Vector3(predator.transform.position.x, initialPosition.y, predator.transform.position.z);
                transform.position = follow;
            } else {
                initCamera();
            }
		} 
	} // end of update

    /*
     * set camera to follow prey
     */
    void observePrey() {
        followPrey = true;
        followPredator = false;
    }

    /*
     * set camera to follow predator
     */
    void observePredator() {
        followPrey = false;
        followPredator = true;
    }

    /*
     * set camera to initial position
     */
    void initCamera() {
        followPrey = false;
        followPredator = false;
        transform.position = initialPosition;
        camera.orthographicSize = initialSize;
    } // end of initCamera

} // end of camera script 
