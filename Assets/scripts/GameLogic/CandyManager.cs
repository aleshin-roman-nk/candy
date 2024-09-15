using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CandyManager : MonoBehaviour
{
	[SerializeField] private Vector3 candyArea = new Vector3 (1, 1, 1);
	[SerializeField] private Candy[] candyPrefabs;
	[SerializeField] private int candyMaxCount = 400;

	[SerializeField] private TextMeshProUGUI candyCountText;

	private PoolMulti<Candy> candiesPool;

	private bool dropCandy = false;

	// Start is called before the first frame update
	void Start()
	{
		candiesPool = new PoolMulti<Candy>(candyPrefabs, candyMaxCount, gameObject.transform);
		candiesPool.autoExpand = true;

		addCandies(candyMaxCount);

		//StartCoroutine(observeCandyCountCoroutine());
		//StartCoroutine(dropCandyCoroutine());
	}

	// Update is called once per frame
	void Update()
	{
		candyCountText.text = candiesPool.ActiveObjects.ToString();
	}

	private IEnumerator observeCandyCountCoroutine()
	{
		while (true) 
		{
			yield return new WaitForSeconds(0.5f);

			// If the lack of candies is greater than N, call addCandies(N)
			//addCandies(candyMaxCount - candiesPool.ActiveObjects);
			if (candyMaxCount > candiesPool.ActiveObjects)
				dropCandy = true;
			else
				dropCandy = false;
		}
	}

	private IEnumerator dropCandyCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.2f);
			if (dropCandy)
				addCandies(3);
		}
	}

	private void addCandy()
	{
		var iCandy = Random.Range(0, candyPrefabs.Length);

		var candy = candiesPool.GetFreeElement();
		candy.transform.position = GetRandomPointInArea();
		candy.transform.rotation = Random.rotation;
	}

	private void addCandies(int c)
	{
		for (int i = 0; i < c; i++)
		{
			addCandy();
		}
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
