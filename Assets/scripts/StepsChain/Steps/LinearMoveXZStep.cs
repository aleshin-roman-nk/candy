using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LinearMoveXZStep : MotionStepBase
{
	public float speed = 1.0f;
	public bool onlyXZMovement = true;
	public bool adjustDirection = true;

	private bool isMoving = false;

	public LinearMoveXZStep(string name): base(name)
	{

	}

	public LinearMoveXZStep()
	{

	}

	public override bool update(ChainRunner prg)
	{
		if (!isMoving)
		{
			isMoving = true;
			OnStartMotion?.Invoke(this);
		}

		// Move the object towards the target position if it's moving
		if (isMoving)
		{
			// Calculate direction and move the object
			Vector3 direction = (targetPoint - movableObject.position).normalized;
			direction.y = 0; // Keep movement in the XZ plane

			if(onlyXZMovement)
				movableObject.position = Vector3.MoveTowards(movableObject.position, new Vector3(targetPoint.x, movableObject.position.y, targetPoint.z), speed * Time.deltaTime);
			else
				movableObject.position = Vector3.MoveTowards(movableObject.position, new Vector3(targetPoint.x, targetPoint.y, targetPoint.z), speed * Time.deltaTime);

			// Make the object look towards the target
			if (direction != Vector3.zero && adjustDirection) // Prevent errors when direction is zero
			{
				movableObject.rotation = Quaternion.LookRotation(direction);
			}

			float dist = 0;

			if(onlyXZMovement)
				dist = Vector2.Distance(new Vector2(movableObject.position.x, movableObject.position.z), new Vector2(targetPoint.x, targetPoint.z));
			else
				dist = Vector3.Distance(movableObject.position, targetPoint);

			// Check if the object has reached the target position
			//if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
			if (dist < 0.1f)
			{
				isMoving = false;
				OnEndMotion?.Invoke(this);
				return false;
			}
		}

		return true;
	}

	public override void Stop()
	{
		isMoving = false ;
	}
}
