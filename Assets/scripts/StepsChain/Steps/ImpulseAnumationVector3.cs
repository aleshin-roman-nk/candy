using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpulseAnimationVector3 : StaticStepBase
{
	public Action<Vector3> OnAnimationStep;

	public Vector3 originalValue;

	public float impulseMagnitudeX;
	public float impulseMagnitudeY;
	public float impulseMagnitudeZ;
	public float duration = 1f;

	private Vector3 currentValue;
	private float timeElapsed;

	private bool isAnimating = false;

	private AnimationCurve impulseCurve;

	public ImpulseAnimationVector3()
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
			currentValue = new Vector3(originalValue.x, originalValue.y, originalValue.z);
		}

		if (isAnimating)
		{
			timeElapsed += Time.deltaTime;
			float t = timeElapsed / duration;

			var x = originalValue.x + impulseCurve.Evaluate(t) * impulseMagnitudeX;
			var y = originalValue.y + impulseCurve.Evaluate(t) * impulseMagnitudeY;
			var z = originalValue.z + impulseCurve.Evaluate(t) * impulseMagnitudeZ;

			currentValue = new Vector3(x, y, z);

			// Optionally, clamp the time to stop updating after reaching the duration
			if (timeElapsed >= duration)
			{
				timeElapsed = duration;
				//currentValue = originalValue;

				OnAnimationStep?.Invoke(originalValue);

				timeElapsed = 0;
				isAnimating = true;

				return false;
			}

			OnAnimationStep?.Invoke(currentValue);
		}

		return true;
	}
}
