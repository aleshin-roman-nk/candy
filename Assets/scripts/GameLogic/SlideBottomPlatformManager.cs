using EasySlimeTouchScript;
using System.Collections;
using System.Collections.Generic;
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

	// Start is called before the first frame update
	void Start()
	{
		pressGesture = GetComponent<PressGesture>();
		if(pressGesture != null)
			pressGesture.Pressed += PressGesture_Pressed;

		transformGesture = GetComponent<TransformGesture>();
		if (transformGesture != null)
			transformGesture.Transformed += TransformGesture_Transformed;
	}

	private void TransformGesture_Transformed(object sender, System.EventArgs e)
	{
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

			if (_slimeForTouchScript != null)
			{
				PointerData d = new PointerData();
				d.position = transformGesture.ScreenPosition;
				d.pointerId = -1;

				_slimeForTouchScript.OnDrag(d);
			}
		}
	}

	private void PressGesture_Pressed(object sender, System.EventArgs e)
	{
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
