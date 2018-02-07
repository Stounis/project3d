using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class ObjectState {

	int id;
	int[] stateArray;

	public ObjectState(int i, int[] ar){
		id = i;
		stateArray = ar;
	}

	public int getId(){return id;}

    public int getState() { return stateArray[0]; }

	public bool doesMatch(int[] ar){
		bool match = true;
		for (int i = 0; i < stateArray.Length; i++) {
			if (stateArray[i] != ar[i]){
				match = false;
				break;
			}
		}
		return match;
	}
} // end of state class