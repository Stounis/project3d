using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eadibles : MonoBehaviour {

	public GameObject eadible;
	public int maxNumEadibles;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(GameObject.FindGameObjectsWithTag("Eadible").Length<maxNumEadibles){
			Vector3 position = new Vector3 (Random.Range(-14f,14f),0.1f,Random.Range(-13f,13f));
			eadible.transform.position = position;
			Instantiate (eadible);
		}
	}
}
