﻿using UnityEngine;
using System.Collections;

public class RandomGenerator : ObjectsManager {

	protected float yOffset = 1.4f, verticalBounds = 0.7f, horizontalBounds = 0.8f;
    private float _virtualObjectScale;

    public void SetObjectScale(float objectScale) {
        _virtualObjectScale = objectScale;
    }

	public RandomGenerator() {
		numberOfObjects = 20;
	}

	protected override Vector3 PositionNewObject() {
        Debug.LogWarning("REHABJIM - getting new position from RandomGenerator");
        FlatAvatarController patient = GameObject.FindGameObjectWithTag("Patient").GetComponent<FlatAvatarController>();
        Vector3 headPosition = SessionManager.GetInstance().GetCave2Manager().getHead(1).position;

        int d = SessionManager.GetInstance().GetPerspective() == (int) Perspective.Third ? 1 : 4;
        /*
		Vector3 newPosition = 
            new Vector3 (Random.Range(-horizontalBounds, horizontalBounds), 
                         yOffset + Random.Range(-verticalBounds/d, verticalBounds/d), 
                         SessionManager.GetInstance ().GetPatientPosition().z + 0.3f);*/
        Vector3 newPosition = new Vector3(Random.Range(-horizontalBounds, horizontalBounds),
                                          yOffset + Random.Range(10f, (headPosition.y - yOffset) + 20f)/100,
                                          SessionManager.GetInstance().GetPatientPosition().z + Random.Range(30f, 45f)/100);

        if (Mathf.Abs(newPosition.x) < xAvatarSize) {
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
        Debug.LogWarning("REHABJIM - creating new rpc object from random");
        virtualObject = (GameObject) GameObject.Instantiate (_objectPrefab, newPosition, newQuaternion);

        /*
		if(SessionManager.GetInstance().GetPerspective() != (int)Perspective.Third) {
			virtualObject.transform.localScale = new Vector3(virtualObject.transform.localScale.x * 0.5f, virtualObject.transform.localScale.y * 0.5f, virtualObject.transform.localScale.z * 0.5f );
		}*/
        virtualObject.transform.localScale =
            new Vector3(virtualObject.transform.localScale.x * _virtualObjectScale, virtualObject.transform.localScale.y * _virtualObjectScale, virtualObject.transform.localScale.z * _virtualObjectScale);

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
