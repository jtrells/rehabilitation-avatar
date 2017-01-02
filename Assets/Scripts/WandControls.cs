using UnityEngine;

public class WandControls : MonoBehaviour {

	protected float lastButtonUpdateTime = 0f;
	protected float antiBouncing = 0.4f;
	
	void Update () {
		if (CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button5) || Input.GetKeyDown(KeyCode.D) ) {           // Start distorted mode
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				SessionManager.GetInstance().EnableDistortedReality();
			}
		} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button7) || Input.GetKeyDown(KeyCode.F1)) {      // Open help menu
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				SessionManager.GetInstance().ToggleHelpPanel();
			}
		} else if (CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.Button3) || Input.GetKeyDown(KeyCode.KeypadEnter)){    // Open menu
            if (lastButtonUpdateTime + antiBouncing < Time.time) {
                lastButtonUpdateTime = Time.time;
                SessionManager.GetInstance().ToggleMenu();
            }
        } else if (Input.GetKeyDown(KeyCode.T)){     // Enable trajectory mode
            SessionManager.GetInstance().ToogleTrajectoryMode();
        }
        else if ((CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.ButtonRight) || Input.GetKeyDown(KeyCode.P))  // Change user perspective
                    && !SessionManager.GetInstance().IsConfirmVisible()){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
                Debug.Log("Changing Perspective");
				SessionManager.GetInstance().ChangePerspective();
			}
		}
	}
}
