using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (FieldOfView))]
public class FieldOfViewEditor : Editor {

	void OnSceneGUI() {
		FieldOfView fow = (FieldOfView)target;
		Handles.color = Color.blue;
		Handles.DrawWireArc (fow.transform.position, Vector3.up, Vector3.forward, 360, fow.viewRadius);
		Vector3 viewAngleA = fow.DirFromAngle (-fow.viewAngle / 2, false);
		Vector3 viewAngleB = fow.DirFromAngle (fow.viewAngle / 2, false);

		Handles.DrawLine (fow.transform.position, fow.transform.position + viewAngleA * fow.viewRadius);
		Handles.DrawLine (fow.transform.position, fow.transform.position + viewAngleB * fow.viewRadius);


		// Draw line between transform and target
		Handles.color = Color.red;
		foreach (Transform visibleTarget in fow.visibleTargets) {
			Handles.DrawLine (fow.transform.position, visibleTarget.position);
		}

		// Draw line between transform and eadible objects
		Handles.color = Color.magenta;
		foreach (Transform visibleEadible in fow.visibleEadibles){			
			Handles.DrawLine (fow.transform.position, visibleEadible.position);
		}
	}

}