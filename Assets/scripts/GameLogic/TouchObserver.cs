using EasySlimeTouchScript;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace Assets.scripts.GameLogic
{
	public class TouchObserver : MonoBehaviour
	{
		public event EventHandler<TouchTrakerPointer> PointerUp;
		public event EventHandler<TouchTrakerPointer> PointerDown;
		public event EventHandler<TouchTrakerPointer> PointerDrag;

		private PressGesture pressGesture;
		private TransformGesture transformGesture;

		private TouchTrackManager _touchTrackManager;

		// Start is called before the first frame update
		void Start()
		{
			pressGesture = GetComponent<PressGesture>();
			if (pressGesture != null)
				pressGesture.Pressed += PressGesture_Pressed;

			transformGesture = GetComponent<TransformGesture>();
			if (transformGesture != null)
				transformGesture.Transformed += TransformGesture_Transformed;

			_touchTrackManager = new TouchTrackManager();

			_touchTrackManager.PointerDown += _touchTrackManager_PointerDown;
			_touchTrackManager.PointerUp += _touchTrackManager_PointerUp;
			_touchTrackManager.PointerDrag += _touchTrackManager_PointerDrag;
			_touchTrackManager.maxTrackDistance = 150;

			StartCoroutine(_touchTrackManager.processCoroutine());
		}

		private void _touchTrackManager_PointerDrag(object sender, TouchTrakerPointer e)
		{
			PointerDrag?.Invoke(this, e);
		}

		private void _touchTrackManager_PointerUp(object sender, TouchTrakerPointer e)
		{
			PointerUp?.Invoke(this, e);
		}

		private void _touchTrackManager_PointerDown(object sender, TouchTrakerPointer e)
		{
			PointerDown?.Invoke(this, e);
		}

		private void TransformGesture_Transformed(object sender, System.EventArgs e)
		{
			_touchTrackManager.PushPoint(transformGesture.ScreenPosition);
		}

		private void PressGesture_Pressed(object sender, System.EventArgs e)
		{
			_touchTrackManager.PushPoint(pressGesture.ScreenPosition);
		}


	}
}

