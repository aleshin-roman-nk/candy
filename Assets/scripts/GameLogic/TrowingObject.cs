using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrowingObject : MonoBehaviour
{
	[SerializeField] private Transform pulsePointA;
	[SerializeField] private Transform pulsePointB;
	[SerializeField] private float pulsePointAForce = 1;
	[SerializeField] private float pulsePointBForce = 2;
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

		Vector3 directionA = pulsePointAForce * (other.gameObject.transform.position - pulsePointA.position).normalized;
		Vector3 directionB = pulsePointBForce * (other.gameObject.transform.position - pulsePointB.position).normalized;
		Vector3 direction = (directionA + directionB).normalized;

		var allYRange = _upY - _downY;

		var currentYLevelRelativeLowY = transform.position.y - _downY;

		var levelForce = Mathf.Lerp(lowForce, highForce, currentYLevelRelativeLowY / allYRange);

		//rbcollision.AddForce(impulseStrength * levelForce * direction, ForceMode.Impulse);

		var c = rbcollision.gameObject.GetComponent<Candy>();

		c.FlyAndReturn(direction, levelForce);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position, 0.1f);

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(pulsePointA.position, 0.1f);

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(pulsePointB.position, 0.1f);
	}
}
