using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    public GameObject[] PoolObjectsPrefabs;

    /// <summary>
    /// Получает объект префаба по данному имени
    /// </summary>
    /// <param name="name">Имя префаба</param>
    /// <returns>Объект префаба</returns>
    public GameObject GetPrefabByName(string name)
    {
        foreach (GameObject obj in PoolObjectsPrefabs)
        {
            if (obj.name == name)
                return obj;
        }
        return null;
    }

    List<GameObject> pool;

    // Start is called before the first frame update
    void Start()
    {
        pool = new List<GameObject>();
        Instance = this;
    }

    /// <summary>
    /// Создаёт и добавляет в пул объект.
    /// Объект должен содержать компонент DestroyablePoolObject.
    /// </summary>
    /// <param name="name">Название создаваемого из массива объекта</param>
    /// <param name="amount">Количество созданных объектов. По умолчанию равно 1</param>
    /// <returns>Массив созданных объектов</returns>
    public GameObject AddObjectToPool(string name)
    {
        GameObject obj = GetPrefabByName(name);

        if (obj == null)
        {
            Debug.LogError("Error adding object to pool! Failed to get prefab by name!");
            return null;
        }
        else if (obj.GetComponent<PoolObject>() == null)
        {
            Debug.LogError("Error adding object to pool! Prefab doesn't have PoolObject component!");
            return null;
        }

        GameObject instantiatedObject = Instantiate(obj.gameObject);
        instantiatedObject.SetActive(false);
        pool.Add(instantiatedObject);

        return instantiatedObject;
    }

    /// <summary>
    /// Создаёт и добавляет в пул объекты из данного массива.
    /// Объекты должны содержать компонент DestroyablePoolobject.
    /// </summary>
    /// <param name="names">Массив с именами создаваемых объектов</param>
    /// <returns>Массив с созданными объектами</returns>
    public GameObject[] AddObjectsToPool(string[] names)
    {
        GameObject[] result = new GameObject[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            result[i] = AddObjectToPool(names[i]);
        }

        return result;
    }

    /// <summary>
    /// Получает из пула объект, содержащий указанный компонент.
    /// </summary>
    /// <typeparam name="T">Класс, по которому мы будем искать объект. Должен быть дочерним к PoolObject</typeparam>
    /// <returns>Объект, содержащий указанный компонент</returns>
    public GameObject GetObjectOfType<T>()
    {
        if (typeof(T).IsSubclassOf(typeof(PoolObject)))
        {
            foreach (GameObject obj in pool)
            {
                if (obj.GetComponent(typeof(T)) && !obj.activeInHierarchy) // Если obj содержит указанный компонент и при этом неактивен, возвращаем его для последующего использования
                {
                    return obj;
                }
            }
        }

        return null; // Если ничего не было найдено, не возвращаем ничего
    }

    /// <summary>
    /// Получает из пула все активные объекты в сцене
    /// </summary>
    /// <typeparam name="T">Класс, по которому мы будем искать объект. Должен быть дочерним к PoolObject</typeparam>
    /// <returns>Массив активных в сцене объектов, содержащих указанный компонент</returns>
    public GameObject[] GetActiveObjectsOfType<T>()
    {
        List<GameObject> foundObjects = new List<GameObject>();

        if (typeof(T).IsSubclassOf(typeof(PoolObject)))
        {
            foreach (GameObject obj in pool)
            {
                if (obj.GetComponent(typeof(T)) && !obj.activeInHierarchy) // Если obj содержит указанный компонент и при этом активен, добавляем его в список
                {
                    foundObjects.Add(obj);
                }
            }
        }

        return foundObjects.ToArray();
    }

    /// <summary>
    /// Получает объект из пула по данному имени
    /// </summary>
    /// <param name="name">Название объекта</param>
    /// <returns>Объект</returns>
    public GameObject GetObjectByName(string name)
    {
        foreach (GameObject obj in pool)
        {
            if (obj.name.Contains(name) && !obj.activeInHierarchy) // Чтобы иметь возможность получить объект, даже если он является клоном и его имя соответственно будет дополнено
            {
                return obj;
            }
        }
        return null;
    }

    /// <summary>
    /// Вызывает метод "уничтожения" объекта и делает его неактивным, либо полностью удаляет объект.
    /// </summary>
    /// <param name="destroyer">От имени какого объекта происходит удаление</param>
    /// <param name="obj">Объект</param>
    /// <param name="invokeComponentDestroy">Если true, перед деактивацией выполняется метод Destroy класса DestroyablePoolObject. По умолчанию true</param>
    /// <returns>Возвращает true, если удаление произошло успешно (если объект был в списке)</returns>
    public bool RemoveObject(GameObject destroyer, GameObject obj, bool invokeComponentDestroy = true)
    {
        bool removed = false;

        if (invokeComponentDestroy)
        {
            obj.GetComponent<PoolObject>().Destroy(destroyer);
        }
        obj.SetActive(false);
        return removed;
    }

    /// <summary>
    /// Уничтожает и удаляет все объекты, содержащие компонент указанного типа.
    /// </summary>
    /// <typeparam name="T">Класс, по которому мы будем искать объект. Должен быть дочерним к DestroyablePoolObject</typeparam>
    public void RemoveAllObjectsOfType<T>(GameObject destroyer)
    {
        if (typeof(T).IsSubclassOf(typeof(PoolObject)))
        {
            foreach (GameObject obj in pool)
            {
                if ( obj.GetComponent(typeof(T)) )
                {
                    RemoveObject(destroyer, obj);
                }
            }
        }
    }

    /// <summary>
    /// Создаёт и/или делает активным указанный объект с заданной позицией и поворотом
    /// </summary>
    /// <param name="name">Имя префаба</param>
    /// <param name="position">Позиция</param>
    /// <param name="rotation">Поворот</param>
    /// <returns></returns>
    public GameObject SpawnObject(string name, Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetObjectByName(name);

        if(obj == null)
        {
            obj = AddObjectToPool(name);
        }

        obj.SetActive(true);
        obj.transform.SetPositionAndRotation(position, rotation);

        obj.GetComponent<PoolObject>().Spawn();

        return obj;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool HasActivePoolObjectsOfType<T>()
    {
        if (typeof(T).IsSubclassOf(typeof(PoolObject)))
        {
            foreach (GameObject obj in pool)
            {
                if (obj.GetComponent(typeof(T)) && obj.activeInHierarchy) // Если obj содержит указанный компонент и при этом активен, возвращаем true
                {
                    return true;
                }
            }
        }

        return false;
    }
}