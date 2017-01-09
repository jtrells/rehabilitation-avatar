﻿using UnityEngine;

public class WandControls : MonoBehaviour {

	protected float lastButtonUpdateTime = 0f;
	protected float antiBouncing = 0.4f;
	
	void Update () {
        if (CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.Button7) || Input.GetKeyDown(KeyCode.F1)) {      // L2
            if (SessionManager.GetInstance().GetStatus() != (int)ExerciseStatus.Calibration)
                if (ControlBouncing()) CalibrationManager.GetInstance().Save();
            else
                if (ControlBouncing()) SessionManager.GetInstance().ToggleHelpPanel();
        } else if (CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.Button3) || Input.GetKeyDown(KeyCode.KeypadEnter)) {    // X
            if (SessionManager.GetInstance().GetStatus() == (int)ExerciseStatus.Running)
                if (ControlBouncing()) SessionManager.GetInstance().ToggleMenu();
        }
        else if (CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.ButtonRight)) {
            if (SessionManager.GetInstance().GetStatus() != (int)ExerciseStatus.Calibration) {
                if (ControlBouncing()) SessionManager.GetInstance().SwitchTrainingMode(true);
            }
            else
                SessionManager.GetInstance().SwitchCalibrationAxis(true);
        }
        else if (CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.ButtonLeft)) {
            if (SessionManager.GetInstance().GetStatus() != (int)ExerciseStatus.Calibration) {
                if (ControlBouncing()) SessionManager.GetInstance().SwitchTrainingMode(false);
            }
            else
                SessionManager.GetInstance().SwitchCalibrationAxis(false);
        }
        else if ((CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.Button5) || Input.GetKeyDown(KeyCode.P))  // L1
                    && !SessionManager.GetInstance().IsConfirmVisible()) {
            if (ControlBouncing()) SessionManager.GetInstance().ChangePerspective();
        }
        else if ((CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.ButtonUp) || Input.GetKeyDown(KeyCode.UpArrow))){
            if (SessionManager.GetInstance().GetStatus() == (int)ExerciseStatus.Calibration)
                if (ControlBouncing()) CalibrationManager.GetInstance().UpdateKinectPosition(0.01f);
        }
        else if ((CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.ButtonDown) || Input.GetKeyDown(KeyCode.DownArrow))) {
            if (SessionManager.GetInstance().GetStatus() == (int)ExerciseStatus.Calibration)
                if (ControlBouncing()) CalibrationManager.GetInstance().UpdateKinectPosition(-0.01f);
        } else if (CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.Button2)) {  // Circle
            if (ControlBouncing()) {
                if (SessionManager.GetInstance().GetStatus() == (int)ExerciseStatus.Pause) {
                    SessionManager.GetInstance().ToggleMenu();
                    SessionManager.GetInstance().Resume();
                }
                else {
                    SessionManager.GetInstance().ToggleCalibrationMode();
                }
            }
        }
    }

    private bool ControlBouncing() {
        if (lastButtonUpdateTime + antiBouncing < Time.time){
            lastButtonUpdateTime = Time.time;
            return true;
        }
        else return false;
    }
}
