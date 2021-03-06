﻿using UnityEngine;
using System.Collections;
using SimpleJSON;

public class CustomGenerator : ObjectsManager {

	protected float yOffset = 1.5f, verticalBounds = 0.7f, horizontalBounds = 0.8f;

	JSONArray customObjects;

	public CustomGenerator() {
		numberOfObjects = 15;
		TextAsset json = (TextAsset) Resources.Load ("CustomGenerator");
		customObjects = (JSON.Parse (json.text)).AsArray;
		numberOfObjects = customObjects.Count;
	}

	protected override Vector3 PositionNewObject() {
        Debug.LogWarning("REHABJIM - getting new position from CustomGenerator");
        JSONNode currentObj = customObjects[currentObject-1];
		Vector3 newPosition = new Vector3 (currentObj["x"].AsFloat, currentObj["y"].AsFloat, SessionManager.GetInstance ().GetPatientPosition().z + currentObj["z"].AsFloat);
		return newPosition;
	}

	protected override void MakeRPCCall(Vector3 newPosition, Quaternion newQuaternion) {
		getReal3D.RpcManager.call("CreateNewObjectRPC", newPosition, newQuaternion);
	}


	[getReal3D.RPC]
	private void CreateNewObjectRPC (Vector3 newPosition, Quaternion newQuaternion) {
        Debug.LogWarning("REHABJIM - creating new rpc object from custom");

        float scale = SessionManager.GetInstance().GetScale();
        virtualObject = (GameObject) GameObject.Instantiate (_objectPrefab, newPosition, newQuaternion);
        virtualObject.transform.localScale =
            new Vector3(virtualObject.transform.localScale.x * scale, virtualObject.transform.localScale.y * scale, virtualObject.transform.localScale.z * scale);

        virtualObject.GetComponent<VirtualObject> ().manager = this;
		CreateOptimalTrajectory(newPosition);
		appearTime = Time.time;
	}

	public override void ClearTrajectories() {
		getReal3D.RpcManager.call("ClearTrajectoriesRPC");
	}
	
	[getReal3D.RPC]
	protected void ClearTrajectoriesRPC() {
		Destroy(directionArrow);
	}

}
