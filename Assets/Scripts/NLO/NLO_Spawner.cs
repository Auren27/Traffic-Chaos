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

    [Header("Настройки Пружины")]
    [SerializeField] private GameObject springPrefab; // префаб пружины
    [SerializeField] private float springYOffset = 2f; // На сколько выше НЛО спавнить пружину

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

        // Позиция: текущая позиция НЛО + смещение по Y вверх
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + springYOffset, transform.position.z);

        // Спавним пружину без наклона (Quaternion.identity) или со случайным, если нужно
        GameObject spring = Instantiate(springPrefab, spawnPosition, Quaternion.identity, null);
    }
}
