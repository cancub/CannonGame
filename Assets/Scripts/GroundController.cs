using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour {
	private Vector3[] vertices;

	// Use this for initialization
	void Start () {
		Mesh mesh = new Mesh ();
		vertices = new Vector3[4];
		vertices [0] = new Vector3 (-12, -4, 0);
		vertices [1] = new Vector3 (12, -4, 0);
		vertices [2] = new Vector3 (-12, -6, 0);
		vertices [3] = new Vector3 (12, -6, 0);
		mesh.vertices = vertices;

		int[] triangles = new int[6]{0,1,3,0,3,2 };

		mesh.triangles = triangles;

		GetComponent<MeshFilter> ().mesh = mesh;
	}

	public Vector2[] GetVertices() {
		Vector2[] ret = new Vector2[vertices.Length];
		for(int i  = 0; i < vertices.Length; i++) {
			ret [i] = new Vector2 (vertices [i].x, vertices [i].y);
		}
		return ret;
	}
	
	// Update is called once per frame
//	void Update () {
//		
//	}
}
