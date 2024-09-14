using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;

public class SlideBottomPlatformManager : MonoBehaviour
{
	private PressGesture pressGesture;
	private ReleaseGesture releaseGesture;
	private TransformGesture transformGesture;

	// Start is called before the first frame update
	void Start()
	{
		//pressGesture = GetComponent<PressGesture>();
		//pressGesture.Pressed += PressGesture_Pressed;

		//releaseGesture = GetComponent<ReleaseGesture>();
		//releaseGesture.Released += ReleaseGesture_Released;

		//transformGesture = GetComponent<TransformGesture>();
		//transformGesture.Transformed += TransformGesture_Transformed;
	}

	private void TransformGesture_Transformed(object sender, System.EventArgs e)
	{
		Debug.Log("TransformGesture_Transformed");
	}

	private void ReleaseGesture_Released(object sender, System.EventArgs e)
	{
		Debug.Log("ReleaseGesture_Released");
	}

	private void PressGesture_Pressed(object sender, System.EventArgs e)
	{
		Debug.Log("PressGesture_Pressed");
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}
