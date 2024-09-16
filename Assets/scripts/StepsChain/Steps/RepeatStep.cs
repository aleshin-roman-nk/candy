using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatStep : StaticStepBase
{

	public Func<bool> RepeatWhile { get; set; }

	public string FromStep { get; set; }

	public RepeatStep(string name): base(name)
	{

	}

	public RepeatStep()
	{
	}

	public override bool update(ChainRunner prg)
	{
		if(RepeatWhile == null) return false;

		if(RepeatWhile.Invoke())
		{
			prg.GoTo(FromStep);
			return true;
		}

		return false;
	}

	public override void Stop()
	{
		throw new NotImplementedException();
	}
}
