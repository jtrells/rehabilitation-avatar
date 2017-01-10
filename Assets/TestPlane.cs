using UnityEngine;
using System.Collections;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
public class TestPlane : MonoBehaviour {

    private Mesh mesh;

	// Use this for initialization
	void Awake () {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        mesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) };
        mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        mesh.triangles = new int[] { 0, 1, 2 };
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
