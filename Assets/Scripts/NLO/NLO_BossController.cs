using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NLO_BossController : MonoBehaviour
{
    private NLO_Move moveScript;
    private NLO_Spawner spawnerScript;

    [Header("Ссылки на компоненты")]
    [SerializeField] private Renderer[] ufoRenderer; // Сюда перетащи графику корпуса (где материал/спрайт)

    [Header("Настройки таймингов фаз (в секундах)")]
    [SerializeField] private float calmDuration = 20f;
    [SerializeField] private float transitionToAggressiveDuration = 3f;
    [SerializeField] private float aggressiveDuration = 10f;
    [SerializeField] private float tiredDuration = 15f;

    private Color originalColor = Color.white;

    private void Start()
    {
        moveScript = GetComponent<NLO_Move>();
        spawnerScript = GetComponent<NLO_Spawner>();

        if (ufoRenderer != null)
        {
            // Запоминаем исходный цвет НЛО (чтобы возвращаться к нему из красного)
            originalColor = ufoRenderer[0].material.color;
        }

        // Запуск бесконечного цикла стадий босса
        StartCoroutine(BossLifecycleRoutine());
    }

    private IEnumerator BossLifecycleRoutine()
    {
        while (true)
        {
            // === СТАДИЯ 1: Спокойная ===
            Debug.Log("Стадия: Спокойная");
            if (ufoRenderer != null) 
            {
                for (int i = 0; i < ufoRenderer.Length; i++)
                {
                    ufoRenderer[i].material.color = originalColor;
                }
            }
            moveScript.CurrentRotationSpeed = 90f;
            moveScript.CurrentFollowSpeed = 3f;      // Менее подвижная (базовая скорость ниже)
            spawnerScript.ActiveSpawn = true;
            spawnerScript.SpawnInterval = 1.2f;      // Чуть менее активный спавн (интервал больше)
            spawnerScript.SpawnSpringAbove(); // Спавним пружину
            yield return new WaitForSeconds(calmDuration);

            // === ПЕРЕХОД 1: НЛО краснеет ===
            Debug.Log("Переход: НЛО закипает...");
            if (ufoRenderer != null)
            {
                for (int i = 0; i < ufoRenderer.Length; i++)
                {
                    ufoRenderer[i].material.color = Color.red; // Краснеет
                }
            }

            moveScript.CurrentRotationSpeed = 200f;   // Скорость прокрутки увеличилась на 150
            spawnerScript.ActiveSpawn = false;        // Во время перехода не спавним (по желанию)

            yield return new WaitForSeconds(transitionToAggressiveDuration);


            // === СТАДИЯ 2: Агрессивная ===
            Debug.Log("Стадия: АГРЕССИВНАЯ!");
            if (ufoRenderer != null)
            {
                for (int i = 0; i < ufoRenderer.Length; i++)
                {
                    ufoRenderer[i].material.color = originalColor;
                }
            }
            // Возвращаем исходную скорость вращения (или оставляем высокую, если нужно — сейчас 90)
            moveScript.CurrentRotationSpeed = 90f;
            moveScript.CurrentFollowSpeed = 6f;       // Активно летает за игроком
            spawnerScript.ActiveSpawn = true;
            spawnerScript.SpawnInterval = 0.4f;       // Очень активный спавн

            yield return new WaitForSeconds(aggressiveDuration);


            // === ПЕРЕХОД 2 / СТАДИЯ 3: Уставшая ===
            Debug.Log("Стадия: Уставшая");
            if (ufoRenderer != null)
            {
                for (int i = 0; i < ufoRenderer.Length; i++)
                {
                    ufoRenderer[i].material.color = originalColor; // Цвет возвращается
                }
            }

            moveScript.CurrentRotationSpeed = 55f;    // Скорость сбавляется до 50-60
            spawnerScript.ActiveSpawn = false;        // Уставшая — не спавнит объекты под собой

            yield return new WaitForSeconds(tiredDuration);

            // После окончания корутина автоматически пойдет на новый круг (в Спокойную стадию)
        }
    }
}
