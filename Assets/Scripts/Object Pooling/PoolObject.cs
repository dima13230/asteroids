using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    public virtual void Destroy(GameObject destroyer) {}
    public virtual void Spawn() {}
}
