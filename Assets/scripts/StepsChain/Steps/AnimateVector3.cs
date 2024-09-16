using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AnimateVector3 : StaticStepBase
{
	public Vector3 valueA;
	public Vector3 valueB;
	public float duration;

	public Action<Vector3> OnAnimationStep;

	private float elapsedTime = 0f;
	private bool isAnimating = false;
	private Vector3 currentValue;

	public override void Stop()
	{
		throw new NotImplementedException();
	}

	public override bool update(ChainRunner prg)
	{
		if (!isAnimating)
		{
			elapsedTime = 0;
			isAnimating = true;
		}

		if (isAnimating)
		{
			elapsedTime += Time.deltaTime;
			float t = Mathf.Clamp01(elapsedTime / duration);
			currentValue = Vector3.Lerp(valueA, valueB, t);

			OnAnimationStep?.Invoke(currentValue);

			if (t >= 1f)
			{
				isAnimating = false;
				return false;
			}
		}

		return true;
	}
}
