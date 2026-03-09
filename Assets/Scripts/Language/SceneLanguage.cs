using UnityEngine;

public class SceneLanguage : MonoBehaviour
{
    public static SceneLanguage Instance;
    public int isTypeLanguage;
    public bool isStart;

    void Awake()
    {
        // Проверяем, есть ли уже экземпляр этого объекта
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Уничтожаем объект не будем при переходе между сценами
        }
        else
        {
            Destroy(gameObject); // Уничтожаем дубликаты
        }
    }
}
