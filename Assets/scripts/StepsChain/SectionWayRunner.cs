using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SectionWayRunner
{
	public Vector3[] wayPoints = null;
	public Action Completed;

	ChainRunner chainRunner;

	private Transform _movableObject;
	private int _currentTargetPoint = 0;

	private readonly RotationStep _rotateChainElement;
	private readonly LinearMoveXZStep _linearMoveChainElement;

	/*
	 * Движущийся объект имеет две характеристики: линенйная скорость, скорость поворота
	 */
	private readonly float _linearSpeed = 1.5f;
	private readonly float _rotationSpeed = 12.0f;

	public bool IsSlowedDown {  get; private set; } = false;

	/*
	 * У объекта есть набор характеристик
	 * добавить набор объектов, меняющих результирующую картину
	 * на вход картина характеристик, на выходе измененные
	 * объект изменения имеет время жизни...
	 */

	public void SlowDown(float linearSpeedPercent, float rotationSpeedPercent)
	{
		IsSlowedDown = true;

		_linearMoveChainElement.speed = _linearSpeed * linearSpeedPercent;
		_rotateChainElement.rotationSpeed = _rotationSpeed * rotationSpeedPercent;
	}

	public void CancelSlowingDown()
	{
		IsSlowedDown = false;
		_linearMoveChainElement.speed = _linearSpeed;
		_rotateChainElement.rotationSpeed = _rotationSpeed;
	}

	public SectionWayRunner(Transform obj)
	{
		this._movableObject = obj;

		_rotateChainElement = new RotationStep
		{
			movableObject = _movableObject,
			rotationSpeed = _rotationSpeed
		};
		_linearMoveChainElement = new LinearMoveXZStep
		{
			movableObject = _movableObject,
			speed = _linearSpeed
		};

		chainRunner = new ChainRunner();
		chainRunner
			.AddStep(_rotateChainElement)
			.AddStep(_linearMoveChainElement);
	}

	// Start is called before the first frame update
	void Start()
	{
		
	}

	public void SetWay(Vector3[] way)
	{
		if(way == null)
		{
			Debug.LogError("way is empty");
			return;
		}

		wayPoints = way;

		_currentTargetPoint = 0;

		_rotateChainElement.targetPoint = wayPoints[_currentTargetPoint];
		_linearMoveChainElement.targetPoint = wayPoints[_currentTargetPoint];

		chainRunner.RestartChain();
	}

	// Update is called once per frame
	public bool Update()
	{
		if (!chainRunner.Update())// двухшаговая цепь выполнена, но точки еще не кончились
		{
			_currentTargetPoint++;

			if(_currentTargetPoint >= wayPoints.Length)
			{
				Completed?.Invoke();
				return false;
			}

			_rotateChainElement.targetPoint = wayPoints[_currentTargetPoint];
			_linearMoveChainElement.targetPoint = wayPoints[_currentTargetPoint];

			chainRunner.RestartChain();
		}
		
		return true;
	}
}
