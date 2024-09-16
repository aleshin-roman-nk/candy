using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candy : MonoBehaviour
{
	[SerializeField] private float disappearYLevel = -2;

	private Vector3 restPosition;
	private Vector3 prevPosition;
	private bool zeroPositionDefined = false;
	private bool isFlying = false;

	private AnimateVector3 animateFly;

	private void Start()
	{
		prevPosition = transform.position - new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
		animateFly = new AnimateVector3();
	}

	// Update is called once per frame
	void Update()
	{
		if (transform.position.y < disappearYLevel) gameObject.SetActive(false);
	}

	public void FlyAndReturn(Vector3 goDirection)
	{
		if (isFlying) return;

		StartCoroutine(flyCoroutine(30 * goDirection));
	}

	private IEnumerator flyCoroutine(Vector3 toPoint)
	{
		isFlying = true;

		animateFly.valueA = transform.position;
		animateFly.valueB = toPoint;
		animateFly.duration = 0.7f;

		var r = GetComponent<Rigidbody>();
		r.useGravity = false;
		r.isKinematic = true;

		animateFly.OnAnimationStep = (pos) =>
		{
			transform.position = pos;
		};

		while(animateFly.update(null))
			yield return null;

		yield return new WaitForSeconds(0.2f);

		animateFly.valueA = transform.position;
		animateFly.valueB = restPosition;
		animateFly.duration = 0.7f;

		while (animateFly.update(null))
			yield return null;

		isFlying = false;
		r.useGravity = true;
		r.isKinematic = false;
	}

	private bool startFindingRestPos = false;

	private void FixedUpdate()
	{
		if (zeroPositionDefined) return;

		var delta = (transform.position - prevPosition).magnitude;

		if (!startFindingRestPos)
		{
			if (delta > 5) startFindingRestPos = true;
		}

		if (startFindingRestPos)
		{
			if(delta <= 0.001f)
			{
				restPosition = transform.position;
				zeroPositionDefined = true;
				//GetComponent<Rigidbody>().useGravity = false;
			}

			prevPosition = transform.position;
		}

	}
}
