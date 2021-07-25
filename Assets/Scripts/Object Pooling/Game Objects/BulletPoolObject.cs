using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletPoolObject : PoolObject
{
    public enum BulletOwnership
    {
        Player,
        UFO
    }


    public BulletOwnership BulletOwner;
    public float BulletSpeed = 3;

    float bulletLifetime;

    float bulletCurrentLifetime;

    void Start()
    {
        bulletLifetime = GameManager.Instance.ScreenWidthToWorld / BulletSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (bulletCurrentLifetime >= bulletLifetime)
            ObjectPool.Instance.RemoveObject(gameObject, gameObject);

        GetComponent<Rigidbody>().velocity = transform.right * GameManager.Instance.BulletsSpeed;

        bulletCurrentLifetime += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        PoolObject poolObject = other.transform.root.GetComponent<PoolObject>();

        if ((BulletOwner == BulletOwnership.Player || BulletOwner == BulletOwnership.UFO) && !(poolObject is SpaceshipController) && !(poolObject is UFO))
        {
            ObjectPool.Instance.RemoveObject(gameObject, poolObject.gameObject);
            ObjectPool.Instance.RemoveObject(gameObject, gameObject);
        }
        else if (BulletOwner == BulletOwnership.UFO && poolObject is SpaceshipController)
        {
            GameManager.Instance.ProcessSpaceshipRespawn();
            ObjectPool.Instance.RemoveObject(gameObject, gameObject);
        }
        else if (BulletOwner == BulletOwnership.Player && poolObject is UFO)
        {
            poolObject.Destroy(gameObject);
            GameObject.Destroy(poolObject.gameObject);
            ObjectPool.Instance.RemoveObject(gameObject, gameObject);
        }
    }

    public override void Destroy(GameObject destroyer)
    {
        bulletCurrentLifetime = 0;
    }
}
