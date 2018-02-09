using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunGame : MonoBehaviour {

    public GameObject preyprefab;
    public GameObject predatorprefab;

    GameObject prey;
    GameObject predator;

    QLearning preyQlearning;
    QLearning predatorQlearning;

    StateArray preyStateArray;
    StateArray predatorStateArray;

    public int runs = 100;
    public int counter = 0; // counts the runs

    //test filemanager
    FileManagement fileManager = new FileManagement();
    bool writefile = false;
    bool save = false;
    bool saved = false;
    public bool loadFiles;
    bool loaded = false;

    // Use this for initialization
    void Start () {

        //instantiate prey
        prey = Instantiate(preyprefab) as GameObject;
        Controller preyController = prey.GetComponent<Controller>();
        preyStateArray = new StateArray();
        preyController.stateArray = preyStateArray;
        preyQlearning = new QLearning(preyController, preyStateArray, 12);
        preyController.qAlgorithm = preyQlearning;

        //instantiate predator
        predator = Instantiate(predatorprefab) as GameObject;
        Controller predatorController = predator.GetComponent<Controller>();
        predatorStateArray = new StateArray();
        predatorController.stateArray = predatorStateArray;
        predatorQlearning = new QLearning(predatorController, predatorStateArray, 11);
        predatorController.qAlgorithm = predatorQlearning;

    } // end of start
	
	// Update is called once per frame
	void Update () {

        // load files
        if (loadFiles) {
            if (!loaded) {
                loaded = true;
                preyStateArray.loadStates(fileManager.readFileStates("Assets/Results/Prey/solotrain/states/preysoloStates1.txt"));
                preyQlearning.loadQTable(fileManager.readFileQ("Assets/Results/Prey/solotrain/Qtable/preysoloQ1.txt"));
            }
        }

        // save files 
        if (save) {
            if (!saved) {
                saved = true;
                fileManager.writeFile("Assets/Results/Prey/solotrain/Qtable/", "preysoloQ", preyQlearning.printQTable());
                fileManager.writeFile("Assets/Results/Prey/solotrain/states/", "preysoloStates", preyStateArray.printStates());
            }
        }
        if (Input.GetKey(KeyCode.H)) { // save progress
            save = true;
        }

        if (Input.GetKey(KeyCode.T)) // print q
            Debug.Log(preyQlearning.printQTable());

        if (Input.GetKey(KeyCode.Y)) // print states
            Debug.Log(preyStateArray.printStates());

        // reset
        if (Input.GetKey(KeyCode.R)) {
            preyQlearning.bestaction = false;
            resetAgents();
        }
        if (Input.GetKey(KeyCode.F)) {
            preyQlearning.bestaction = true;
            resetAgents();
        }

        if (prey.GetComponent<Controller>().isDead() || predator.GetComponent<Controller>().isDead()) {
            resetAgents();
        }
	} // end of update

    void resetAgents()
    {
        // restart prey
        Destroy(prey);
        prey = Instantiate(preyprefab) as GameObject;
        Controller preyController = prey.GetComponent<Controller>();
        preyQlearning.setController(preyController);
        preyController.stateArray = preyStateArray;
        preyController.qAlgorithm = preyQlearning;
        preyController.keyboard = false;

        // restart predator
        Destroy(predator);
        predator = Instantiate(predatorprefab) as GameObject;
        Controller predatorController = predator.GetComponent<Controller>();
        predatorQlearning.setController(predatorController);
        predatorController.stateArray = predatorStateArray;
        predatorController.qAlgorithm = predatorQlearning;

        counter += 1;
        Debug.Log("Run: " + (counter));
    }

} // end of rungame class
