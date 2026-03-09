using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyWaypointManager : MonoBehaviour
{
    //// Singleton
    //public static EnemyWaypointManager Instance { get; private set; }

    [Header("Точки движения противников")]
    public List<GameObject> enemyPointsDown = new List<GameObject>();
    public List<GameObject> enemyPointsUp = new List<GameObject>();
    private int pointsPerRoad = 4;

    [Header("Машины противников")]
    public List<GameObject> enemyCars = new List<GameObject>();

    public void Initialize(int maxRoads)
    {
        enemyPointsDown.Clear();
        enemyPointsUp.Clear();
        enemyCars.Clear();

        //// Можно предварительно выделить память для оптимизации
        //int totalPoints = pointsPerRoad * maxRoads;
        //enemyPointsDown.Capacity = totalPoints;
        //enemyPointsUp.Capacity = totalPoints;
        //enemyCars.Capacity = maxRoads * 2;
    }

    public void SetupRoadPoints(Road_Chareacter road, int roadType, int roadIndex)
    {
        if (road == null || road.Grass_Collisions.Length <= roadType || road.Grass_Collisions[roadType] == null)
            return;

        Road_colliisionPoints rcp = road.Grass_Collisions[roadType].GetComponent<Road_colliisionPoints>();
        if (rcp == null) return;

        // Точки встречного движения
        for (int j = 0; j < rcp.CarPoints_down.Length && j < pointsPerRoad; j++)
        {
            int pointIndex = roadIndex * pointsPerRoad + j;
            if (pointIndex < enemyPointsDown.Count)
            {
                // Если индекс уже существует - заменяем
                enemyPointsDown[pointIndex] = rcp.CarPoints_down[j];
            }
            else
            {
                // Если индекс не существует - добавляем новые элементы
                // Заполняем null до нужного индекса
                while (enemyPointsDown.Count <= pointIndex)
                {
                    enemyPointsDown.Add(null);
                }
                enemyPointsDown[pointIndex] = rcp.CarPoints_down[j];
            }
        }

        // Точки попутного движения
        for (int j = 0; j < rcp.CarPoints_up.Length && j < pointsPerRoad; j++)
        {
            int pointIndex = roadIndex * pointsPerRoad + j;
            if (pointIndex < enemyPointsUp.Count)
            {
                enemyPointsUp[pointIndex] = rcp.CarPoints_up[j];
            }
            else
            {
                // Заполняем null до нужного индекса
                while (enemyPointsUp.Count <= pointIndex)
                {
                    enemyPointsUp.Add(null);
                }
                enemyPointsUp[pointIndex] = rcp.CarPoints_up[j];
            }
        }
    }

    public void RegisterEnemyCars(Road_Chareacter road, int roadIndex)
    {
        if (road == null || road.enemys_car == null) return;

        for (int j = 0; j < road.enemys_car.Length; j++)
        {
            // Ищем первый пустой слот в списке
            bool slotFound = false;
            for (int z = 0; z < enemyCars.Count; z++)
            {
                if (enemyCars[z] == null)
                {
                    enemyCars[z] = road.enemys_car[j].gameObject;
                    road.enemys_car[j].number_car = z;
                    slotFound = true;
                    break;
                }
            }

            // Если пустых слотов не нашли - добавляем новую машину в конец
            if (!slotFound)
            {
                enemyCars.Add(road.enemys_car[j].gameObject);
                road.enemys_car[j].number_car = enemyCars.Count - 1;
            }
        }
    }

    public void ActivateAllEnemyCars()
    {
        for (int i = 0; i < enemyCars.Count; i++)
        {
            if (enemyCars[i] != null)
            {
                E_Car enemyCar = enemyCars[i].GetComponent<E_Car>();
                if (enemyCar != null)
                {
                    //enemyCar.UpdateWaypoints();//////////////////////////////////////////
                }
            }
        }
    }

    public void UpdateAllCarWaypoints()
    {
        List<GameObject> activeCars = enemyCars;
        foreach (var car in activeCars.ToList())
        {
            if (car != null)
            {
                E_Car eCar = car.GetComponent<E_Car>();
                if (eCar != null)
                {
                    eCar.UpdateWaypoints();
                }
            }
        }
    }

    public void ShiftPointsLists()
    {
        // Удаляем первые pointsPerRoad элементов из списка
        if (enemyPointsDown.Count >= pointsPerRoad)
        {
            enemyPointsDown.RemoveRange(0, Mathf.Min(pointsPerRoad, enemyPointsDown.Count));
        }

        if (enemyPointsUp.Count >= pointsPerRoad)
        {
            enemyPointsUp.RemoveRange(0, Mathf.Min(pointsPerRoad, enemyPointsUp.Count));
        }

        Debug.Log($"Waypoints shifted: Down[{enemyPointsDown.Count}], Up[{enemyPointsUp.Count}]");

        UpdateAllCarWaypoints();
    }

    public void DestroyEnemyCar(int carId)
    {
        if (carId >= 0 && carId < enemyCars.Count && enemyCars[carId] != null)
        {
            Debug.Log("уничтожен");
            Destroy(enemyCars[carId]);
            enemyCars[carId] = null;
        }
    }

    // Получение всех точек в виде списка
    public List<GameObject> GetAllEnemyPointsDown()
    {
        return enemyPointsDown;
    }

    public List<GameObject> GetAllEnemyPointsUp()
    {
        return enemyPointsUp;
    }

    // Получение всех машин в виде списка
    public List<GameObject> GetAllEnemyCars()
    {
        return enemyCars;
    }

    // Получение только активных машин (без null)
    public List<GameObject> GetActiveEnemyCars()
    {
        List<GameObject> activeCars = new List<GameObject>();
        foreach (var car in enemyCars)
        {
            if (car != null) activeCars.Add(car);
        }
        return activeCars;
    }

    public int GetActiveEnemyCarsCount()
    {
        int count = 0;
        foreach (var car in enemyCars)
        {
            if (car != null) count++;
        }
        return count;
    }

    public void UnloadAllEnemies()
    {
        string[] tagsToDestroy = { "Enemy" };

        foreach (string tag in tagsToDestroy)
        {
            GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag(tag);
            for (int i = 0; i < objectsToDestroy.Length; i++)
            {
                Destroy(objectsToDestroy[i]);
            }
        }

        // Очищаем списки
        enemyCars.Clear();
        enemyPointsDown.Clear();
        enemyPointsUp.Clear();
    }

    // Метод для регистрации машины
    public void RegisterCar(GameObject car)
    {
        if (car == null) return;

        // Ищем пустой слот
        for (int i = 0; i < enemyCars.Count; i++)
        {
            if (enemyCars[i] == null)
            {
                enemyCars[i] = car;

                // Устанавливаем ID машине
                E_Car eCar = car.GetComponent<E_Car>();
                if (eCar != null)
                {
                    eCar.number_car = i;
                }

                //Debug.Log($"Car registered at index {i}");
                return;
            }
        }

        // Если пустых слотов нет - добавляем в конец
        enemyCars.Add(car);
        E_Car newECar = car.GetComponent<E_Car>();
        if (newECar != null)
        {
            newECar.number_car = enemyCars.Count - 1;
        }
        //Debug.Log($"Car registered at index {enemyCars.Count - 1}");
    }

    // Новые удобные методы для работы со списками

    // Получить точки для конкретной дороги
    public List<GameObject> GetRoadPointsDown(int roadIndex)
    {
        List<GameObject> roadPoints = new List<GameObject>();
        int startIndex = roadIndex * pointsPerRoad;
        int endIndex = startIndex + pointsPerRoad;

        for (int i = startIndex; i < endIndex && i < enemyPointsDown.Count; i++)
        {
            if (enemyPointsDown[i] != null)
            {
                roadPoints.Add(enemyPointsDown[i]);
            }
        }

        return roadPoints;
    }

    public List<GameObject> GetRoadPointsUp(int roadIndex)
    {
        List<GameObject> roadPoints = new List<GameObject>();
        int startIndex = roadIndex * pointsPerRoad;
        int endIndex = startIndex + pointsPerRoad;

        for (int i = startIndex; i < endIndex && i < enemyPointsUp.Count; i++)
        {
            if (enemyPointsUp[i] != null)
            {
                roadPoints.Add(enemyPointsUp[i]);
            }
        }

        return roadPoints;
    }

    // Добавить точку вручную (удобно для тестирования)
    public void AddEnemyPointDown(GameObject point, int roadIndex)
    {
        int index = roadIndex * pointsPerRoad + (enemyPointsDown.Count % pointsPerRoad);
        if (index < enemyPointsDown.Count)
        {
            enemyPointsDown[index] = point;
        }
        else
        {
            enemyPointsDown.Add(point);
        }
    }

    public void AddEnemyPointUp(GameObject point, int roadIndex)
    {
        int index = roadIndex * pointsPerRoad + (enemyPointsUp.Count % pointsPerRoad);
        if (index < enemyPointsUp.Count)
        {
            enemyPointsUp[index] = point;
        }
        else
        {
            enemyPointsUp.Add(point);
        }
    }

    // Получить все не-null точки
    public List<GameObject> GetAllValidPointsDown()
    {
        List<GameObject> validPoints = new List<GameObject>();
        foreach (var point in enemyPointsDown)
        {
            if (point != null) validPoints.Add(point);
        }
        return validPoints;
    }

    public List<GameObject> GetAllValidPointsUp()
    {
        List<GameObject> validPoints = new List<GameObject>();
        foreach (var point in enemyPointsUp)
        {
            if (point != null) validPoints.Add(point);
        }
        return validPoints;
    }
}