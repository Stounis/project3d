using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorTestEat : MonoBehaviour {

	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Prey") {
            if (!other.transform.GetComponent<Controller>().isDead()) {
                PredatorScript script = transform.GetComponentInParent<PredatorScript>();
                script.addPreyToList(other.gameObject);
            }
		}
	}

	void OnTriggerExit(Collider other){
		if(other.gameObject.tag=="Prey"){
            if (!other.transform.GetComponent<Controller>().isDead()) {
                PredatorScript script = transform.GetComponentInParent<PredatorScript>();
                script.removePreyFromList(other.gameObject);
            }
		}
	}
}
