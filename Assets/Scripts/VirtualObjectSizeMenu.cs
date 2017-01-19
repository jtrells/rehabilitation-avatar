using UnityEngine;
using UnityEngine.UI;

public class VirtualObjectSizeMenu : MonoBehaviour {

    public Text objectSizeLabel;

    protected float lastButtonUpdateTime = 0f;
    protected float antiBouncing = 0.4f;

    void Update () {
        int size = int.Parse(objectSizeLabel.text);

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (ControlBouncing()) size--;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (ControlBouncing()) size++;
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
