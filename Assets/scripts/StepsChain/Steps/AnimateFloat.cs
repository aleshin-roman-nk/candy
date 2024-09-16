using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateFloat: StaticStepBase
{
	public float valueA;
	public float valueB;
	public float duration;

	public Action<float> OnAnimationStep;

	private float elapsedTime = 0f;
	private bool isAnimating = false;
	private float currentValue;

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
			currentValue = Mathf.Lerp(valueA, valueB, t);

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
