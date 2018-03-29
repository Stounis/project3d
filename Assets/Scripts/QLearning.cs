using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QLearning {

    Controller controller;

    float alpha = 0.3f; // alpha : Learning rate
    float gamma = 0.8f; // gamma : 0 looks near future, 1 looks distant future
    float epsilon = 0.1f; // epsilon : exploration of actions

    // list of states 
    // list of actions
    //float[,] Q;// q table mapping actions to states
    ArrayList Q;
    StateArray stateArray;
    int actionSize;

    public bool bestaction = false;

    ArrayList memory; // stores previous state/actions to update the q table
    int maxMemory = 5; // max number of state/actions stored in memory
    int memoryRewardRed = 1;

    ArrayList rewardList = new ArrayList();
    int totalReward = 0;

    /*
	 * Constructor
	 */
    public QLearning(Controller c, StateArray sr, int asize) {
        controller = c;
        Q = new ArrayList();
        stateArray = sr;
        actionSize = asize;
        memory = new ArrayList();
    } // end of Constructor

    /*
	 * updates the q table according to the selected action and the current state
     * rl = reinforcement learning
	 */
    public void rl(ObjectState oldState, int action, ObjectState newState) {

        updateStates(); // add state in the array if new state       

        // Q(s,a) = Q(s,a) + a * (r(s,a) + g * maxQ(s') - Q(s,a)) 
        float q = getQvalue(oldState.getId(), action); //float q = Q[state,action];
        float maxq = maxQ(newState); //float maxQ = maxQ (state);
        int r = controller.reward(newState.getState(), action);

        //float value = q + alpha * (r + gamma * maxq - q);//float value = q + alpha * (r + gamma * maxQ - q);
        float value = q + alpha * (r + gamma * 1 - q);
        Debug.Log("q: " + q + " maxq: " + maxq);
        Debug.Log("reward: " + r + " value: " + value);
        updateQvalue(oldState.getId(), action, value); // q[oldstate,action] = value

        addMemory(oldState.getId(), action);
        maintainMemory();

        totalReward += r;
        // repeat q process for memory
        /*if (memory.Count > 1 && (Mathf.Abs(r)>10)) {
            for (int i = memory.Count - 2; i > 0; i--) {
                if (r - memoryRewardRed > 0) {
                    r -= memoryRewardRed;
                }
                else if (r + memoryRewardRed < 0) {
                    r += memoryRewardRed;
                }
                else {
                    r = 0;
                    return;
                } 
                //r = 0; //test 

                int[] newestMem = (int[])memory[i + 1]; // new memory
                int newMemoryState = newestMem[0]; // new state

                int[] oldestMem = (int[])memory[i]; // old memory
                int oldMemoryState = oldestMem[0]; // old state
                int oldMemoryAction = oldestMem[1]; // old action

                float qMemory = getQvalue(oldMemoryState, oldMemoryAction); //float q = Q[state,action];
                float maxQMemory = maxQ(newMemoryState); //float maxQ = maxQ (state);

                float memoryValue = qMemory + alpha * (r + gamma * maxQMemory - qMemory);//float value = q + alpha * (r + gamma * maxQ - q);

                updateQvalue(oldMemoryState, oldMemoryAction, memoryValue); // q[oldstate,action] = value
            }
        } */
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
    } // end of addMemory

    /*
     * resets the memory arraylist
     */
    public void resetMemory() {
        memory.Clear();
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
     * finish run by storing the totalreward of the run in the reward list
     * and reseting the total reward to 0
     */
    public void saveReward() {
        rewardList.Add(totalReward);
        totalReward = 0;
    }

    /*
     * prints the reward list as a string
     */
    public string printRewardList() {
        string m = "";

        for (int i = 0; i < rewardList.Count; i++) {
            if (i != rewardList.Count - 1)
                m = m + rewardList[i] + " ";
            else
                m = m + rewardList[i];
        }

        return m;
    } // end of printReward

    /*
     *  returns the best or a random action according to the variable
     */
    public int nextAction(ObjectState s) {
        if (bestaction)
            return bestAction(s);
        else
            return randomAction(s);
    }

    /*
     * returns the action with the highest q value
     */
    /*int bestAction(ObjectState s) {

        updateStates();

        int action = 0;
        float maxValue = float.MinValue;

        bool[] possibleActions = controller.getAvailableActions(s.getState());
        float[] Qactions = (float[])Q[s.getId()];
        for (int i = 0; i < possibleActions.Length; i++) {
            if (possibleActions[i]) {
                float value = Qactions[i];
                if (value > maxValue) { // > or >=
                    maxValue = value;
                    action = i;
                }
            }
        }
        return action;
    } // end of bestAction */

    /*
     * best action test
     */
     public int bestAction(ObjectState s) {
        int action = 0;
        float maxValue = float.MinValue;

        bool[] possibleActions = controller.getAvailableActions(s.getState());
        List<int> actions = new List<int>();
        for(int i=0; i<possibleActions.Length; i++) {
            if (possibleActions[i])
                actions.Add(i);
        }
        float[] Qactions = (float[])Q[s.getId()];

        // add an element of randomness for exploration
        if (Random.Range(0.0f, 1.0f) < epsilon) {
            int random = Random.Range(0, actions.Count);
            action = actions[random];
        } else {
            // get a list of all the max q. 
            // in case that the max q is shared with more than one action
            List<int> bestActions = new List<int>();
            foreach (int a in actions) {
                float tempValue = Qactions[a];
                if (tempValue > maxValue) {
                    maxValue = tempValue;
                    bestActions.Clear();
                    bestActions.Add(a);
                }
                else if (tempValue == maxValue) {
                    bestActions.Add(a);
                }
            }

            // choose a random action from the best ones
            if (bestActions.Count > 1) {
                int random = Random.Range(0, bestActions.Count);
                action = bestActions[random];
            }
            else if(bestActions.Count == 1){
                action = bestActions[0];
            }
        }    

        return action;
    }// end of ba

    int randomAction(ObjectState s) {
        updateStates();
        int state = 0;
        bool[] possibleActions = controller.getAvailableActions(s.getState());

        for (int i = 0; i < possibleActions.Length; i++) {
            int random = Random.Range(0, possibleActions.Length - 1);
            if (possibleActions[random]) {
                state = random;
                break;
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
    float maxQ(ObjectState nextState) {
        float maxValue = float.MinValue;

        bool[] possibleActions = controller.getAvailableActions(nextState.getState());
        float[] Qactions = (float[])Q[nextState.getId()];
        for (int i = 0; i < possibleActions.Length; i++) {
            if (possibleActions[i]) {
                float value = Qactions[i];
                if (value > maxValue)
                    maxValue = value;
            }
        }

       /* bool[] possibleActions = controller.getAvailableActions(nextState.getState());
        float[] Qactions = (float[])Q[nextState.getId()];
        if (possibleActions.Length > 0) {
            maxValue= 
        }*/

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

    /*
     * load q table from file
     */
    public void loadQTable(ArrayList list) {
        if (Q.Count > 0)
            return;
        for (int i = 0; i < list.Count; i++) {
            float[] row = (float[])list[i];
            Q.Add(row);
        }
    } // end of loadQTable

    /*
     * prints the q table in a message
     */
    public string printQTable() {
        string message = "";
        foreach (float[] row in Q) {
            for (int i = 0; i < row.Length; i++) {
                if (i == 0)
                    message = message + row[i];
                else
                    message = message + " " + row[i];
            }
            message = message + "\r\n";
        }
        return message;
    } // end of printQTable
} // end of QLearning
