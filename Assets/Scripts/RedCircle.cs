using UnityEngine;

public class RedCircle : MonoBehaviour {

	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
		if(other.gameObject.CompareTag("Foot") && SessionManager.GetInstance().GetStatus() == (int)ExerciseStatus.Preparing) {
			SessionManager.GetInstance().PatientInPosition();
			Destroy(gameObject);
		}
	}
}
