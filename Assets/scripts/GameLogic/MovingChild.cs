using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MovingChild : MonoBehaviour
{
	[SerializeField] private float forceValue = 10.0f;
	[SerializeField] private TextMeshProUGUI velocityText;
	[SerializeField] private Transform pulsePoint;
	[SerializeField] private float impulseStrength;

	private Rigidbody rb;

	// Start is called before the first frame update
	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private bool go = false;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			go = true;
		}

		if (velocityText != null)
			velocityText.text = rb.velocity.magnitude.ToString();
	}

	private void FixedUpdate()
	{
		if (go)
		{
			//go = false;
			// Apply a constant force in the forward direction
			Vector3 force = transform.forward * forceValue;
			rb.AddForce(force, ForceMode.Force); // Continuous force

			//rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
		}
	}

	//private void OnCollisionEnter(Collision collision)
	//{

	//}

	private void OnTriggerEnter(Collider other)
	{
		var rbcollision = other.gameObject.GetComponent<Rigidbody>();

		Vector3 direction = (other.gameObject.transform.position - pulsePoint.position).normalized;
		var impulseAdd = rb.velocity.magnitude;
		rbcollision.AddForce(impulseStrength * impulseAdd * direction, ForceMode.Impulse);
	}
}
