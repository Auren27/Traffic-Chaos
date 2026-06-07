using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MODEL_WORLD : MonoBehaviour
{
    // Singleton
    public static MODEL_WORLD Instance { get; private set; }

    public int active_car_id = 1;
    public int active_levels_id = 1;
    //[SerializeField] private GameObject Button_Start;
    private bool online_json = true;

    // полоска загрузки
    [SerializeField] private Image Strip;
    [SerializeField] private Sprite[] sprite_strip;

    [SerializeField] private VehicleDatabaseCar vehicleDatabaseCar;// Хранилище загруженных данных
    // для полоски загрузки
    int mmin = 0;
    int mmax = 5;

    [System.Serializable]
    public class VehicleDataCar
    {
        public int id;
        public string name;
        public string imagePath;

        public float speed_min;
        public float speed_max;
        public float grounded_speed_min;
        public float grounded_speed_max;

        public float transfer_time;
        public float rotationSpeed;
        public float petrol_rashod;

        public string description; // пару слов о машине

        public string purchase_method; // способ оплаты
        public int currency; // кол-во валюты
        public bool purchase; //куплен?
    }

    [System.Serializable]
    public class VehicleDatabaseCar
    {
        public List<VehicleDataCar> vehicles;// Список всех машин
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // метод загрузки данных (с сервера)
    protected IEnumerator DownloadVehicleDataFromServer()
    {
        string url = "http://game.ispu.ru/lebedev/api.php?action=get_vehicles";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ошибка загрузки с сервера: " + request.error);
                online_json = false;
            }
            else
            {
                string jsonData = request.downloadHandler.text;

                // Проверяем, не вернул ли PHP ошибку
                if (jsonData.Contains("\"error\""))
                {
                    Debug.LogError("Ошибка от сервера: " + jsonData);
                    online_json = false;
                }
                else
                {
                    // Успешная загрузка
                    vehicleDatabaseCar = JsonConvert.DeserializeObject<VehicleDatabaseCar>(jsonData);
                    Debug.Log($"Успешно загружено {vehicleDatabaseCar.vehicles.Count} машин с сервера");

                    // Сохраняем резервную копию
                    SaveLocalBackup(jsonData);
                    SetupVehicleUICar();
                }
            }
        }
    }

    // Загрузка данных об уровнях (с сервера)
    protected IEnumerator DownloadLevelsDataFromServer()
    {
        string url = "http://game.ispu.ru/lebedev/api.php?action=get_levels";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text;
                if (!jsonData.Contains("\"error\""))
                {
                    // Десериализуем данные уровней
                    // LevelDatabase levelData = JsonConvert.DeserializeObject<LevelDatabase>(jsonData);
                    Debug.Log("Данные уровней загружены с сервера");
                }
            }
        }
    }

    // Загрузка пользовательских данных (с сервера)
    protected IEnumerator DownloadUserDataFromServer(string userId)
    {
        string url = $"http://game.ispu.ru/lebedev/api.php?action=get_user_data&user_id={userId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text;
                if (!jsonData.Contains("\"error\""))
                {
                    // Десериализуем пользовательские данные
                    // UserData userData = JsonConvert.DeserializeObject<UserData>(jsonData);
                    Debug.Log("Данные пользователя загружены с сервера");
                }
            }
        }
    }

    protected IEnumerator LoadVehicleDataCar()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "vehicles.json");

        #if UNITY_EDITOR || UNITY_STANDALONE
        filePath = "file://" + filePath;
        #elif UNITY_ANDROID
        filePath = "jar:file://" + Application.dataPath + "!/assets/vehicles.json";
        #endif

        using (UnityWebRequest request = UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ошибка загрузки: " + request.error);
                yield break;
            }

            string jsonData = request.downloadHandler.text;
            vehicleDatabaseCar = JsonConvert.DeserializeObject<VehicleDatabaseCar>(jsonData);
        }
    }

    // Сохранение данных как локальной резервной копии
    private void SaveLocalBackup(string jsonData)
    {
        #if UNITY_EDITOR || UNITY_STANDALONE
        string backupPath = System.IO.Path.Combine(Application.dataPath, "StreamingAssets", "vehicles_backup.json");
        System.IO.File.WriteAllText(backupPath, jsonData);
        Debug.Log("Резервная копия сохранена: " + backupPath);
        #endif
    }

    // Метод для настройки UI
    protected void SetupVehicleUICar()
    {
        if (vehicleDatabaseCar != null && vehicleDatabaseCar.vehicles.Count > 0)
        {
            VehicleDataCar currentVehicle = GetVehicleByIdCar(active_car_id);
            // Обновляем UI на основе currentVehicle
        }
    }

    public VehicleDataCar GetVehicleByIdCar(int id)
    {
        // Ищем машину по ID в списке
        return vehicleDatabaseCar.vehicles.Find(v => v.id == id);
    }

    public List<VehicleDataCar> GetAllVehiclesCar()
    {
        // Возвращаем весь список машин
        return vehicleDatabaseCar.vehicles;
    }

    //levels

    [SerializeField] private LevelDatabaseCar levelsDatabase; // Хранилище загруженных данных

    [System.Serializable]
    public class LevelDataCar
    {
        public int id;
        public int currency; // кол-во км
        public bool purchase; // открыт?
        public bool purchase_car; // открыта машина?
        public string imageFence; // путь к изображению забора
    }

    [System.Serializable]
    public class LevelDatabaseCar
    {
        public List<LevelDataCar> levels; // Список всех уровней (обновлено)
    }

    protected IEnumerator LoadLevelDataCar() // Имя метода обновлено
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "levels.json");

        #if UNITY_EDITOR || UNITY_STANDALONE
                filePath = "file://" + filePath;
        #elif UNITY_ANDROID
                            filePath = "jar:file://" + Application.dataPath + "!/assets/levels.json";
        #endif

        using (UnityWebRequest request = UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ошибка загрузки: " + request.error);
                yield break;
            }

            string jsonData = request.downloadHandler.text;
            levelsDatabase = JsonConvert.DeserializeObject<LevelDatabaseCar>(jsonData);
        }
    }

    //// Метод для настройки UI
    //protected void SetupLevelUI()
    //{
    //    if (levelDatabaseCar != null && levelDatabaseCar.levels.Count > 0)
    //    {
    //        LevelDataCar currentLevel = GetLevelById(active_car_id);
    //        // Обновляем UI на основе currentLevel
    //    }
    //}

    public LevelDataCar GetLevelsById(int id) // Имя метода обновлено
    {
        // Ищем уровень по ID в списке
        return levelsDatabase.levels.Find(v => v.id == id);
    }

    public List<LevelDataCar> GetAllLevels() // Имя метода обновлено
    {
        // Возвращаем весь список уровней
        return levelsDatabase.levels;
    }

    // 1. Универсальный класс для данных дорог на уровне
    [System.Serializable]
    public class RoadLevelData
    {
        public int id;
        public string imageRoad_1; // путь к изображению дороги (лево)
        public string imageRoad_2; // путь к изображению дороги (центр)
        public string imageRoad_3; // путь к изображению дороги (право)
        public string imageRoad_4; // путь к изображению дороги (дорога за забором)
        public string description; // комментарий
    }

    // 2. Универсальная обёртка для базы данных уровня
    [System.Serializable]
    public class RoadLevelDatabase
    {
        // Эти свойства нужны для совместимости, если в JSON ключи называются "level1" или "level2"
        [JsonProperty("level1")]
        private List<RoadLevelData> Level1Setter { set => levelsRoad = value; }

        [JsonProperty("level2")]
        private List<RoadLevelData> Level2Setter { set => levelsRoad = value; }

        // Основной список, куда в итоге попадают данные
        [JsonProperty("levelsRoad")]
        public List<RoadLevelData> levelsRoad { get; set; } = new List<RoadLevelData>();
    }

    // Хранилища для загруженных данных разных уровней
    public RoadLevelDatabase level1Database;
    public RoadLevelDatabase level2Database;

    // Универсальный метод загрузки данных из JSON
    private IEnumerator LoadLevelData(string fileName, System.Action<RoadLevelDatabase> callback)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        #if UNITY_EDITOR || UNITY_STANDALONE
                filePath = "file://" + filePath;
        #elif UNITY_ANDROID
                        filePath = "jar:file://" + Application.dataPath + "!/assets/" + fileName;
        #endif

        using (UnityWebRequest request = UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Ошибка загрузки {fileName}: " + request.error);
                yield break;
            }

            string jsonData = request.downloadHandler.text;
            RoadLevelDatabase db = JsonConvert.DeserializeObject<RoadLevelDatabase>(jsonData);

            // Возвращаем результат через callback
            callback?.Invoke(db);
        }
    }

    // Метод для запуска загрузки (вызывай его на старте игры)
    public void InitAllLevelsLoading()
    {
        StartCoroutine(LoadLevelData("level1.json", result => level1Database = result));
        StartCoroutine(LoadLevelData("level2.json", result => level2Database = result));
    }

    // --- УНИВЕРСАЛЬНЫЕ МЕТОДЫ ПОЛУЧЕНИЯ ДАННЫХ ---

    // Поиск конкретного элемента по ID в нужной базе данных
    // Пример вызова: RoadLevelData data = GetRoadDataById(level1Database, 2);
    public RoadLevelData GetRoadDataById(RoadLevelDatabase database, int id)
    {
        return database?.levelsRoad?.Find(v => v.id == id);
    }

    // Получение всего списка дорог из нужной базы данных
    // Пример вызова: List<RoadLevelData> allRoads = GetAllRoads(level1Database);
    public List<RoadLevelData> GetAllRoads(RoadLevelDatabase database)
    {
        return database?.levelsRoad;
    }

    //public IEnumerator Download()
    //{
    //    int mmin = 0;
    //    int mmax = 5;

    //    yield return StartCoroutine(LoadLevelDataCar());
    //    // После этой точки данные гарантированно загружены
    //    //SetupLevelUI();

    //    int m = 0 + ((1 - mmin) / (mmax - mmin) * (14 - 0));
    //    Strip.sprite = sprite_strip[m];

    //    yield return StartCoroutine(LoadVehicleDataCar());
    //    // После этой точки данные гарантированно загружены
    //    SetupVehicleUICar();

    //    m = 0 + ((2 - mmin) / (mmax - mmin) * (14 - 0));
    //    Strip.sprite = sprite_strip[m];

    //    yield return StartCoroutine(LoadLevel_1_Data());
    //    // После этой точки данные гарантированно загружены
    //    //SetupVehicleUICar();

    //    m = 0 + ((3 - mmin) / (mmax - mmin) * (14 - 0));
    //    Strip.sprite = sprite_strip[m];

    //    //gameObject.GetComponent<Model_car>().Start_Model();

    //    //m = 0 + ((3 - mmin) / (mmax - mmin) * (14 - 0));
    //    //Strip.sprite = sprite_strip[m];

    //    gameObject.GetComponent<MenuController>().Levels_Download();

    //    m = 0 + ((4 - mmin) / (mmax - mmin) * (14 - 0));
    //    Strip.sprite = sprite_strip[m];

    //    //arh.Spawn_Start_Road();

    //    //m = 0 + ((5 - mmin) / (mmax - mmin) * (14 - 0));
    //    //Strip.sprite = sprite_strip[m];
    //}


    private void DownloadLane(int i)
    {
        int m = 0 + ((i - mmin) / (mmax - mmin) * (14 - 0));
        Strip.sprite = sprite_strip[m];
    }

    public IEnumerator Download()
    {

        ////////////////////////////////////////////////////////////////////////////////////////////////
        //yield return StartCoroutine(DownloadVehicleDataFromServer()); /// загрузка онлайн данных

        //if (!online_json) /// загрузка онлайн данных повторно
        //{
        //    online_json = true;
        //    yield return StartCoroutine(DownloadVehicleDataFromServer()); /// загрузка онлайн данных
        //}

        //DownloadLane(1);

        //if (!online_json) /// загрузка офлайн данных
        //{

            yield return StartCoroutine(LoadVehicleDataCar());
            SetupVehicleUICar();

            DownloadLane(2);

        //}
        ////////////////////////////////////////////////////////////////////////////////////////////////
        yield return StartCoroutine(LoadLevelDataCar());

        DownloadLane(3);

        yield return StartCoroutine(LoadLevelData("level1.json", result => level1Database = result)); ;

        DownloadLane(4);

        yield return StartCoroutine(LoadLevelData("level2.json", result => level2Database = result)); ;

        DownloadLane(5);

        DataManager.Instance.Levels_Download();

        //for (int i = 0; i < EnemyCarManager.Instance.activeCars.Length; i++)
        //{
        //    EnemyCarManager.Instance.activeCars[i].GetComponentInChildren<Enemy_JSON>().StartJSON_Car();
        //}
    }

}
