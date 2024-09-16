using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateColor : StaticStepBase
{
	public Color valueA;
	public Color valueB;
	public float duration;

	public Action<Color> OnAnimationStep;

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

			var c = Color.Lerp(valueA, valueB, t);

			OnAnimationStep?.Invoke(c);

			if (t >= 1f)
			{
				isAnimating = false;
				elapsedTime = 0.0f;
				return false;
			}
		}

		return true;
	}
}
