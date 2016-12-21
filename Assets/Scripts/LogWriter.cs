using UnityEngine;
using System.IO;
using System;
using SimpleJSON;
using System.Collections;
using System.Text;

public class LogWriter : MonoBehaviour { 

    public static LogWriter instance = null;
    public SessionManager sessionManager = null;

    private string _directoryPath = null;
    private string _completionTimeLogs = null;
    private string _numberObjectsLogs = null;
    private string _movementStraightnessLogs = null;

    private GameObject patient;
    private JSONNode outputData;

    void Awake() {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        // Do not destroy log writer when reloading the scene
        DontDestroyOnLoad(gameObject);

        CreateNewPatientLog(PlayerPrefs.GetString("PatientId").Replace(" ", ""));
    }

    void Start() {
        patient = GameObject.FindGameObjectWithTag("Patient");
    }

    private void CreateNewPatientLog(string patientId) {
        string localData = DateTime.Now.ToShortDateString().Replace("/","-") + DateTime.Now.ToLongTimeString().Replace(":","");
        _directoryPath = patientId + "-" + localData;
        _directoryPath = Path.Combine("Logs",_directoryPath);

        // create a directory to store all the logs produced in a session
        Directory.CreateDirectory(_directoryPath);

        string filePath = Path.Combine(LogWriter.instance.GetDirectory(), "positions.txt");
        StreamWriter sw = File.CreateText(filePath);
        sw.Close();
    }

    public void WriteLogs(ObjectsManager objectManager, ArrayList logPositions) {
        /*
        outputData = new JSONClass();

        outputData["patient_id"] = PlayerPrefs.GetString("PatientId");
        outputData["training_type"] = PlayerPrefs.GetString("TrainingMode");
        outputData["total_time"].AsFloat = objectManager.GetTotalElapsedTime();
        outputData["no_objects_caught"].AsInt = objectManager.GetNumberOfObjectsCaught();
        outputData["no_objects"].AsInt = objectManager.GetNumberOfObjects();
        outputData["objects"] = objectManager.GetObjectsData();
        //outputData["positions"] = patient.GetComponent<FlatAvatarController>().GetPositionsLog();*/

        Debug.Log("Starting to dump data to file: " + DateTime.Now.ToLongTimeString() );
        Debug.Log("No Position Objects: " + patient.GetComponent<FlatAvatarController>().GetPositionsLog().Count);

        string trainingMode = PlayerPrefs.GetString("TrainingMode").Replace(" ", "");
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
        string filePath = Path.Combine(_directoryPath, trainingMode + "-" + timestamp);

        StringBuilder sb = new StringBuilder();
        sb.Append("{\"patient_id\":\"").Append(PlayerPrefs.GetString("PatientId")).Append("\",").AppendLine();
        sb.Append("\"date\":\"").Append(DateTime.Now.ToLongDateString()).Append("\",").AppendLine();
        sb.Append("\"training_type\":\"").Append(PlayerPrefs.GetString("TrainingMode")).Append("\",").AppendLine();
        sb.Append("\"total_time\":\"").Append(objectManager.GetTotalElapsedTime()).Append("\",").AppendLine();
        sb.Append("\"no_objects_caught\":\"").Append(objectManager.GetNumberOfObjectsCaught()).Append("\",").AppendLine();
        sb.Append("\"no_objects\":\"").Append(objectManager.GetNumberOfObjects()).Append("\",").AppendLine();
        sb.Append("\"objects\":").Append(objectManager.GetObjectsData().ToString()).Append(",").AppendLine();
        sb.Append("\"tracking\":[");

        using (StreamWriter sw = new StreamWriter(filePath + ".txt")){
            sw.Write(sb.ToString());

            for (int i = 0; i < logPositions.Count - 1; i++) 
                sw.WriteLine(logPositions[i] + ",");
            sw.WriteLine(logPositions[logPositions.Count - 1]);
            sw.Write("]}");
            sw.Close();
        }

        Debug.Log("Finishing dumping data " + DateTime.Now.ToLongTimeString());

    }

    public void WritePositionLog(JSONData data) {
        string filePath = Path.Combine(_directoryPath, "positions");
        File.AppendAllText(filePath, data.ToString());
    }


    // Getters
    public string GetDirectory() {
        return _directoryPath;
    }
}
