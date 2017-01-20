using UnityEngine;
using System.Collections;

public class RandomGenerator : ObjectsManager {

	protected float yOffset = 1.4f, verticalBounds = 0.7f, horizontalBounds = 0.8f;
    private float _virtualObjectScale;

	public RandomGenerator() {
		numberOfObjects = 20;
	}

	protected override Vector3 PositionNewObject() {
        Debug.LogWarning("REHABJIM - getting new position from RandomGenerator");
        FlatAvatarController patient = GameObject.FindGameObjectWithTag("Patient").GetComponent<FlatAvatarController>();
        Vector3 headPosition = SessionManager.GetInstance().GetCave2Manager().getHead(1).position;

        //int d = SessionManager.GetInstance().GetPerspective() == (int) Perspective.Third ? 1 : 4;
        /*
		Vector3 newPosition = 
            new Vector3 (Random.Range(-horizontalBounds, horizontalBounds), 
                         yOffset + Random.Range(-verticalBounds/d, verticalBounds/d), 
                         SessionManager.GetInstance ().GetPatientPosition().z + 0.3f);*/
        Vector3 newPosition = new Vector3(Random.Range(-horizontalBounds, horizontalBounds),
                                          yOffset + Random.Range(10f, (headPosition.y - yOffset) + 20f)/100,
                                          SessionManager.GetInstance().GetPatientPosition().z + 0.5f);
        
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

        float scale = SessionManager.GetInstance().GetScale();
        Debug.LogWarning("REHABJIM - Scale from session manager:" + scale);
        /*
		if(SessionManager.GetInstance().GetPerspective() != (int)Perspective.Third) {
			virtualObject.transform.localScale = new Vector3(virtualObject.transform.localScale.x * 0.5f, virtualObject.transform.localScale.y * 0.5f, virtualObject.transform.localScale.z * 0.5f );
		}*/
        Debug.LogWarning("REHABJIM - Random RPC new object 1 scale: " + virtualObject.transform.localScale.x + ", " + virtualObject.transform.localScale.y + ", " + virtualObject.transform.localScale.z);
        Debug.LogWarning("apply scale" + _virtualObjectScale.ToString());
        virtualObject.transform.localScale = new Vector3(1f, 1f, 1f);
        virtualObject.transform.localScale =
            new Vector3(virtualObject.transform.localScale.x * scale, virtualObject.transform.localScale.y * scale, virtualObject.transform.localScale.z * scale);
        Debug.LogWarning("REHABJIM - Random RPC new object position: " + virtualObject.transform.position.x + ", " + virtualObject.transform.position.y + ", " + virtualObject.transform.position.z);
        Debug.LogWarning("REHABJIM - Random RPC new object 2 scale: " + virtualObject.transform.localScale.x + ", " + virtualObject.transform.localScale.y + ", " + virtualObject.transform.localScale.z);

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
