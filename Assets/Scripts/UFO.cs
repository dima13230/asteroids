using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class UFO : PoolObject
{

    public enum UFODirection
    {
        Left,
        Right
    }

    public UFODirection MovingDirection;

    public float MovementSpeed = 3;
    public int ScoresForDestroy = 200;

    public float ShootDelayMin = 2;
    public float ShootDelayMax = 5;

    public string BulletName;

    public AudioClip FireSound;
    public AudioClip ExplosionSound;

    float shootDelay;
    float currentShootTime;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().velocity = (MovingDirection == UFODirection.Left) ? new Vector2(-MovementSpeed, 0) : new Vector2(MovementSpeed, 0);

        shootDelay = Random.Range(ShootDelayMin, ShootDelayMax);

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentShootTime >= shootDelay)
        {
            Vector2 direction = (GameManager.Instance.Spaceship.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            ObjectPool.Instance.SpawnObject(BulletName, transform.position, 
                rotation
            );

            audioSource.PlayOneShot(FireSound);

            shootDelay = Random.Range(ShootDelayMin, ShootDelayMax);
            currentShootTime = 0;
        }
        currentShootTime += Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<AsteroidPoolObject>())
        {
            ObjectPool.Instance.RemoveObject(gameObject, collision.gameObject);
        }
        GameObject.Destroy(gameObject);
    }

    public override void Destroy(GameObject destroyer)
    {
        AudioSource.PlayClipAtPoint(ExplosionSound, transform.position);

        if (destroyer.GetComponent<BulletPoolObject>())
        {
            if (destroyer.GetComponent<BulletPoolObject>().BulletOwner == BulletPoolObject.BulletOwnership.Player)
                GameManager.Instance.Scores += ScoresForDestroy;
        }
    }
}
