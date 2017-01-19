using UnityEngine;
using UnityEngine.UI;

public class VirtualObjectSize : MonoBehaviour {

    public Text objectSizeLabel;

    protected float lastButtonUpdateTime = 0f;
    protected float antiBouncing = 0.4f;

    void Update () {
        int size = int.Parse(objectSizeLabel.text);

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (ControlBouncing()) size--;
            Debug.Log("---");
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (ControlBouncing()) size++;
            Debug.Log("+++");
        }
        else if (CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.ButtonRight)) {
            if (ControlBouncing()) size++;
        }
        else if (CAVE2Manager.GetButtonDown(1, CAVE2Manager.Button.ButtonLeft))
        {
            if (ControlBouncing()) size--;
        }


        objectSizeLabel.text = size.ToString();
        PlayerPrefs.SetInt("random_objects_size", size);
    }

    private bool ControlBouncing()
    {
        if (lastButtonUpdateTime + antiBouncing < Time.time)
        {
            lastButtonUpdateTime = Time.time;
            return true;
        }
        else return false;
    }
}
