using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpulseAnimationFloat : StaticStepBase
{
	public Action<float> OnAnimationStep;

	public float impulseMagnitude;
	public float duration = 1f;

	private float startValue = 1f;
	private float currentValue;
	private float timeElapsed;

	private bool isAnimating = false;

	private AnimationCurve impulseCurve;

	public ImpulseAnimationFloat()
	{
		impulseCurve = new AnimationCurve(
			new Keyframe(0, 0),
			new Keyframe(0.2f, 1),
			new Keyframe(1, 0));
	}

	public override void Stop()
	{
		throw new NotImplementedException();
	}

	public override bool update(ChainRunner prg)
	{
		if (!isAnimating)
		{
			timeElapsed = 0;
			isAnimating = true;
		}

		if (isAnimating)
		{
			timeElapsed += Time.deltaTime;
			float t = timeElapsed / duration;
			currentValue = startValue + impulseCurve.Evaluate(t) * impulseMagnitude;

			// Optionally, clamp the time to stop updating after reaching the duration
			if (timeElapsed >= duration)
			{
				timeElapsed = duration;
				currentValue = startValue;

				OnAnimationStep?.Invoke(currentValue);

				timeElapsed = 0;
				isAnimating = true;

				return false;
			}

			OnAnimationStep?.Invoke(currentValue);
		}

		return true;
	}
}
