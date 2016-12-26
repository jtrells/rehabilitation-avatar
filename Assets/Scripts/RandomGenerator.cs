﻿using UnityEngine;
using System.Collections;

public class RandomGenerator : ObjectsManager {

	protected float yOffset = 1.65f, verticalBounds = 0.7f, horizontalBounds = 0.8f;

	public RandomGenerator() {
		numberOfObjects = 30;
	}

	protected override Vector3 PositionNewObject() {
		FlatAvatarController patient = GameObject.FindGameObjectWithTag("Patient").GetComponent<FlatAvatarController>();
		int d = SessionManager.GetInstance().GetPerspective() == (int) Perspective.Third ? 1 : 4;
			Vector3 newPosition = new Vector3 (UnityEngine.Random.Range(-horizontalBounds, horizontalBounds), yOffset + UnityEngine.Random.Range(-verticalBounds/d, verticalBounds/d), SessionManager.GetInstance ().GetPatientPosition().z + 0.3f);
			if(Mathf.Abs(newPosition.x) < xAvatarSize) {
				if (newPosition.x > 0)
					newPosition.x = newPosition.x + xAvatarSize;
				else if(newPosition.x < 0)
					newPosition.x = newPosition.x - xAvatarSize;
			}
			return newPosition;
	}

	protected override void MakeRPCCall(Vector3 newPosition, Quaternion newQuaternion) {
		getReal3D.RpcManager.call("CreateNewObjectRPC", newPosition, newQuaternion);
	}


	[getReal3D.RPC]
	private void CreateNewObjectRPC (Vector3 newPosition, Quaternion newQuaternion) {
		virtualObject = (GameObject) GameObject.Instantiate (_objectPrefab, newPosition, newQuaternion);
		if(SessionManager.GetInstance().GetPerspective() != (int)Perspective.Third) {
			virtualObject.transform.localScale = new Vector3(virtualObject.transform.localScale.x * 0.5f, virtualObject.transform.localScale.y * 0.5f, virtualObject.transform.localScale.z * 0.5f );
		}
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
