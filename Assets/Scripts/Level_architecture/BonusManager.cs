using UnityEngine;

public class BonusManager : MonoBehaviour
{
    [Header("Префабы бонусов")]
    [SerializeField] private GameObject[] bonusPrefabs;

    [Header("Активные бонусы")]
    [SerializeField] private GameObject[] activeBonuses;
    private int bonusesPerRoad = 3;

    [Header("Папка хранения бонусов")]
    public GameObject bonusesPack;

    public void Initialize(int maxRoads)
    {
        activeBonuses = new GameObject[bonusesPerRoad * maxRoads];
    }

    public void SpawnBonusOnRoad(Road_Chareacter road, int bonusArrayIndex)
    {
        if (road == null || road.Trigger_Oblast == null)
            return;

        BoxCollider2D spawnArea = road.Trigger_Oblast;
        Bounds bounds = spawnArea.bounds;

        // Генерируем случайную точку внутри прямоугольника
        Vector2 spawnPosition = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );

        // Проверяем коллизии
        Collider2D hitCollider = Physics2D.OverlapCircle(spawnPosition, 0.5f, LayerMask.GetMask("Buns"));
        if (hitCollider == null)
        {
            int randomBonus = Random.Range(0, bonusPrefabs.Length);
            activeBonuses[bonusArrayIndex] = Instantiate(
                bonusPrefabs[randomBonus],
                spawnPosition,
                Quaternion.identity,
                bonusesPack.transform
            );
        }
    }

    public void SpawnBonusesForRoad(int roadIndex, Road_Chareacter road)
    {
        if (road == null) return;

        for (int j = 0; j < bonusesPerRoad; j++)
        {
            SpawnBonusOnRoad(road, roadIndex * bonusesPerRoad + j);
        }
    }

    public void ClearBonusesForRoad(int roadIndex)
    {
        for (int j = 0; j < bonusesPerRoad; j++)
        {
            int bonusIndex = roadIndex * bonusesPerRoad + j;
            if (bonusIndex >= 0 && bonusIndex < activeBonuses.Length && activeBonuses[bonusIndex] != null)
            {
                Destroy(activeBonuses[bonusIndex]);
                activeBonuses[bonusIndex] = null;
            }
        }
    }

    public void ShiftBonusesArray()
    {
        // Сдвигаем бонусы на одну позицию влево
        for (int i = 0; i < activeBonuses.Length - bonusesPerRoad; i++)
        {
            activeBonuses[i] = activeBonuses[i + bonusesPerRoad];
        }

        // Очищаем последние элементы
        for (int i = activeBonuses.Length - bonusesPerRoad; i < activeBonuses.Length; i++)
        {
            activeBonuses[i] = null;
        }
    }

    public GameObject GetBonusAt(int index)
    {
        return (index >= 0 && index < activeBonuses.Length) ? activeBonuses[index] : null;
    }

    public int GetActiveBonusesCount()
    {
        int count = 0;
        foreach (var bonus in activeBonuses)
        {
            if (bonus != null) count++;
        }
        return count;
    }

    public void UnloadAllBonuses()
    {
        for (int i = 0; i < activeBonuses.Length; i++)
        {
            if (activeBonuses[i] != null)
            {
                Destroy(activeBonuses[i]);
                activeBonuses[i] = null;
            }
        }
    }

    public GameObject[] GetAllBonuses()
    {
        return activeBonuses;
    }
}