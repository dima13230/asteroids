using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedDestroyPoolObject : PoolObject
{
    
    public float DestroyDelay = 1;

    public override void Spawn()
    {
        StartCoroutine(WaitForDestroy());
    }

    public IEnumerator WaitForDestroy()
    {
        yield return new WaitForSeconds(DestroyDelay);
        ObjectPool.Instance.RemoveObject(gameObject ,gameObject);
    }
}