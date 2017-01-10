using UnityEngine;
using System.Xml;
using System.IO;
using System;

public class CalibrationManager : getReal3D.MonoBehaviourWithRpc
{

    private static CalibrationManager instance;

    public GameObject kinect;

    private Config _configuration;
    private string _directoryPath;

    public static CalibrationManager GetInstance() { return instance; }
    public void SetConfiguration(Config config) { _configuration = config; }

    private int _numberRecords = 0;
    private Vector3[] _records = new Vector3[3];
    
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
            float kinectX = float.Parse(node.SelectSingleNode("kinect_x").InnerText);
            float kinectY = float.Parse(node.SelectSingleNode("kinect_y").InnerText);
            float kinectZ = float.Parse(node.SelectSingleNode("kinect_z").InnerText);

            _configuration = new Config();
            _configuration.kinect_x = kinectX;
            _configuration.kinect_y = kinectY;
            _configuration.kinect_z = kinectZ;

            Debug.LogWarning("Reading configuration file from: " + Application.dataPath);
            Debug.LogWarning("Kinect position: " + _configuration.kinect_y + " ," + _configuration.kinect_z);

            getReal3D.RpcManager.call("SetKinectPosition", _configuration.kinect_x, _configuration.kinect_y, _configuration.kinect_z);
        }
    }

    [getReal3D.RPC]
    public void SetKinectPosition(float x, float y, float z) {
        kinect.transform.position = new Vector3(x, y, z);
    }

    public void Save() {
        if (getReal3D.Cluster.isMaster) {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(_directoryPath, "rehab-config.xml"));

            XmlNode node = doc.SelectSingleNode("config");
            node.SelectSingleNode("kinect_y").InnerText = _configuration.kinect_y.ToString();
            node.SelectSingleNode("kinect_z").InnerText = _configuration.kinect_z.ToString();

            Debug.LogWarning("REHABJIM: saving rehab-config to " + _directoryPath);

            try {
                doc.Save(Path.Combine(_directoryPath, "rehab-config.xml"));
            }
            catch (Exception e) {
                Debug.LogError(e.Message);
            }
        }
    }

    public void UpdateKinectPosition(float offset) {
        int axis = SessionManager.GetInstance().GetCalibrationAxis();

        if (axis == (int)CalibrationAxis.X) kinect.transform.position += new Vector3(offset, 0, 0);
        else if (axis == (int)CalibrationAxis.Y) kinect.transform.position += new Vector3(0, offset, 0);
        else kinect.transform.position += new Vector3(0, 0, offset);

        SessionManager.GetInstance().GetAvatarController().UpdateOffset();
        _configuration.kinect_x = kinect.transform.position.x;
        _configuration.kinect_y = kinect.transform.position.y;
        _configuration.kinect_z = kinect.transform.position.z;
    }

    public void DrawPlane() {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = _records;
        mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        mesh.triangles = new int[] { 0, 1, 2 };
    }
}
