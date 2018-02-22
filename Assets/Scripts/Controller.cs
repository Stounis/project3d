using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class Controller : MonoBehaviour {

    //keyboard
    public bool keyboard;

    // dummy stuff
    public bool dummy = false; // auto mode, following waypoints
    protected Transform[] points;
    protected int destPoint = 0;

    public float moveSpeed = 6;
    protected float availableSpeed;
	public float weight = 50;
	public float stamina = 100; // used for walk, run etc
	public float hunger = 50;
	public string id;
    public bool dead = false;

    //State and Action sizes
    //public int stateSize;
    //public int actionSize;

    //Reinforcement Learning
    public QLearning qAlgorithm;
    public StateArray stateArray;

    public bool moving = false; // true if the controller is moving
	public float reducedSpeed; // the speed that the controller is moving after being modified according to state rotation etc

	public ArrayList eadibleList = new ArrayList(); // list that holds all the eadible objects that the controller collides with
	public List<Vector3> soundList =  new List<Vector3>(); // holds vector3 points when the sound capsule collides with the controller

	protected Rigidbody rigidbody;
	protected Camera viewCamera;
	public Vector3 velocity;
	protected NavMeshAgent agent;

	// Field Of View Variables
	protected float fowSpotedAngle = 30;
	protected float fowSpotedRadius = 8;     
	protected float fowSeekAngle = 45;
	protected float fowSeekRadius = 8;
	protected float fowTransitionAngle = 0.3f;
	protected float fowTransitionRadius = 0.01f;


	/*
	 * changes the field of view of the controller according to the state
	 * >>ABSTRACT<<
	 */
	protected virtual void changeFieldOfView(){}
		
	/*
	 * returns the string id of the object
	 */
	public string getId(){
		return id;
	}
    
    public bool isDead() {
        return dead;
    }

    public bool agentEnabled() {
        return agent.enabled;
    }

    /*
     * given by the runGame
     */
    public void setQLearning(QLearning algorithm) {
        qAlgorithm = algorithm;
    }

    /*
     * given by the runGame
     */
    public void setStateArray(StateArray array) {
        stateArray = array;
    }

	/*
	 * controller rests to regenerate stamina
	 */ 
	protected void rest(float s){
		if (stamina + s <= 100) {
			stamina += s;
		}
	}

	/*
	 * controller consumes stamina s from its stamina level
	 */
	protected void consumeStamina(float s){
		if (stamina - s > 0) {
			stamina -= s;
		}
	}

	/*
	 * adds the eadible object that collided with the controller in the list
	 */
	public void addEadibleObject(GameObject g){
		eadibleList.Add (g);
	}

	/*
	 * removes the eadible object when the controller exits the collision with it
	 */
	public void removeEadibleObject(GameObject g){
		for (int i = 0; i < eadibleList.Count; i++) {
			if (g == eadibleList [i]) {
				eadibleList.RemoveAt(i);
			}
		}
	} // end of removeEadibleObject

	/*
	 * adds the object that its sound has collided with this to the sound list
	 */
	public void addSoundObject(Vector3 soundOrigin) {
		if (soundList.Count > 0)
			soundList.RemoveAt (0);
		soundList.Add (soundOrigin);
	} // end of addSoundObject

	/*
	 * returns a boolean array with the available actions according to the state
	 * >>ABSTRACT<<
	 */
	public virtual bool[] getAvailableActions(int s){ return new bool[0];}

	/*
	 * selects the next action of the object
	 * >>ABSTRACT<<
	 */
	public virtual void selectAction(int action){}

    /*
     * returns a reward according to state and action
     * >>ABSTRACT<<
     */
    public virtual int reward(int state, int action) { return 0; }

    /*
     * test of collision with wall to stop controller
     */
    protected void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.tag == "Obstacle") {
            moving = false;
            rigidbody.velocity = Vector3.zero;
        }
    } // end of OnCollisionEnter

    protected void OnCollisionStay(Collision collision) {
        if(collision.gameObject.tag == "Obstacle") {
            moving = false;
            rigidbody.velocity = Vector3.zero;
        }
    }

    /*
	 * sets stamina as a continuous variable
	 * that is split into 4 different categories
	 */
    protected int staminaLevel() {
        return (int)stamina / 10;
    } // end of staminaLevel

    /*
	 * sets hunger level as a continuous variable
	 * that is split into 4 different categories
	 */
    protected int hungerLevel() {
        return (int)hunger / 10;
    } // end of hungerLevel

    /*
     * works as a compass
     * return an int according to where the controller looks
     */
    public int compass() {
        float angle = transform.rotation.eulerAngles.y;

        if (angle > 45 && angle <= 135)
            return 1; // east
        else if (angle > 135 && angle <= 225)
            return 2; //south
        else if (angle > 225 && angle <= 315)
            return 3; // west
        else      
            return 0; // north
    }

} // end of Controller Class