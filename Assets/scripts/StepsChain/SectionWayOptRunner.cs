using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionWayOptRunner
{
	public Vector3[] wayPoints = null;
	public Action Completed;

	ChainRunner chainRunner;

	private Transform _movableObject;
	private int _currentTargetPoint = 0;

	private MotionStepBase rotateChainElement;
	private MotionStepBase linearMoveChainElement;

	/*
	 * Движущийся объект имеет две характеристики: линенйная скорость, скорость поворота
	 */
	private readonly float _linearSpeed = 1.5f;
	private readonly float _rotationSpeed = 12.0f;

	public bool IsSlowedDown { get; private set; } = false;

	public SectionWayOptRunner(Transform obj, 
		MotionStepBase rotatingLink,
		StaticStepBase stayLink,
		MotionStepBase linearLink
		)
	{
		this._movableObject = obj;

		//rotateChainElement = new AnimateRotationStep
		//{
		//	movableObject = _movableObject,
		//	rotationSpeed = _rotationSpeed
		//};
		//linearMoveChainElement = new LinearMoveXZStep
		//{
		//	movableObject = _movableObject,
		//	speed = _linearSpeed
		//};

		rotateChainElement = rotatingLink;
		linearMoveChainElement = linearLink;

		chainRunner = new ChainRunner();
		chainRunner
			.AddStep(rotateChainElement)
			.AddStep(stayLink)
			.AddStep(linearMoveChainElement);
	}

	// Start is called before the first frame update
	void Start()
	{

	}

	public void SetWay(Vector3[] way)
	{
		if (way == null)
		{
			Debug.LogError("way is empty");
			return;
		}

		wayPoints = way;

		_currentTargetPoint = 0;

		rotateChainElement.targetPoint = wayPoints[_currentTargetPoint];
		linearMoveChainElement.targetPoint = wayPoints[_currentTargetPoint];

		chainRunner.RestartChain();
	}

	// Update is called once per frame
	public bool Update()
	{
		if (!chainRunner.Update())// двухшаговая цепь выполнена, но точки еще не кончились
		{
			_currentTargetPoint++;

			if (_currentTargetPoint >= wayPoints.Length)
			{
				Completed?.Invoke();
				return false;
			}

			rotateChainElement.targetPoint = wayPoints[_currentTargetPoint];
			linearMoveChainElement.targetPoint = wayPoints[_currentTargetPoint];

			chainRunner.RestartChain();
		}

		return true;
	}
}
