using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class FileManagement {

    public FileManagement() {
        //testing
        //writeFile("test");
    }

    /*
     * stores the output in a text file
     */
    public void writeFile(string directory, string filename, string message) {

        //string dirpath = "Assets/Results/";
        //string fname = "test";
        //string path = "Assets/Results/test.txt";
        //string mtest = "0 1 2 3 4 12\r\n1 5 1 2 0 1\r\n2 12 15 1"; // \r\n for new line ***

        int filesNumber = Directory.GetFiles(directory , "*.txt").Length;
        //Debug.Log("number of files in " + directory + " " + filesNumber);

        string path = directory + filename + filesNumber + ".txt"; // Assets/Results/test.txt
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.Write(message);
        writer.Close();

        //Debug.Log(message);
    } // end of writeFile

    /*
     * reads the q table from a file
     */
    public ArrayList readFileStates(string directoryPath, string fileName) {

        char delimeter = ' ';

        int filenum = Directory.GetFiles(directoryPath, "*.txt").Length-1;
        string path = directoryPath + fileName + filenum + ".txt";
        StreamReader reader = new StreamReader(path);
        ArrayList list = new ArrayList();
       
        // read file
        while (reader.Peek() > 0) {
            string[] line = reader.ReadLine().Split(delimeter);
            int[] input = new int[line.Length];
            for (int i = 0; i < line.Length; i++) {
                int.TryParse(line[i], out input[i]);
            }
            list.Add(input);
        }
        reader.Close();

        return list;
    } // end of readFileQ

    /*
    * reads the states table from a file
    */
    public ArrayList readFileQ(string directoryPath, string fileName) {

        char delimeter = ' ';

        int filenum = Directory.GetFiles(directoryPath, "*.txt").Length - 1;
        string path = directoryPath + fileName + filenum + ".txt";
        StreamReader reader = new StreamReader(path);
        ArrayList list = new ArrayList();

        // read file
        while (reader.Peek() > 0) {
            string[] line = reader.ReadLine().Split(delimeter);
            float[] input = new float[line.Length];
            for (int i = 0; i < line.Length; i++) {
                float.TryParse(line[i], out input[i]);
            }
            list.Add(input);
        }
        reader.Close();

        return list;
    } // end of readFile

} // end of class FileManagement
