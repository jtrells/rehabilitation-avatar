using UnityEngine;
using System.Collections;

public class RandomGenerator : ObjectsManager {

	protected float yOffset = 1.4f, verticalBounds = 0.7f, horizontalBounds = 0.8f;
    private float _virtualObjectScale;

	public RandomGenerator() {
		numberOfObjects = 30;
	}

	protected override Vector3 PositionNewObject() {
        //GameObject randomObjectReference = GameObject.Find("RandomObjectsReference");
        //randomObjectReference.transform.rotation = Quaternion.identity;

        FlatAvatarController patient = GameObject.FindGameObjectWithTag("Patient").GetComponent<FlatAvatarController>();        

        Vector3 newPosition;
        /*
        = new Vector3(Random.Range(-horizontalBounds, horizontalBounds),
                                      yOffset + Random.Range(10f, (headPosition.y - yOffset) + 20f)/100,
                                      SessionManager.GetInstance().GetPatientPosition().z + 0.5f);*/
        Vector3 posOnSurface = Random.onUnitSphere * 0.7f;
        if (posOnSurface.y < 0) posOnSurface = new Vector3(posOnSurface.x, -posOnSurface.y, posOnSurface.z);
        if (posOnSurface.z < 0) posOnSurface = new Vector3(posOnSurface.x, posOnSurface.y, -posOnSurface.z);

        /*
        GameObject empty = new GameObject();
        empty.transform.SetParent(randomObjectReference.transform);
        empty.transform.position = posOnSurface;

        Vector3 reference = new Vector3(posOnSurface.x, 0f, posOnSurface.z);
        float angle = Vector3.Angle(reference.normalized, posOnSurface.normalized); */

        // remember unity is left hand sided
        /* Do not use angle correction
        if (angle > 30f) {
            Debug.LogWarning("Angle > 30 object:" + SessionManager.GetInstance().labelLeft.text + " - " + angle.ToString());
            float angleWithZAxis = Mathf.Abs(Vector3.Angle(reference.normalized, new Vector3(0f, 0f, 1f)));
            float xRotation = angle - 30f + Random.Range(5f, 25f);

            if (posOnSurface.x < 0)
                angleWithZAxis = -angleWithZAxis;

            randomObjectReference.transform.Rotate(new Vector3(0f, angleWithZAxis, 0f));
            randomObjectReference.transform.Rotate(new Vector3(xRotation, 0f, 0f));
            randomObjectReference.transform.Rotate(new Vector3(0f, -angleWithZAxis, 0f));            
        }

        posOnSurface = empty.transform.position;

        Vector3 reference2 = new Vector3(posOnSurface.x, 0f, posOnSurface.z);
        float angle2 = Vector3.Angle(reference2.normalized, posOnSurface.normalized);
        Debug.LogWarning("Position with angle: " + angle2);
        */

        newPosition = patient.spineShoulder.transform.position + posOnSurface;

        //Destroy(empty);
        /*
        if (Mathf.Abs(newPosition.x) < 10f) {
				if (newPosition.x > 0)
					newPosition.x = newPosition.x + 10;
				else if(newPosition.x < 0)
					newPosition.x = newPosition.x - 10;
			}*/
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
