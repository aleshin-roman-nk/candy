using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrowingObject : MonoBehaviour
{
	[SerializeField] private Transform pulsePoint;
	[SerializeField] private float impulseStrength;
	[SerializeField] private float lowForce;
	[SerializeField] private float highForce;

	private float _upY;
	private float _downY;

	// Start is called before the first frame update
	void Start()
	{
		Destroy(gameObject, 0.2f);
	}

	public void SetYLevels(float upY, float downY)
	{
		_downY = downY;
		_upY = upY;
	}

	private void OnTriggerEnter(Collider other)
	{
		var rbcollision = other.gameObject.GetComponent<Rigidbody>();

		Vector3 direction = (other.gameObject.transform.position - pulsePoint.position).normalized;
		var allYRange = _upY - _downY;

		var currentYLevelRelativeLowY = transform.position.y - _downY;

		var levelForce = Mathf.Lerp(highForce, lowForce, currentYLevelRelativeLowY / allYRange);

		//rbcollision.AddForce(impulseStrength * levelForce * direction, ForceMode.Impulse);

		var c = rbcollision.gameObject.GetComponent<Candy>();

		c.FlyAndReturn(direction);
	}
}
