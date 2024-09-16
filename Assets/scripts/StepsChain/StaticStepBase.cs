using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaticStepBase : IStep
{
	public string name { get; private set; }

	public StaticStepBase()
	{

	}

	public StaticStepBase(string name)
	{
		this.name = name;
	}

	public abstract bool update(ChainRunner prg);

	public abstract void Stop();
}
