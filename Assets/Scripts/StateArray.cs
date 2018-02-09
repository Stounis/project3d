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

    /*
     * adds a state to the list
     * giving it an id of the size of the list
     */
	public ObjectState addState(int[] s){
		ObjectState state = new ObjectState (states.Count, s);
		states.Add (state);
		Debug.Log ("Added state , total states: " + states.Count);
        return state;
	}

    /*
     * looks for a state in the list
     */
	public ObjectState findState(int[] array){
		foreach (ObjectState state in states) {
            if (state.doesMatch(array)) {
                //Debug.Log("state found: " + state.getId());
                return state;
            }
		}
		return null;
	} // end of findstate

    /*
     * load states from file if state list is empty
     */
    public void loadStates(ArrayList list) {
        if (states.Count > 0)
            return;
        for (int i=0; i<list.Count; i++) {
            int[] state = (int[])list[i];
            ObjectState s = addState(state);
        }
    } // end of loadState

    /*
     * prints all the states in a message
     */
    public string printStates() {
        string m = "";
        for (int i = 0; i < states.Count; i++) {
            ObjectState state = (ObjectState)states[i];
            if (i != states.Count - 1)
                m = m + state.printState() + "\r\n";
            else
                m = m + state.printState();
        }
        return m;
    } // end of printStates

} // end of stateArray class
