using UnityEngine;
using System.Xml;
using System.IO;

public class CalibrationManager : getReal3D.MonoBehaviourWithRpc
{

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
        if (getReal3D.Cluster.isMaster)
        {
            _directoryPath = Path.Combine(Application.dataPath, "StreamingAssets").ToString();

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

            getReal3D.RpcManager.call("SetKinectPosition", _configuration.kinect_y, _configuration.kinect_z);
        }
    }

    [getReal3D.RPC]
    public void SetKinectPosition(float y, float z) {
        kinect.transform.position = new Vector3(0, y, z);
    }


    public void Save() {
        if (getReal3D.Cluster.isMaster) {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(_directoryPath, "rehab-config.xml"));

            XmlNode node = doc.SelectSingleNode("config");
            node.SelectSingleNode("kinect_y").InnerText = _configuration.kinect_y.ToString();
            node.SelectSingleNode("kinect_z").InnerText = _configuration.kinect_z.ToString();

            Debug.Log("Path: " + _directoryPath);
            doc.Save(Path.Combine(_directoryPath, "rehab-config.xml"));
        }
    }

    public void MoveKinectY(float offset) {
        kinect.transform.position += new Vector3(0, offset, 0);
        SessionManager.GetInstance().GetAvatarController().UpdateOffset();
        _configuration.kinect_y = kinect.transform.position.y;
    }
}
