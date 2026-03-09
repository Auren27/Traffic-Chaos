using UnityEngine;
using System.Collections.Generic;

public class RoadManager : MonoBehaviour
{
    [Header("Префаб дороги")]
    [SerializeField] private GameObject roadPrefab;

    [Header("Активные дороги")]
    [SerializeField] private GameObject[] activeRoads;
    private int maxRoadNumber = 11;

    [Header("Папка хранения дорог")]
    public GameObject roadsPack;

    // Кэшированные данные
    private Dictionary<int, int[]> roadTransitions;
    private Dictionary<string, Sprite[]> spriteCache;

    // Оптимизация: кэш компонентов дорог
    private Road_Chareacter[] roadComponents;

    public void Initialize()
    {
        activeRoads = new GameObject[maxRoadNumber];
        roadComponents = new Road_Chareacter[maxRoadNumber];
        InitializeRoadTransitions();
        spriteCache = new Dictionary<string, Sprite[]>();
    }

    private void InitializeRoadTransitions()
    {
        roadTransitions = new Dictionary<int, int[]>
        {
            {1, new int[] {1, 2, 5, 8, 13}},
            {2, new int[] {3}},
            {3, new int[] {3, 4}},
            {4, new int[] {1}},
            {5, new int[] {6}},
            {6, new int[] {6, 7}},
            {7, new int[] {1}},
            {8, new int[] {9}},
            {9, new int[] {10}},
            {10, new int[] {10, 11}},
            {11, new int[] {12}},
            {12, new int[] {1}},
            {13, new int[] {14}},
            {14, new int[] {15}},
            {15, new int[] {15, 16}},
            {16, new int[] {17}},
            {17, new int[] {1}}
        };
    }

    public GameObject CreateRoad(Vector2 position, Transform parent)
    {
        return Instantiate(roadPrefab, position, Quaternion.identity, parent);
    }

    public void SetupRoad(int roadIndex, int roadType, GameObject roadObject)
    {
        if (roadIndex < 0 || roadIndex >= activeRoads.Length)
            return;

        activeRoads[roadIndex] = roadObject;
        roadComponents[roadIndex] = roadObject.GetComponent<Road_Chareacter>();

        SetupRoadSprites(roadIndex, roadType);
        SetupRoadCollisions(roadIndex, roadType);

        if (roadComponents[roadIndex] != null)
        {
            roadComponents[roadIndex].type_road = roadType;
        }
    }

    private void SetupRoadSprites(int roadIndex, int roadType)
    {
        if (activeRoads[roadIndex] == null || roadComponents[roadIndex] == null)
            return;

        // Заборы
        string fencePath = MODEL_WORLD.Instance.GetLevelsById(DataManager.Instance.scene_active).imageFence;
        Sprite fenceSprite = GetCachedSprite(fencePath, 0);

        foreach (var fence in roadComponents[roadIndex].fence)
        {
            if (fence != null)
                fence.GetComponent<SpriteRenderer>().sprite = fenceSprite;
        }

        // Текстуры дороги
        var levelData = MODEL_WORLD.Instance.GetLevel1ById(roadType);
        SetupRoadTexture(roadComponents[roadIndex].background[0], levelData.imageRoad_2, 1);
        SetupRoadTexture(roadComponents[roadIndex].background[1], levelData.imageRoad_2, 0);
        SetupRoadTexture(roadComponents[roadIndex].background[2], levelData.imageRoad_1, 1);
        SetupRoadTexture(roadComponents[roadIndex].background[3], levelData.imageRoad_1, 0);
        SetupRoadTexture(roadComponents[roadIndex].background[4], levelData.imageRoad_3, 1);
        SetupRoadTexture(roadComponents[roadIndex].background[5], levelData.imageRoad_3, 0);
    }

    private void SetupRoadTexture(GameObject background, string texturePath, int spriteIndex)
    {
        if (background == null) return;

        Sprite sprite = GetCachedSprite(texturePath, spriteIndex);
        if (sprite != null)
        {
            background.GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }

    private Sprite GetCachedSprite(string path, int spriteIndex)
    {
        if (!spriteCache.ContainsKey(path))
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            spriteCache[path] = sprites;
        }

        Sprite[] cachedSprites = spriteCache[path];
        if (cachedSprites != null && spriteIndex < cachedSprites.Length)
        {
            return cachedSprites[spriteIndex];
        }

        return null;
    }

    private void SetupRoadCollisions(int roadIndex, int roadType)
    {
        if (roadComponents[roadIndex] == null) return;

        for (int i = 0; i < roadComponents[roadIndex].Grass_Collisions.Length; i++)
        {
            if (roadComponents[roadIndex].Grass_Collisions[i] != null)
                roadComponents[roadIndex].Grass_Collisions[i].SetActive(i == roadType);
        }
    }

    public int GetRandomRoad(int currentRoadType)
    {
        if (roadTransitions.TryGetValue(currentRoadType, out int[] possibleRoads))
        {
            int randomIndex = Random.Range(0, possibleRoads.Length);
            int randomRoad = possibleRoads[randomIndex];
            //Debug.Log($"Рандомная дорога: {randomRoad}");
            return randomRoad;
        }

        //Debug.Log("Рандомная дорога = 1 (по умолчанию)");
        return 1;
    }

    public Vector2 GetNextRoadPosition(int currentRoadIndex)
    {
        if (currentRoadIndex < 0 || currentRoadIndex >= activeRoads.Length || activeRoads[currentRoadIndex] == null)
            return Vector2.zero;

        return new Vector2(
            activeRoads[currentRoadIndex].transform.position.x,
            activeRoads[currentRoadIndex].transform.position.y + 6.4f
        );
    }

    public GameObject GetRoadAt(int index)
    {
        return (index >= 0 && index < activeRoads.Length) ? activeRoads[index] : null;
    }

    public Road_Chareacter GetRoadComponentAt(int index)
    {
        return (index >= 0 && index < roadComponents.Length) ? roadComponents[index] : null;
    }

    public void ShiftRoadsArray()
    {
        for (int i = 0; i < activeRoads.Length - 1; i++)
        {
            activeRoads[i] = activeRoads[i + 1];
            roadComponents[i] = roadComponents[i + 1];
        }

        // Очищаем последний элемент
        activeRoads[activeRoads.Length - 1] = null;
        roadComponents[roadComponents.Length - 1] = null;
    }

    public void DestroyRoad(int index)
    {
        if (index >= 0 && index < activeRoads.Length && activeRoads[index] != null)
        {
            Destroy(activeRoads[index]);
            activeRoads[index] = null;
            roadComponents[index] = null;
        }
    }

    public void UnloadAllRoads()
    {
        for (int i = 0; i < activeRoads.Length; i++)
        {
            if (activeRoads[i] != null)
            {
                Destroy(activeRoads[i]);
                activeRoads[i] = null;
                roadComponents[i] = null;
            }
        }

        if (spriteCache != null)
            spriteCache.Clear();
    }

    public int GetActiveRoadsCount()
    {
        int count = 0;
        foreach (var road in activeRoads)
        {
            if (road != null) count++;
        }
        return count;
    }

    public GameObject[] GetAllRoads()
    {
        return activeRoads;
    }
}