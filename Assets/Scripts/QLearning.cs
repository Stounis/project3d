using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QLearning {

	PredatorScript predator;

	float alpha = 0.1f; // alpha : Learning rate
	float gamma = 0.9f;// gamma : 0 looks near future, 1 looks distant future

	// list of states 
	// list of actions
	float[,] Q;// q table mapping actions to states
	// list of rewards according to actions, given by the class

	/*
	 * Constructor
	 */
	public QLearning(PredatorScript p){
		predator = p;
		Q = new float[predator.stateSize,predator.actionSize];

		// test
		Q [0,0] = 1f;
		Q [0,2] = 2f;
		Debug.Log (maxQ (0));
	} // end of Constructor
		
	/*
	 * updates the q table according to the selected action and the current state
	 */
	void calculateQ(int state, int action){

		// Q(s,a) = Q(s,a) + a * (r(s,a) + g * maxQ(s') - Q(s,a)) 
		float q = Q[state,action];
		float maxQ = maxQ (state);
		float r = R (state, action);

		float value = q + alpha * (r + gamma * maxQ - q);
		// TODO
	}

	/*
	 * returns the maximum Q value From the q table according to the next state
	*/
	double maxQ(int nextState){
		double maxValue = float.MinValue;

		bool[] possibleActions = predator.getAvailableActions (intToState(nextState));
		for (int i = 0; i < possibleActions.Length; i++) {
			if (possibleActions [i]) {
				float value = Q [nextState,i];
				if (value > maxValue)
					maxValue = value;	
			}
		}
		return maxValue;
	} // end of maxQ

	/*
	 * returns the reward according to the state and action 
	 */
	float R(int state, int action){
		float reward = 0f;
		if (intToState (state) != PredatorScript.State.Dead) {
			if (action == (int)PredatorScript.Action.Eat)
				reward = 100f;
			else
				reward = 0f;
		} else {
			reward = -100f;
		}
		return reward;
	} // end of R

	/*
	 * converts an integer to a state
	 */
	PredatorScript.State intToState(int i){
		if (i == (int)PredatorScript.State.Idle)
			return PredatorScript.State.Idle;
		else if (i == (int)PredatorScript.State.Seek)
			return PredatorScript.State.Seek;
		else if (i == (int)PredatorScript.State.Attack)
			return PredatorScript.State.Attack;
		else 
			return PredatorScript.State.Dead;
	} // end of intToState
} // end of QLearning
