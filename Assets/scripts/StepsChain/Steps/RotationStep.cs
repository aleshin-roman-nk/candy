using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationStep : MotionStepBase
{
	public float rotationTolerance = 0.01f;
	public float rotationSpeed = 7.0f;

	private bool isRotating = false;

	public override void Stop()
	{
		isRotating = false;
	}

	public override bool update(ChainRunner prg)
	{
		if (!isRotating)
		{
			isRotating = true;
			OnStartMotion?.Invoke(this);
		}

		if (isRotating)
		{
			// Calculate the direction to the target point
			Vector3 direction = (targetPoint - movableObject.position).normalized;
			direction.y = 0.0f;

			if(direction != Vector3.zero)
			{
				// Calculate the rotation required to look at the target point
				Quaternion targetRotation = Quaternion.LookRotation(direction);

				// Smoothly rotate towards the target point
				movableObject.rotation = Quaternion.Slerp(movableObject.rotation, targetRotation, rotationSpeed * Time.deltaTime);

				// Check if the rotation is close enough to the target rotation
				if (Quaternion.Angle(movableObject.rotation, targetRotation) < rotationTolerance)
				{
					// Snap to the exact rotation to avoid jittering
					movableObject.rotation = targetRotation;
					isRotating = false;
					OnEndMotion?.Invoke(this);
					return false;
				}
			}
		}

		return true;
	}
}
