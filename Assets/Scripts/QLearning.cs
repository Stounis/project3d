using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QLearning {

    Controller controller;

    float alpha = 0.1f; // alpha : Learning rate
    float gamma = 0.9f;// gamma : 0 looks near future, 1 looks distant future

    // list of states 
    // list of actions
    //float[,] Q;// q table mapping actions to states
    ArrayList Q;
    StateArray stateArray;
    int actionSize;


    ArrayList memory; // stores previous state/actions to update the q table
    int maxMemory = 60; // max number of state/actions stored in memory

    /*
	 * Constructor
	 */
    public QLearning(Controller c, StateArray sr, int asize) {
        controller = c;
        //Q = new float[controller.stateSize,controller.actionSize];
        Q = new ArrayList();
        stateArray = sr;
        actionSize = asize;
        memory = new ArrayList();
        // test
        //Q [0,0] = 1f;
        //Q [0,2] = 2f;
        //Debug.Log (maxQ (0));
    } // end of Constructor

    /*
	 * updates the q table according to the selected action and the current state
     * rl = reinforcement learning
	 */
    public void rl(ObjectState oldState, int action, ObjectState newState) {

        updateStates();

        addMemory(oldState.getId(), action);

        maintainMemory();
        
        // Q(s,a) = Q(s,a) + a * (r(s,a) + g * maxQ(s') - Q(s,a)) 
        float q = getQvalue(oldState.getId(), action); //float q = Q[state,action];
        float maxq = maxQ(newState.getId()); //float maxQ = maxQ (state);
        float r = controller.reward(oldState.getState(), action);

        float value = q + alpha * (r + gamma * maxq - q);//float value = q + alpha * (r + gamma * maxQ - q);

        updateQvalue(oldState.getId(), action, value);
    } // end of rl

    void calculateQ() {

    }

    /*
     * sets new Controller for the algorithms
     */
    public void setController(Controller c) {
        controller = c;
    }

    /*
     * add to memory
     */
    void addMemory(int state, int action) {
        int[] mem = new int[2];
        mem[0] = state;
        mem[1] = action;
        memory.Add(mem);
    }

    /*
     * keep track of the memory to be max maxMemory size
     */
    void maintainMemory() {
        if (memory.Count > maxMemory) {
            for (int i = memory.Count; i > maxMemory; i--) {
                memory.RemoveAt(0); // remove the first memory FIFO
            }
        }
    } // end of maintain memory

    /*
     * returns the action with the highest q value
     */
    public int bestAction(ObjectState s) {

        updateStates();

        int action = 0;
        float maxValue = float.MinValue;

        bool[] possibleActions = controller.getAvailableActions(s.getId());
        float[] Qactions = (float[])Q[s.getId()];
        for (int i = 0; i < possibleActions.Length; i++)
        {
            if (possibleActions[i])
            {
                float value = Qactions[i];
                if (value >= maxValue) {
                    maxValue = value;
                    action = i;
                }
            }
        }
        return action;
    } // end of bestAction

    public int randomAction(ObjectState s) {
        updateStates();
        int state = 0;
        bool[] possibleActions = controller.getAvailableActions(s.getId());
        int trues=0;
        for (int i = 0; i < possibleActions.Length; i++) {
            if (possibleActions[i])
                trues += 1;
        }
        if (trues > 0)
        {
            for (int i = 0; i < possibleActions.Length; i++)
            {
                if (possibleActions[i]) {
                    trues -= 1;
                    if (trues == 0)
                    {
                        state = i;
                        break;
                    }
                }
            }
        }

        return state;
    } // end of randomaction

    /*
	 * Updates the Q table in case that the state does not exist in the q
	 */
    void updateStates() {
        if (Q.Count < stateArray.size()) {
            for (int i = Q.Count; i < stateArray.size(); i++) {
                float[] newRow = new float[actionSize];
                Q.Add(newRow);
            }
        }
    } // end of updateStates

    /*
	 * return the Q Value according to the state and action
	 */
    float getQvalue(int s, int a) {
        float[] state = (float[])Q[s];
        float q = state[a];
        return q;
    }

    /*
	 * updates the Q value of the state and action in the q table
	 */
    void updateQvalue(int s, int a, float q) {
        float[] state = (float[])Q[s];
        state[a] = q;
    }

    /*
	 * returns the maximum Q value From the q table according to the next state
	*/
    float maxQ(int nextState) {
        float maxValue = float.MinValue;

        bool[] possibleActions = controller.getAvailableActions(nextState);
        float[] Qactions = (float[])Q[nextState];
        for (int i = 0; i < possibleActions.Length; i++) {
            if (possibleActions[i]) {
                float value = Qactions[i];
                if (value > maxValue)
                    maxValue = value;
            }
        }
        return maxValue;
    } // end of maxQ

    /*
	 * converts an integer to a state
	 */
    PredatorScript.State intToState(int i) {
        if (i == (int)PredatorScript.State.Idle)
            return PredatorScript.State.Idle;
        else if (i == (int)PredatorScript.State.Seek)
            return PredatorScript.State.Seek;
        else if (i == (int)PredatorScript.State.Attack)
            return PredatorScript.State.Attack;
        else
            return PredatorScript.State.Dead;
    } // end of intToState

    public void printQTable() {
        string message = "";
        foreach (float[] row in Q) {            
            for (int i = 0; i < row.Length; i++) {
                message = message + " " + row[i];
            }
            message = message + "\n";
        }
        Debug.Log(message);
    } // end of printQTable
} // end of QLearning
