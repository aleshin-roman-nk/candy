using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.scripts.GameLogic
{
	class TouchTrackManager
	{
		public event EventHandler<TouchTrakerPointer> PointerDown;
		public event EventHandler<TouchTrakerPointer> PointerUp;
		public event EventHandler<TouchTrakerPointer> PointerDrag;

		public float maxTrackDistance = 1.0f;

		private int nextPointerId = 0;
		private int maxPointerId = 10;

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
						pointerId = nextId(),
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

		private int nextId()
		{
			if (nextPointerId > maxPointerId) nextPointerId = 0;
			return nextPointerId++;
		}

		private void fireEvents()
		{
			for (int i = touchTrackers.Count - 1; i >= 0; i--)
			{
				if (!touchTrackers[i].isNewTrack && !touchTrackers[i].hasNewPoint)
				{
					PointerUp?.Invoke(this, new TouchTrakerPointer { point = touchTrackers[i].previousPoint, pointerId = touchTrackers[i].pointerId });
					touchTrackers.RemoveAt(i);
					continue;
				}

				if (!touchTrackers[i].isNewTrack && touchTrackers[i].hasNewPoint)
				{
					PointerDrag?.Invoke(this, new TouchTrakerPointer { point = touchTrackers[i].newPoint, pointerId = touchTrackers[i].pointerId });

					var tt = touchTrackers[i];
					tt.hasNewPoint = false;
					tt.isNewTrack = false;
					tt.previousPoint = tt.newPoint;

					touchTrackers[i] = tt;

					continue;
				}

				if (touchTrackers[i].isNewTrack)
				{
					var tt = touchTrackers[i];
					tt.isNewTrack = false;
					tt.hasNewPoint = false;
					tt.previousPoint = tt.newPoint;
					touchTrackers[i] = tt;
					PointerDown?.Invoke(this, new TouchTrakerPointer { point = touchTrackers[i].newPoint, pointerId = touchTrackers[i].pointerId });
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
		public int pointerId;
		public bool hasNewPoint;
		public bool isNewTrack;
	}

	public struct TouchTrakerPointer
	{
		public Vector2 point;
		//public Vector2 previousPoint;
		public int pointerId;
	}
}

