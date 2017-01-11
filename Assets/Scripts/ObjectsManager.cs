using UnityEngine;
using SimpleJSON;

public class ObjectsManager : getReal3D.MonoBehaviourWithRpc {

    protected JSONArray objects = new JSONArray();
    
    protected GameObject virtualObject;
    protected float xAvatarSize = 0.3f;
    protected GameObject directionArrow;

    protected float appearTime;                 // time when the object appeared on scene. Set by the descendant classes
    protected float allowedTime = 10f;          // amount of time allowed to catch an object
    protected int currentObject = 0;            // Number of current object
    protected int numberOfObjects;              // Total number of objects in the exercise
    protected int _objectsCaught = 0;           // Total number of objects caught during the exercise
    protected Object _objectPrefab;             // Defines the prefab for the virtual objects to create

    private Vector3 _newObjectPosition;         // Position in the scene for a new object

    // Getters 
    public int GetNumberOfObjectsCaught() { return _objectsCaught; }
    public int GetNumberOfObjects() { return numberOfObjects; }
    public int GetCurrentObjectNumber() { return currentObject; }
    public JSONArray GetObjectsData() { return objects; }

    void Start() {
        _objectPrefab = Resources.Load("BasicObject");
    }

    // Track the time to see if the patient did not catch the object on time
    void Update() {
        if (currentObject > 0 && virtualObject != null && Time.time > allowedTime + appearTime) {
            Debug.LogWarning("REHABJIM - Destroying object cuz it wasn't reached on time");
            Destroy(virtualObject);
            ObjectNotCaught(Time.time);
        }
    }

    // Create a new object in the rehab exercise if still needed. Otherwise, it end the exercise session
    public void NextObject() {
        Debug.LogWarning("REHABJIM - Getting next object from ObjectManager");
        currentObject++;
        SessionManager.GetInstance().StartTimer();
        SessionManager.GetInstance().UpdateCurrentObject(currentObject);

        // If there was an optimal path suggested displayed, clean it
        if (directionArrow) ClearTrajectories();

        // if all the objects have been rendered, wait 1 second and end the session
        if (currentObject == numberOfObjects + 1) {
            SessionManager.GetInstance().StopTimer();
            Invoke("EndSession", 1f);
            return;
        }
        else {
            if (getReal3D.Cluster.isMaster) {
                _newObjectPosition = PositionNewObject();
                Debug.LogWarning("REHABJIM - position for new object: " + _newObjectPosition.x + ", " + _newObjectPosition.y + ", " + _newObjectPosition.z);
                MakeRPCCall(_newObjectPosition, Quaternion.identity);
            }
        }
    }

    virtual protected Vector3 PositionNewObject() { return Vector3.zero; }
    virtual protected void MakeRPCCall(Vector3 newPosition, Quaternion newQuaternion) { }
    virtual public void ClearTrajectories() { }

    protected void CreateOptimalTrajectory(Vector3 newPosition) {
        if (!(SessionManager.GetInstance().IsTrajectoryEnabled())) return;

        Vector3 hand = SessionManager.GetInstance().GetNearestHand(newPosition);
        directionArrow = (GameObject)GameObject.Instantiate(Resources.Load("Cube"), newPosition, Quaternion.identity);
        directionArrow.transform.LookAt(hand);
        directionArrow.transform.localScale = new Vector3(0.005f, 0.005f, Vector3.Distance(newPosition, hand));
        directionArrow.transform.position = ((hand - newPosition) / 2f) + newPosition;
    }

    private void EndSession() {
        if (directionArrow) ClearTrajectories();
        SessionManager.GetInstance().EndSession();
    }

    public void ObjectCaught(float caughtTime) {
        LogObject(caughtTime);
        SessionManager.GetInstance().RestartTimer();
        _objectsCaught++;
        NextObject();
    }

    private void ObjectNotCaught(float expirationTime) {
        SessionManager.GetInstance().StopTimer();
        LogObject(expirationTime);
        SessionManager.GetInstance().RestartTimer();
        NextObject();
    }

    private void LogObject(float time) {
        if (getReal3D.Cluster.isMaster) {
            JSONNode objectLog = new JSONClass();
            objectLog["id"].AsInt = currentObject;
            objectLog["appear_time"].AsFloat = appearTime;
            objectLog["caught_time"].AsFloat = time;
            objectLog["time"].AsFloat = time - appearTime;
            objectLog["reached"] = "Yes";
            objectLog["mode"].AsInt = SessionManager.GetInstance().GetTrainingMode();
            objectLog["pers"].AsInt = SessionManager.GetInstance().GetPerspective();

            // position of new object
            objectLog["ox"].AsFloat = _newObjectPosition.x;
            objectLog["oy"].AsFloat = _newObjectPosition.y;
            objectLog["oz"].AsFloat = _newObjectPosition.z;

            objectLog["hand"] = SessionManager.GetInstance().GetNearestHandName(_newObjectPosition);

            objects.Add(objectLog);
        }
    }

	public float GetTotalElapsedTime() {
		float time = 0f;
		foreach(JSONNode obj in objects.Childs) time += obj["time"].AsFloat;
		return time;
	}

	public bool isEnded() {
		return currentObject >= numberOfObjects;
	}

	public void CancelSession() {
		if(directionArrow) ClearTrajectories();
		
		Destroy (virtualObject);
		Destroy(this);
	}
}
