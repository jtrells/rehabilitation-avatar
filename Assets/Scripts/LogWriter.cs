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

        if (getReal3D.Cluster.isMaster) {
            CreateNewPatientLog(PlayerPrefs.GetString("PatientId").Replace(" ", ""));
        }
    }

    void Start() {
        patient = GameObject.FindGameObjectWithTag("Patient");
    }

    private void CreateNewPatientLog(string patientId) {
        string localData = DateTime.Now.ToShortDateString().Replace("/","-") + DateTime.Now.ToLongTimeString().Replace(":","");
        _directoryPath = patientId + "-" + localData;
        _directoryPath = Path.Combine("Logs",_directoryPath);

        Debug.Log("Create new log folder: " + _directoryPath);
        // create a directory to store all the logs produced in a session
        Directory.CreateDirectory(_directoryPath);
    }

    public void WriteLogs(ObjectsManager objectManager, ArrayList logPositions, float objectScale) {
        Debug.Log("Starting to dump data to file: " + DateTime.Now.ToLongTimeString() );

        string trainingMode = PlayerPrefs.GetString("TrainingMode").Replace(" ", "");
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
        string filePath = Path.Combine(_directoryPath, trainingMode + "-" + timestamp);

        StringBuilder sb = new StringBuilder();
        sb.Append("{\"patient_id\":\"").Append(PlayerPrefs.GetString("PatientId")).Append("\",").AppendLine();
        sb.Append("\"scale\":\"").Append(objectScale.ToString()).Append("\",").AppendLine();
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

    // Getters
    public string GetDirectory() {
        return _directoryPath;
    }
}
