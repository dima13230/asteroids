using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AsteroidPoolObject : PoolObject
{
    public string PrefabSpawnedOnDeath;
    public int ScoresForDestroy = 100;

    public AudioClip ExplosionSound;

    Rigidbody m_rigidbody;

    public float SmallerAsteroidsRandomSpeedMin = 0.5f;
    public float SmallerAsteroidsRandomSpeedMax = 1.5f;

    // Start is called before the first frame update
    public override void Spawn()
    {
        if (PrefabSpawnedOnDeath != "")
            ObjectPool.Instance.AddObjectToPool(PrefabSpawnedOnDeath);

        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<AsteroidPoolObject>())
        {
            ObjectPool.Instance.RemoveObject(collision.gameObject, gameObject);
        }
    }

    public override void Destroy(GameObject destroyer)
    {
        if (destroyer != GameManager.Instance.gameObject)
            AudioSource.PlayClipAtPoint(ExplosionSound, transform.position);

        if (destroyer.GetComponent<BulletPoolObject>())
        {
            if (destroyer.GetComponent<BulletPoolObject>().BulletOwner == BulletPoolObject.BulletOwnership.Player)
                GameManager.Instance.Scores += ScoresForDestroy;

            SpawnSmallerAsteroids();
        }
        else if (destroyer.GetComponent<AsteroidPoolObject>())
        {
            SpawnSmallerAsteroids();
        }
        else if (destroyer.transform.root.GetComponent<SpaceshipController>() || destroyer.transform.root.GetComponent<UFO>())
        {
            ObjectPool.Instance.RemoveObject(gameObject, gameObject, false);
        }
    }

    void SpawnSmallerAsteroids()
    {
        if (PrefabSpawnedOnDeath != "")
        {
            float speedMultiplier = Random.Range(SmallerAsteroidsRandomSpeedMin, SmallerAsteroidsRandomSpeedMax);

            GameObject firstAsteroid = ObjectPool.Instance.SpawnObject(PrefabSpawnedOnDeath, transform.position, Quaternion.identity);
            GameObject secondAsteroid = ObjectPool.Instance.SpawnObject(PrefabSpawnedOnDeath, transform.position, Quaternion.identity);

            firstAsteroid.transform.position += (Quaternion.Euler(0, 0, 45) * m_rigidbody.velocity.normalized) * firstAsteroid.transform.lossyScale.x * 1.05f;
            secondAsteroid.transform.position += (Quaternion.Euler(0, 0, -45) * m_rigidbody.velocity.normalized) * secondAsteroid.transform.lossyScale.x * 1.05f;

            firstAsteroid.GetComponent<Rigidbody>().velocity = Quaternion.Euler(0, 0, 45) * m_rigidbody.velocity * speedMultiplier;
            secondAsteroid.GetComponent<Rigidbody>().velocity = Quaternion.Euler(0, 0, -45) * m_rigidbody.velocity * speedMultiplier;
        }
    }
}
