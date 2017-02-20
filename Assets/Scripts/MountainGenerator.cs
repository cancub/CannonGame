using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainSide {
	public Mesh mesh;
	public List<Vector3> surfaceVertices;

	public MountainSide() {
		mesh = new Mesh ();
		surfaceVertices = new List<Vector3> ();
	}
}

public class MountainGenerator : MonoBehaviour {
	public int bisectionDepth;
	public float bendiness;
	public float width;
	public float height;
	private GameObject mountainParent;
	private GameObject[] mountObjects;
	MountainSide rightMount;
	MountainSide leftMount;

	// Use this for initialization
	void Start () {
		Random.InitState ((int)System.DateTime.Now.Ticks);

		mountObjects = new GameObject[2];

		mountObjects [0] = new GameObject ("Left Facade");
		mountObjects [0].AddComponent<MeshFilter>();
		mountObjects [0].AddComponent <MeshRenderer>();
		mountObjects [0].GetComponent<MeshRenderer> ().material = gameObject.GetComponent<MeshRenderer>().material;
		mountObjects [0].transform.SetParent (gameObject.transform);

		mountObjects [1] = new GameObject ("Right Facade");
		mountObjects [1].AddComponent<MeshFilter>();
		mountObjects [1].AddComponent <MeshRenderer>();
		mountObjects [1].GetComponent<MeshRenderer> ().material = gameObject.GetComponent<MeshRenderer>().material;
		mountObjects [1].transform.SetParent (gameObject.transform);

		// the extreme vertices of the mountain
		Vector3 topRight = new Vector3 (1,-4 + height,0);
		Vector3 bottomRight = new Vector3 (width, -4, 0);
		Vector3 topLeft = new Vector3 (-1,-4 + height,0);
		Vector3 bottomLeft = new Vector3 (-width, -4, 0);

		// get the sides of the mountain
		rightMount = GenerateMountainside (topRight, bottomRight, "right");
		leftMount = GenerateMountainside (topLeft, bottomLeft, "left");

		// save the mesh so that it may be displayed
		mountObjects [0].GetComponent<MeshFilter> ().mesh = rightMount.mesh;
		mountObjects [1].GetComponent<MeshFilter> ().mesh = leftMount.mesh;

	}

	private List<Vector3> MidpointBisect(Vector3 vertex1, Vector3 vertex2, int depthRemaining) {
		/*
		 * A function which uses recursion to build a 2D mountainside. 
		 * inputs: vertex1, vertex2: the start and end points of this section of the mountain
		 * 			depthRemainin: how many times to recur after this midpoint bisection
		 * output: a list of the vertices found through midpoint bisection
		 */

		// create the list that will be returned
		List<Vector3> vertices = new List<Vector3>();

		// find the midpoint of the line that is described by vertex1 and vertex 2
		Vector3 midpoint  = (vertex1 + vertex2) / 2f;

		// find the norm of the line
		Vector3 norm = GetNorm(vertex1,vertex2);

		// find the length of the vector to know how far to move the midpoint along the norm
		float length = (vertex1 - vertex2).magnitude;

		// get a random value to push the midpoint that is in [-length/10,length/10]
		float displacement = Random.value * length/(bendiness/2) - length/bendiness;

		// determine where the new vertex will be located
		Vector3 newMidpoint = midpoint + norm * displacement;

		// add this new location to the list of vertices
		vertices.Add(newMidpoint);

		// recur if there is still more levels to work with
		if (depthRemaining > 0) {
			vertices.AddRange (MidpointBisect (vertex1, newMidpoint, depthRemaining - 1));
			vertices.AddRange (MidpointBisect (newMidpoint, vertex2, depthRemaining - 1));
		}

		// return the vertices so that they may be used to display the surface
		return vertices;
	}

	private Vector3 GetNorm(Vector3 vertex1, Vector3 vertex2) {
		Vector3 line = vertex1 - vertex2;
		float length = (vertex1 - vertex2).magnitude;
		return new Vector3 (-line.y, line.x, 0) / length;
	}

	private MountainSide GenerateMountainside(Vector3 vertex1,Vector3 vertex2, string side = "right") {
		/*
		 * inputs are the start and end verices to describe the facade of a mountain and the
		 * keyword side to state which side of the mountain it is. Because triangles are shown to the 
		 * camera based on the clockwise/counterclockwise listing of vertices, it is imperative to know
		 * which side of the mountian is being generated		 
		 * 
		 * output is a list of the vertices that describe the surface of the mountainside, will be used
		 * for collision detection
		 * 
		 * the process here is to find and add the exterior vertices then the vertices that can be used
		 * to complete the triangles on the surface. Following this, the vertices that can be used to 
		 * complete the triangles to fill out the interior of the mountain are added. Once all the vertices
		 * are found, the triangles are added to a mesh in a method that looks at the orientation of the
		 * mountainside as well as the relationship between edge, surface-completing and interior-completing
		 * vertices
		 */ 


		MountainSide mount = new MountainSide ();

		// get the vertices from midpoint bisection
		List<Vector3> vertexList = MidpointBisect (vertex1, vertex2, bisectionDepth);

		// add the original vertices to the list
		vertexList.Add (vertex1);
		vertexList.Add (vertex2);

		// sort the vertices in order of descending height
		vertexList.Sort ((a, b) => b.y.CompareTo (a.y));

		// these are the vertices that will define the shape of the mountain
		mount.surfaceVertices = vertexList.GetRange(0,vertexList.Count);

		// this value helps in determining the number of triangles that will be used
		int length = vertexList.Count;

		// this covers the triangles on the surface and the triangles that make up the squares to fill in the half mountain
		int numberOfTriangles = (length - 1)*3;
		int numberOfSurfaceTriangles = (length - 1);

		// add the vertices that will complete the triangles that make up the surface
		for (int i = 1; i < length; i++) {
			if (side.Equals ("right")) {
				if (vertexList [i].x < vertexList [i - 1].x) {
					// this line is actually pointing inwards toward the mountain's centre, so the completing vertex will be level
					// with the top vertex of this line
					vertexList.Add (new Vector3 (vertexList [i].x, vertexList [i - 1].y, 0));
				} else {
					// this line is pointing away from the mountain's centre, so the completing vertex is level with the
					// bottom vertex of this line
					vertexList.Add (new Vector3 (vertexList [i - 1].x, vertexList [i].y, 0));
				}
			} else {
				if (vertexList [i].x < vertexList [i - 1].x) {
					// this line is actually pointing inwards toward the mountain's centre, so the completing vertex will be level
					// with the top vertex of this line
					vertexList.Add (new Vector3 (vertexList [i - 1].x, vertexList [i].y, 0));

				} else {
					// this line is pointing away from the mountain's centre, so the completing vertex is level with the
					// bottom vertex of this line
					vertexList.Add (new Vector3 (vertexList [i].x, vertexList [i - 1].y, 0));
				}
			}
		}

		// use this value to keep track of which vertex to start with when filling the interior triangles 
		int firstInteriorVertex = vertexList.Count;

		// add the final vertices that account for the middle line of the mountain
		for (int i = 0; i < length; i++) {
			vertexList.Add (new Vector3 (0, vertexList [i].y, 0));
		}

		Vector3[] vertices = vertexList.ToArray();
		mount.mesh.vertices = vertices;


		int[] triangles = new int[numberOfTriangles*3];
		int j = 0;

		// create the triangles that make up the exterior of the mountain (one third of the triangles)
		for (int i = 0; i < numberOfSurfaceTriangles; i++) {
//			print (j.ToString ());
			if (side.Equals ("right")) {
				triangles [j] = i;
				triangles [j + 1] = i + 1;
				triangles [j + 2] = i + numberOfSurfaceTriangles + 1;
			} else {
				triangles [j] = i;
				triangles [j + 1] = i + numberOfSurfaceTriangles + 1;
				triangles [j + 2] = i + 1;
			}

//			print (triangles [j].ToString() + "," + triangles [j + 1].ToString () + "," + triangles [j + 2].ToString ());
			j += 3;
		}

		// create the triangles that make up the interior of the mountain (one third * 2 of the triangles)
		for (int i = firstInteriorVertex; i < vertexList.Count-1; i++) {
			//			print (j.ToString ());
			if (vertices [i].y.Equals (vertices [i - (firstInteriorVertex - 1) / 2].y)) {
				// this is a layer for an inward facing edge triangle
				if (side.Equals ("right")) {
					// this is on the right side, so use this specific order to add triangle vertices
					triangles [j] = i;
					triangles [j + 1] = i - (firstInteriorVertex - 1) / 2;
					triangles [j + 2] = i - (firstInteriorVertex - 1);

					triangles [j + 3] = i;
					triangles [j + 4] = i - (firstInteriorVertex - 1);
					triangles [j + 5] = i + 1;
				} else {
					// this is on the left side, so reverse the order of the second and third vertex in each triangle to maintain
					// a clockwise rotation
					triangles [j] = i;
					triangles [j + 1] = i - (firstInteriorVertex - 1);
					triangles [j + 2] = i - (firstInteriorVertex - 1) / 2;

					triangles [j + 3] = i;
					triangles [j + 4] = i + 1;
					triangles [j + 5] = i - (firstInteriorVertex - 1);
				}
					
			} else {
				// this is a layer for an outward facing edge triangle
				if (side.Equals ("right")) {
					// this is on the right side, so use this specific order to add triangle vertices
					triangles [j] = i;
					triangles [j + 1] = i - firstInteriorVertex;
					triangles [j + 2] = i - (firstInteriorVertex - 1) / 2;

					triangles [j + 3] = i;
					triangles [j + 4] = i - (firstInteriorVertex - 1) / 2;
					triangles [j + 5] = i + 1;

				} else {
					// this is on the left side, so reverse the order of the second and third vertex in each triangle to maintain
					// a clockwise rotation
					triangles [j] = i;
					triangles [j + 1] =  i - (firstInteriorVertex - 1) / 2;
					triangles [j + 2] = i - firstInteriorVertex;

					triangles [j + 3] = i;
					triangles [j + 4] = i + 1;
					triangles [j + 5] = i - (firstInteriorVertex - 1) / 2;
				}
			}

			j += 6;
		}

		mount.mesh.triangles = triangles;

		return mount;
	}

	public Vector2[,] GetVertices() {
		/*
		 * This function will return an (n-1) x 3 multidemensional array of vertices,
		 * where n = the number of vertices that define one slope of the mountian and 3
		 * is the number of vertices to describe each triangle made by the mountain
		 */

		Vector2[,] ret = new Vector2[leftMount.surfaceVertices.Count * 2 -1,3];

		// we know that the vertices for each side have been sorted from largest to smallest y
		// and we can use this to our advantage. Rather than testing all possible triangles for a collision,
		// we can help the object focus on solely the triangles in the same height range and same side of the playing field.
		// we can do this by ensuring that that the order of the triangles goes like so:
		// - first triangle always describes the top surface of the mountiain
		// - after this:
		// 		- the first triangle is the triangle on the left hand side of the mountiain just below the top
		//		- the next triangle is the triangle on the right hand side of the mountain just below the top
		// 		- repeat until all triangles have been added
		// Of MASSIVE importance is that the first two vertices listed for each triangle will be the vertices that
		// are the line for the surface, and these move in a clockwise manner to add uniforimty when attempting to
		// find the normal for bouncing. To do this, we need to make the assumption that the number of vertices
		// is equal on each side of the mountain

		int j = 0;
		// find the top triangle
		ret[j,0] = leftMount.surfaceVertices[0];
		ret[j,1] = rightMount.surfaceVertices[0];
		// use the point just a bit below the midpoint of these vertices as the vertex to complete the triangle
		ret[j,2] = new Vector2(0,leftMount.surfaceVertices[0].y - 0.3f);

		j++;
		Vector2 test;

		// now work our way from the top town 
		for(int i  = 0; i < leftMount.surfaceVertices.Count-1; i++) {

			// keep a clockwise fashion for the surface vertices
			ret[j,0] = leftMount.surfaceVertices[i+1];
			ret[j,1] = leftMount.surfaceVertices[i];
			// just get a midpoint slightly behind the surface at the midpoint
			test = ret[j,0] + ret[j,1];
			ret[j,2] = (ret[j,0] + ret[j,1])/2 + Vector2.right * 0.3f;
			j++;

			// keep a clockwise fashion for the surface vertices
			ret[j,0] = rightMount.surfaceVertices[i];
			ret[j,1] = rightMount.surfaceVertices[i+1];
			test = ret[j,0] + ret[j,1];
			// just get a midpoint slightly behind the surface at the midpoint
			ret[j,2] = (ret[j,0] + ret[j,1])/2 + Vector2.left* 0.3f;
			j++;
		}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               

		return ret;
	}
}
 