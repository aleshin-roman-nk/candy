using EasySlimeTouchScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static EasySlimeTouchScript.SlimeForTouchScript;

public class SlideBottomPlatformManager : MonoBehaviour
{
	[SerializeField] private SlimeForTouchScript _slimeForTouchScript;

	[SerializeField] private TrowingObject pulseObjectPrefab;
	[SerializeField] private GameObject pointerVis;

	[SerializeField] private Transform upPoint;
	[SerializeField] private Transform downPoint;

	private PressGesture pressGesture;
	private TransformGesture transformGesture;

	private DragManager _dragManager;

	// Start is called before the first frame update
	void Start()
	{
		pressGesture = GetComponent<PressGesture>();
		if(pressGesture != null)
			pressGesture.Pressed += PressGesture_Pressed;

		transformGesture = GetComponent<TransformGesture>();
		if (transformGesture != null)
			transformGesture.Transformed += TransformGesture_Transformed;

		_dragManager = new DragManager();

		_dragManager.PointerDown += _dragManager_PointerDown;
		_dragManager.PointerUp += _dragManager_PointerUp;
		_dragManager.PointerDrag += _dragManager_PointerDrag;
		_dragManager.maxTrackDistance = 150;

		StartCoroutine(_dragManager.processCoroutine());
	}

	private void _dragManager_PointerDrag(object sender, TouchTrakerDrag e)
	{
		if (_slimeForTouchScript != null)
		{
			PointerData d = new PointerData();
			d.position = e.newPoint;
			d.pointerId = -1;

			_slimeForTouchScript.OnDrag(d);
		}
	}

	private void _dragManager_PointerUp(object sender, Vector2 e)
	{
		if (_slimeForTouchScript != null)
		{
			PointerData d = new PointerData();
			d.position = e;
			d.pointerId = -1;

			_slimeForTouchScript.OnPointerUp(d);
		}
	}

	private void _dragManager_PointerDown(object sender, Vector2 e)
	{
		if (_slimeForTouchScript != null)
		{
			PointerData d = new PointerData();
			d.position = e;
			d.pointerId = -1;

			_slimeForTouchScript.OnPointerDown(d);
		}
	}

	//private Vector2 newPos;
	//private Vector2 prevPos;
	 

	private void TransformGesture_Transformed(object sender, System.EventArgs e)
	{
		//{
		//	newPos = transformGesture.ScreenPosition;
		//	Debug.Log(Vector2.Distance(newPos, prevPos));
		//	prevPos = newPos;
		//}


		_dragManager.PushPoint(transformGesture.ScreenPosition);

		var ray = Camera.main.ScreenPointToRay(transformGesture.ScreenPosition);
		RaycastHit hit;

		int platformLayer = LayerMask.GetMask("slide-bottom");

		if (Physics.Raycast(ray, out hit, Mathf.Infinity, platformLayer))
		{
			// If the ray hits something on the "platform" layer
			Vector3 hitPoint = hit.point;

			// You can now use the hit.point to do whatever you need

			//TrowingObject o = Instantiate(pulseObjectPrefab, hitPoint, Quaternion.identity);
			TrowingObject o = Instantiate(pulseObjectPrefab, hitPoint, hit.collider.gameObject.transform.rotation);
			o.SetYLevels(upPoint.position.y, downPoint.position.y);

			//if (_slimeForTouchScript != null)
			//{
			//	PointerData d = new PointerData();
			//	d.position = transformGesture.ScreenPosition;
			//	d.pointerId = -1;

			//	_slimeForTouchScript.OnDrag(d);
			//}
		}
	}

	private void PressGesture_Pressed(object sender, System.EventArgs e)
	{
		_dragManager.PushPoint(pressGesture.ScreenPosition);

		var ray = Camera.main.ScreenPointToRay(pressGesture.ScreenPosition);
		RaycastHit hit;

		int platformLayer = LayerMask.GetMask("slide-bottom");

		if (Physics.Raycast(ray, out hit, Mathf.Infinity, platformLayer))
		{
			// If the ray hits something on the "platform" layer
			Vector3 hitPoint = hit.point;

			// You can now use the hit.point to do whatever you need

			//TrowingObject o = Instantiate(pulseObjectPrefab, hitPoint, Quaternion.identity);
			TrowingObject o = Instantiate(pulseObjectPrefab, hitPoint, hit.collider.gameObject.transform.rotation);
			o.SetYLevels(upPoint.position.y, downPoint.position.y);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(upPoint.transform.position, 1f);

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(downPoint.transform.position, 1f);
	}
}

class DragManager
{
	public event EventHandler<Vector2> PointerDown;
	public event EventHandler<Vector2> PointerUp;
	public event EventHandler<TouchTrakerDrag> PointerDrag;

	public float maxTrackDistance = 1.0f;

	// This stack contains only new points
	// And it is empty after each frame
	private Stack<Vector2> newPoints = new Stack<Vector2>();

	private List<TouchTracker> touchTrackers = new List<TouchTracker>();

	public void PushPoint(Vector2 p)
	{
		newPoints.Push(p);
	}

	private void checkNewPoints()
	{
		while (newPoints.Count > 0)
		{
			var newP = newPoints.Pop();

			// 1. find track for the point

			int index = touchTrackers.FindIndex(tr => Vector2.Distance(newP, tr.previousPoint) <= maxTrackDistance);

			if (index == -1) // newP is a new touch
			{
				var newTouchTrack = new TouchTracker
				{
					isNewTrack = true,
					hasNewPoint = true,
					newPoint = newP,
					previousPoint = newP // Set previousPoint to newP for initialization
				};
				touchTrackers.Add(newTouchTrack);
			}
			else // newP is an old touch
			{
				var tt = touchTrackers[index];
				tt.hasNewPoint = true;
				tt.newPoint = newP;
				touchTrackers[index] = tt;
			}
		}
	}

	private void fireEvents()
	{
		for (int i = touchTrackers.Count - 1; i >= 0; i--)
		{
			if(!touchTrackers[i].isNewTrack && !touchTrackers[i].hasNewPoint)
			{
				PointerUp?.Invoke(this, touchTrackers[i].previousPoint);
				touchTrackers.RemoveAt(i);
				continue;
			}

			if(!touchTrackers[i].isNewTrack && touchTrackers[i].hasNewPoint)
			{
				PointerDrag?.Invoke(this, new TouchTrakerDrag { newPoint = touchTrackers[i].newPoint, previousPoint = touchTrackers[i].previousPoint});

				var tt = touchTrackers[i];
				tt.hasNewPoint = false;
				tt.isNewTrack = false;
				tt.previousPoint = tt.newPoint;

				touchTrackers[i] = tt;
			}

			if (touchTrackers[i].isNewTrack)
			{
				var tt = touchTrackers[i];
				tt.isNewTrack = false;
				tt.hasNewPoint = false;
				tt.previousPoint = tt.newPoint;
				touchTrackers[i] = tt;
				PointerDown?.Invoke(this, tt.newPoint);
				continue;
			}
		}
	}

	public IEnumerator processCoroutine()
	{
		while (true)
		{
			checkNewPoints();
			fireEvents();
			yield return null;
		}
	}

	public void updateFixed()
	{
		checkNewPoints();
		fireEvents();
	}
}

struct TouchTracker
{
	public Vector2 newPoint;
	public Vector2 previousPoint;
	public bool hasNewPoint;
	public bool isNewTrack;
}

struct TouchTrakerDrag
{
	public Vector2 newPoint;
	public Vector2 previousPoint;
}