using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SlideBottomPlatformManager : MonoBehaviour
{
	[SerializeField] private TrowingObject pulseObjectPrefab;
	[SerializeField] private GameObject pointerVis;

	[SerializeField] private Transform upPoint;
	[SerializeField] private Transform downPoint;

	private PressGesture pressGesture;

	// Start is called before the first frame update
	void Start()
	{
		pressGesture = GetComponent<PressGesture>();
		pressGesture.Pressed += PressGesture_Pressed;
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
