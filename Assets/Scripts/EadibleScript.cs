using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EadibleScript : MonoBehaviour {

	public bool eadible = false;

	bool isEadible(){
		return eadible;
	}


	// The Controller walks collides with the object
	// The object adds it self to the eadible object list of the controller
	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Predator" || other.gameObject.tag=="Prey") {
			Controller c = other.gameObject.GetComponent<Controller> ();
			c.addEadibleObject (this.gameObject);
			//Debug.Log (c.id + " entered");
		}
	}

	void OnTriggerStay(Collider other){
	}


	// The controller leaves the edible object
	// It removes itself from the list of the controller
	void OnTriggerExit(Collider other){
		if (other.gameObject.tag == "Predator" || other.gameObject.tag=="Prey") {
			Controller c = other.gameObject.GetComponent<Controller> ();
			c.removeEadibleObject (this.gameObject);
			//Debug.Log (c.id + " exited");
		}
	}
}
