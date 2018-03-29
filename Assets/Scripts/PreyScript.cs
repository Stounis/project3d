using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PreyScript : Controller {

    //states and actions
    enum State { Idle, Seek, Flee, Dead }; // possible states
    public int stateSize = System.Enum.GetValues(typeof(State)).Length;
    enum Action { None, RotateLeft, RotateRight, LookAtObject, LookAtSound, MoveForward, MoveBackwards, Stop, FollowObject, Eat, Idle, Seek, Flee }; // possible actions
    public int actionSize = System.Enum.GetValues(typeof(Action)).Length;
    State currentState;
    Action currentAction;
    ObjectState oldState = null;

    public int compass = 0;

    //speed
    public float FleeMovepSpeed = 6;
    public float SeekMoveSpeed = 4;
    public float LowSpeed = 2;

    Vector3 destination; // agent destination
    Vector3 movingDir; // moving direction

    //distances
    float eadibleDistance;
    float targetDistance;
    float wallDistance;
    float soundDistance;

    // rotation reduction speed
    float initialAngle;
    Rotation rotation; // class which controls the rotation of the object

    // init method
    void Start() {
        rigidbody = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        initialAngle = transform.rotation.eulerAngles.y; // the object only rotates on the y axis
        rotation = GetComponent<Rotation>();
        currentState = State.Seek;
        currentAction = Action.None;

        float eadibleDistance = GetComponent<FieldOfView>().viewRadius;
        float targetDistance = GetComponent<FieldOfView>().minDstToTarget;
        float wallDistance = GetComponent<FieldOfView>().minDstToWall;

        if (dummy) {
            GameObject[] ways = GameObject.FindGameObjectsWithTag("Waypoint");
            points = new Transform[ways.Length];
            for (int i = 0; i < ways.Length; i++) {
                points[i] = ways[i].transform;
            }
            agent.autoBraking = false;
            GotoNextPoint();
        }
    } // end of Start

    /*
	 * Updates every frame
	 */
    void Update() {

        //test compass
        compass = compass();



        if (dummy) { // dummy mode
            dummyBehaviour();
            return;
        }
        if (hunger > 0)
            hunger -= 0.01f;

        if (stamina < 25 || hunger > 75) {
            moveSpeed = 2f;
        }

        if (hunger < 0 || hunger > 115) { // starved to death or sploded from too much food
            Dead();
        }

        switch (currentState) {
            case State.Idle:
                rest(0.1f);
                stop();
                availableSpeed = 0;
                break;
            case State.Seek:
                consumeStamina(0.01f);
                availableSpeed = SeekMoveSpeed;
                break;
            case State.Flee:
                consumeStamina(0.1f);
                availableSpeed = FleeMovepSpeed;
                break;
        }

        int[] states = generateStates();
        ObjectState curState = stateArray.findState(states);
        if (curState == null)   // check that state exist 
            curState = stateArray.addState(states);

        // q learning
        if (oldState == null) {
            qAlgorithm.rl(curState, (int)currentAction, curState);
        }
        else if (oldState.getId() != curState.getId()) {
            qAlgorithm.rl(oldState, (int)currentAction, curState);
        }

        bool[] availableActions = getAvailableActions((int)currentState); // get the available actions

        if (currentState == State.Dead) {
            dead = true;
            return;
        }

        if (keyboard) {// if boolean variable is true use the keyboard commands
            Action action = keyboardActions(availableActions);
            selectAction((int)action);
        }
        else if (oldState != curState) {
            Action action = intToAction(qAlgorithm.nextAction(curState));// get best action according to q table
            //Debug.Log("selected action: " + action);
            selectAction((int)action);
        }

        oldState = curState;

    } // end of Update

    /*
	 * Updates Physics 
	 */
    void FixedUpdate() {
        // reducing the speed according to the rotation of the object
        if (!dummy) {
            if (stamina < 10f)
                availableSpeed = 2f;
            moveSpeed = availableSpeed;
            if (agent.enabled) {
                if (agent.enabled && Vector3.Distance(transform.position, destination) <= 1f) {
                    agent.destination = transform.position;
                    agent.enabled = false;
                }
            }
            else {
                if (moving) {
                    reducedSpeed = moveSpeed - ((moveSpeed / 2) * Mathf.Abs((initialAngle - transform.rotation.eulerAngles.y) % 360) / 360);
                    rigidbody.velocity = movingDir * reducedSpeed;
                }
                else {
                    rigidbody.velocity = transform.forward * 0;
                }
            }
        }
    } // end of Fixed-Update

    /*
	 * Look At sound Source
	 */
    void lookAtSound() {
        if (soundList.Count > 0) {
            Vector3 origin = soundList[0];
            rotation.lookAtObject(origin, true);
        }
    } // end of lookAtSound

    /*
     * look at eadible object
     */
    void lookAtEadible() {
        if (GetComponent<FieldOfView>().visibleEadibles.Count > 0) {
            Vector3 eadiblePosition = GetComponent<FieldOfView>().visibleEadibles[0].position;
            rotation.lookAtObject(eadiblePosition, false);
        }
    }

    /*
	 * Action Move towards the movingDir (direction)
	 * initialAngle is to reduce the speed according to how much the rotation of the object has altered during that initial move action
	 */
    void moveForward() {
        agent.enabled = false;
        movingDir = transform.forward;
        initialAngle = transform.rotation.eulerAngles.y;
        moving = true;
    }

    /*
	 * Action MoveBack
	 */
    void moveBackward() {
        agent.enabled = false;
        movingDir = -transform.forward;
        initialAngle = (transform.rotation.eulerAngles.y + 180) % 360;
        moving = true;
    }

    /*
	 * move towards an eadible object
	 */
    void moveToEadible() {
        if (!agent.enabled)
            stop();
        agent.enabled = true;
        agent.speed = moveSpeed;
        if (GetComponent<FieldOfView>().visibleEadibles.Count > 0) {
            Vector3 eadiblePosition = closestEadible();
            agent.SetDestination(eadiblePosition);
            destination = eadiblePosition;
        }
    }

    /*
	 * Action Stop. stops the movement of the object
	 */
    void stop() {
        agent.enabled = false;
        moving = false;
        rigidbody.velocity = transform.forward * 0;
    }

    /*
	 * Action Eat. eats an eadible object if it collides with it
	 */
    void eat() {
        if (eadibleList.Count > 0) {
            GameObject e = (GameObject)eadibleList[0];
            eadibleList.RemoveAt(0);
            Destroy(e);
            hunger += 10;
        }
    } // end of eat

    /*
     * damaged by the predator
     */
    public void damage() {
        Dead();
    }

    /*
	 * Selects the next action
	 */
    public override void selectAction(int a) {
        Action action = intToAction(a);
        currentAction = action;
        switch (currentAction) {
            case Action.RotateLeft:
                rotation.RotateLeft();
                break;
            case Action.RotateRight:
                rotation.RotateRight();
                break;
            case Action.LookAtObject:
                lookAtEadible();
                break;
            case Action.LookAtSound:
                lookAtSound();
                break;
            case Action.MoveForward:
                moveForward();
                break;
            case Action.MoveBackwards:
                moveBackward();
                break;
            case Action.Stop:
                stop();
                break;
            case Action.FollowObject:
                moveToEadible();
                Debug.Log("agent enabled");
                break;
            case Action.Eat:
                eat();
                break;
            case Action.Idle:
                Idle();
                break;
            case Action.Seek:
                Seek();
                break;
            case Action.Flee:
                Flee();
                break;
        }
    } // end of selectAction

    /*
	 * returns a boolean array with all the available actions of the object.
	 * the actions are selected according to the object's state and other parameters.
	 */
    public override bool[] getAvailableActions(int s) {
        State state = intToState(s);
        bool[] availableActions = new bool[actionSize];
        Action[] chooseActions = null;

        switch (state) {
            case State.Idle:
                chooseActions = new Action[]{
                Action.RotateLeft,
                Action.RotateRight,
                Action.Flee,
                Action.Seek
            };
                if (eadibleList.Count > 0 && !moving) // eat rule
                    availableActions[(int)Action.Eat] = true;
                break;
            case State.Seek:
                chooseActions = new Action[] {
                Action.None,
                Action.RotateLeft,
                Action.RotateRight,
                Action.MoveForward,
                Action.MoveBackwards,
                Action.Flee,
                Action.Idle
            };
                if (eadibleList.Count > 0 && !moving) // eat rule
                    availableActions[(int)Action.Eat] = true;
                if (GetComponent<FieldOfView>().visibleEadibles.Count > 0) { // move towards object rule
                    if (!agent.enabled)
                        availableActions[(int)Action.FollowObject] = true;
                    //availableActions[(int)Action.LookAtObject] = true;
                }

                /*if (moving || agent.enabled)
                    availableActions[(int)Action.Stop] = true;*/
                break;
            case State.Flee:
                chooseActions = new Action[]{
                Action.None,
                Action.RotateLeft,
                Action.RotateRight,
                Action.MoveForward,
                Action.MoveBackwards,
                Action.Seek,
                Action.Idle
            };
                /*if (moving || agent.enabled)
                    availableActions[(int)Action.Stop] = true;*/
                break;
            case State.Dead:
                chooseActions = null;
                return availableActions;
                break;
        }
        /*if (GetComponent<FieldOfView>().visibleEadibles.Count>0){ // look at eadible object
			availableActions [(int)Action.LookAtObject] = true;
		}*/
        if (soundList.Count > 0) {  // look at sound source
            availableActions[(int)Action.LookAtSound] = true;
        }

        if (agent.enabled) {
            chooseActions = new Action[] {
                Action.None,
                Action.Stop
            };
        }

        
        if (chooseActions != null) {
            for (int i = 0; i < chooseActions.Length; i++) {
                availableActions[(int)chooseActions[i]] = true;
            }
        }

        // if bumped into a wall, stop moving towards the wall
        if (wallCollision) {
            if (GetComponent<FieldOfView>().minDstToWall < 0.75f) {
                availableActions[(int)Action.MoveForward] = false;
            }
            else if (GetComponent<FieldOfView>().minDstToWall > 0.75f) {
                availableActions[(int)Action.MoveBackwards] = false;
            }
        }
        return availableActions;
    } // end of getAvailableActions 


    /*
	 * State Change to dead.
	 * When the object dies from hunger or is eaten by the predator.
	 */
    public void Dead() {
        currentState = State.Dead;
    }

    /*
	 * State Change to Idle.
	 * Only available from the other 2 states.
	 */
    void Idle() {
        currentState = State.Idle;
    }

    /*
	 * State Change to Seek.
	 * Only available from the other 2 states.
	 */
    void Seek() {
        currentState = State.Seek;
    }

    /*
	 * State Change to Flee.
	 * Only available from the other 2 states.
	 */
    void Flee() {
        currentState = State.Flee;
    }

    /*
	 * generate states
	 */
    int[] generateStates() {

        int[] stateArray = new int[12];
        stateArray[0] = (int)currentState; // current state
        stateArray[1] = compass; // the rotation that the object is currently looking
        stateArray[2] = (int)System.Convert.ToSingle(moving); // is it moving
        stateArray[3] = (int)System.Convert.ToSingle(agent.enabled); // agent
        stateArray[4] = (int)System.Convert.ToSingle(soundList.Count > 0); // length of soundlist
        stateArray[5] = (int)System.Convert.ToSingle(GetComponent<FieldOfView>().visibleTargets.Count > 0); // size of visible targets
        stateArray[6] = (int)System.Convert.ToSingle(GetComponent<FieldOfView>().visibleEadibles.Count > 0); // size of visible targets
        stateArray[7] = (int)System.Convert.ToSingle(eadibleList.Count > 0); // contact with eadibles
        stateArray[8] = (int)GetComponent<FieldOfView>().minDstToWall; // distance between agent and wall
        stateArray[9] = (int)System.Convert.ToSingle(wallCollision); // collision with the wall
        stateArray[10] = staminaLevel(); // stamina level
        stateArray[11] = hungerLevel(); // hunger level
        return stateArray;
    } // end of generateStates

    /*
	 * returns the reward according to the state and action 
	 */
    public override int reward(int state, int action) {
        int reward = 0;
        if (intToState(state) != PreyScript.State.Dead) {
            if (action == (int)PreyScript.Action.Eat)
                reward = 50;
            else {
                reward = 0;
            }
        }
        else {
            reward = -50;
            Debug.Log("reward -100");
        }
        return reward;
    } // end of R

    /*
	 * converts an integer to a state
	 */
    PreyScript.State intToState(int i) {
        if (i == (int)PreyScript.State.Idle)
            return PreyScript.State.Idle;
        else if (i == (int)PreyScript.State.Seek)
            return PreyScript.State.Seek;
        else if (i == (int)PreyScript.State.Flee)
            return PreyScript.State.Flee;
        else
            return PreyScript.State.Dead;
    } // end of intToState

    /*
     * converts int to an action
     */
    PreyScript.Action intToAction(int a) {
        foreach (Action action in System.Enum.GetValues(typeof(Action))) {
            if ((int)action == a)
                return action;
        }
        return Action.None;
    } // end of intToAction

    /*
     * find the closest eadible object in FOV
     */
    Vector3 closestEadible() {
        Vector3 eadiblePosition = transform.position;
        float distance = -1f;

        if (GetComponent<FieldOfView>().visibleEadibles.Count > 0) {
            Transform eadible = GetComponent<FieldOfView>().visibleEadibles[0];
            if (eadible != null) {
                eadiblePosition = eadible.position;
                distance = Vector3.Distance(transform.position, eadiblePosition);
            }
            if (GetComponent<FieldOfView>().visibleEadibles.Count > 1) {
                for (int i = 1; i < GetComponent<FieldOfView>().visibleEadibles.Count; i++) {
                    Transform eadibles = GetComponent<FieldOfView>().visibleEadibles[i];
                    if (eadibles != null) {
                        if (Vector3.Distance(transform.position, eadibles.position) < distance) {
                            eadiblePosition = eadibles.position;
                            distance = Vector3.Distance(transform.position, eadibles.position);
                        }
                    }
                }
            }
        }

        eadibleDistance = distance;

        return eadiblePosition;
    } // end of closestEadible

    PreyScript.Action keyboardActions(bool[] availableActions) {
        Action action = Action.None;
        ////// ACTIONS CONTROLLED BY THE KEYBOARD ////// 
        if (Input.GetKey(KeyCode.E) && availableActions[(int)Action.Eat]) {
            Debug.Log("Prey Eating!");
            //eat ();
            action = Action.Eat;
        }

        if (Input.GetKey(KeyCode.A) && availableActions[(int)Action.MoveForward]) {
            //rigidbody.velocity = transform.forward * moveSpeed;
            //move();
            action = Action.MoveForward;
        }
        if (Input.GetKey(KeyCode.S) && availableActions[(int)Action.MoveBackwards]) {
            action = Action.MoveBackwards;
        }
        if (Input.GetKey(KeyCode.Z) && availableActions[(int)Action.FollowObject]) {
            action = Action.FollowObject;
        }
        if (Input.GetKey(KeyCode.D) && availableActions[(int)Action.Stop]) {
            //rigidbody.velocity = transform.forward * 0;
            //stop();
            action = Action.Stop;
        }
        if (Input.GetKey(KeyCode.X) && availableActions[(int)Action.LookAtObject]) {
            action = Action.LookAtObject;
        }

        //switch state test
        if (Input.GetKey(KeyCode.Alpha1) && availableActions[(int)Action.Idle]) { // Idle
                                                                                  //Idle ();
            action = Action.Idle;
        }
        if (Input.GetKey(KeyCode.Alpha2) && availableActions[(int)Action.Seek]) { // Seek
                                                                                  //Seek ();
            action = Action.Seek;
        }
        if (Input.GetKey(KeyCode.Alpha3) && availableActions[(int)Action.Flee]) { // Flee
                                                                                  //Flee ();
            action = Action.Flee;
        }

        if (Input.GetKey(KeyCode.Q) && availableActions[(int)Action.RotateLeft]) { // 1 on keyboard to rotate left
                                                                                   //rotation.RotateLeft (); // method will be called by controller
            action = Action.RotateLeft;
        }
        if (Input.GetKey(KeyCode.W) && availableActions[(int)Action.RotateRight]) { // 2 on keyboard to rotate right
                                                                                    //rotation.RotateRight (); // method will be called by controller
            action = Action.RotateRight;
        }
        return action;
    } //end of keyboard actions

    /*
     * follows waypoints and flees to the sight of the enemy
     */
    void dummyBehaviour() {
        if (currentState == State.Dead) {
            dead = true;
            return;
        }
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GotoNextPoint();
    } // end of dummy behaviour

    /*
     * follows waypoints
     */
    void GotoNextPoint() {
        agent.enabled = true;
        // Returns if no points have been set up
        if (points.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        agent.destination = points[destPoint].position;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % points.Length;
    } // end of gotonextpoint
} // end of PreyScript Class