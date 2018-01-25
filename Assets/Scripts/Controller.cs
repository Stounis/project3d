using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Controller : MonoBehaviour {

	public float moveSpeed = 6;
	public float weight = 50;
	public float stamina = 100; // used for walk, run etc
	public float hunger = 0;
	public string id;

	protected bool alive = true;

	public ArrayList eadibleList = new ArrayList();

	protected Rigidbody rigidbody;
	protected Camera viewCamera;
	protected Vector3 velocity;
	protected NavMeshAgent agent;

	// Field Of View Variables
	/*float fowSpotedAngle = 30;
	float fowSpotedRadius = 8;     
	float fowSeekAngle = 75;
	float fowSeekRadius = 4;
	float fowTransitionAngle = 0.3f;
	float fowTransitionRadius = 0.01f;*/

	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		//viewCamera = Camera.main;
		agent = GetComponent<NavMeshAgent> ();
	}
		
	void Update () {
		/*if (alive) {
			Vector3 mousePos = viewCamera.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
			transform.LookAt (mousePos + Vector3.up * transform.position.y);
			velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * moveSpeed;

			if (stamina < 0) {
				alive = false;
			}
		}*/	
	}

	void FixedUpdate() {
		/*if (alive) { 
			Vector3 stationary = new Vector3 (0, 0, 0);
			if (velocity != stationary) {	
				stamina -= 0.01f * moveSpeed;
			} else {
				stamina -= 0.01f;

			}
			rigidbody.MovePosition (rigidbody.position + velocity * Time.fixedDeltaTime);
			stamina -= 0.01f;
		} */
	} 

	void changeFieldOfView(){
		/*float angle = GetComponent<FieldOfView>().viewAngle;
		float radius = GetComponent<FieldOfView>().viewRadius;
		if (spotted) {
			if (GetComponent<FieldOfView>().viewAngle > fowSpotedAngle)
				GetComponent<FieldOfView>().viewAngle -= fowTransitionAngle;
			if (GetComponent<FieldOfView>().viewRadius < fowSpotedRadius)
				GetComponent<FieldOfView>().viewRadius += fowTransitionRadius;
		} else {
			if (GetComponent<FieldOfView>().viewAngle < fowSeekAngle)
				GetComponent<FieldOfView>().viewAngle += fowTransitionAngle;
			if (GetComponent<FieldOfView>().viewRadius > fowSeekRadius)
				GetComponent<FieldOfView>().viewRadius -= fowTransitionRadius;
		} */
	}

	void movement(){
		//velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * moveSpeed;
	}

	void rotation(){
		Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
	}

	public string getId(){
		return id;
	}



	public void addEadibleObject(GameObject g){
		eadibleList.Add (g);
	}

	public void removeEadibleObject(GameObject g){
		for (int i = 0; i < eadibleList.Count; i++) {
			if (g == eadibleList [i]) {
				eadibleList.RemoveAt(i);
			}
		}
	}
		
	public void printTest(){
		Debug.Log ("Parent test");
	}
		
}