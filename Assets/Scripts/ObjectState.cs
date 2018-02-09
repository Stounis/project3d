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

    /*
     * prints the state array in a message
     */
    public string printState() {
        string m = "";

        for (int i = 0; i < stateArray.Length; i++) {
            if (i != stateArray.Length - 1)
                m = m + stateArray[i] + " ";
            else
                m = m + stateArray[i];
        }

        return m;
    }
} // end of state class