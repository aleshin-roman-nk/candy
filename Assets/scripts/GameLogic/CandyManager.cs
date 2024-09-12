using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyManager : MonoBehaviour
{
	[SerializeField] private Vector3 candyArea = new Vector3 (1, 1, 1);
	[SerializeField] private Candy[] candyPrefabs;
	[SerializeField] private int candyCount = 400;

	private List<Candy> candies = new List<Candy>();

	// Start is called before the first frame update
	void Start()
	{
		for (int i = 0; i < candyCount; i++)
		{
			var iCandy = Random.Range (0, candyPrefabs.Length);

			var c = Instantiate(candyPrefabs[iCandy], GetRandomPointInArea(), Random.rotation);

			candies.Add(c);
		}
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position, candyArea);
	}

	private Vector3 GetRandomPointInArea()
	{
		// Generate a random point in local space
		Vector3 localRandomPoint = new Vector3(
			Random.Range(-candyArea.x / 2, candyArea.x / 2),
			Random.Range(-candyArea.y / 2, candyArea.y / 2),
			Random.Range(-candyArea.z / 2, candyArea.z / 2)
		);

		// Transform the point from local space to world space
		Vector3 worldRandomPoint = transform.TransformPoint(localRandomPoint);

		return worldRandomPoint;
	}
}
