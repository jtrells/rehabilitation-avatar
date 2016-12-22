using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;
using SimpleJSON;

// Doing now: Remove hardcoded values for third person. Instead, move the camera backwards.

public class FlatAvatarController : OmicronEventClient {

	protected float samplingRate = 5f;
    public ArrayList jointsLog = new ArrayList();

	private bool _isThirdPerson = true;
	public bool _isPatient = false;
	private bool _isDistortedReality;

	public GameObject protoGuyBody, protoGuyHead;
	public GameObject[] bodyParts;

	private float lastUpdate, timeout = 0.1f;
	public int bodyId = -1;

	public OmicronKinectManager kinectManager;
    public GameObject kinect;
	
	public GameObject hips, leftHand, rightHand, leftElbow, rightElbow, leftShoulder, rightShoulder;
	public GameObject leftHip, rightHip, leftKnee, rightKnee, leftFoot, rightFoot;

	public enum KinectHandState { Unknown, NotTracked, Open, Closed, Lasso };
	private KinectHandState leftHandState, rightHandState;

    private Vector3 _offset;

    public bool IsThirdPerson() { return _isThirdPerson; }
    public bool IsPatient() { return _isPatient; }
    public bool IsDistortedReality() { return _isDistortedReality; }
    public void SetDistortedReality(bool state) { _isDistortedReality = state; }

	void Start() {
		OmicronManager omicronManager = GameObject.FindGameObjectWithTag("OmicronManager").GetComponent<OmicronManager>();
		omicronManager.AddClient(this);

        // Set an offset on the Y axis as the kinect is above the ground
        // and the kinect's coordinate system is set at the infrared sensor
        _offset = new Vector3(0, kinectManager.transform.position.y, kinectManager.transform.position.z);
    }

	// Fecth data from the kinect in a Omicron Mocap EventData object. Update the data positions based on the sampling
    // frequency provided by the Kinect v2
	void OnEvent(EventData e) {
		if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap)
			UpdateJointsPosition(e);
	}

    // Get the joints position from the array of vectors in Omicron. The structure
    // requires on this version (don't know the version) to pass the jointID
	private Vector3 GetJointPosition(EventData e, int jointId) {
		float[] jointPosition = new float[3];
		e.getExtraDataVector3(jointId, jointPosition);
        
		return new Vector3(jointPosition[0], jointPosition[1], -jointPosition[2]);
	}

    /*
	private int GetJointNumber(EventData e) {
		return (int) e.extraDataItems;
	}*/

	private KinectHandState FetchHandState(float value) {
        if (value == 0) return KinectHandState.Unknown;
        else if (value == 1) return KinectHandState.NotTracked;
        else if (value == 2) return KinectHandState.Open;
        else if (value == 3) return KinectHandState.Closed;
        else if (value == 4) return KinectHandState.Lasso;
        else  return KinectHandState.Unknown;
	}
	
    // Update all the avatar joints positions based on the fetched data
	private void UpdateJointsPosition(EventData e) {

		if (e.serviceType != EventBase.ServiceType.ServiceTypeMocap) return;
		
		int sourceId = (int)e.sourceId;
		if (bodyId != sourceId) return;

		if (!_isDistortedReality) {
			UpdateHipsPosition (e);
			UpdateJointPosition (leftElbow, e, 7);
			UpdateJointPosition (rightElbow, e, 17);
				
			UpdateJointPosition (leftHand, e, 9);
			UpdateJointPosition (rightHand, e, 19);

			UpdateJointPosition (leftShoulder, e, 6);
            UpdateJointPosition(rightShoulder, e, 16);
			leftHandState = FetchHandState(e.orw);
			rightHandState = FetchHandState(e.orx);
			
			UpdateJointPosition (leftHip, e, 11);
			UpdateJointPosition (rightHip, e, 21);
			
			UpdateJointPosition (leftKnee, e, 12);
			UpdateJointPosition (rightKnee, e, 22);
			
			UpdateJointPosition (leftFoot, e, 13);
			UpdateJointPosition (rightFoot, e, 23);
		} else {
			UpdateHipsPositionDistorted(e);
			UpdateJointPositionDistorted (leftElbow, e, 17);
			UpdateJointPositionDistorted (rightElbow, e, 7);
			
			UpdateJointPositionDistorted (leftHand, e, 19);
			UpdateJointPositionDistorted (rightHand, e, 9);
			
			UpdateJointPositionDistorted (leftShoulder, e, 16);
			UpdateJointPositionDistorted (rightShoulder, e, 6);

			UpdateJointPositionDistorted (leftHip, e, 21);
			UpdateJointPositionDistorted (rightHip, e, 11);
			
			UpdateJointPositionDistorted (leftKnee, e, 22);
			UpdateJointPositionDistorted (rightKnee, e, 12);
			
			UpdateJointPositionDistorted (leftFoot, e, 23);
			UpdateJointPositionDistorted (rightFoot, e, 13);
		}

		lastUpdate = Time.time;
	}

    // Get the string value for each joint id for log purposes
    private string GetJointName(int jointId) {
        if (jointId == 7) return "left_elbow";
        if (jointId == 17) return "right_elbow";
        if (jointId == 9) return "left_hand";
        if (jointId == 19) return "right_hand";
        if (jointId == 6) return "left_shoulder";
        if (jointId == 16) return "right_shoulder";
        if (jointId == 11) return "left_hip";
        if (jointId == 21) return "right_hip";
        if (jointId == 12) return "left_knee";
        if (jointId == 22) return "right_knee";
        if (jointId == 13) return "left_foot";
        if (jointId == 23) return "right_foot";
        return jointId.ToString();
    }

    private void addNewLogPosition(int jointId, Vector3 position, Quaternion rotation) {
        JSONNode positionLog = new JSONClass();
        positionLog["joint"] = GetJointName(jointId);
        positionLog["time"].AsFloat = Time.time;
        positionLog["x"].AsFloat = position.x;
        positionLog["y"].AsFloat = position.y;
        positionLog["z"].AsFloat = position.z;
        positionLog["rx"].AsFloat = rotation.x;
        positionLog["ry"].AsFloat = rotation.y;
        positionLog["rz"].AsFloat = rotation.z;
        positionLog["rw"].AsFloat = rotation.w;

        jointsLog.Add(positionLog.ToString());
    }

    // Get new position and update localTransform value
	private void UpdateJointPosition(GameObject joint, EventData e, int jointId) {
		Vector3 newPosition = GetJointPosition(e, jointId);
        if (_isDistortedReality) newPosition = new Vector3(-newPosition.x, newPosition.y, newPosition.z);
        UpdateAndLogPosition(joint, jointId, newPosition);
    }

    // Update joint values for distorted mode. The method was divided to avoid a bunch of if statements while sending the jointIds values
	private void UpdateJointPositionDistorted(GameObject joint, EventData e, int jointId) {
		Vector3 newPosition = GetJointPosition(e, jointId);
		newPosition = new Vector3(-newPosition.x, newPosition.y, newPosition.z);
        UpdateAndLogPosition(joint, jointId, newPosition);
	}

    private void UpdateAndLogPosition(GameObject joint, int jointId, Vector3 newPosition) {
        if (!newPosition.Equals(Vector3.zero)){
            //joint.transform.localPosition = newPosition + new Vector3(0f, _isThirdPerson ? yOffset : 0f, _isThirdPerson ? zOffset : 0f) + offset;
            joint.transform.position = newPosition + _offset;
            // Only store information while the user is running an exercise. Avoid pre-settings, pause and finished states
            if (SessionManager.GetInstance().GetStatus() == (int)ExerciseStatus.Running)
                addNewLogPosition(jointId, joint.transform.position, joint.transform.rotation);
        }
    }

	private void UpdateHipsPosition(EventData e) {
		Vector3 newPosition = GetJointPosition(e, 0);
		if(!newPosition.Equals(Vector3.zero)) {
			//hips.transform.localPosition = new Vector3(newPosition.x, newPosition.y, newPosition.z) + new Vector3(0f, yOffset, zOffset) + _offset;
            hips.transform.position = new Vector3(newPosition.x, newPosition.y, newPosition.z) + _offset;
        }
	}

	private void UpdateHipsPositionDistorted(EventData e) {
		Vector3 newPosition = GetJointPosition(e, 0);
		if(!newPosition.Equals(Vector3.zero)) {
			//hips.transform.localPosition = new Vector3(-newPosition.x, newPosition.y, newPosition.z) + new Vector3(0f, yOffset, zOffset) + _offset;
            hips.transform.position = new Vector3(-newPosition.x, newPosition.y, newPosition.z)  + _offset;
        }
	}

	private void KillAvatar() {
		SetFlaggedForRemoval();
		kinectManager.RemoveBody(bodyId);
		Destroy(gameObject);
	}

	private void KillPatient() {
		SetFlaggedForRemoval();
		kinectManager.RemoveBody(bodyId);
	}

	void LateUpdate() {
		if (!_isPatient && kinectManager.GetPatientId() == bodyId) 
			KillAvatar();
		
		if (Time.time > lastUpdate + timeout) {
			if (!_isPatient) {
				KillAvatar();
			} else if (bodyId != -1) {
				KillPatient();
				bodyId = -1;
				gameObject.SetActive(false);
			}
		}
	}

	public void SetBodyId(int newBodyId) {
		bodyId = newBodyId;
		lastUpdate = Time.time;
	}

    // Only enable body elements in first person
	public void SetFirstPerson() {
		_isThirdPerson = false;
		protoGuyHead.SetActive(false);
		protoGuyBody.SetActive(false);

		foreach(GameObject g in bodyParts) g.SetActive(true);
	}

	public void SetThirdPerson() {
		_isThirdPerson = true;
		protoGuyHead.SetActive(true);
		protoGuyBody.SetActive(true);

		foreach(GameObject g in bodyParts) g.SetActive(false);
	}
}
