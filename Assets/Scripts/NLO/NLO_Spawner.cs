using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NLO_Spawner : MonoBehaviour
{
    [Header("Настройки Спавна")]
    [SerializeField] private bool activeSpawn = true;
    [SerializeField] private GameObject[] prefabsToSpawn;
    private float spawnInterval = 0.5f;

    [Header("Настройки Циклического Массива")]
    [SerializeField] private int maxObjectsCount = 30;

    [Header("Настройки Пружины (Корутиной)")]
    [SerializeField] private GameObject springPrefab; // префаб пружины
    [SerializeField] private Sprite springSprites;

    [Tooltip("На сколько ниже НЛО пружина должна приземлиться")]
    private float dropDistanceY = 2f;
    [Tooltip("Максимальный разброс влево/вправо от центра НЛО при падении")]
    private float randomRangeX = 4f;
    [Tooltip("Скорость полета пружины вниз")]
    private float throwSpeed = 5f;

    private float spawnTimer;
    private GameObject[] spawnedObjects;
    private int currentIndex = 0;

    // Свойства для внешнего управления стадииями
    public bool ActiveSpawn { get; set; }
    public float SpawnInterval { get; set; }



    private void Awake()
    {
        spawnedObjects = new GameObject[maxObjectsCount];
        ActiveSpawn = activeSpawn;
        SpawnInterval = spawnInterval;
    }

    private void Update()
    {
        if (ActiveSpawn)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= SpawnInterval)
            {
                SpawnObject();
                spawnTimer = 0f;
            }
        }
    }

    void SpawnObject()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning("Добавьте префабы в массив prefabsToSpawn!");
            return;
        }

        // 1. Ищем пустую ячейку (если объект уже был уничтожен извне)
        int spawnIndex = -1;
        for (int i = 0; i < spawnedObjects.Length; i++)
        {
            if (spawnedObjects[i] == null)
            {
                spawnIndex = i;
                break;
            }
        }

        // 2. Если свободных мест нет, заменяем самый старый объект
        if (spawnIndex == -1)
        {
            spawnIndex = currentIndex;
            if (spawnedObjects[spawnIndex] != null)
            {
                Destroy(spawnedObjects[spawnIndex]);
            }
            currentIndex = (currentIndex + 1) % spawnedObjects.Length;
        }

        // 3. Выбор случайного префаба и спавн
        int randomIndex = Random.Range(0, prefabsToSpawn.Length);
        GameObject prefab = prefabsToSpawn[randomIndex];

        // Точка спавна — текущая позиция НЛО
        Vector3 spawnPosition = transform.position;

        // Поворот на случайный угол (360 градусов вокруг оси Z)
        float randomAngle = Random.Range(-90f, 90f);
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, randomAngle);

        // Спавним в мире (без привязки к родителю, чтобы префаб летел независимо)
        GameObject newProj = Instantiate(prefab, spawnPosition, spawnRotation, null);

        // Сохраняем в циклическую структуру
        spawnedObjects[spawnIndex] = newProj;
    }

    public void ClearAllSpawnedObjects()
    {
        for (int i = 0; i < spawnedObjects.Length; i++)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

    }

    public void SpawnSpringAbove()
    {
        if (springPrefab == null)
        {
            Debug.LogWarning("Не добавлен префаб пружины в NLO_Spawner!");
            return;
        }

        // 1. Спавним в центре НЛО
        Vector3 startPosition = transform.position;
        GameObject spring = Instantiate(springPrefab, startPosition, Quaternion.identity, null);

        // Настройка спрайта пружины
        Booster boosterScript = spring.GetComponent<Booster>();
        if (boosterScript != null)
        {
            boosterScript.SetSprite(springSprites);
        }

        // 2. Рассчитываем случайную целевую точку на дороге под НЛО
        float randomX = Random.Range(-randomRangeX, randomRangeX);
        Vector3 targetPosition = new Vector3(startPosition.x + randomX, startPosition.y - dropDistanceY, startPosition.z);

        // 3. Запускаем плавное перемещение, аналогично твоему магниту
        StartCoroutine(ThrowSpringRoutine(spring, targetPosition));
    }

    // Корутина плавного выброса пружины
    private IEnumerator ThrowSpringRoutine(GameObject spring, Vector3 targetPos)
    {
        // Пока пружина существует (её не подобрали по дороге) и не долетела до цели
        while (spring != null)
        {
            // Плавно перемещаем пружину в целевую точку
            spring.transform.position = Vector3.MoveTowards(
                spring.transform.position,
                targetPos,
                throwSpeed * Time.deltaTime
            );

            // Если прилетели на место — останавливаем корутину
            if (Vector3.Distance(spring.transform.position, targetPos) < 0.05f)
            {
                yield break;
            }

            yield return null; // Ждем следующий кадр
        }
    }
}
