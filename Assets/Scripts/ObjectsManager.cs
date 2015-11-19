﻿using UnityEngine;
using System.Collections;
using SimpleJSON;

public class ObjectsManager : getReal3D.MonoBehaviourWithRpc {

	protected int currentObject = 0;
	protected int numberOfObjects;

	protected JSONArray objects = new JSONArray();
	protected float appearTime;

	protected GameObject virtualObject;
	//protected float 

	protected float xAvatarSize = 0.3f;

	protected float allowedTime = 10f;

	protected Object objectPrefab;

	protected void Start() {
		objectPrefab = Resources.Load ("BasicObject");
	}

	public void NextObject() {
		if (getReal3D.Cluster.isMaster) {
			if (currentObject == numberOfObjects) {
				Invoke("EndSession", 1f);
				return;
			}
			SessionManager.GetInstance ().StartTimer();
			Vector3 newPosition = this.PositionNewObject();
			Quaternion newQuaternion = Quaternion.Euler (UnityEngine.Random.Range (0f, 360f), UnityEngine.Random.Range (0.0f, 360f), UnityEngine.Random.Range (0.0f, 360f));
			getReal3D.RpcManager.call("CreateNewObjectRPC", newPosition, newQuaternion);
		}
	}

	virtual protected Vector3 PositionNewObject () { return Vector3.zero; }

	protected void EndSession() {
		SessionManager.GetInstance().EndSession();
	}

	public int GetNumberOfObjects () {
		return numberOfObjects;
	}

	[getReal3D.RPC]
	private void CreateNewObjectRPC (Vector3 newPosition, Quaternion newQuaternion) {
		currentObject++;
		//elapsedTime = Time.time;
		//labelLeft.text = "Object #" + currentObject;
		virtualObject = (GameObject) GameObject.Instantiate (objectPrefab, newPosition, newQuaternion);
		virtualObject.GetComponent<VirtualObject> ().manager = this;
		appearTime = Time.time;
	}

	public void ObjectCaught(float caughtTime) {
		if (getReal3D.Cluster.isMaster) {
			JSONNode obj = new JSONClass ();
			obj ["id"].AsInt = currentObject;
			obj ["time"].AsFloat = caughtTime - appearTime;
			obj["reached"] = "Yes";
			objects.Add (obj);

			NextObject ();
		}
	}

	public void ObjectNotCaught(float expirationTime) {
		if (getReal3D.Cluster.isMaster) {
			JSONNode obj = new JSONClass ();
			obj ["id"].AsInt = currentObject;
			obj ["time"].AsFloat = expirationTime - appearTime;
			obj["reached"] = "No";
			objects.Add (obj);
			
			NextObject ();
		}
	}

	public JSONArray GetObjectsData() {
		return objects;
	}

	void Update() {
		if (currentObject > 0 && virtualObject != null && Time.time > allowedTime + appearTime) {
			Destroy(virtualObject);
			ObjectNotCaught(Time.time);
		}
	}

}
