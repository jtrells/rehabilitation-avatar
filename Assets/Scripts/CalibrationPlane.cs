using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CalibrationPlane : MonoBehaviour {

    private Mesh _mesh;
    private Vector3[] _vertices;

	// Use this for initialization
	void Start () {
        //_vertices = new Vector3[3];
        //_vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 10, 0), new Vector3(1, 20, 1) };
        Generate();
    }

    public void Generate() {
        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        _mesh.name = "Calibration Plane";

        _mesh.vertices = new Vector3[] { new Vector3(0, 1, 10), new Vector3(0, 10, 0), new Vector3(10, 2, 5) };
        _mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        _mesh.triangles = new int[] { 0, 1, 2 };
        _mesh.RecalculateNormals();

        Vector3 ray = transform.TransformDirection(Vector3.down);
        Debug.DrawRay(transform.position, ray, Color.red);
    }

    public Quaternion GetRotations() {
        return gameObject.transform.rotation;
    }
}
