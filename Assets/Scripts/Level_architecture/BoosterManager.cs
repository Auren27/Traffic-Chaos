using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterManager : MonoBehaviour
{
    [Header("Префабы бустеров")]
    [SerializeField] private GameObject[] boosterPrefabs;

    [Header("Спрайты для бустеров")]
    [SerializeField] private Sprite[] boosterSprites; // <-- НОВЫЙ МАССИВ СПРАЙТОВ

    [Header("Активные бустеры")]
    [SerializeField] private GameObject[] activeBoosters;
    private int boostersPerRoad = 1;

    [Header("Папка хранения бустеров")]
    public GameObject boostersPack;

    public void Initialize(int maxRoads)
    {
        activeBoosters = new GameObject[boostersPerRoad * maxRoads];
    }

    public void SpawnBoosterOnRoad(Road_Chareacter road, int boosterArrayIndex)
    {
        if (road == null || road.Trigger_Oblast == null)
            return;

        BoxCollider2D spawnArea = road.Trigger_Oblast;
        Bounds bounds = spawnArea.bounds;

        Vector2 spawnPosition = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );

        int layerMask = LayerMask.GetMask("Buns", "Boosters");
        Collider2D hitCollider = Physics2D.OverlapCircle(spawnPosition, 0.6f, layerMask);

        if (hitCollider == null)
        {
            int randomBooster = Random.Range(0, boosterPrefabs.Length);

            // Спавним объект
            GameObject spawnedBoosterObj = Instantiate(
                boosterPrefabs[randomBooster],
                spawnPosition,
                Quaternion.identity,
                boostersPack.transform
            );

            activeBoosters[boosterArrayIndex] = spawnedBoosterObj;

            // НАСТРОЙКА РАНДОМНОГО СПРАЙТА
            if (boosterSprites != null && boosterSprites.Length > 0)
            {
                Booster boosterScript = spawnedBoosterObj.GetComponent<Booster>();
                if (boosterScript != null)
                {
                    int randomSpriteIndex = Random.Range(0, boosterSprites.Length);
                    boosterScript.SetSprite(boosterSprites[randomSpriteIndex]);
                }
            }
        }
    }

    public void SpawnBoostersForRoad(int roadIndex, Road_Chareacter road)
    {
        if (road == null) return;

        if (Random.value < 0.5f)
        {
            for (int j = 0; j < boostersPerRoad; j++)
            {
                SpawnBoosterOnRoad(road, roadIndex * boostersPerRoad + j);
            }
        }
    }

    public void ClearBoostersForRoad(int roadIndex)
    {
        for (int j = 0; j < boostersPerRoad; j++)
        {
            int boosterIndex = roadIndex * boostersPerRoad + j;
            if (boosterIndex >= 0 && boosterIndex < activeBoosters.Length && activeBoosters[boosterIndex] != null)
            {
                Destroy(activeBoosters[boosterIndex]);
                activeBoosters[boosterIndex] = null;
            }
        }
    }

    public void ShiftBoostersArray()
    {
        for (int i = 0; i < activeBoosters.Length - boostersPerRoad; i++)
        {
            activeBoosters[i] = activeBoosters[i + boostersPerRoad];
        }

        for (int i = activeBoosters.Length - boostersPerRoad; i < activeBoosters.Length; i++)
        {
            activeBoosters[i] = null;
        }
    }

    public GameObject GetBoosterAt(int index)
    {
        return (index >= 0 && index < activeBoosters.Length) ? activeBoosters[index] : null;
    }

    public int GetActiveBoostersCount()
    {
        int count = 0;
        foreach (var booster in activeBoosters)
        {
            if (booster != null) count++;
        }
        return count;
    }

    public void UnloadAllBoosters()
    {
        for (int i = 0; i < activeBoosters.Length; i++)
        {
            if (activeBoosters[i] != null)
            {
                Destroy(activeBoosters[i]);
                activeBoosters[i] = null;
            }
        }
    }

    public GameObject[] GetAllBoosters()
    {
        return activeBoosters;
    }
}
