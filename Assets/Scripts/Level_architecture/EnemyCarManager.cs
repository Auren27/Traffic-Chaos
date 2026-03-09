using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class EnemyCarManager : MonoBehaviour
{
    // Singleton
    public static EnemyCarManager Instance { get; private set; }

    [Header("Префаб машины")]
    [SerializeField] private GameObject[] enemyCarsPrefabs;

    [Header("Активные машины")]
    public GameObject[] activeCars;
    public int carsPerRoad = 1;

    [Header("Папка хранения машин")]
    public GameObject carsPack;

    public void Initialize(int maxRoads)
    {
        activeCars = new GameObject[carsPerRoad * maxRoads];
    }

    public void SpawnCarsOnRoad(Road_Chareacter road, int carArrayIndex)
    {
        if (road == null || road.Trigger_Oblast == null)
            return;

        BoxCollider2D spawnArea = road.Trigger_Oblast;
        Bounds cars = spawnArea.bounds;

        // Генерируем случайную точку внутри прямоугольника
        Vector2 spawnPosition = new Vector2(
            Random.Range(cars.min.x, cars.max.x),
            Random.Range(cars.min.y, cars.max.y)
        );

        // Проверяем коллизии
        Collider2D hitCollider = Physics2D.OverlapCircle(spawnPosition, 0.5f, LayerMask.GetMask("EnemyCars"));
        if (hitCollider == null)
        {
            //Debug.Log(carArrayIndex);
            int randomCars = Random.Range(0, enemyCarsPrefabs.Length);
            activeCars[carArrayIndex] = Instantiate(
                enemyCarsPrefabs[randomCars],
                spawnPosition,
                Quaternion.identity,
                carsPack.transform
            );
        }
    }

    public void SpawnCarsForRoad(int roadIndex, Road_Chareacter road)
    {
        if (road == null) return;

        for (int j = 0; j < carsPerRoad; j++)
        {
            SpawnCarsOnRoad(road, roadIndex * carsPerRoad + j);
        }
    }

    //public void ClearCarsForRoad(int roadIndex)
    //{
    //    // Только отмечаем машины для обновления, не удаляем
    //    for (int j = 0; j < carsPerRoad; j++)
    //    {
    //        int carsIndex = roadIndex * carsPerRoad + j;
    //        if (carsIndex >= 0 && carsIndex < activeCars.Length && activeCars[carsIndex] != null)
    //        {
    //            // Помечаем машину для обновления waypoints
    //            E_Car eCar = activeCars[carsIndex].GetComponent<E_Car>();
    //            if (eCar != null)
    //            {
    //                // Вызываем плавное обновление
    //                eCar.UpdateWaypoints();
    //            }

    //            // Оставляем машину в сцене
    //        }
    //    }
    //}

    //public void ShiftCarsArray()
    //{
    //    // Сдвигаем бонусы на одну позицию влево
    //    for (int i = 0; i < activeCars.Length - carsPerRoad; i++)
    //    {
    //        activeCars[i] = activeCars[i + carsPerRoad];
    //    }

    //    // Очищаем последние элементы
    //    for (int i = activeCars.Length - carsPerRoad; i < activeCars.Length; i++)
    //    {
    //        activeCars[i] = null;
    //    }
    //}

    public GameObject GetCarsAt(int index)
    {
        return (index >= 0 && index < activeCars.Length) ? activeCars[index] : null;
    }

    public int GetActiveCarsCount()
    {
        int count = 0;
        foreach (var car in activeCars)
        {
            if (car != null) count++;
        }
        return count;
    }

    public void UnloadAllCars()
    {
        for (int i = 0; i < activeCars.Length; i++)
        {
            if (activeCars[i] != null)
            {
                Debug.Log("уничтожен");
                Destroy(activeCars[i]);
                activeCars[i] = null;
            }
        }
    }

    public GameObject[] GetAllCars()
    {
        return activeCars;
    }
}
