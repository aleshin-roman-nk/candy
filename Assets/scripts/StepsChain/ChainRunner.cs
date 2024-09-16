using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ChainRunner
{
	private List<IStep> moveSteps = new List<IStep>();

	private int currentPointer = 0;

	private bool _run = false;

	public Action ChainCompleted { get; set; }

	public ChainRunner AddStep(IStep step)
	{
		moveSteps.Add(step);

		return this;
	}

	public void GoTo(string stepName)
	{
		var i = moveSteps.FindIndex(x => !string.IsNullOrEmpty(x.name) && x.name.Equals(stepName));
		if(i >= 0) currentPointer = i;
	}

	public void Pause()
	{
		_run = false;
	}

	public void StartChain()
	{
		if(_run) moveSteps[currentPointer].Stop();

		currentPointer = 0;
		_run = true;
	}

	public void StopChain()
	{
		_run = false;
		if (currentPointer < moveSteps.Count)
			moveSteps[currentPointer].Stop();

		currentPointer = 0;
	}

	public void RestartChain()
	{
		if (_run)
		{
			if(currentPointer < moveSteps.Count)
				moveSteps[currentPointer].Stop();
		}

		currentPointer = 0;
		_run = true;
	}

	public void Resume()
	{
		_run = true;
	}

	public bool Update()
	{
		if(!_run) return false;

		if (currentPointer < moveSteps.Count)
		{
			if (!moveSteps[currentPointer].update(this))
				currentPointer++;

			return true;
		}
		else
		{
			_run = false;
			ChainCompleted?.Invoke();
			return false;
		}
	}
}
