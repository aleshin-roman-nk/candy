using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;

namespace Assets.scripts.GameLogic
{
	internal class InclinedBoard: MonoBehaviour
	{
		[SerializeField] private TouchObserver touchObserver = null;

		[SerializeField] private ThrowingObject throwingObjectPrefab;

		[SerializeField] private Transform topPoint;
		[SerializeField] private Transform bottomPoint;

		private PoolMono<ThrowingObject> poolMonoTrowingObject;

		private Dictionary<int, ThrowingObject> throwingObjectDictionary;

		private void Start()
		{
			poolMonoTrowingObject = new PoolMono<ThrowingObject>(throwingObjectPrefab, 1);
			poolMonoTrowingObject.autoExpand = true;

			touchObserver.PointerDrag += TouchObserver_PointerDrag;
			touchObserver.PointerUp += TouchObserver_PointerUp;
			touchObserver.PointerDown += TouchObserver_PointerDown;

			throwingObjectDictionary = new Dictionary<int, ThrowingObject>();
		}

		private void TouchObserver_PointerDown(object sender, TouchTrakerPointer e)
		{
			ThrowingObject o = poolMonoTrowingObject.GetFreeElement();
			throwingObjectDictionary[e.pointerId] = o;
			putOnTheBoard(e.point, o);
		}

		private void TouchObserver_PointerUp(object sender, TouchTrakerPointer e)
		{
			if (throwingObjectDictionary.ContainsKey(e.pointerId))
			{
				var o = throwingObjectDictionary[e.pointerId];
				o.gameObject.SetActive(false);
			}
		}

		private void TouchObserver_PointerDrag(object sender, TouchTrakerPointer e)
		{
			if (throwingObjectDictionary.ContainsKey(e.pointerId))
			{
				var o = throwingObjectDictionary[e.pointerId];
				putOnTheBoard(e.point, o);
			}
			else
				Debug.LogError($"No pointer with id = {e.pointerId}");
		}

		private void putOnTheBoard(Vector2 screenPosition, ThrowingObject o)
		{
			var ray = Camera.main.ScreenPointToRay(screenPosition);
			RaycastHit hit;

			int platformLayer = LayerMask.GetMask("slide-bottom");

			if (Physics.Raycast(ray, out hit, Mathf.Infinity, platformLayer))
			{
				// If the ray hits something on the "platform" layer

				//ThrowingObject o = Instantiate(throwingObjectPrefab, hitPoint, hit.collider.gameObject.transform.rotation);
				//o.SetYLevels(topPoint.position.y, bottomPoint.position.y);

				o.transform.position = hit.point;
				o.transform.rotation = hit.collider.gameObject.transform.rotation;
				o.SetYLevels(topPoint.position.y, bottomPoint.position.y);
			}
		}



		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(topPoint.transform.position, 1f);

			Gizmos.color = Color.red;
			Gizmos.DrawSphere(bottomPoint.transform.position, 1f);
		}
	}
}
