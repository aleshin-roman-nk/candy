using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStep
{
	string name { get; }
	bool update(ChainRunner prg);
	void Stop();
}