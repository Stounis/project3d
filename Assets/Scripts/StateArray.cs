using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class StateArray {

	ArrayList states = new ArrayList();

	public StateArray(){
	}

	public int size(){
		return states.Count;
	}

	public ObjectState addState(int[] s){
		ObjectState state = new ObjectState (states.Count, s);
		states.Add (state);
		Debug.Log ("Added state , total states: " + states.Count);
        return state;
	}

	public ObjectState findState(int[] array){
		foreach (ObjectState state in states) {
            if (state.doesMatch(array)) {
                //Debug.Log("state found: " + state.getId());
                return state;
            }
		}
		return null;
	}

} // end of stateArray class
