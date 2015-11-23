﻿using UnityEngine;
using System.Collections;

public class TrainingModeMenu : ScrollableMenu {

	// Use this for initialization
	void Start () {
		numberOfButtons = 3;
	}
	
	// Update is called once per frame
	void Update () {
		if (gameObject.activeSelf) {
			base.Update();
			if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button3)){
				if (lastButtonUpdateTime + antiBouncing < Time.time) {
					lastButtonUpdateTime = Time.time;
					Debug.Log(index);
					if (index > 0 && index <= 3) {
						SessionManager.GetInstance().ToggleTrainingMode();
						PlayerPrefs.SetInt("TrainingModeId", index);
						string modeName = "";
						switch(index) {
							case 1: modeName = "Tutorial"; break;
							case 2: modeName = "Random Objects"; break;
							case 3: modeName = "Progressive distance"; break;
						}
						PlayerPrefs.SetString("TrainingMode", modeName);
						SessionManager.GetInstance().CreateObjectManager();
					} 
				}
			} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button2)){
				if (lastButtonUpdateTime + antiBouncing < Time.time) {
					lastButtonUpdateTime = Time.time;
					SessionManager.GetInstance().ToggleMenu();
					SessionManager.GetInstance().ToggleTrainingMode();
				}
			}
		}
	}
}