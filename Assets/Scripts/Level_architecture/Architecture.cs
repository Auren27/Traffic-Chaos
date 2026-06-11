using UnityEngine;
using System.Collections;

public class Architecture : MonoBehaviour
{
    // Singleton
    public static Architecture Instance { get; private set; }

    [Header("Менеджеры")]
    [SerializeField] private RoadManager roadManager;
    [SerializeField] private BonusManager bonusManager;
    [SerializeField] private EnemyWaypointManager enemyWaypointManager;
    [SerializeField] private EnemyCarManager enemyCarManager;  // Добавлен новый менеджер

    [Header("Настройки")]
    private int maxRoadNumber = 11;
    private int passedRoads = 0;
    private bool delSpawnFlag = false;
    public int activeNumberLevel = 1; // номер запущенного уровня

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManagers()
    {
        // Инициализация менеджера дорог
        if (roadManager == null)
            roadManager = GetComponent<RoadManager>();
        roadManager.Initialize();
        roadManager.architecture = this;

        // Инициализация менеджера бонусов
        if (bonusManager == null)
            bonusManager = GetComponent<BonusManager>();
        bonusManager.Initialize(maxRoadNumber);

        // Инициализация менеджера точек противников
        if (enemyWaypointManager == null)
            enemyWaypointManager = GetComponent<EnemyWaypointManager>();
        enemyWaypointManager.Initialize(maxRoadNumber);

        // Инициализация менеджера машин противников
        if (enemyCarManager == null)
            enemyCarManager = GetComponent<EnemyCarManager>();
        enemyCarManager.Initialize(maxRoadNumber);
    }

    public void SpawnStartRoad()
    {
        int randomRoad = 1; // тип первой дороги (прямая)

        // Создаем первую дорогу
        GameObject firstRoad = roadManager.CreateRoad(new Vector2(0, 0), roadManager.roadsPack.transform);
        roadManager.SetupRoad(0, randomRoad, firstRoad);

        // Настраиваем бонусы для первой дороги
        bonusManager.SpawnBonusesForRoad(0, roadManager.GetRoadComponentAt(0));

        //// Настраиваем машины для первой дороги
        //enemyCarManager.SpawnCarsForRoad(0, roadManager.GetRoadComponentAt(0));  // Новый вызов

        // Настраиваем точки для первой дороги
        enemyWaypointManager.SetupRoadPoints(roadManager.GetRoadComponentAt(0), randomRoad, 0);

        // Регистрируем созданные машины в EnemyWaypointManager
        RegisterCarsForRoad(0);

        // Создаем остальные дороги
        for (int i = 0; i < maxRoadNumber - 1; i++)
        {
            if (i == 0) randomRoad = 1;
            else randomRoad = roadManager.GetRandomRoad(roadManager.GetRoadComponentAt(i).type_road);

            Vector2 nextPosition = roadManager.GetNextRoadPosition(i);
            GameObject newRoad = roadManager.CreateRoad(nextPosition, roadManager.roadsPack.transform);

            roadManager.SetupRoad(i + 1, randomRoad, newRoad);
            bonusManager.SpawnBonusesForRoad(i + 1, roadManager.GetRoadComponentAt(i + 1));
            if(i>2)
            {
                // Спавним машины для новой дороги
                enemyCarManager.SpawnCarsForRoad(i + 1, roadManager.GetRoadComponentAt(i + 1));  // Новый вызов
            }
            enemyWaypointManager.SetupRoadPoints(roadManager.GetRoadComponentAt(i + 1), randomRoad, i + 1);

            // Регистрируем созданные машины
            RegisterCarsForRoad(i + 1);
        }

        enemyWaypointManager.ActivateAllEnemyCars();
    }

    // Метод для регистрации машин в EnemyWaypointManager
    private void RegisterCarsForRoad(int roadIndex)
    {
        if(!delSpawnFlag)
        {
            // Получаем все машины для этой дороги
            for (int j = 0; j < enemyCarManager.carsPerRoad; j++)
            {
                int carIndex = roadIndex * enemyCarManager.carsPerRoad + j;
                GameObject car = enemyCarManager.GetCarsAt(carIndex);

                if (car != null)
                {
                    // Регистрируем машину в EnemyWaypointManager
                    enemyWaypointManager.RegisterCar(car);

                }
            }
        }
        else
        {
            GameObject car = enemyCarManager.GetCarsAt(10);

            if (car != null)
            {
                // Регистрируем машину в EnemyWaypointManager
                enemyWaypointManager.RegisterCar(car);

            }
        }
    }

    public void SpawnNewRoad(int currentRoadType)
    {
        passedRoads = (passedRoads + 1) % maxRoadNumber;

        if (!delSpawnFlag && passedRoads == 5)
        {
            delSpawnFlag = true;
        }

        if (delSpawnFlag)
        {
            ReplaceOldestRoad(currentRoadType);
        }
    }

    private void ReplaceOldestRoad(int currentRoadType)
    {
        int oldestRoadIndex = 0;
        int newestRoadIndex = maxRoadNumber - 1;

        // Удаляем старую дорогу
        roadManager.DestroyRoad(oldestRoadIndex);
        bonusManager.ClearBonusesForRoad(oldestRoadIndex);

        // ВАЖНО: НЕ удаляем машины и НЕ сдвигаем их массив
        // enemyCarManager.ClearCarsForRoad(oldestRoadIndex); - ЗАКОММЕНТИРОВАТЬ

        // Создаем новую дорогу
        int randomRoad = roadManager.GetRandomRoad(roadManager.GetRoadComponentAt(newestRoadIndex).type_road);
        Vector2 newPosition = roadManager.GetNextRoadPosition(newestRoadIndex);

        GameObject newRoad = roadManager.CreateRoad(newPosition, roadManager.roadsPack.transform);

        // Сдвигаем только дороги, бонусы и точки
        roadManager.ShiftRoadsArray();
        bonusManager.ShiftBonusesArray();
        enemyWaypointManager.ShiftPointsLists();

        // ВАЖНО: НЕ сдвигаем массив машин!
        // enemyCarManager.ShiftCarsArray(); - ЗАКОММЕНТИРОВАТЬ

        // Настраиваем новую дорогу
        roadManager.SetupRoad(newestRoadIndex, randomRoad, newRoad);
        bonusManager.SpawnBonusesForRoad(newestRoadIndex, roadManager.GetRoadComponentAt(newestRoadIndex));

        // Спавним машины для новой дороги
        enemyCarManager.SpawnCarsForRoad(newestRoadIndex, roadManager.GetRoadComponentAt(newestRoadIndex));

        enemyWaypointManager.SetupRoadPoints(roadManager.GetRoadComponentAt(newestRoadIndex), randomRoad, newestRoadIndex);

        // Регистрируем созданные машины
        RegisterCarsForRoad(newestRoadIndex);

        // Обновляем waypoints для всех активных машин ПЛАВНО
        StartCoroutine(SmoothUpdateAllEnemyWaypoints());
    }

    // Плавное обновление всех машин
    private IEnumerator SmoothUpdateAllEnemyWaypoints()
    {
        yield return new WaitForSeconds(0.1f);

        GameObject[] allCars = enemyCarManager.GetAllCars();
        for (int i = 0; i < allCars.Length; i++)
        {
            if (allCars[i] != null)
            {
                E_Car eCar = allCars[i].GetComponent<E_Car>();
                if (eCar != null)
                {
                    //eCar.UpdateWaypoints();////////////////////////////////////////////////////
                    yield return new WaitForSeconds(0.05f); // Задержка для плавности
                }
            }
        }

        enemyWaypointManager.ActivateAllEnemyCars();
    }

    // Метод для обновления waypoints всех активных машин
    private void UpdateAllEnemyWaypoints()
    {
        GameObject[] allCars = enemyCarManager.GetAllCars();
        for (int i = 0; i < allCars.Length; i++)
        {
            if (allCars[i] != null)
            {
                E_Car eCar = allCars[i].GetComponent<E_Car>();
                if (eCar != null)
                {
                    //eCar.UpdateWaypoints();//////////////////////////////////////////
                }
            }
        }
    }

    public void DestroyEnemyCar(int carId)
    {
        enemyWaypointManager.DestroyEnemyCar(carId);
    }

    public void DestroyWorldObjects()
    {
        string[] tagsToDestroy = { "Coin", "Crystal", "Petrol" };

        foreach (string tag in tagsToDestroy)
        {
            GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag(tag);
            for (int i = 0; i < objectsToDestroy.Length; i++)
            {
                Destroy(objectsToDestroy[i]);
            }
        }
    }

    public void UnloadAll()
    {
        roadManager.UnloadAllRoads();
        bonusManager.UnloadAllBonuses();
        enemyCarManager.UnloadAllCars();  // Новый вызов
        enemyWaypointManager.UnloadAllEnemies();

        passedRoads = 0;
        delSpawnFlag = false;
    }

    // Методы для получения информации
    public RoadManager GetRoadManager() => roadManager;
    public BonusManager GetBonusManager() => bonusManager;
    public EnemyWaypointManager GetEnemyWaypointManager() => enemyWaypointManager;
    public EnemyCarManager GetEnemyCarManager() => enemyCarManager;  // Новый метод

    //public GameData GetGameData()
    //{
    //    return new GameData
    //    {
    //        roads = roadManager.GetAllRoads(),
    //        bonuses = bonusManager.GetAllBonuses(),
    //        enemyCars = enemyCarManager.GetAllCars(),  // Используем машин из EnemyCarManager
    //        enemyPointsDown = enemyWaypointManager.GetAllEnemyPointsDown(),
    //        enemyPointsUp = enemyWaypointManager.GetAllEnemyPointsUp(),
    //        stats = new GameStats
    //        {
    //            totalRoads = roadManager.GetActiveRoadsCount(),
    //            totalBonuses = bonusManager.GetActiveBonusesCount(),
    //            activeEnemies = enemyCarManager.GetActiveCarsCount(),  // Используем счетчик из EnemyCarManager
    //            passedRoads = passedRoads
    //        }
    //    };
    //}

    public struct GameData
    {
        public GameObject[] roads;
        public GameObject[] bonuses;
        public GameObject[] enemyCars;  // Теперь из EnemyCarManager
        public GameObject[] enemyPointsDown;
        public GameObject[] enemyPointsUp;
        public GameStats stats;
    }

    public struct GameStats
    {
        public int totalRoads;
        public int totalBonuses;
        public int activeEnemies;
        public int passedRoads;
    }
}