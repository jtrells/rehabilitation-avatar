﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SessionMenu : ScrollableMenu {

	/*private float lastButtonUpdateTime = 0f;
	private float antiBouncing = 0.05f;
	private int index = 0;
	private int numberOfButtons = 4;
	public Image[] images = new Image[3];
	public Sprite[] sprites = new Sprite[2];*/

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (gameObject.activeSelf) {
			base.Update();
			if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button3)){
				if (lastButtonUpdateTime + antiBouncing < Time.time) {
					lastButtonUpdateTime = Time.time;
					if (index >= 0 && index < 3) {
						switch (index) {
						case 1:
							break;
						case 2:
							SessionManager.GetInstance ().ToggleTrainingMode();
							SessionManager.GetInstance().ToggleMenu();
							break;
						case 3:
							break;
						default:
							break;
						}
					} 
				}
			} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button2)){
				if (lastButtonUpdateTime + antiBouncing < Time.time) {
					lastButtonUpdateTime = Time.time;
					SessionManager.GetInstance().ToggleMenu();
				}
			}
		}
	}


	private void UpdateGraphics() {
		for(int i=0; i<images.Length; i++) {
			if(i == index-1) {
				images[i].sprite = sprites[1];
			}
			else {
				images[i].sprite = sprites[0];
			}
		}
	}
}
