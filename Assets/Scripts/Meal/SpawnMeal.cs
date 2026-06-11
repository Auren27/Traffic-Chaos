using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMeal : MonoBehaviour
{
    //public Player player;

    [Header("Настройки спавна")]
    [SerializeField] private GameObject[] prefabsToSpawn; // Массив префабов, которые будут спавниться
    private float spawnInterval = 0.5f;   // Интервал между спавном (в секундах)

    [Header("Границы по горизонтали")]
    [SerializeField] private float minX = -8f;            // Левая граница спавна
    [SerializeField] private float maxX = 8f;             // Правая граница спавна

    private float timer;

    [Header("Массив хранения")]
    private int maxObjectsCount = 30;    // Вынес размер в инспектор для удобства
    [SerializeField] private GameObject[] MassObject;
    private int currentIndex = 0;                         // Индекс для циклической перезаписи

    private void Awake()
    {
        MassObject = new GameObject[maxObjectsCount];
    }

    void Update()
    {
        if (GameManager.Instance.active_game)
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                SpawnObject();
                timer = 0f; // Сбрасываем таймер
            }
        }
    }

    void SpawnObject()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning("Забыли добавить префабы в массив SpawnMeal!");
            return;
        }

        // 1. Ищем свободное место в массиве (если кто-то уже удалился)
        int spawnIndex = -1;
        for (int i = 0; i < MassObject.Length; i++)
        {
            if (MassObject[i] == null)
            {
                spawnIndex = i;
                break; // Нашли пустое место, выходим из цикла
            }
        }

        // 2. Если свободных мест нет, берем индекс по кругу и перезаписываем
        if (spawnIndex == -1)
        {
            spawnIndex = currentIndex;

            // Уничтожаем старый объект на сцене, который лежал в этой ячейке
            if (MassObject[spawnIndex] != null)
            {
                Destroy(MassObject[spawnIndex]);
            }

            // Сдвигаем циклический индекс для следующего раза
            currentIndex = (currentIndex + 1) % MassObject.Length;
        }

        // 3. Логика спавна объекта
        int randomIndex = Random.Range(0, prefabsToSpawn.Length);
        GameObject prefab = prefabsToSpawn[randomIndex];

        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, transform.position.z);

        // Создаем объект и сразу сохраняем его в найденную ячейку массива
        GameObject nobj = Instantiate(prefab, spawnPosition, Quaternion.identity, gameObject.transform);
        MassObject[spawnIndex] = nobj;
    }

    public void OFFMassMeal()
    {
        for (int i = 0; i < MassObject.Length; i++)
        {
            Destroy(MassObject[i]);
        }
    }

    // Рисуем линию в окне Scene для визуального удобства
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 leftPos = new Vector3(minX, transform.position.y, transform.position.z);
        Vector3 rightPos = new Vector3(maxX, transform.position.y, transform.position.z);
        Gizmos.DrawLine(leftPos, rightPos);
    }
}