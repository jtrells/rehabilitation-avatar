using UnityEngine;
using System.Xml;
using System.IO;

public class CalibrationManager : MonoBehaviour {

    private static CalibrationManager instance;

    public GameObject kinect;
    private Config _configuration;
    private string _directoryPath;

    public static CalibrationManager GetInstance() { return instance; }
    public void SetConfiguration(Config config) { _configuration = config; }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    void Start () {
        _directoryPath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "Config").ToString();

        XmlDocument doc = new XmlDocument();
        doc.Load(Path.Combine(_directoryPath, "rehab-config.xml"));

        XmlNode node = doc.SelectSingleNode("config");
        float kinectY = float.Parse(node.SelectSingleNode("kinect_y").InnerText);
        float kinectZ = float.Parse(node.SelectSingleNode("kinect_z").InnerText);

        _configuration = new Config();
        _configuration.kinect_y = kinectY;
        _configuration.kinect_z = kinectZ;

        Debug.Log("Path: " + Application.dataPath);
        Debug.Log("Kinect position: " + _configuration.kinect_y + " ," + _configuration.kinect_z);
        kinect.transform.position = new Vector3(0, kinectY, kinectZ);
    }

    public void Save() {
        XmlDocument doc = new XmlDocument();
        doc.Load(Path.Combine(_directoryPath, "rehab-config.xml"));

        XmlNode node = doc.SelectSingleNode("config");
        node.SelectSingleNode("kinect_y").InnerText = _configuration.kinect_y.ToString();
        node.SelectSingleNode("kinect_z").InnerText = _configuration.kinect_z.ToString();

        doc.Save(Path.Combine(_directoryPath, "rehab-config.xml"));
    }

    public void MoveKinectY(float offset) {
        kinect.transform.position += new Vector3(0, offset, 0);
        SessionManager.GetInstance().GetAvatarController().UpdateOffset();
    }
}
