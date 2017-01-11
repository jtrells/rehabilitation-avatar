using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Text;

// Controller of the rehab session environment
public class SessionManager : getReal3D.MonoBehaviourWithRpc {

    private static SessionManager instance;             // Singleton instance
    private int _trainingType;                          // Random Objects, Progressive
    private int _trainingMode;                          // specifies if the training is being done in Normal/Trajectory/Distorted mode/Trajectory+Distorted
    private int _perspective;                           // specifies if the user is in first/third person perspective
    private int _status, _oldStatus;                    // status of the exercise based on the ExerciseStatus Enum
    private int _currentCalibrationAxis;                // axis being calibrated: X, Y, Z
    private FlatAvatarController _avatarController;     // controls the avatar positioning by gathering the Kinect's values
    private CAVE2Manager cave2Manager;

    public GameObject objectPrefab, menuPanel, trainingPanel, camDisplay, helpPanel, confirmPanel, mapPanel;
	public Text textHint;
	public Material litMaterial, normalMaterial;
	public Text labelLeft, labelRight, labelMode, labelHelp; 
	public GameObject sessionCompleteAnimation;
	public AudioSource voice;
    public GameObject firstPersonTransform;
    public GameObject kinectOffset;

	private bool tutorialMode = false, lastPhaseOfTutorial = false;

    public Text labelHands;

	
	private float xAvatarSize = 0.3f;
	private bool patientInsideCircle;
	private float elapsedTime = 0f;
	private GameObject patient, patientHips;
	private float lastButtonUpdateTime;
	private float antiBouncing = 0.4f;
	private float minimumZ = 4.6f;

	public float bodyOffset = 4.6f;

	private GameObject redCircle;

	private AudioSource audio;

	private ObjectsManager manager;

	protected bool isTimerStopped = true;

	delegate void ConfirmDelegate();
	private ConfirmDelegate currentDelegate;



    public GameObject cameraController;

    // Getters
    public static SessionManager GetInstance() { return instance; }
    public int GetTrainingMode() { return _trainingMode; }
    public int GetPerspective()  { return _perspective; }
    public int GetStatus() { return _status; }
    public int GetCalibrationAxis() { return _currentCalibrationAxis; }
    public FlatAvatarController GetAvatarController() { return _avatarController; } 

    public bool IsTrajectoryEnabled() { return (_trainingMode == (int)TrainingMode.Trajectory || _trainingMode == (int)TrainingMode.DistortedAndTrajectory); }

    // set the singleton SessionManager
	void Awake () {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
	}

    // Use this for initialization
    void Start() {
        // find objects on scene
        patient = GameObject.FindGameObjectWithTag("Patient");
        patientHips = GameObject.FindGameObjectWithTag("Hips");
        audio = GetComponent<AudioSource>();
        _avatarController = patient.GetComponent<FlatAvatarController>();
        _currentCalibrationAxis = (int)CalibrationAxis.Y;
        _status = (int)ExerciseStatus.Preparing;
        _oldStatus = _status;

        // start scene in third person perspective
        SetThirdPersonPerspective();
        _avatarController.UpdateOffset();

        //CreateObjectManager();
        // If the Main scene was called to start a training, then start the training
        if (PlayerPrefs.HasKey("TrainingModeId")) StartNewTraining(PlayerPrefs.GetInt("TrainingModeId"));
    }

    // Update the timer, timer text and patient joints log
    void Update() {
        if (!isTimerStopped && _status == (int)ExerciseStatus.Running) UpdateTime();

        string status = "";
        if (_status == (int)ExerciseStatus.Preparing) status = "Preparing";
        else if (_status == (int)ExerciseStatus.Running) status = "Running";
        else if (_status == (int)ExerciseStatus.Pause) status = "Pause";
        else if (_status == (int)ExerciseStatus.Finished) status = "Finished";
        else if (_status == (int)ExerciseStatus.Calibration) status = "Calibration";
        else status = "other";

        string oldstatus = "";
        if (_oldStatus == (int)ExerciseStatus.Preparing) oldstatus = "Preparing";
        else if (_oldStatus == (int)ExerciseStatus.Running) oldstatus = "Running";
        else if (_oldStatus == (int)ExerciseStatus.Pause) oldstatus = "Pause";
        else if (_oldStatus == (int)ExerciseStatus.Finished) oldstatus = "Finished";
        else if (_oldStatus == (int)ExerciseStatus.Calibration) oldstatus = "Calibration";
        else status = "other";

        string axis = "";
        if (_currentCalibrationAxis == (int)CalibrationAxis.X) axis = "X";
        else if (_currentCalibrationAxis == (int)CalibrationAxis.Y) axis = "Y";
        else if (_currentCalibrationAxis == (int)CalibrationAxis.Z) axis = "Z";
        else axis = "OTHER";

        int noObjectsCaught = -1, noObjects = -1, currentObject = -1;
        if (manager)
        {
            noObjects = manager.GetNumberOfObjects();
            noObjectsCaught = manager.GetNumberOfObjectsCaught();
            currentObject = manager.GetCurrentObjectNumber();
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("Status: ").Append(status).AppendLine();
        sb.Append("Old Status: ").Append(oldstatus).AppendLine();
        sb.Append("Calibration Axis: ").Append(axis).AppendLine();
        sb.Append("Kinect Offset:").Append(GetFormattedPosition(kinectOffset)).AppendLine();
        sb.Append("Number objects caught: ").Append(noObjects).AppendLine();
        sb.Append("Current object number: ").Append(currentObject).AppendLine();
        sb.Append("Total objects: ").Append(noObjectsCaught).AppendLine();
        sb.Append("Head: ").Append(GetFormattedPosition(firstPersonTransform)).AppendLine().AppendLine();
        sb.Append("Shoulders: ").Append(GetFormattedPosition(_avatarController.leftShoulder)).Append(" - ").Append(GetFormattedPosition(_avatarController.rightShoulder)).AppendLine();
        sb.Append("Elbows:    ").Append(GetFormattedPosition(_avatarController.leftElbow)).Append(" - ").Append(GetFormattedPosition(_avatarController.rightElbow)).AppendLine();
        sb.Append("Hands:     ").Append(GetFormattedPosition(_avatarController.leftHand)).Append(" - ").Append(GetFormattedPosition(_avatarController.rightHand)).AppendLine();
        sb.Append("Hips:      ").Append(GetFormattedPosition(_avatarController.leftHip)).Append(" - ").Append(GetFormattedPosition(_avatarController.rightHip)).AppendLine();
        sb.Append("Knees:     ").Append(GetFormattedPosition(_avatarController.leftKnee)).Append(" - ").Append(GetFormattedPosition(_avatarController.rightKnee)).AppendLine();
        sb.Append("Feet:      ").Append(GetFormattedPosition(_avatarController.leftFoot)).Append(" - ").Append(GetFormattedPosition(_avatarController.rightFoot)).AppendLine();

        labelHands.text = sb.ToString();
    }

    public void Pause(int status) {
        isTimerStopped = true;
        SetNewStatus(status);
    }

    public void Resume() {
        isTimerStopped = false;
        SetNewStatus(_oldStatus);
    }

    public void SwitchTrainingMode(bool dir) {
        if (dir)
            if (_trainingMode == (int)TrainingMode.DistortedAndTrajectory)
                _trainingMode = (int)TrainingMode.Normal;
            else _trainingMode++;
        else
            if (_trainingMode == (int)TrainingMode.Normal)
                _trainingMode = (int)TrainingMode.DistortedAndTrajectory;
            else _trainingMode--;

        if (_trainingMode == (int)TrainingMode.Distorted) {
            SetDistortedReality(true);
            SetTrajectoryMode(false);
        }
        else if (_trainingMode == (int)TrainingMode.Trajectory) {
            SetDistortedReality(false);
            SetTrajectoryMode(true);
        }
        else if (_trainingMode == (int)TrainingMode.DistortedAndTrajectory) {
            SetDistortedReality(true);
            SetTrajectoryMode(true);
        }
        else if (_trainingMode == (int)TrainingMode.Normal) {
            SetDistortedReality(false);
            SetTrajectoryMode(false);
        }
    }

    public void ToggleCalibrationMode() {
        if (_status == (int)ExerciseStatus.Calibration){
            Resume();
        }
        else {
            Pause((int)ExerciseStatus.Calibration);
        }
    }

    public void SwitchCalibrationAxis(bool dir) {
        if (dir)
            if (_currentCalibrationAxis == (int)CalibrationAxis.Z)
                _currentCalibrationAxis = (int)CalibrationAxis.X;
            else _currentCalibrationAxis++;
        else
            if (_currentCalibrationAxis == (int)CalibrationAxis.X)
                _currentCalibrationAxis = (int)CalibrationAxis.Z;
            else _currentCalibrationAxis--;
    }

    private void ConfirmMethod(string message, ConfirmDelegate del)
    {

        ToggleMenus(confirmPanel);

        //confirmPanel.SetActive(true);
        currentDelegate = del;
    }

    public void ExecuteDelegate()
    {
        currentDelegate();
        CloseMenus();
        confirmPanel.SetActive(false);
    }

    public bool IsConfirmVisible()
    {
        return confirmPanel.activeSelf;
    }

    public void CancelDelegate()
    {
        ToggleMenus(confirmPanel);
    }


    public bool isTutorialMode () {
		return tutorialMode;
	}

	public IEnumerator StartCountdown() {
		ShowRedCircle();
		while (!patientInsideCircle || _status != (int)ExerciseStatus.Preparing) {
			yield return null;
		}
		//HideRedCircle();
		patientInsideCircle = false;
		for(int i=5; i>0; i--) {
			DisplayText (".. " + i + " ..");
			PlayAudio("Countdown");
			yield return new WaitForSeconds(1f);
		}

        SetNewStatus((int)ExerciseStatus.Running);

        CreateFirstObject();
		PlayAudio ("Start");
		DisplayText ("Session started!!");
		yield return new WaitForSeconds(2f);
		DisplayText ("");
	}

	private void ShowRedCircle() {
		if(redCircle == null) {
			redCircle = (GameObject) GameObject.Instantiate(Resources.Load("RedCircle"));
            redCircle.transform.position = new Vector3(0, 0, 0);
        }
	}

	private void HideRedCircle() {
		if(redCircle != null) {
			Destroy(redCircle);
		}
	}

	private void CreateObjectManager() {
		if(manager != null) {
			manager.CancelSession();
			StopTimer();
			labelLeft.text = "";
			labelRight.text = "";
			StopAllCoroutines();

            //Destroy(manager);
		}
		if (tutorialMode) {
			StopAllCoroutines();
		}
		DisplayText (" ");
		patient.SetActive(true);
		GameObject[] objects = GameObject.FindGameObjectsWithTag("BasicObject");
		foreach (GameObject o in objects) {
			Destroy(o);
		}
		if(PlayerPrefs.HasKey("TrainingModeId")) {
			switch(PlayerPrefs.GetInt("TrainingModeId")) {
			case 1: StartCoroutine(Tutorial()); break;
			case 2: manager = gameObject.AddComponent<RandomGenerator> (); break;
			case 3: manager = gameObject.AddComponent<ProgressiveDistanceGenerator> (); break;
			case 4: manager = gameObject.AddComponent<CustomGenerator> (); break;
			}
			if(manager != null && PlayerPrefs.GetInt("TrainingModeId")!=1) {
                SetNewStatus((int)ExerciseStatus.Preparing);
				StartCoroutine(StartCountdown());
			}
		}
	}

	public void StartNewTraining(int trainingId) {
        _trainingType = trainingId;
		PlayerPrefs.SetInt("TrainingModeId", trainingId);

        // if the exercise routine has not started or it has ended
        // Otherwise. The call was made from the confirmation popup
		if(!manager || manager.isEnded())
			StartNewTrainingConfirmed();
		else 
			ConfirmMethod("", StartNewTrainingConfirmed);
		
	}

	private void StartNewTrainingConfirmed() {
		int trainingId = PlayerPrefs.GetInt("TrainingModeId");
		string modeName = "";
		switch(trainingId) {
		    case 1: modeName = "Tutorial"; break;
		    case 2: modeName = "Random Objects"; break;
		    case 3: modeName = "Progressive distance"; break;
		    case 4: modeName = "Custom training"; break;
		}

		CheckIfLabelNeeded();
		PlayerPrefs.SetString("TrainingMode", modeName);
		labelMode.text = modeName;

        Debug.LogWarning("REHABJIM - Starting a new training " + modeName);
		CreateObjectManager();
		DisplayText("Please walk into the red circle");

        // set the training mode and perspective to defaults
        _trainingMode = (int) TrainingMode.Normal;
    }

	private void CreateFirstObject() {
        Debug.LogWarning("REHABJIM - create the first object");
        manager.NextObject ();
		elapsedTime = Time.time;
	}

	private void UpdateTime() {
		labelRight.text = "Time: " + Math.Floor((Time.time-elapsedTime) * 10) / 10 ;
	}

	public void UpdateCurrentObject(int objectNumber) {
		labelLeft.text = "Object #" + (objectNumber);
	}

	public IEnumerator Tutorial() {
		tutorialMode = true;
		CheckIfLabelNeeded();
		ToggleCamDisplay();
		DisplayText ("Welcome to the tutorial");
		PlayVoice ("Voce00002");

		patient.SetActive (false);
		yield return new WaitForSeconds (4f);

		GameObject pat = (GameObject)GameObject.Instantiate (Resources.Load("TutorialPatient"));
		DisplayText ("The boy on the left will represent your avatar");
		PlayVoice ("Voce00003");
		yield return new WaitForSeconds (4f);

		GameObject the = (GameObject)GameObject.Instantiate (Resources.Load("TutorialTherapist"));
		DisplayText ("The skeleton on the right will represent your therapist's avatar");
		PlayVoice ("Voce00004");
		yield return new WaitForSeconds (6f);

		Destroy(pat);
		Destroy (the);
		patient.SetActive (true);

		DisplayText ("Now walk forward to the red circle");
		ShowRedCircle();
		PlayVoice ("Voce00005");
		yield return new WaitForSeconds (6f);
		while (!patientInsideCircle) {
			yield return null;
		}

		DisplayText ("This is the position you will have to maintain during your training");
		PlayVoice ("Voce00006");
		yield return new WaitForSeconds (5f);
		DisplayText ("Now let's start!");
		//Missing audio
		yield return new WaitForSeconds (2f);
		HideRedCircle();

		DisplayText ("Reach the ball with your rigth hand and stay in position");
		PlayVoice ("Voce00007");
		UnityEngine.Object objPrefab = Resources.Load ("BasicObject");
		GameObject obj1 = (GameObject)GameObject.Instantiate (objPrefab, new Vector3 (0.8f, 1.5f, patientHips.transform.position.z + 0.1f), Quaternion.identity);

		while(obj1 != null) {
			yield return null;
		}

		DisplayText ("Great! Now do the same with your left hand");
		PlayVoice ("Voce00008");
		GameObject obj2 = (GameObject)GameObject.Instantiate (objPrefab, new Vector3 (-0.8f, 1.5f, patientHips.transform.position.z + 0.1f), Quaternion.identity);

		while(obj2 != null) {
			yield return null;
		}

		PlayAudio ("Victory");
		DisplayText ("");
		GameObject vfx = (GameObject) GameObject.Instantiate (sessionCompleteAnimation, patientHips.transform.position, Quaternion.identity);
		yield return new WaitForSeconds (4f);
		patient.SetActive (false);
		PlayVoice ("Voce00010");
		DisplayText ("Good job! Now you'll learn how to open the menu");
		yield return new WaitForSeconds (5f);
		UnityEngine.Object wandPrefab = Resources.Load ("wand");
		GameObject wand = (GameObject)GameObject.Instantiate (wandPrefab);

		wand.GetComponentsInChildren<Renderer> () [0].material = litMaterial;

		DisplayText ("Press the 'X' button on the wand controller");
		PlayVoice ("Voce00014");
		while(!menuPanel.activeSelf) {
			yield return null;
		}
		wand.GetComponentsInChildren<Renderer> () [0].material = normalMaterial;

		wand.GetComponentsInChildren<Renderer> () [3].material = litMaterial;
		wand.GetComponentsInChildren<Renderer> () [6].material = litMaterial;

		yield return new WaitForSeconds (2f);
		lastPhaseOfTutorial = true;
		DisplayText ("Now use the arrows to select 'Training Mode' and then press 'X' again");
		PlayVoice ("Voce00015");
		while(!trainingPanel.activeSelf) {
			yield return null;
		}
		wand.GetComponentsInChildren<Renderer> () [3].material = normalMaterial;
		wand.GetComponentsInChildren<Renderer> () [6].material = normalMaterial;
		yield return new WaitForSeconds (1f);
		DisplayText ("From here you can select which training mode to start.");
		PlayVoice ("Voce00016");
		yield return new WaitForSeconds (6f);

		wand.GetComponentsInChildren<Renderer> () [1].material = litMaterial;
		DisplayText ("Now press two times the 'O' button on the wand controller to exit the menu.");
		PlayVoice ("Voce00017");
		while(trainingPanel.activeSelf || menuPanel.activeSelf) {
			yield return null;
		}
		wand.GetComponentsInChildren<Renderer> () [1].material = normalMaterial;

		Destroy (wand);
		patient.SetActive (true);
		lastPhaseOfTutorial = false;
		yield return new WaitForSeconds (1f);
		DisplayText ("Nice! Now we'll try to open the menu using your voice!");
		PlayVoice ("Voce00019");
		yield return new WaitForSeconds (5f);

		DisplayText ("Say aloud the word 'menu' and the menu will appear.");
		PlayVoice ("Voce00020");
		while(!menuPanel.activeSelf) {
			yield return null;
		}
		yield return new WaitForSeconds (1f);
		DisplayText ("Good job! Your training is complete");
		ToggleCamDisplay();
		PlayVoice ("Voce00021");
		yield return new WaitForSeconds (3f);
		DisplayText ("");
		tutorialMode = false;
	}

    // Methods for finishing an exercise session --------------------------------------------------------------------------------------------------
	public void EndSession() {
		labelRight.text = "";
		labelLeft.text = "";
		StartCoroutine(EndSessionCoroutine());
		PlayerPrefs.SetFloat ("TotalTime", elapsedTime);
	}

    //display visual and sounds effects and write the logs in the exercise log file
	private IEnumerator EndSessionCoroutine() {
		DisplayText ("!! Training Complete !!");
		PlayAudio ("Victory");
		GameObject vfx = (GameObject) GameObject.Instantiate (sessionCompleteAnimation, patientHips.transform.position, Quaternion.identity);
		yield return new WaitForSeconds (2f);

        SetNewStatus((int)ExerciseStatus.Finished);
        StopTimer();
        getReal3D.RpcManager.call("DisplayTrainingSummary", manager.GetNumberOfObjectsCaught(), manager.GetTotalElapsedTime());

        if (getReal3D.Cluster.isMaster) {
            LogWriter.instance.WriteLogs(manager, _avatarController.jointsLog);
		}
	}
    // End methods for finishing an exercise session ------------------------------------------------------------------------------------------------

    public bool isLastPhaseOfTutorial() {
		return lastPhaseOfTutorial;
	}

	[getReal3D.RPC]
	private void DisplayTrainingSummary(int numberOfObjectsCaught, float time) {
		DisplayText(/*"Mode: " + PlayerPrefs.GetString("TrainingMode") + */"Objects caught: " + numberOfObjectsCaught + " out of " + manager.GetNumberOfObjects ()+ "\nElapsed time: " + Mathf.Round(time) + "s");
	}

	private void ChangeScene(){
		getReal3D.RpcManager.call("ChangeSceneRPC");
	}

	[getReal3D.RPC]
	private void ChangeSceneRPC() {
		Application.LoadLevel ("Results");
	}
	

	public void RestartSession(){
		if(!manager || manager.isEnded()) {
			RestartConfirmed();
		}
		else {

			ConfirmMethod ("", new ConfirmDelegate(RestartConfirmed));
		}
	}

	public void RestartConfirmed() {
		CreateObjectManager();
	}

	public void ConfirmExit() {
		ConfirmMethod ("", new ConfirmDelegate(ExitSession));
	}

	public void ExitSession() {
		if(getReal3D.Cluster.isMaster){
			getReal3D.RpcManager.call("ExitSessionRPC");
		}
	}

	[getReal3D.RPC]
	private void ExitSessionRPC(){
		Application.LoadLevel("Menu");
	}
	
	public void VoiceCommand(string command) {
		PlayAudio ("Activation");
		switch(command) {
			case "RESTART": case "START": Invoke("RestartSession", 1f); break;
			case "MENU": case "OPEN MENU": ToggleMenu(); break;
			case "STOP": AbortSession(); break;
			case "EXIT": ConfirmExit(); break;
			case "MAP": ToggleMap(); break;
			case "TRAJECTORY": SetTrajectoryMode(true); break;
			case "FIRST PERSON": SetFirstPersonPerspective(); break;
			case "DISTORTED REALITY": SetDistortedReality(true); break;
			case "THIRD PERSON": SetThirdPersonPerspective(); break;
			case "MODE": case "TRAINING MODE": ShowTrainingModes(); break;
			case "CLOSE": case "CLOSE MENU": CloseMenus(); break;
			case "RANDOM OBJECTS": StartNewTraining(2); break;
			case "PROGRESSIVE DISTANCE": StartNewTraining(3); break;
			case "TUTORIAL": StartNewTraining(1); break;
			case "CUSTOM TRAINING": StartNewTraining(4); break;
			case "HELP": ToggleHelpPanel(); break;
			case "YES": VoiceYes(); break;
			case "NO": VoiceNo(); break;
		}
	}

	public void ToggleMap() {
		ToggleMenus (mapPanel);
	}

	public void CloseMenus() {
		if(menuPanel.activeSelf || trainingPanel.activeSelf) {
			PlayAudio ("Cancel");
			menuPanel.SetActive(false);
			trainingPanel.SetActive(false);
		}
	}

	private void VoiceYes() {
		if(confirmPanel.activeSelf) {
			ExecuteDelegate();
		}
	}

	private void VoiceNo() {
		if(confirmPanel.activeSelf) {
			CancelDelegate();
		}
	}

	public void ToggleMenu() {
		closeHelpPanel();

        if (trainingPanel.activeSelf) ToggleMenus(trainingPanel);
        ToggleMenus(menuPanel);

        if (menuPanel.activeSelf) Pause((int)ExerciseStatus.Pause);
        else Resume();
	}

    private void SetNewStatus(int newStatus) {
        if (newStatus != _status) {
            _oldStatus = _status;
            _status = newStatus;
        } 
    }

	public void ToggleTrainingMode() {
		closeHelpPanel();
        if (menuPanel.activeSelf) ToggleMenus(menuPanel);
		ToggleMenus(trainingPanel);
	}

	public void AbortSession() {
		StopTimer();
		if(getReal3D.Cluster.isMaster) {
			getReal3D.RpcManager.call("DisplayTrainingSummary", manager.GetTotalElapsedTime());
		}
	}

	public void ShowTrainingModes() {
		if(menuPanel.activeSelf) {
			menuPanel.SetActive(false);
		}
		closeHelpPanel();
		trainingPanel.SetActive(true);
	}

    // Switch between first and third person perspectives
	public void ChangePerspective() {
        Debug.Log("Current Perspective: " + _perspective);
        if (_perspective == (int)Perspective.Third)
            SetFirstPersonPerspective();
        else SetThirdPersonPerspective();
	}

    // Enable Distorted Reality Mode
	public void SetDistortedReality(bool val) {
        _avatarController.SetDistortedReality(val);
		PlayAudio("Start");
		RegenerateLabelMode();
	}

    // Enable trajectory mode
    public void SetTrajectoryMode(bool val) {
        TrailRenderer left = _avatarController.leftHand.GetComponent<TrailRenderer>();
        TrailRenderer right = _avatarController.rightHand.GetComponent<TrailRenderer>();
        left.enabled = val;
        right.enabled = val;

        if (val) PlayAudio("Start");
        RegenerateLabelMode();
    }

    private void SetFirstPersonPerspective() {
        _perspective = (int)Perspective.First;

        // for CAVE2, set the cameraController position at 0,0,0 and the Omicron script will update the head position
        cameraController.transform.position = new Vector3(0, 0, 0);
        cameraController.transform.rotation = Quaternion.identity;

        _avatarController.SetFirstPerson();
    }

    private void SetThirdPersonPerspective() {
        _perspective = (int) Perspective.Third;

        // Positions and rotations calculated manually by moving the camera around during design
        cameraController.transform.position = new Vector3(0, 1.2f, -9.1f);
        cameraController.transform.eulerAngles = new Vector3(10.5f, 0, 0);
        _avatarController.SetThirdPerson();
	}

	public void ToggleCamDisplay() {
		if(camDisplay.activeSelf) {
			camDisplay.SetActive(false);
		}
		else {
			camDisplay.SetActive(true);
		}
	}

	private void ToggleMenus (GameObject menu) {
		if(menu.GetComponent<ScrollableMenu>())
			menu.GetComponent<ScrollableMenu>().SetActivationTime(Time.time);
	
		if(menu.activeSelf) {
			PlayAudio ("Cancel");
			menu.SetActive(false);
		}
		else {
			PlayAudio ("Activation");
			menu.SetActive(true);
		}
		CheckIfLabelNeeded();
	}

	private void CheckIfLabelNeeded() {
		if (menuPanel.activeSelf || trainingPanel.activeSelf || confirmPanel.activeSelf || helpPanel.activeSelf || tutorialMode) {
			labelHelp.enabled = false;
		} else {
			labelHelp.enabled = true;
		}
	}

	public Vector3 GetPatientPosition() {
        return patientHips.transform.position;
	}

	public void RestartTimer() {
		elapsedTime = Time.time;
	}
	

	public bool IsTimerStopped() {
		return isTimerStopped;
	}

    private string GetFormattedPosition(GameObject joint){
        Vector3 position = joint.transform.position;
        return "(" + position.x.ToString("0.00") + ", " + position.y.ToString("0.00") + ", " + position.z.ToString("0.00") + ") ";
    }

	public void DisplayText(string text) {
		textHint.text = text;
	}

	public void ToggleHelpPanel() {
		closeAllMenu();
		ToggleMenus(helpPanel);
	}

	public void closeHelpPanel() {
		helpPanel.SetActive(false);
	}

	private void closeAllMenu() {
		menuPanel.SetActive(false);
		trainingPanel.SetActive(false);
	}
    
	public Vector3 GetNearestHand(Vector3 pos) {
		FlatAvatarController controller = patient.GetComponent<FlatAvatarController>();
		Vector3 leftHand = controller.leftHand.transform.position;
		Vector3 rightHand = controller.rightHand.transform.position;
		if(Vector3.Distance(leftHand, pos) > Vector3.Distance(rightHand, pos)) {
			return rightHand;
		}
		return leftHand;
	}

    public string GetNearestHandName(Vector3 pos)
    {
        FlatAvatarController controller = patient.GetComponent<FlatAvatarController>();
        Vector3 leftHand = controller.leftHand.transform.position;
        Vector3 rightHand = controller.rightHand.transform.position;
        if (Vector3.Distance(leftHand, pos) > Vector3.Distance(rightHand, pos))
        {
            return "right_hand";
        }
        return "left_hand";
    }

	private void RegenerateLabelMode() {
		string text = PlayerPrefs.GetString("TrainingMode") + "\n";
		
        if (_trainingMode == (int)TrainingMode.Distorted)
            text += " Distorsion enabled";
        else if (_trainingMode == (int)TrainingMode.DistortedAndTrajectory)
            text += " Trajectory + Distortion enabled";
        else if (_trainingMode == (int)TrainingMode.Trajectory)
            text += " Trajectory enabled";
        labelMode.text = text;
	}

	public void PatientInPosition() {
		patientInsideCircle = true;
	}

    private void PlayAudio(string name) {
        audio.clip = (AudioClip)Resources.Load("Audio/" + name);
        audio.Play();
    }

    private void PlayVoice(string name) {
        voice.clip = (AudioClip)Resources.Load("Audio/" + name);
        voice.Play();
    }

    // Methods to control the timer on screen. The timer only runs 
    // when isTimerStopped is false
    public void StopTimer() {
        isTimerStopped = true;
    }
    public void StartTimer() {
        isTimerStopped = false;
    }
}
