using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunScene2 : MonoBehaviour {

    public GameObject preyprefab;
    public GameObject eadibleprefab;

    GameObject prey;
    GameObject eadible;

    StateArray preyStateArray;
    QLearning preyQlearning;

    string relativePath;
    FileManagement fileManager = new FileManagement();

    public float timer;
    public float maxTimer;
    ArrayList timePrey = new ArrayList();

    bool start = true;

    int reset = 0;

    public int totalRuns = 1000;
    public int tempSave = 100;
    public int counter = 1;

    public bool loadfiles = false;
    bool savefiles = false;
    bool savedRound = false;
    bool exit = false;
	// Use this for initialization
	void Start () {
        relativePath = Application.dataPath;
        Application.runInBackground = true;
        Application.targetFrameRate = 60;

        //instantiate prey
        prey = Instantiate(preyprefab) as GameObject;
        Controller preyController = prey.GetComponent<Controller>();
        preyStateArray = new StateArray();
        preyController.stateArray = preyStateArray;
        preyQlearning = new QLearning(preyController, preyStateArray, prey.GetComponent<PreyScript>().actionSize);
        preyQlearning.bestaction = true;
        preyController.qAlgorithm = preyQlearning;

        //instantiate eadible
        eadible = Instantiate(eadibleprefab) as GameObject; 

    } // end of start
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;

        if (loadfiles) {
            loadfiles = false;
            loadFiles();
        }

        if ((counter % tempSave) == 0 && savedRound) {
            savedRound = false;
            savefiles = true;
            //Debug.Log("SAVE AND EXIT AT 10 RUNS");
        }
        if ((counter % totalRuns) == 0) {
            savefiles = true;
            exit = true;
        }

        if (savefiles) {
            savefiles = false;
            saveFiles();
        }

        if (start) {
            start = false;
            resetAgents();
        }

        if (exit) {
            exitGame();
        }

        if (Input.GetKey(KeyCode.R))
            resetAgents();

        if (Input.GetKey(KeyCode.T))
            Debug.Log(preyQlearning.printQTable());

        if (prey.GetComponent<Controller>().isDead() || eadible == null || timer > maxTimer) {
            if (reset>15) {
                reset = 0;
                resetAgents();
            }

            reset += 1;
        }
            
	} // end of update

    void resetAgents() {
        
        timePrey.Add(timer);
        Destroy(prey);
        prey = Instantiate(preyprefab) as GameObject;
        Controller preyController = prey.GetComponent<Controller>();
        preyQlearning.setController(preyController);
        preyQlearning.resetMemory();
        preyQlearning.saveReward();
        preyController.stateArray = preyStateArray;
        preyController.qAlgorithm = preyQlearning;

        if (eadible == null)
            eadible = Instantiate(eadibleprefab) as GameObject;

        timer = 0;
        counter += 1;
    } // end of resetAgents

    void saveFiles() {
        // save prey files
        fileManager.writeFile(relativePath + "/Results/Prey/solotrain/Qtable/", "preysoloQ", preyQlearning.printQTable());
        fileManager.writeFile(relativePath + "/Results/Prey/solotrain/states/", "preysoloStates", preyStateArray.printStates());
        fileManager.writeFile(relativePath + "/Results/Prey/solotrain/reward/", "preysoloreward", preyQlearning.printRewardList());
        fileManager.writeFile(relativePath + "/Results/Prey/solotrain/timer/", "preysolotimer", timerToString(timePrey));
    }

    void loadFiles() {
        // load files for prey
        preyStateArray.loadStates(fileManager.readFileStates(relativePath + "/Results/Prey/solotrain/states/", "preysoloStates"));
        preyQlearning.loadQTable(fileManager.readFileQ(relativePath + "/Results/Prey/solotrain/Qtable/", "preysoloQ"));
    }

    /*
     * transforms the timer array into a message
     */
    string timerToString(ArrayList list) {
        string m = "";

        for (int i = 0; i < list.Count; i++) {
            if (i != 0)
                m = m + " " + (float)list[i];
            else
                m = m + (float)list[i];
        }

        return m;
    }

    /*
    * exit application
    */
    void exitGame() {
        Application.Quit();
    }
}
