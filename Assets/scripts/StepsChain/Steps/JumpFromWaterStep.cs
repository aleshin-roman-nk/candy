using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class JumpFromWaterStep : MotionStepBase
{
	public float jumpHeight = 2.0f;
	public float jumpDuration = 2.0f;

	private Vector3 startPoint;
	private Vector3 endPoint;
	private float elapsedJumpTime = 0.0f;
	private bool isJumping = false;

	private bool waterSplashOccured = false;

	public JumpFromWaterStep(string name): base(name) { }

	public JumpFromWaterStep() { }

	public Action<Vector3> OnExitWater {  get; set; }

	public override bool update(ChainRunner prg)
	{
		//if (isCompleted) return false;

		if (!isJumping)
		{
			OnStartMotion?.Invoke(this);

			waterSplashOccured = false;

			endPoint = targetPoint;
			startPoint = movableObject.position;

			isJumping = true;

			elapsedJumpTime = 0.0f;

			Vector3 direction = (targetPoint - movableObject.position).normalized;
			direction.y = 0; // Keep movement in the XZ plane

			// Make the object look towards the target
			if (direction != Vector3.zero) // Prevent errors when direction is zero
			{
				movableObject.rotation = Quaternion.LookRotation(direction);
			}
		}

		if (isJumping)
		{
			// Calculate the elapsed time
			elapsedJumpTime += Time.deltaTime;

			// Calculate the percentage of completion
			float t = elapsedJumpTime / jumpDuration;

			// Ensure t does not exceed 1
			t = Mathf.Clamp01(t);

			// Calculate the parabolic trajectory
			Vector3 currentPosition = CalculateParabolicPoint(startPoint, endPoint, jumpHeight, t);

			if (!waterSplashOccured)
			{
				if(currentPosition.y >= -0.1f && currentPosition.y < 0.1)
				{
					waterSplashOccured = true;
					OnExitWater?.Invoke(new Vector3(currentPosition.x, 0, currentPosition.z));
				}
			}

			// Update the frog's position
			movableObject.position = currentPosition;

			// Reset the jump when it reaches the end point
			if (t >= 1.0f)
			{
				elapsedJumpTime = 0.0f;
				startPoint = movableObject.position;

				OnEndMotion?.Invoke(this);
				isJumping = false;
				return false;
			}
		}

		return true;
	}

	//void CalculateEndPoint()
	//{
	//	// Calculate the end point based on the frog's forward direction
	//	endPoint = startPoint + movableObject.forward * jumpDistance;
	//}

	Vector3 CalculateParabolicPoint(Vector3 start, Vector3 end, float height, float t)
	{
		// Calculate the parabolic trajectory using a quadratic equation
		Vector3 midPoint = Vector3.Lerp(start, end, t);
		float parabolaHeight = 4 * height * t * (1 - t);

		// Interpolate the y position between start and end, then add the parabolic height offset
		float y = Mathf.Lerp(start.y, end.y, t) + parabolaHeight;
		return new Vector3(midPoint.x, y, midPoint.z);
	}

	public override void Stop()
	{
		isJumping = false;
	}
}
