using UnityEngine;
using System.Collections;
using System.IO;
using System;
using SimpleJSON;

// Dummy temporal class for testing
public class Export : MonoBehaviour {


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string path = "Logs\\Juan-12-20-201630821 PM\\positions.txt";
            Debug.Log("Exporting: " + DateTime.Now.ToLongTimeString());
            /*
           
            string myString;

            using (FileStream fs = new FileStream(path, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                byte[] bin = br.ReadBytes(Convert.ToInt32(fs.Length));
                myString = System.Text.Encoding.Default.GetString(bin);
                //myString = Convert.ToBase64String(bin);
            }

            using (FileStream fs2 = new FileStream("Logs\\Juan-12-20-201630821 PM\\out.txt", FileMode.Create))
            using (StreamWriter bw = new StreamWriter(fs2))
                bw.Write(myString);
                */
            JSONNode node = SimpleJSON.JSONArray.LoadFromFile(path);
            using (FileStream fs2 = new FileStream("Logs\\Juan-12-20-201630821 PM\\out.txt", FileMode.Create))
            using (StreamWriter bw = new StreamWriter(fs2))
                bw.Write(node.ToString());

            Debug.Log("Finish: " + DateTime.Now.ToLongTimeString());


        }

        if (Input.GetKeyDown(KeyCode.F1)) {
            Debug.Log("press f1");
        }
    }
}
