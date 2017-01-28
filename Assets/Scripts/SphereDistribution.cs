using UnityEngine;
using System.Collections;

public class SphereDistribution : MonoBehaviour {

    public int numberOfPoints = 80;
    public float scale = 3.0f;

    // Use this for initialization
    void Start () {
        GameObject innerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        innerSphere.transform.localScale = innerSphere.transform.localScale * (scale * 2);
        innerSphere.transform.name = "Inner Sphere";

        Vector3[] myPoints = GetPointsOnSphere(numberOfPoints);
        foreach (Vector3 point in myPoints)
        {
            GameObject outerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            outerSphere.transform.position = point * scale;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    Vector3[] GetPointsOnSphere(int nPoints)
    {
        float fPoints = (float)nPoints;

        Vector3[] points = new Vector3[nPoints];

        float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
        float off = 2 / fPoints;

        for (int k = 0; k < nPoints; k++)
        {
            float y = k * off - 1 + (off / 2);
            float r = Mathf.Sqrt(1 - y * y);
            float phi = k * inc;

            points[k] = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
        }

        return points;
    }
}
