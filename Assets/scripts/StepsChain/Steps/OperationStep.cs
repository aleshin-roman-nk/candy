using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperationStep : StaticStepBase
{
	public Action Do {  get; set; }

	public OperationStep(string name): base(name)
	{

	}

	public OperationStep()
	{

	}

	public override bool update(ChainRunner prg)
	{
		Do?.Invoke();
		return false;
	}

	public override void Stop()
	{
		
	}
}
