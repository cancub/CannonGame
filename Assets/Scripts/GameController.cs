using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
	private CannonController[] cannons;
	private int cannonSwitch;
	public Text windText;
	public float windMax;
	private float windVelocity;
	public float windIncrementPercent;
	private float windIncrement;
	private GameObject ball;

    void Start()
    {

		Random.InitState ((int)System.DateTime.Now.Ticks);
		windVelocity = 0; // the velocity will be used to tell the cannonballs how to move
		cannonSwitch = 0; // we need to know which cannon is currently beign used

		// get the cannons that will be toggled between
		cannons = new CannonController[2];
		cannons [0] = (CannonController)GameObject.Find ("Ball Cannon").GetComponent<CannonController> ();
		cannons [1] = (CannonController) GameObject.Find ("Goat Cannon").GetComponent<CannonController> ();

		// update the wind velocity once every 0.5 seconds
		InvokeRepeating ("UpdateWind", 0, 0.5f);
    }
	
	// Update is called once per frame
	void Update () {
		// fire the current cannon if the space has been pressed
		if (Input.GetKeyDown (KeyCode.Space) == true) {
			cannons [cannonSwitch].Fire ();
		}

		// switch between the cannons if Tab has been pressed
		if (Input.GetKeyDown (KeyCode.Tab) == true) {
			cannonSwitch = (cannonSwitch + 1) % 2;
		}

		windIncrement = (windIncrementPercent * windMax) / 100;
	}

	// allow the projectiles to determine how far they should move this next update
	public float GetWindSpeed() {
//		return windVelocity;
		return windMax;
	}
//
	// modify the wind's velocity based on a random value in the range of [-max,max]
	void UpdateWind() {


		
		// we don't want the wind to just be swing wildly back and forth. it would be better if it
		// were natural and gradualy changed, potentially even refraining from changing directions 

		// start with a random walk. the wind velocity can either increase of decrease unless it
		// is already at the maximum windspeed
		if (windVelocity <= -windMax) {
			// increase the wind towards the left by the increment
			windVelocity += windIncrement;
		} else if (windVelocity >=  windMax) {
			// increase the wind towards the right by the increment
			windVelocity -= windIncrement;
		} else {
			if (Random.value > 0.5) {
				windVelocity -= windIncrement;
			} else {
				windVelocity += windIncrement;
			}
		}

		// update the text on the screen to show the wind direction

		int arrowCount = (int)(Mathf.Abs (windVelocity) / windIncrement);
//		print ("updating wind: velocity = " + windVelocity.ToString() + " -- arrow count = " + arrowCount.ToString());
		if (windVelocity == 0 || arrowCount == 0) {
			windText.text = "O";
		} else {
			string arrowString;
			if (windVelocity > 0) {
				arrowString = new string('>',arrowCount);
			} else {
				arrowString = new string('<',arrowCount);
			}
			windText.text = arrowString;
		}
	}




}
