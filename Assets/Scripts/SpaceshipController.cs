using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class SpaceshipController : PoolObject
{
    public enum ControlType
    {
        Keyboard,
        KeyboardAndMouse
    }

    public ControlType SpaceshipControlType = ControlType.Keyboard;

    public Transform ShootingPositionHint;

    public float RotationSpeed = 1;
    public float AccelerationSpeed = 1;

    public float MaximumAcceleration = 5;

    public float InvincibilityTime = 3;

    public KeyCode ShootingKey = KeyCode.Space;

    public Material SpaceshipMaterial;

    public string BulletName;

    public AudioClip ShootSound;
    public AudioClip AccelerateSound;

    [HideInInspector]
    public Rigidbody m_rigidbody;

    bool invincible;
    float currentShootingTime;
    float currentInvincibleTime;

    AudioSource audioSource;
    AudioSource fireAudioSource;

    public bool Invincible
    {
        get { return invincible; }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = AccelerateSound;
        fireAudioSource = ShootingPositionHint.GetComponent<AudioSource>();
        fireAudioSource.clip = ShootSound;

        invincible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {

            Vector3 mousePosition = GameManager.Instance.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            Vector2 direction = (mousePosition - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            float rotationAxis = Input.GetAxis("Rotate");
            float accelerationValue = (SpaceshipControlType == ControlType.Keyboard) ? Input.GetAxis("Accelerate") : Utils.ClampN1P1(Input.GetAxis("Accelerate") + Input.GetAxis("AccelerateMouse"));

            if (rotationAxis != 0)
            {
                m_rigidbody.AddTorque(0, 0, -rotationAxis * RotationSpeed / 4);
            }

            if (SpaceshipControlType == ControlType.KeyboardAndMouse)
                m_rigidbody.rotation = Quaternion.Slerp(m_rigidbody.rotation, rotation, Time.deltaTime * RotationSpeed * 4);

            if (accelerationValue > 0)
            {
                m_rigidbody.velocity += transform.right * AccelerationSpeed / 10;
                m_rigidbody.velocity = Vector3.ClampMagnitude(m_rigidbody.velocity, MaximumAcceleration);

                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();
            }

            if (Input.GetKeyDown(ShootingKey) || (SpaceshipControlType == ControlType.KeyboardAndMouse && Input.GetMouseButtonDown(0)))
            {
                if (currentShootingTime >= 1 / 3)
                {
                    GameObject bullet = ObjectPool.Instance.SpawnObject(BulletName, ShootingPositionHint.position, transform.rotation);
                    currentShootingTime = 0;
                    if (ShootSound != null)
                        fireAudioSource.Play();
                }
            }

            if (invincible)
                SpaceshipMaterial.color = Color.Lerp(Color.white, Color.black, Mathf.Cos(4 * Mathf.PI * Time.time));
            else
                SpaceshipMaterial.color = Color.Lerp(SpaceshipMaterial.color, Color.white, Time.deltaTime * 2);

            if (currentInvincibleTime >= InvincibilityTime)
            {
                invincible = false;
                currentInvincibleTime = 0;
            }
            else
            {
                currentInvincibleTime += Time.deltaTime;
            }

            currentShootingTime += Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameManager.Instance.ProcessSpaceshipRespawn();

        if (collision.gameObject.GetComponent<AsteroidPoolObject>())
        {
            ObjectPool.Instance.RemoveObject(gameObject, collision.gameObject);
        }
        else
        {
            Destroy(collision.gameObject);
        }
    }

    public void ActivateInvincible()
    {
        invincible = true;
    }

    public override void Destroy(GameObject destroyer)
    { }
}
