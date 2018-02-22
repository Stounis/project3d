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

    public int runs = 1000; // max number of runs
    public int tempSave = 10;
    public int counter = 1; // counts the runs

    //test filemanager
    FileManagement fileManager = new FileManagement();
    bool save = false;
    public bool loadFiles = false;
    string relativePath;

    public bool coadapt = false;
    public bool preysolo = false;
    public bool predatorsolo = false;

    // Use this for initialization
    void Start () {
        relativePath = Application.dataPath;
        Application.runInBackground = true;
        
        //instantiate prey
        prey = Instantiate(preyprefab) as GameObject;
        Controller preyController = prey.GetComponent<Controller>();
        preyStateArray = new StateArray();
        preyController.stateArray = preyStateArray;
        preyQlearning = new QLearning(preyController, preyStateArray, prey.GetComponent<PreyScript>().actionSize);
        preyQlearning.bestaction = true;
        preyController.qAlgorithm = preyQlearning;
        if (predatorsolo)
            preyController.dummy = true;

        //instantiate predator
        predator = Instantiate(predatorprefab) as GameObject;
        Controller predatorController = predator.GetComponent<Controller>();
        predatorStateArray = new StateArray();
        predatorController.stateArray = predatorStateArray;
        predatorQlearning = new QLearning(predatorController, predatorStateArray, predator.GetComponent<PredatorScript>().actionSize);
        predatorQlearning.bestaction = true;
        predatorController.qAlgorithm = predatorQlearning;
        if (preysolo)
            predatorController.dummy = true;

    } // end of start
	
	// Update is called once per frame
	void Update () {
        // load files

        /*if (counter % tempSave == 0) {
            save = true;
            exitGame();
        }*/

        if (loadFiles) {
            loadFiles = false;
            loadFilesInGame();
        }

        // save files 
        if (save) {
            save = false;
            saveFilesInGame();
            exitGame();
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
            predatorQlearning.bestaction = false;
            resetAgents();
        }
        if (Input.GetKey(KeyCode.F)) {
            preyQlearning.bestaction = true;
            predatorQlearning.bestaction = true;
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
        preyQlearning.resetMemory();
        preyQlearning.saveReward();
        preyController.stateArray = preyStateArray;
        preyController.qAlgorithm = preyQlearning;
        preyController.keyboard = false;
        if (predatorsolo)
            preyController.dummy = true;

        // restart predator
        Destroy(predator);
        predator = Instantiate(predatorprefab) as GameObject;
        Controller predatorController = predator.GetComponent<Controller>();
        predatorQlearning.setController(predatorController);
        predatorQlearning.resetMemory();
        predatorQlearning.saveReward();
        predatorController.stateArray = predatorStateArray;
        predatorController.qAlgorithm = predatorQlearning;
        if (preysolo)
            predatorController.dummy = true;

        counter += 1;
        Debug.Log("Run: " + (counter));
    } // end of resetAgents

    /*
     * loadfiles
     */
    void loadFilesInGame() {

        if (coadapt) {
            // load files for both
            preyStateArray.loadStates(fileManager.readFileStates(relativePath + "/Results/Prey/coadapt/states/", "preycoadaptStates"));
            preyQlearning.loadQTable(fileManager.readFileQ(relativePath + "/Results/Prey/coadapt/Qtable/", "preycoadaptQ"));
            predatorStateArray.loadStates(fileManager.readFileStates(relativePath + "/Results/Predator/coadapt/states/", "predatorcoadaptStates"));
            predatorQlearning.loadQTable(fileManager.readFileQ(relativePath + "/Results/Predator/coadapt/Qtable/", "predatorcoadaptQ"));
        } else if (preysolo) {
            // load files for prey
            preyStateArray.loadStates(fileManager.readFileStates(relativePath + "/Results/Prey/solotrain/states/", "preysoloStates"));
            preyQlearning.loadQTable(fileManager.readFileQ(relativePath + "/Results/Prey/solotrain/Qtable/", "preysoloQ"));
        } else if (predatorsolo) {
            // load files for predator
            preyStateArray.loadStates(fileManager.readFileStates(relativePath + "/Results/Predator/solotrain/states/", "predatorsoloStates"));
            preyQlearning.loadQTable(fileManager.readFileQ(relativePath + "/Results/Predator/solotrain/Qtable/", "predatorsoloQ"));
        }
    } // end of loadFilesInGame

    /*
     * save the tables in text files
     */
    void saveFilesInGame() {

        if (coadapt) {
            //save both files
            fileManager.writeFile(relativePath + "/Results/Prey/coadapt/Qtable/", "preycoadaptQ", preyQlearning.printQTable());
            fileManager.writeFile(relativePath + "/Results/Prey/coadapt/states/", "preycoadaptStates", preyStateArray.printStates());
            fileManager.writeFile(relativePath + "/Results/Prey/coadapt/reward/", "preycoadaptreward", preyQlearning.printRewardList());
            fileManager.writeFile(relativePath + "/Results/Predator/coadapt/Qtable/", "predatorcoadaptQ", predatorQlearning.printQTable());
            fileManager.writeFile(relativePath + "/Results/Predator/coadapt/states/", "predatorcoadaptStates", predatorStateArray.printStates());
            fileManager.writeFile(relativePath + "/Results/Predator/coadapt/reward/", "predatorcoadaptreward", predatorQlearning.printRewardList());
        } else if (preysolo) {
            // save prey files
            fileManager.writeFile(relativePath + "/Results/Prey/solotrain/Qtable/", "preysoloQ", preyQlearning.printQTable());
            fileManager.writeFile(relativePath + "/Results/Prey/solotrain/states/", "preysoloStates", preyStateArray.printStates());
            fileManager.writeFile(relativePath + "/Results/Prey/solotrain/reward/", "preysoloreward", preyQlearning.printRewardList());
        } else if (predatorsolo) {
            //save predator files
            fileManager.writeFile(relativePath + "/Results/Predator/solotrain/Qtable/", "predatorsoloQ", predatorQlearning.printQTable());
            fileManager.writeFile(relativePath + "/Results/Predator/solotrain/states/", "predatorsoloStates", predatorStateArray.printStates());
            fileManager.writeFile(relativePath + "/Results/Predator/solotrain/reward/", "predatorsoloreward", predatorQlearning.printRewardList());
        }
    } // end of save files

    /*
     * exit application
     */
    void exitGame() {
        Application.Quit();
    }

} // end of rungame class
