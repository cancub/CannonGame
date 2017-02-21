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
	private GameObject hornLine;
	private GameObject beardLine;
	private GameObject tailLine;
	private GameObject frontLegLine;
	private GameObject backLegLine;
	private GameObject goatOutline;
	private GameObject eye;
	private Vector2[] groundVertices;
	private Vector2[,] mountainVertices;
	private GameController controller;




	void Start() {
		
		numberOfParticles = 26;
		m_x = new Vector3[numberOfParticles];
		m_oldx = new Vector3[numberOfParticles];
		m_a = new Vector3[numberOfParticles];
		m_vGravity = Vector3.down * 10f;
		constraints = new List<Constraint> ();
		controller = (GameController) GameObject.Find ("Game Controller").GetComponent<GameController> ();
		groundVertices = GameObject.Find ("Ground").GetComponent<GroundController> ().GetVertices ();
		mountainVertices = GameObject.Find ("Mountain").GetComponent<MountainGenerator> ().GetVertices ();
		DrawGoat ();



	}

	void Update() {
		AccumulateForces ();
		Verlet ();
		SatisfyConstraints ();
		CheckCollisions ();
		if (m_x[0].y < -5.2f || Mathf.Abs (m_x[0].x + transform.position.x) > 11.5f) {
			Destroy (gameObject);
		}

		DrawLines ();

//		LineRenderer lineRenderer = GetComponent<LineRenderer>();
//		var t = Time.time;
//		for(int i = 0; i < lengthOfLineRenderer; i++) {
//			lineRenderer.SetPosition(i, new Vector3(i * 0.5f, Mathf.Sin(i + t), -2.0f));
//		}
	}

	private void DrawGoat () {

		Material newMatertial =  new Material (Shader.Find("Unlit/Color"));
		newMatertial.color = Color.white;

		// get the initial rotation of the goat based on the cannon's spawn point
		Quaternion rotation = transform.rotation;

		// reset the rotation so that the goat's frame of reference is oriented properly
		transform.rotation = new Quaternion(0,0,0,1);

		Matrix4x4 m = Matrix4x4.TRS (new Vector3 (0, 0, 0), rotation, new Vector3 (1, 1, 1));

		// but save the original rotation as a rotation matrix to move the points of the mesh

		// make the vertices that will make the mesh for the goat
		m_x = new Vector3[numberOfParticles];
		m_x[0] = m.MultiplyPoint3x4(new Vector3 (3,-1,0.5f) * scale);
		m_x[1] = m.MultiplyPoint3x4( new Vector3 (2.2f,-1,0.5f)* scale);
		m_x[2] = m.MultiplyPoint3x4( new Vector3 (0.6f,-1,0.5f)* scale);
		m_x[3] = m.MultiplyPoint3x4( new Vector3 (0,-1,0.5f)* scale);
		m_x[4] = m.MultiplyPoint3x4( new Vector3 (-1.2f,0.2f,0.5f)* scale);
		m_x[5] = m.MultiplyPoint3x4( new Vector3 (-1.4f,0.4f,0.5f)* scale);
		m_x[6] = m.MultiplyPoint3x4( new Vector3 (-1.6f,0.6f,0.5f)* scale);
		m_x[7] = m.MultiplyPoint3x4( new Vector3 (-2.4f,0.6f,0.5f)* scale);
		m_x[8] = m.MultiplyPoint3x4( new Vector3 (-2.6f,0,0.5f)* scale);
		m_x[9] = m.MultiplyPoint3x4( new Vector3 (-2.8f,0.6f,0.5f)* scale);
		m_x[10] = m.MultiplyPoint3x4( new Vector3 (-3.2f,0.6f,0.5f) * scale);
		m_x[11] = m.MultiplyPoint3x4( new Vector3 (-3,1.2f,0.5f) * scale);
		m_x[12] = m.MultiplyPoint3x4( new Vector3 (-1.8f,2.2f,0.5f) * scale);
		m_x[13] = m.MultiplyPoint3x4( new Vector3 (-1.2f,2.4f,0.5f) * scale);
		m_x[14] = m.MultiplyPoint3x4( new Vector3 (-1.6f,1.8f,0.5f) * scale);
		m_x[15] = m.MultiplyPoint3x4( new Vector3 (-1.2f,1.2f,0.5f) * scale);
		m_x[16] = m.MultiplyPoint3x4( new Vector3 (-1,1,0.5f) * scale);
		m_x[17] = m.MultiplyPoint3x4( new Vector3 (-0.6f,1,0.5f) * scale);
		m_x[18] = m.MultiplyPoint3x4( new Vector3 (2.6f,1,0.5f) * scale);
		m_x[19] = m.MultiplyPoint3x4( new Vector3 (2.8f,1.4f,0.5f) * scale);
		m_x[20] = m.MultiplyPoint3x4( new Vector3 (3,1,0.5f) * scale);
		m_x[21] = m.MultiplyPoint3x4( new Vector3 (-2,1.6f,0.5f) * scale); // eye
		m_x[22] = m.MultiplyPoint3x4( new Vector3 (0.6f,-1.8f,0.5f) * scale); // leg1
		m_x[23] = m.MultiplyPoint3x4( new Vector3 (0.6f,-2.6f,0.5f) * scale); // leg1
		m_x[24] = m.MultiplyPoint3x4( new Vector3 (2.2f,-1.8f,0.5f) * scale); // leg2
		m_x[25] = m.MultiplyPoint3x4( new Vector3 (2.2f,-2.6f,0.5f) * scale); // leg2

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

		AddConstraints (6, 7, 17);
		AddConstraints (7, 8, 17);
		AddConstraint (8, 12);
		AddConstraints (9, 8, 17);
		AddConstraints (10, 8, 17);
		AddConstraints (11, 12, 17);
		AddConstraints (12, 13, 17);
		AddConstraints (14, 13, 17);
		AddConstraints (15, 16, 17);

		// body
		AddConstraints(0,1,5);
		AddConstraints(0,16,18);
		AddConstraint(0,20);
		AddConstraints(1,0,5);
		AddConstraints(1,16,18);
		AddConstraint(1,20);
		AddConstraints(2,1,5);
		AddConstraints(2,16,18);
		AddConstraint (2, 20);
		AddConstraints(3,2,5);
		AddConstraints(3,16,18);
		AddConstraint (3, 20);
		AddConstraints(4,1,5);
		AddConstraints(4,16,18);
		AddConstraint (4, 20);
		AddConstraints (5, 4, 6);
		AddConstraints (5, 15, 17);
		AddConstraints (16, 15, 17);
		AddConstraint (17, 18);
		AddConstraints (18, 19, 20);
		AddConstraints (19, 18, 20);
		AddConstraint (19, 0);

		AddConstraints (21, 7, 14);

		AddConstraint (22, 2);
		AddConstraint (22, 23);

		AddConstraint (24, 1);
		AddConstraint (24, 25);



//		AddConstraint(6,21);
//		AddConstraint(7,21);
//		AddConstraint(9,21);
//		AddConstraint(10,21);
//		AddConstraint(11,21);
//		AddConstraint(12,21);
//		AddConstraint(14,21);
//		AddConstraint(15,21);






		GetComponent<MeshFilter> ().mesh.triangles = triangles;

		GetComponent<MeshRenderer> ().material = newMatertial;

		// set the original positions of the vertices and set the old positions of the vertices
		for (int i = 0; i < numberOfParticles; i++) {
			// give the head some speed
			if (i >= 4 && i <= 17) {
				m_oldx [i] = m_x [i] + m.MultiplyPoint3x4 (new Vector3 (0.43f, 0, 0));
			} else {
				m_oldx [i] = m_x [i];
			}
		}
			
		// make the lines that will show off the horn, beard and tail
		beardLine = new GameObject("beard");
		hornLine = new GameObject("horn");
		tailLine = new GameObject("tail");
		frontLegLine = new GameObject("front leg");
		backLegLine = new GameObject("back leg");
		goatOutline = new GameObject("outline");
		eye = new GameObject ("eye");


		beardLine.transform.parent = transform;
		hornLine.transform.parent = transform;
		tailLine.transform.parent = transform;
		frontLegLine.transform.parent = transform;
		backLegLine.transform.parent = transform;
		goatOutline.transform.parent = transform;
		eye.transform.parent = transform;

		beardLine.AddComponent<LineRenderer> ();
		hornLine.AddComponent<LineRenderer> ();
		tailLine.AddComponent<LineRenderer> ();
		frontLegLine.AddComponent<LineRenderer> ();
		backLegLine.AddComponent<LineRenderer> ();
		goatOutline.AddComponent<LineRenderer> ();
		eye.AddComponent<LineRenderer> ();


		Material lineMatertial =  new Material (Shader.Find("Unlit/Color"));
		lineMatertial.color = Color.black;

		beardLine.GetComponent<LineRenderer> ().material = lineMatertial;
		hornLine.GetComponent<LineRenderer> ().material = lineMatertial;
		tailLine.GetComponent<LineRenderer> ().material = lineMatertial;
		frontLegLine.GetComponent<LineRenderer> ().material = lineMatertial;
		backLegLine.GetComponent<LineRenderer> ().material = lineMatertial;
		goatOutline.GetComponent<LineRenderer> ().material = lineMatertial;
		eye.GetComponent<LineRenderer> ().material = lineMatertial;

		beardLine.GetComponent<LineRenderer> ().numPositions = 2;
		hornLine.GetComponent<LineRenderer> ().numPositions = 2;
		tailLine.GetComponent<LineRenderer> ().numPositions = 2;
		eye.GetComponent<LineRenderer> ().numPositions = 2;
		frontLegLine.GetComponent<LineRenderer> ().numPositions = 3;
		backLegLine.GetComponent<LineRenderer> ().numPositions = 3;
		goatOutline.GetComponent<LineRenderer> ().numPositions = 22;

		DrawLines ();





		//		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		//		newMatertial.color = c1;
		//		lineRenderer.material = newMatertial;
		//		lineRenderer.widthMultiplier = 0.1f;
		//		lineRenderer.numPositions = lengthOfLineRenderer;
	}

	private void AddConstraint(int start, int end) {
		Constraint constraint;
		constraint.a = start;
		constraint.b = end;
		constraint.restLength = (m_x [start] - m_x [end]).sqrMagnitude;
		constraints.Add (constraint);
	}

	private void AddConstraints(int start, int first, int last) {
		bool constraintExists;
		for (int i = first; i <= last; i++) {
			if (i != start) {
				constraintExists = false;
				foreach (Constraint c in constraints) {
					if ((c.a == start && c.b == i) || (c.b == start && c.a == i)) {
						constraintExists = true;
						break;
					}
				}

				if (!constraintExists) {
					AddConstraint (start, i);
				}
			}
		}
	}

	private void DrawLines() {
		float lineWidth = 0.007f;
		Vector3 position = beardLine.transform.parent.position + new Vector3(0,0,-0.5f);
		LineRenderer line = beardLine.GetComponent<LineRenderer> ();
		line.SetPosition (0, m_x [9] + position);
		line.SetPosition (1, m_x [7] + position);
		line.startWidth = lineWidth;
		line.endWidth = lineWidth;

		line = hornLine.GetComponent<LineRenderer> ();
		line.SetPosition (0, m_x [12] + position);
		line.SetPosition (1, m_x [14] + position);
		line.startWidth = lineWidth;
		line.endWidth = lineWidth;

		line = tailLine.GetComponent<LineRenderer> ();
		line.SetPosition (0, m_x [18] + position);
		line.SetPosition (1, m_x [20] + position);
		line.startWidth = lineWidth;
		line.endWidth = lineWidth;

		line = eye.GetComponent<LineRenderer> ();
		line.SetPosition (0, m_x [21] + position);
		line.SetPosition (1, m_x [21] + position + new Vector3(lineWidth*2,lineWidth*2,0));
		line.startWidth = lineWidth*2;
		line.endWidth = lineWidth*2;

		line = frontLegLine.GetComponent<LineRenderer> ();
		line.SetPosition (0, m_x [2] + position);
		line.SetPosition (1, m_x [22] + position);
		line.SetPosition (2, m_x [23] + position);
		line.startWidth = lineWidth;
		line.endWidth = lineWidth;

		line = backLegLine.GetComponent<LineRenderer> ();
		line.SetPosition (0, m_x [1] + position);
		line.SetPosition (1, m_x [24] + position);
		line.SetPosition (2, m_x [25] + position);
		line.startWidth = lineWidth;
		line.endWidth = lineWidth;

		line = goatOutline.GetComponent<LineRenderer> ();
		for (int i = 0; i < 22; i++) {
			line.SetPosition (i, m_x [i % 21] + position);
		}
		line.startWidth = lineWidth;
		line.endWidth = lineWidth;


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
//			Vector3 oldx = m_oldx [i];
//			Vector3 a = m_a [i];
			m_x [i] =  1.9999f * m_x [i] - m_oldx [i] + m_a [i] * Time.deltaTime * Time.deltaTime;
			m_x [i].x += controller.GetWindSpeed () * 0.01f;
			m_oldx [i] = temp;
		}
		GetComponent<MeshFilter> ().mesh.vertices = m_x;
		GetComponent<MeshFilter> ().mesh.triangles = triangles;
	}

	private void SatisfyConstraints() {
		for (int j = 0; j < 20; j++) {
//			for (int i = 0; i < numberOfParticles; i++) {
//				m_x [i] = Vector3.Min (Vector3.Max (m_x [i], new Vector3 (-20, -20, -4)), new Vector3 (5, 5, 5));
//			}

			foreach (Constraint c in constraints) {
				// get the distance between the points of the verlet goat
				Vector3 delta = m_x [c.a] - m_x [c.b];
				delta *= c.restLength / (Vector3.Dot (delta, delta) + c.restLength) - 0.5f;
				m_x [c.b] -= delta;
				m_x [c.a] += delta;
			}
		}


		GetComponent<MeshFilter> ().mesh.vertices = CopyVerletMesh(m_x);
		GetComponent<MeshFilter> ().mesh.triangles = triangles;
	}

	private Vector3[] CopyVerletMesh(Vector3[] verletVertices) {
		Vector3[] meshVertices = new Vector3[21];
		for (int i = 0; i < 21; i++) {
			meshVertices [i] = verletVertices [i];
		}
		return meshVertices;
	}

	private void CheckCollisions () {
		// check to see if any part of the goat collided with the ground
		Vector3 position = beardLine.transform.parent.position + new Vector3(0,0,-0.5f);
		Vector2[] goatVertices =  new Vector2[3];

		for (int i = 0; i < triangles.Length; i+= 3) {
			// get the vertices for triangle of the goat
			goatVertices[0] = m_x[triangles[i]] + position;
			goatVertices[1] = m_x[triangles[i+1]] + position;
			goatVertices[2] = m_x[triangles[i+2]] + position;

			if (GTK (groundVertices, goatVertices)) {
				Destroy (gameObject);
			}
		}
	}

	private bool GTK (Vector2[] colliderA, Vector2[] colliderB) {
		// the Gilbert-Johnson-Keerthi algorithm to determine if a collision
		// occured by checking if the minkowski difference between the goat
		// and an object contains the origin

		// we already have a list of the triangles that we need to test,
		// so we just need to go through them one by one and compare to the collider object

		// we have 21 vertices that make up the goat's mesh
		Vector2[] goatVerts = new Vector2[21];

		for (int i = 0; i < 21; i++) {
			goatVerts [i] = (Vector2)(m_x[i]);
		}


		// the simplex will have points added and removed often, so make it a list
		List<Vector2> simplex = new List<Vector2> ();

		// take as the start direction the vector between the midpoints
		Vector2 d = GetCenter(colliderB) - GetCenter(colliderA);

		// add the first point to the simplex that is the furthest point in the MD along d
		simplex.Add(support(colliderB,colliderA,d));

		// for the next point (in the loop), we want to check in the opposite direction
		d *= -1;

		// begin the algorithm
		while (true) {
			// we haven't determined if the origin is in the MD, so get the next point 
			// for the simplex
			simplex.Add(support(colliderB,colliderA,d));

			// check to see if this last point added to the simplex was indeed past the origin
			if (Vector2.Dot (simplex [simplex.Count - 1], d) <= 0) {
				// the point was not past the origin and therefor we can stop because
				// this point is on the surface of the MD and thus the MD does not
				// contain the origin

				// may need to check the case of == 0 where the origin lies on the line

				return false;
			} else {
				// keep moving to determine if the simplex contains the origin or if we 
				// need to do more work
				// check if the origin is contained by the simplex
				if (ContainsOrigin(ref simplex,ref d)) {
					// there was a collision
					return true;
				}

				// otherwise, keep looping with the new direction. The simplex and the
				// search direction vector d have been updated
			}
		}

	}

//	private bool GTK (Vector2[,] mountainCollider) {
//		/* this does something much different than the GTK for the ground.
//		 * we know that this is a whole mountian and we want to save processing time, so
//		 * in this function, we will look specifically at the subsurface of the mountain that 
//		 * are within range of the ball. This is to say that we won't consider surfaces on the
//		 * right hand side if we are on the left and we won't consider surfaces at the top of the 
//		 * mountain if we are at the bottom. Once we figure out a collision has occured, create a bounce 
//		 */
//		Vector2[] test = new Vector2[3];
//		for (int i = 0; i < mountainCollider.GetLength (0); i++) {
//			test [0] = mountainCollider [i, 0];
//			test [1] = mountainCollider [i, 1];
//			test [2] = mountainCollider [i, 2];
//			if (GTK (test)) {
//
//				// find the vector pointing from this current position to the last known
//				// non-collision position
//				Vector3 movementVector = lastGoodPosition - transform.position;
//				// get the unit vector
//				movementVector /= movementVector.magnitude;
//
//				// move the ball back an incremental amount in comparison to the radius
//				transform.Translate(movementVector * radius * 0.1f,Space.World);
//
//				while (GTK (test)) {
//					// keep doing so until there is no longer a collision
//					transform.Translate(movementVector * radius * 0.1f,Space.World);
//				}
//
//				// get the norm of the surface
//				Vector2 temp = test[1] - test[0];
//				Vector2 norm = new Vector2 (-temp.y, temp.x) / temp.magnitude;
//
//				// get the directional vector for the ball
//				Vector2 velocity = horizontalMovement + verticalMovement;
//
//				// now use the norm to reflect the direction and take something off of the speed
//				velocity = restitution * (-2f * (Vector2.Dot(velocity,norm)*norm) + velocity);
//
//				horizontalMovement.x = velocity.x;
//				verticalMovement.y = velocity.y;
//
//				//				Destroy (gameObject);
//				break;
//			}
//		}
//
//		// there was no collision so this was a good position
//		lastGoodPosition = transform.position;
//
//		return false;
//	}

	private Vector2 GetCenter(Vector2[] vertices) {
		Vector2 ret = new Vector2 (0, 0);
		foreach (Vector2 vert in vertices) {
			ret += vert;
		}
		return ret / vertices.Length;


	}

	private Vector2 support(Vector2[] A, Vector2[] B, Vector2 d) {
		// we want to find max(d*(a-b)) = max(d*a - d*b) = max(d*a)-max(-d*b)

		// get the maximum point in shape A along d
		Vector2 a = GetMaxPointInDirection(A,d);
		// get the minium point in shape B along d (maximum along -d)
		Vector2 b = GetMaxPointInDirection (B, -1*d);

		// find the Minkowski difference to get a vertex on the convex hull of the 
		// difference between the two shapes
		return a - b;
	}

	private bool ContainsOrigin(ref List<Vector2>simplex, ref Vector2 d) {
		// we want to start by obtaining the last point to be added to the simplex
		Vector2 a = simplex[simplex.Count -1];

		// find the vector from a to the origin to test for region and find a new search direction
		Vector2 a0 = -1 * a;

		// only a polygon can "contain" the origin, so check to see if that's what
		// we have

		if (simplex.Count == 3) {
			// we have a polygon, test to see which region the origin lies in.
			// first we need to find the edges that define the triangle
			Vector3 c = simplex [0];

			// find the edges
			Vector2 ab = simplex [1] - a;
			Vector2 ac = simplex [0] - a;

			// we don't need to look at bc because we already performed a test on this vector 
			// for the location of the origin and that's how we found "a"

			// get the normals for each of these vectors pointing away from the center of the simplex
			Vector2 abNorm = TripleProduct (ac, ab, ab);
			Vector2 acNorm = TripleProduct (ab, ac, ac);

			// test if the origin lies beyond ab
			if (Vector2.Dot (abNorm, a0) > 0) {
				// point c is no longer needed then
				simplex.RemoveAt (0);
				// and the new search direction should be the norm of ab
				d = abNorm;
			} else if (Vector2.Dot (acNorm, a0) > 0) {
				// the point b is no longer needed
				simplex.RemoveAt (1);
				// and the new search direction should be the norm of ac 
				d = acNorm;
			} else {
				// neither case is true (and we already know that the origin is not beyond bc) so
				// the triangle does indeed contain the origin
				return true;
			}
		} else {
			// the simplex is only a line at this point, so update the direction to seach in to
			// be the norm of ab in the direction of the origin
			Vector2 ab = simplex[0] - a;

			d = TripleProduct (ab, a0, ab);
		}

		return false;

	}

	private Vector2 GetMaxPointInDirection(Vector2[] shapeVertices, Vector2 d) {
		//  as part of the GJK algorithm, find the point in a shape furthest in the direction of
		// a vector d

		// get some initial values
		int maxIndex = 0;
		float maxVal = Vector2.Dot(shapeVertices[maxIndex], d);

		float testVal;

		for (int i = 1; i < shapeVertices.Length; i++) {
			testVal = Vector2.Dot (shapeVertices [i], d);

			if (testVal > maxVal) {
				maxVal = testVal;
				maxIndex = i;
			}
		}

		return shapeVertices [maxIndex];

	}

	private Vector2 TripleProduct(Vector2 a, Vector2 b, Vector2 c) {
		// use the triple product expansion to find the triple product (a x b) x c

		return b * (Vector2.Dot (c, a)) - a * (Vector2.Dot (c, b));
	}
}
