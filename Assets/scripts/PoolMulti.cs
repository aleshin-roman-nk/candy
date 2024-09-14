using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoolMulti<T> where T : MonoBehaviour
{
	public T[] prefab { get; }
	public bool autoExpand { get; set; }
	public Transform container;
	private List<T> pool;

	public PoolMulti(T[] prefab, int count)
	{
		this.prefab = prefab;
		this.container = null;

		CreatePool(count);
	}

	public PoolMulti(T[] prefab, int count, Transform container)
	{
		this.prefab = prefab;
		this.container = container;
		CreatePool(count);
	}

	private void CreatePool(int count)
	{
		this.pool = new List<T>();

		for (int i = 0; i < count; i++)
			this.CreateObject();
	}

	private T CreateObject(bool isActiveByDefault = false)
	{
		var i = Random.Range(0, prefab.Length);

		var createdObject = Object.Instantiate(this.prefab[i], this.container);
		createdObject.gameObject.SetActive(isActiveByDefault);
		this.pool.Add(createdObject);
		return createdObject;
	}

	public bool HasFreeElement(out T element)
	{
		foreach (var t in this.pool)
		{
			if (!t.gameObject.activeInHierarchy)
			{
				element = t;
				t.gameObject.SetActive(true);
				return true;
			}
		}

		element = null;
		return false;
	}

	public T GetFreeElement()
	{
		if (this.HasFreeElement(out var element))
			return element;

		if (this.autoExpand)
			return this.CreateObject(true);

		throw new System.Exception($"There is no object of type {typeof(T)}");
	}

	public int ActiveObjects
	{
		get
		{
			return this.pool.Count(x => x.isActiveAndEnabled);
		}
	}
}
