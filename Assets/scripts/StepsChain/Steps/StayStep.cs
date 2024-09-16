using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayStep : StaticStepBase
{
	public Action<StayStep> OnEnter { get; set; }


	public float stayForSeconds = 0.0f;

	private bool isGoing = false;
	private float elapsedTime = 0.0f;

	public StayStep(string name): base(name)
	{
	}

	public StayStep()
	{
	}

	public override void Stop()
	{
		elapsedTime = 0.0f;
		isGoing = false;
	}

	public override bool update(ChainRunner prg)
	{
		if (!isGoing)
		{
			isGoing = true;
			elapsedTime = 0.0f;
			OnEnter?.Invoke(this);
		}

		if (isGoing)
		{
			elapsedTime += Time.deltaTime;

			if(elapsedTime >= stayForSeconds)
			{
				isGoing = false;
				return false;
			}
		}

		return true;
	}

}
