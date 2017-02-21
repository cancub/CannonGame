using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoatController : MonoBehaviour {
	public float scale;
	private Vector3[] m_x; // current positions
	private Vector3[] m_oldx; // previous positions
	private Vector3[] m_a;	// forces
	private Vector3 m_vGravity;
	private GameObject goat;
	private int numberOfParticles;
	private int[] triangles;

	void Start() {
		numberOfParticles = 3;
		m_x = new Vector3[numberOfParticles];
		m_oldx = new Vector3[numberOfParticles];
		m_a = new Vector3[numberOfParticles];
		m_vGravity = Vector3.down * 9.81f;

		DrawGoat ();



	}

	void Update() {
		AccumulateForces ();
		Verlet ();
		SatisfyConstraints ();

//		LineRenderer lineRenderer = GetComponent<LineRenderer>();
//		var t = Time.time;
//		for(int i = 0; i < lengthOfLineRenderer; i++) {
//			lineRenderer.SetPosition(i, new Vector3(i * 0.5f, Mathf.Sin(i + t), -2.0f));
//		}
	}

	private void DrawGoat () {
		Material newMatertial =  new Material (Shader.Find("Unlit/Color"));
		newMatertial.color = Color.white;

		// make a simple triangle
		m_x = new Vector3[3];
		m_x[0] = new Vector3 (0, 0, 0);
		m_x[1] = new Vector3 (1, 0, 0);
		m_x[2] = new Vector3 (0.5f, Mathf.Sqrt(0.75f), 0);
		//		GetComponent<MeshFilter> ().mesh.vertices = vertices;
		//		int[] triangles = new int[3]{ 0, 2, 1};
		//		GetComponent<MeshFilter> ().mesh.triangles = triangles;


		// move the wheel to the position that we'd like it to be
		transform.position = new Vector3 (0, 0,0);

		GetComponent<MeshFilter> ().mesh.vertices = m_x;

		// now that we have all the vertices of the circle object, we can draw them
		triangles = new int[3];

		triangles [0] = 0;
		triangles [1] =2;
		triangles [2] = 1;

		GetComponent<MeshFilter> ().mesh.triangles = triangles;

		GetComponent<MeshRenderer> ().material = newMatertial;

		// set the original positions of the vertices and set the old positions of the vertices
		for (int i = 0; i < numberOfParticles; i++) {
			m_oldx [i] = m_x [i] + new Vector3(0.08f,0,0);
		}

		//		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		//		Material newMatertial =  new Material (Shader.Find("Unlit/Color"));
		//		newMatertial.color = c1;
		//		lineRenderer.material = newMatertial;
		//		lineRenderer.widthMultiplier = 0.1f;
		//		lineRenderer.numPositions = lengthOfLineRenderer;
	}

	private void AccumulateForces () {
		for (int i = 0; i < numberOfParticles; i++) {
			m_a [i] = m_vGravity;
		}
	}

	private void Verlet() {
		for (int i = 0; i < numberOfParticles; i++) {
			Vector3 x = m_x [i];
			Vector3 temp = x;
			Vector3 oldx = m_oldx [i];
			Vector3 a = m_a [i];
			m_x [i] += m_x [i] - m_oldx [i] + m_a [i] * Time.deltaTime * Time.deltaTime;
			m_oldx [i] = temp;
		}
		GetComponent<MeshFilter> ().mesh.vertices = m_x;
		GetComponent<MeshFilter> ().mesh.triangles = triangles;
	}

	private void SatisfyConstraints() {
		float restLength = 1;
		for (int j = 0; j < 3; j++) {
			for (int i = 0; i < numberOfParticles; i++) {
				m_x [i] = Vector3.Min (Vector3.Max (m_x [i], new Vector3 (-4, -4, 0)), new Vector3 (5, 5, 0));
			}

			// get the distance between the points of the verlet goat
			Vector3 delta = m_x [1] - m_x [0];
			delta *= restLength * restLength / (Vector3.Dot (delta, delta) + restLength * restLength) - 0.5f;
			m_x [0] -= delta;
			m_x [1] += delta;

			delta = m_x [2] - m_x [1];
			delta *= restLength * restLength / (Vector3.Dot (delta, delta) + restLength * restLength) - 0.5f;
			m_x [1] -= delta;
			m_x [2] += delta;

			delta = m_x [0] - m_x [2];
			delta *= restLength * restLength / (Vector3.Dot (delta, delta) + restLength * restLength) - 0.5f;
			m_x [2] -= delta;
			m_x [0] += delta;



		}

		GetComponent<MeshFilter> ().mesh.vertices = m_x;
		GetComponent<MeshFilter> ().mesh.triangles = triangles;
	}
}
