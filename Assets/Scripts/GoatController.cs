using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Constraint {
	public int a,b;
	public float restLength;
}

public class GoatController : MonoBehaviour {
	public float scale;
	private Vector3[] m_x; // current positions
	private Vector3[] m_oldx; // previous positions
	private Vector3[] m_a;	// forces
	private Vector3 m_vGravity;
	private GameObject goat;
	private int numberOfParticles;
	private int[] triangles;
	private List<Constraint> constraints;




	void Start() {
		
		numberOfParticles = 21;
		m_x = new Vector3[numberOfParticles];
		m_oldx = new Vector3[numberOfParticles];
		m_a = new Vector3[numberOfParticles];
		m_vGravity = Vector3.down * 2f;
		constraints = new List<Constraint> ();
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

		// make the vertices that will make the mesh for the goat
		m_x = new Vector3[21];
		m_x[0] = new Vector3 (3,-1,0) * scale;
		m_x[1] = new Vector3 (2.2f,-1,0)* scale;
		m_x[2] = new Vector3 (0.6f,-1,0)* scale;
		m_x[3] = new Vector3 (0,-1,0)* scale;
		m_x[4] = new Vector3 (-1.2f,0.2f,0)* scale;
		m_x[5] = new Vector3 (-1.4f,0.4f,0)* scale;
		m_x[6] = new Vector3 (-1.6f,0.6f,0)* scale;
		m_x[7] = new Vector3 (-2.4f,0.6f,0)* scale;
		m_x[8] = new Vector3 (-2.6f,0,0)* scale;
		m_x[9] = new Vector3 (-2.8f,0.6f,0)* scale;
		m_x[10] = new Vector3 (-3.2f,0.6f,0) * scale;
		m_x[11] = new Vector3 (-3,1.2f,0) * scale;
		m_x[12] = new Vector3 (-1.8f,2.2f,0) * scale;
		m_x[13] = new Vector3 (-1.2f,2.4f,0) * scale;
		m_x[14] = new Vector3 (-1.6f,1.8f,0) * scale;
		m_x[15] = new Vector3 (-1.2f,1.2f,0) * scale;
		m_x[16] = new Vector3 (-1,1,0) * scale;
		m_x[17] = new Vector3 (-0.6f,1,0) * scale;
		m_x[18] = new Vector3 (2.6f,1,0) * scale;
		m_x[19] = new Vector3 (2.8f,1.4f,0) * scale;
		m_x[20] = new Vector3 (3,1,0) * scale;

		GetComponent<MeshFilter> ().mesh.vertices = m_x;

//		transform.position = new Vector3 (0, 0,0);


		// now that we have all the vertices of the goat object, we can draw them
		triangles = new int[19*3];

		triangles [0] = 0;
		triangles [1] = 1;
		triangles [2] = 18;

		triangles [3] = 1;
		triangles [4] = 2;
		triangles [5] = 18;

		triangles [6] = 2;
		triangles [7] = 17;
		triangles [8] = 18;

		triangles [9] = 2;
		triangles [10] = 3;
		triangles [11] = 17;

		triangles [12] = 3;
		triangles [13] = 4;
		triangles [14] = 17;

		triangles [15] = 4;
		triangles [16] = 16;
		triangles [17] = 17;

		triangles [18] = 4;
		triangles [19] = 5;
		triangles [20] = 16;

		triangles [21] = 5;
		triangles [22] = 6;
		triangles [23] = 16;

		triangles [24] = 6;
		triangles [25] = 15;
		triangles [26] = 16;

		triangles [27] = 6;
		triangles [28] = 14;
		triangles [29] = 15;

		triangles [30] = 6;
		triangles [31] = 7;
		triangles [32] = 14;

		triangles [33] = 7;
		triangles [34] = 12;
		triangles [35] = 14;

		triangles [36] = 7;
		triangles [37] = 11;
		triangles [38] = 12;

		triangles [39] = 7;
		triangles [40] = 9;
		triangles [41] = 11;

		triangles [42] = 9;
		triangles [43] = 10;
		triangles [44] = 11;

		triangles [45] = 7;
		triangles [46] = 8;
		triangles [47] = 9;

		triangles [48] = 12;
		triangles [49] = 13;
		triangles [50] = 14;

		triangles [51] = 18;
		triangles [52] = 19;
		triangles [53] = 20;

		triangles [54] = 0;
		triangles [55] = 18;
		triangles [56] = 20;

		// now add the constraints based on the vertices current positions

		// head
		AddHardConstraint(5,16);
		AddHardConstraint(5,17);
		AddHardConstraint(6,11);
		AddHardConstraint(6,12);
		AddHardConstraint(6,14);
		AddHardConstraint(6,15);
		AddHardConstraint(6,5); // soft
		AddHardConstraint(6,16); // soft
		AddHardConstraint(7,8); // soft
		AddHardConstraint(7,6);
		AddHardConstraint(7,11);
		AddHardConstraint(7,12);
		AddHardConstraint(7,14);
		AddHardConstraint(7,15);
		AddHardConstraint(9,12);
		AddHardConstraint(9,14);
		AddHardConstraint(9,15);
		AddHardConstraint(9,10);
		AddHardConstraint(9,7);
		AddHardConstraint(9,8); // soft
		AddHardConstraint(10,12);
		AddHardConstraint(10,14);
		AddHardConstraint(10,15);
		AddHardConstraint(10,11);
		AddHardConstraint(10,9);
		AddHardConstraint(11,14);
		AddHardConstraint(11,15);
		AddHardConstraint(11,12);
		AddHardConstraint(12,13);
		AddHardConstraint(12,14);
		AddHardConstraint(14,13);
		AddHardConstraint(14,15);
		AddHardConstraint(15,16); // soft
		AddHardConstraint(15,5); // soft
		AddHardConstraint(14,17);
		AddHardConstraint(14,18);
		AddHardConstraint(7,17);
		AddHardConstraint(7,3);
		AddHardConstraint(8,11);
		AddHardConstraint(19,1);
		AddHardConstraint(13,11);


//		AddHardConstraint(6,21);
//		AddHardConstraint(7,21);
//		AddHardConstraint(9,21);
//		AddHardConstraint(10,21);
//		AddHardConstraint(11,21);
//		AddHardConstraint(12,21);
//		AddHardConstraint(14,21);
//		AddHardConstraint(15,21);

		// body
		AddHardConstraint(4,5);
		AddHardConstraint(4,16);
		AddHardConstraint(4,3);
		AddHardConstraint(4,2);
		AddHardConstraint(4,1);
		AddHardConstraint(4,0);
		AddHardConstraint(4,20);
		AddHardConstraint(4,17);
		AddHardConstraint(3,2);
		AddHardConstraint(3,17);
		AddHardConstraint(3,18);
		AddHardConstraint(3,20);
		AddHardConstraint(2,1);
		AddHardConstraint(2,17);
		AddHardConstraint(2,18);
		AddHardConstraint(2,20);
		AddHardConstraint(1,0);
		AddHardConstraint(1,17);
		AddHardConstraint(1,18);
		AddHardConstraint(1,20);
		AddHardConstraint(0,20);
		AddHardConstraint(0,17);
		AddHardConstraint(0,18);
		AddHardConstraint(20,18);
		AddHardConstraint(20,19);
		AddHardConstraint(18,17);
		AddHardConstraint(18,19);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);
//		AddHardConstraint(,);




		GetComponent<MeshFilter> ().mesh.triangles = triangles;

		GetComponent<MeshRenderer> ().material = newMatertial;

		// set the original positions of the vertices and set the old positions of the vertices
		for (int i = 0; i < numberOfParticles; i++) {
			m_oldx [i] = m_x [i] + new Vector3(0.08f,0,0);
		}

		//		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		//		newMatertial.color = c1;
		//		lineRenderer.material = newMatertial;
		//		lineRenderer.widthMultiplier = 0.1f;
		//		lineRenderer.numPositions = lengthOfLineRenderer;
	}

	private void AddHardConstraint(int start, int end) {
		Constraint constraint;
		constraint.a = start;
		constraint.b = end;
		constraint.restLength = (m_x [start] - m_x [end]).sqrMagnitude;
		constraints.Add (constraint);
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
		for (int j = 0; j < 5; j++) {
			for (int i = 0; i < numberOfParticles; i++) {
				m_x [i] = Vector3.Min (Vector3.Max (m_x [i], new Vector3 (-4, -4, 0)), new Vector3 (5, 5, 0));
			}

			foreach (Constraint c in constraints) {
				// get the distance between the points of the verlet goat
				Vector3 delta = m_x [c.a] - m_x [c.b];
				delta *= c.restLength / (Vector3.Dot (delta, delta) + c.restLength) - 0.5f;
				m_x [c.b] -= delta;
				m_x [c.a] += delta;
			}
		}

		GetComponent<MeshFilter> ().mesh.vertices = m_x;
		GetComponent<MeshFilter> ().mesh.triangles = triangles;
	}
}
