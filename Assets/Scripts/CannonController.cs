using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour {


	private float middleAngle;
	private float currentAngle;
	public bool isBallCannon;
	public Material wheelMaterial;
	public Material barrelMaterial;
	public GameObject cannonBallPrefab;
	public float minAngle;
	public float maxAngle;
	private GameObject wheel;
	private GameObject barrel;
	private GameObject cannonBallSpawn;


	// Use this for initialization
	void Start () {

		barrel = new GameObject ("barrel");
		barrel.AddComponent<MeshFilter>();
		barrel.AddComponent <MeshRenderer>();
		barrel.GetComponent<MeshRenderer> ().material = barrelMaterial;
		barrel.transform.parent = transform;

		wheel = new GameObject ("wheel");
		wheel.AddComponent<MeshFilter>();
		wheel.AddComponent <MeshRenderer>();
		wheel.GetComponent<MeshRenderer> ().material = wheelMaterial;
		wheel.transform.parent = transform;

		cannonBallSpawn = new GameObject ("projectile spawn");
		cannonBallSpawn.transform.parent = barrel.transform;

		DrawCannon ();
//		middleAngle = barrel.localEulerAngles.z;
//		currentAngle = middleAngle;
//		ResetTimer ();
	}

	private void DrawCannon() {
		// we need to draw both the cannon barrel and the wheel

		// start with the wheel

		// how detailed we want the wheel to be
		List<Vector3> wheelVertices = new List<Vector3> ();
		int wheelVerticesCount = 20;
		float radius = 0.14f;

		int multiple = isBallCannon ? -1 : 1;

		// move the wheel to the position that we'd like it to be
		wheel.transform.position = new Vector3 (multiple * 10f, -4f + radius,-0.2f);

		float rads = Mathf.PI * 2 / (float)wheelVerticesCount;

		// add the center vertex
		wheelVertices.Add(new Vector3(0,0,0));

		for (int i = 0; i < wheelVerticesCount; i++) {
			// add the next vertex along the circle 
			wheelVertices.Add(radius * new Vector3(Mathf.Cos(i*rads),Mathf.Sin(i*rads),0));
			//			print (vertices [vertices.Count - 1]);
		}

		wheel.GetComponent<MeshFilter> ().mesh.vertices = wheelVertices.ToArray ();

		// now that we have all the vertices of the circle object, we can draw them
		int[] wheelTriangles = new int[wheelVerticesCount*3];

		int j = 0;
		for (int i = wheelVerticesCount; i > 0; i--) {
			wheelTriangles [j] = i;
			if (i > 1) {
				wheelTriangles [j + 1] = i - 1;
			} else {
				wheelTriangles [j + 1] = wheelVerticesCount;
			}
			wheelTriangles [j + 2] = 0;
			//			print (triangles [j].ToString () + ", " + triangles [j + 1] + ", " + triangles [j + 2]);
			j += 3;
		}

		wheel.GetComponent<MeshFilter> ().mesh.triangles = wheelTriangles;


		// now draw the barrel, it should just be a rectangle

		// move the barrel to the position that we want
		barrel.transform.position = new Vector3 (multiple * 9.9f, -4f + radius + 0.2f,-0.1f);

//		barrel.transform.position = wheel.transform.position;

		List<Vector3> barrelVertices = new List<Vector3> ();

		// we want this barrel to be just a bit taller than the wheel and about 3 time as long
		barrelVertices.Add(new Vector3(-3*radius,radius*1.2f,0f));
		barrelVertices.Add(new Vector3(3*radius,radius*1.2f,0));
		barrelVertices.Add(new Vector3(-3*radius,-radius*0.9f,0));
		barrelVertices.Add(new Vector3(3*radius,-radius*0.9f,0));

		barrel.GetComponent<MeshFilter> ().mesh.vertices = barrelVertices.ToArray ();

		// now that we have all the vertices of the circle object, we can draw them
		int[] barrelTriangles = new int[6] {0,1,2,1,3,2};



		barrel.GetComponent<MeshFilter> ().mesh.triangles = barrelTriangles;

		// move the spawn point object
		cannonBallSpawn.transform.position += Vector3.left * 2f * radius * multiple + Vector3.up * radius * 0.3f/2f;


		barrel.transform.Rotate (Vector3.back*multiple*45);

		// rotate the spawn point back for the cannonball (the goat is doing its own thing)
		if (isBallCannon) {
			cannonBallSpawn.transform.Rotate (Vector3.forward * multiple * 45);
		}



	}

	public void Fire() {

		// reset the cannon angle to 0 degrees (local) and add a random rotation
		float rotateAngle = -currentAngle + Random.value * (maxAngle-minAngle); 

		// offset the rotation so that it includes the minimum
		if (wheel.transform.position.x < 0) {
			rotateAngle -= (45-minAngle);
		} else {
			// this is a bit convoluted for one of the cannons because its angles
			// are actually the reverse of the universal "counterclockwise = positive" standard
			rotateAngle -= (maxAngle-45);
		}

		// remember the new angle so that we can reset the angle to 45 again
		currentAngle += rotateAngle;

		// rotate the barrel about the wheel on the z axis, as it would occur in real life
		barrel.transform.RotateAround(wheel.transform.position,new Vector3(0,0,1),rotateAngle);

//		 Create the cannonball from the prefab (it will move itself)
//		print(cannonBallSpawn.transform.position);
		var ball = (GameObject)Instantiate(
			cannonBallPrefab,
//			new Vector3(-12.0f,cannonBallSpawn.position.y,0),
			cannonBallSpawn.transform.position,
			cannonBallSpawn.transform.rotation);

	}
	
	// Update is called once per frame
	void Update () {
		
	}

//	public void ResetTimer() {
//		timerText.text = "Hangtime: 0.00s";
//	}
}
