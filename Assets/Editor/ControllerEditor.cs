using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (PredatorScript))]
public class ControllerEditor : Editor {

	void OnSceneGUI() {
		PredatorScript c = (PredatorScript)target;
		Handles.color = Color.black;

		// Draw line between transform and eadible objects
		//Handles.color = Color.magenta;
		foreach (Vector3 soundO in c.soundList){			
			Handles.DrawLine (c.transform.position, soundO);
		}
	}

}