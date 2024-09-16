using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForContitionStep : StaticStepBase
{
	private bool _started = false;

	public Action Do { get; set; }
	public Action Init { get; set; }
	public Func<bool> While { get; set; }

	public WaitForContitionStep(string name): base(name)
	{

	}

	public WaitForContitionStep()
	{

	}

	public override bool update(ChainRunner prg)
	{
		if(While == null)
		{
			return false;
		}

		if (!_started)
		{
			_started = true;
			Init?.Invoke();
		}

		if (!While.Invoke())
		{
			_started = false;
			return false;
		}

		Do?.Invoke();
		return true;
	}

	public override void Stop()
	{
		throw new NotImplementedException();
	}
}
