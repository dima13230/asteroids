using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public static class Utils
{
    /// <summary>
    /// Возвращает true, если объект находится в поле обзора камеры
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="camera">Камера</param>
    /// <returns></returns>
    public static bool IsVisible(GameObject obj, Camera camera)
    {
        return IsVisible(GetObjectBounds(obj), camera);
    }

    /// <summary>
    /// Возвращает true, если границы находятся в поле обзора камеры
    /// </summary>
    /// <param name="bounds"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    public static bool IsVisible(Bounds bounds, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    /// <summary>
    /// Получает границы объекта, совмещая в одно целое границы всех дочерних объектов, имеющих какой-либо отрисовщик
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <returns>Границы объекта</returns>
    public static Bounds GetObjectBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(obj.transform.position, Vector3.zero);

        var b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            b.Encapsulate(renderers[i].bounds);
        }

        return b;
    }

    /// <summary>
    /// Получает случайный элемент из enum
    /// </summary>
    /// <typeparam name="T">Тип</typeparam>
    /// <returns>Случайный элемент из enum</returns>
    public static T RandomEnumValue<T>() where T : Enum
    {
        Array values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(RandomInt(0, values.Length - 1));
    }

    /// <summary>
    /// Целочисленный рандом включая максимальное число
    /// </summary>
    /// <param name="min">Минимальное число</param>
    /// <param name="max">Максимальное число, рандом считается включая это число</param>
    /// <returns></returns>
    public static int RandomInt(int min, int max)
    {
        return Random.Range(min, max + 1);
    }

    /// <summary>
    /// Ограничивает значение числа между -1 и 1
    /// </summary>
    /// <param name="value">Число</param>
    /// <returns>Ограниченное число</returns>
    public static float ClampN1P1(float value) => Mathf.Clamp(value, -1, 1);
}