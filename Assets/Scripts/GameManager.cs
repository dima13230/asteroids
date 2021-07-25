using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Playing,
        Paused,
        MainMenu
    }

    public enum AsteroidSize
    {
        Big,
        Medium,
        Small,
        Random
    }

    public enum GameControlType
    {
        Keyboard,
        MouseAndKeyboard
    }

    public GameObject UFOPrefab;

    public GameState CurrentGameState;

    public SpaceshipController Spaceship;

    public float AsteroidSpeed = 1;

    public int TotalLifes = 4;

    public float BulletsSpeed = 3;

    int scores;
    int lifes;
    int asteroidCount = 2;
    bool asteroidSpawnScheduled;
    bool ufoSpawnScheduled;
    Camera m_camera;

    Quaternion initialSpaceshipRotation;

    public int Scores
    {
        get
        {
            return scores;
        }
        set
        {
            scores = value;
        }
    }

    public int Lifes
    {
        get
        {
            return lifes;
        }
        set
        {
            lifes = value;
        }
    }

    public float ScreenWidthToWorld
    {
        get
        {
            return m_camera.ViewportToWorldPoint(new Vector3(1.5f, 0, 10)).x;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        m_camera = Camera.main;

        lifes = TotalLifes;

        initialSpaceshipRotation =  Spaceship.transform.rotation;

        asteroidCount = 8;
        SpawnAsteroids(AsteroidSize.Random);
    }

    // Update is called once per frame
    void Update()
    {
        Mathf.Clamp(lifes, 0, TotalLifes);

        if (Input.GetKeyDown("escape"))
        {
            if (CurrentGameState == GameState.Paused)
                ResumeGame();
            else if (CurrentGameState == GameState.Playing)
                PauseGame();
            else
                Quit();
        }

        // Проходим по объектам и проверяем, находятся ли они за пределами экрана
        PoolObject[] objects = FindObjectsOfType<PoolObject>();

        foreach (PoolObject obj in objects)
        {
            if (!Utils.IsVisible(obj.gameObject, m_camera))
            {
                Vector2 objectScreenPosition = m_camera.WorldToViewportPoint(obj.transform.position);

                Vector3 resultPosition = m_camera.ViewportToWorldPoint(new Vector2(
                    (objectScreenPosition.x > 1) || (objectScreenPosition.x < 0) ? 1 - Mathf.Clamp01(objectScreenPosition.x) : Mathf.Clamp01(objectScreenPosition.x),
                    (objectScreenPosition.y > 1) || (objectScreenPosition.y < 0) ? 1 - Mathf.Clamp01(objectScreenPosition.y) : Mathf.Clamp01(objectScreenPosition.y)
                ));

                resultPosition.z = 0;

                obj.transform.position = resultPosition;
            }
        }

        UFO ufo = FindObjectOfType<UFO>();
        if (ufo == null && !ufoSpawnScheduled)
        {
            StartCoroutine(WaitForUFOSpawn());
            ufoSpawnScheduled = true;
        }

        if (CurrentGameState == GameState.Playing)
        {
            if (Lifes == 0)
            {
                GoToMainMenu();
            }

            if (!ObjectPool.Instance.HasActivePoolObjectsOfType<AsteroidPoolObject>() && !asteroidSpawnScheduled)
            {
                StartCoroutine(WaitForAsteroidSpawn());
                asteroidSpawnScheduled = true;
            }
        }
    }

    IEnumerator WaitForUFOSpawn()
    {
        yield return new WaitForSeconds(Random.Range((int)20, (int)41));

        ufoSpawnScheduled = false;

        UFO.UFODirection direction = Utils.RandomEnumValue<UFO.UFODirection>();

        Vector2 ufoPosition = m_camera.ViewportToWorldPoint( new Vector2(
            direction == UFO.UFODirection.Left ? 1 : 0,
            Random.Range(0.2f, 0.8f)
        ));

        GameObject obj = Instantiate(UFOPrefab, ufoPosition, Quaternion.identity);
        obj.GetComponent<UFO>().MovingDirection = direction;
    }

    IEnumerator WaitForAsteroidSpawn()
    {
        yield return new WaitForSeconds(2);
        SpawnAsteroids(AsteroidSize.Random);
        asteroidSpawnScheduled = false;
    }

    public void ProcessSpaceshipRespawn()
    {
        if (!Spaceship.Invincible)
        {
            Lifes--;
            Spaceship.gameObject.SetActive(false);

            StartCoroutine(WaitForSpaceshipSpawn());
        }
    }

    IEnumerator WaitForSpaceshipSpawn()
    {
        yield return new WaitForSeconds(2);

        yield return new WaitUntil(() => Physics.OverlapBox(Vector3.zero, Utils.GetObjectBounds(Spaceship.gameObject).extents).Length == 0);

        Spaceship.gameObject.SetActive(true);
        ResetSpaceshipState(true);
    }

    void SpawnAsteroids(AsteroidSize size)
    {
        for (int i = 0; i < asteroidCount; i++)
        {
            SpawnAsteroid(size);
        }
        asteroidCount++;
    }

    void SpawnAsteroid(AsteroidSize size)
    {
        string prefabName = size == AsteroidSize.Random ? "Asteroid" + ((AsteroidSize)Random.Range((int)0, (int)3)).ToString() : "Asteroid" + size.ToString();

        int screenSide = Utils.RandomInt(0, 3);

        Vector2 targetViewportPosition = Vector2.zero;
        Vector2 targetVelocity = Vector2.zero;
        switch (screenSide)
        {
            // Лево
            case 0:
                targetViewportPosition = new Vector2(0, Random.Range(0.1f, 0.9f));
                targetVelocity = new Vector2(
                    Random.Range(-AsteroidSpeed, -0.1f),
                    Random.Range(-AsteroidSpeed, AsteroidSpeed)
                );
                break;
            // Верх
            case 1:
                targetViewportPosition = new Vector2(Random.Range(0.1f, 0.9f), 0);
                targetVelocity = new Vector2(
                    Random.Range(-AsteroidSpeed, AsteroidSpeed),
                    Random.Range(-AsteroidSpeed, -0.1f)
                );
                break;
            // Право
            case 2:
                targetViewportPosition = new Vector2(1, Random.Range(0.1f, 0.9f));
                targetVelocity = new Vector2(
                    Random.Range(0.1f, AsteroidSpeed),
                    Random.Range(-AsteroidSpeed, AsteroidSpeed)
                );
                break;
            // Низ
            case 3:
                targetViewportPosition = new Vector2(Random.Range(0.1f, 0.9f), 1);
                targetVelocity = new Vector2(
                    Random.Range(-AsteroidSpeed, AsteroidSpeed),
                    Random.Range(0.1f, AsteroidSpeed)
                );
                break;
        }

        GameObject asteroid = ObjectPool.Instance.SpawnObject(prefabName, m_camera.ViewportToWorldPoint(targetViewportPosition), Quaternion.identity);
        asteroid.GetComponent<Rigidbody>().velocity = targetVelocity;
    }

    public void PauseGame()
    {
        CurrentGameState = GameState.Paused;
        Time.timeScale = 0;
    }

    public void GoToMainMenu()
    {
        CurrentGameState = GameState.MainMenu;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        CurrentGameState = GameState.Playing;
        Time.timeScale = 1;
    }

    public void NewGame()
    {
        ResumeGame();

        ResetSpaceshipState(true);

        ObjectPool.Instance.RemoveAllObjectsOfType<AsteroidPoolObject>(gameObject);
        ObjectPool.Instance.RemoveAllObjectsOfType<BulletPoolObject>(gameObject);

        UFO ufo = FindObjectOfType<UFO>();
        if (ufo)
            Destroy(ufo.gameObject);

        scores = 0;
        lifes = TotalLifes;
        asteroidCount = 2;
        SpawnAsteroids(AsteroidSize.Big);
    }

    public void SwitchControlType()
    {
        Spaceship.SpaceshipControlType = Spaceship.SpaceshipControlType == SpaceshipController.ControlType.Keyboard ? SpaceshipController.ControlType.KeyboardAndMouse : SpaceshipController.ControlType.Keyboard;
    }

    public void Quit()
    {
        Application.Quit();
    }

    void ResetSpaceshipState(bool newGame)
    {
        if (newGame)
        {
            Spaceship.ActivateInvincible();
            Spaceship.gameObject.SetActive(true);
        }

        Spaceship.transform.position = Vector3.zero;
        Spaceship.transform.rotation = initialSpaceshipRotation;
        Spaceship.m_rigidbody.velocity = Vector3.zero;
        Spaceship.m_rigidbody.angularVelocity = Vector3.zero;
    }
}
