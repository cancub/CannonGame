using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallController : MonoBehaviour {

	public float radius;
	public float initialSpeed;
	public float gravity;
	public float drag;
	public float restitution;
	private Vector3 horizontalMovement;
	private Vector3 verticalMovement;
	private GameController controller;
	private Vector2[] groundVertices;
	private Vector2[,] mountainVertices;
	// Use this for initialization
	void Start () {

		controller = (GameController) GameObject.Find ("Game Controller").GetComponent<GameController> ();

		groundVertices = GameObject.Find ("Ground").GetComponent<GroundController> ().GetVertices ();

//		print (transform.eulerAngles.z);
		DrawCannonBall(radius);
		float angle_deg;
		float angle_rad;

		mountainVertices = GameObject.Find ("Mountain").GetComponent<MountainGenerator> ().GetVertices ();

		if (transform.position.x > 0) {
			// the goat cannon fired this
			angle_deg = ((45-transform.eulerAngles.z) % 360f);
			angle_rad = angle_deg*Mathf.PI / 180;
			horizontalMovement = Vector3.left * initialSpeed *Mathf.Cos(angle_rad);
			verticalMovement = Vector3.up * initialSpeed *Mathf.Sin(angle_rad);
		} else {
			// the cannon ball cannon fired this
			angle_deg = ((transform.eulerAngles.z+45) % 360f);
			angle_rad = angle_deg*Mathf.PI / 180;
			horizontalMovement = Vector3.right * initialSpeed *Mathf.Cos(angle_rad);
			verticalMovement = Vector3.up * initialSpeed *Mathf.Sin(angle_rad);
		}
	}

	// Update is called once per frame
	void Update () {

		if (GTK (groundVertices)) {
			Destroy (gameObject);
		} else {
			GTK (mountainVertices);
		}

//		foreach (Vector2[]

		if (transform.position.y < -5.2f || Mathf.Abs (transform.position.x) > 11.5f) {
			Destroy (gameObject);
		} else {
			// move up by a certain amount (value that is affected by gravity)
			transform.Translate (horizontalMovement * Time.deltaTime, Space.World);

			// move over by a certain amount (value that is affected by wind)
			transform.Translate (verticalMovement * Time.deltaTime, Space.World);
		}

		verticalMovement.y -= gravity;

		if (verticalMovement.y > 0) {
			verticalMovement.y -= verticalMovement.y * drag;
		} else {
			verticalMovement.y += verticalMovement.y * drag;
		}

		horizontalMovement.x -= horizontalMovement.x * drag;
		horizontalMovement.x += controller.GetWindSpeed ();

//		if (transform.position.y < groundVertices [0].y) {
//			print (GTK (groundVertices));
//		}


	}

	private void DrawCannonBall (float radius) {

		int numberOfVertices = 12;

		float rads = Mathf.PI * 2 / (float)numberOfVertices;

		List<Vector3> vertices = new List<Vector3> ();
		// add the center vertex
		vertices.Add(new Vector3(0,0,0));

		for (int i = 0; i < numberOfVertices; i++) {
			// add the next vertex along the circle 
			vertices.Add(radius * new Vector3(Mathf.Cos(i*rads),Mathf.Sin(i*rads),0));
//			print (vertices [vertices.Count - 1]);
		}

		GetComponent<MeshFilter> ().mesh.vertices = vertices.ToArray ();

		// now that we have all the vertices of the circle object, we can draw them

		int[] triangles = new int[numberOfVertices*3];

		int j = 0;
		for (int i = numberOfVertices; i > 0; i--) {
			triangles [j] = i;
			if (i > 1) {
				triangles [j + 1] = i - 1;
			} else {
				triangles [j + 1] = numberOfVertices;
			}
			triangles [j + 2] = 0;
//			print (triangles [j].ToString () + ", " + triangles [j + 1] + ", " + triangles [j + 2]);
			j += 3;
		}

		GetComponent<MeshFilter> ().mesh.triangles = triangles;
		//		go.AddComponent<CannonBallController> ();

	}

	private bool GTK (Vector2[] collider) {
		// the Gilbert-Johnson-Keerthi algorithm to determine if a collision
		// occured by checking if the minkowski difference between the cannonball
		// and an object contains the origin

		// better to create an array that contains the vertices of the ball
		// rather than continually making a function call
		Vector3[] vertices = GetComponent<MeshFilter> ().mesh.vertices;
		Vector2[] ballVerts = new Vector2[vertices.Length];

//		Vector2[] MD = new Vector2[vertices.Length * collider.Length];

		for (int i = 0; i < vertices.Length; i++) {
			ballVerts [i] = (Vector2)(vertices [i] + transform.position);
//			for (int j = 0; j < collider.Length; j++) {
//				MD[i*collider.Length + j] = ballVerts[i] - collider [j];
//			}
		}





		// the simplex will have points added and removed often, so make it a list
		List<Vector2> simplex = new List<Vector2> ();

		// take as the start direction the vector between the midpoints
		Vector2 d = (Vector2)ballVerts[0] - GetCenter(collider);

		// add the first point to the simplex that is the furthest point in the MD along d
		simplex.Add(support(ballVerts,collider,d));

		// for the next point (in the loop), we want to check in the opposite direction
		d *= -1;

		// begin the algorithm
		while (true) {
			// we haven't determined if the origin is in the MD, so get the next point 
			// for the simplex
			simplex.Add(support(ballVerts,collider,d));

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

	private bool GTK (Vector2[,] mountainCollider) {
		/* this does something much different than the GTK for the ground.
		 * we know that this is a whole mountian and we want to save processing time, so
		 * in this function, we will look specifically at the subsurface of the mountain that 
		 * are within range of the ball. This is to say that we won't consider surfaces on the
		 * right hand side if we are on the left and we won't consider surfaces at the top of the 
		 * mountain if we are at the bottom. Once we figure out a collision has occured, create a bounce 
		 */
		Vector2[] test = new Vector2[3];
		for (int i = 0; i < mountainCollider.GetLength (0); i++) {
			test [0] = mountainCollider [i, 0];
			test [1] = mountainCollider [i, 1];
			test [2] = mountainCollider [i, 2];
			if (GTK (test)) {
				
				// get the norm of the surface
				Vector2 temp = test[1] - test[0];
				Vector2 norm = new Vector2 (-temp.y, temp.x) / temp.magnitude;

				// get the directional vector for the ball
				Vector2 velocity = horizontalMovement + verticalMovement;

				// now use the norm to reflect the direction and take something off of the speed
				velocity = restitution * (-2f * (Vector2.Dot(velocity,norm)*norm) + velocity);

				horizontalMovement.x = velocity.x;
				verticalMovement.y = velocity.y;

//				Destroy (gameObject);
				break;
			}
		}


		return false;
	}

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
