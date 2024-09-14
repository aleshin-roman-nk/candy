using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMono<T> where T : MonoBehaviour
{
	public T prefab { get; }
	public bool autoExpand { get; set; }
	public Transform container;
	private List<T> pool;

	public PoolMono(T prefab, int count)
	{
		this.prefab = prefab;
		this.container = null;

		CreatePool(count);
	}

	public PoolMono(T prefab, int count, Transform container)
	{
		this.prefab = prefab;
		this.container = container;
		CreatePool(count);
	}

	private void CreatePool(int count)
	{
		this.pool = new List<T>();

		for(int i = 0; i < count; i++)
			this.CreateObject();
	}

	private T CreateObject(bool isActiveByDefault = false)
	{
		var createdObject = Object.Instantiate(this.prefab, this.container);
		createdObject.gameObject.SetActive(isActiveByDefault);
		this.pool.Add(createdObject);
		return createdObject;
	}

	public bool HasFreeElement(out T element)
	{
		foreach(var t in this.pool)
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
		if(this.HasFreeElement(out var element))
			return element;

		if(this.autoExpand)
			return this.CreateObject(true);

		throw new System.Exception($"There is no object of type {typeof(T)}");
	}
}
