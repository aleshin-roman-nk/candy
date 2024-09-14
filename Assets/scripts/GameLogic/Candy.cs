using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candy : MonoBehaviour
{
	[SerializeField] private float disappearYLevel = -2;

	// Update is called once per frame
	void Update()
	{
		if (transform.position.y < disappearYLevel) gameObject.SetActive(false);
	}
}
