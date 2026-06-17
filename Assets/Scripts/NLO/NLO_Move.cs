using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NLO_Move : MonoBehaviour
{
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject tower;
    [SerializeField] private float rotationSpeed = 90f; // градусов в секунду

    public Transform target;
    [Tooltip("Скорость")]
    private float smoothSpeed = 0.8f;
    [Tooltip("Смещение от игрока по Y")]
    private float yOffset = +4f;

    [Header("Настройки парения (Рандом)")]
    private float hoverRadiusX = 4f; // диапазон X от -2 до +2
    private float hoverRadiusY = 1.5f; // диапазон Y от -1 до +1
    [Tooltip("Скорость изменения направления парения")]
    private float hoverSpeed = 1f;
    private float noiseSeedX;
    private float noiseSeedY;

    [Tooltip("Контраст шума: чем выше, тем ближе к краям радиуса будет подлетать НЛО (попробуйте от 1.5 до 2.5)")]
    private float noiseContrast = 2f;

    private void Start()
    {
        // Используем GetInstanceID, чтобы у разных НЛО были абсолютно разные траектории
        noiseSeedX = Random.Range(0f, 1000f) + transform.GetInstanceID();
        noiseSeedY = Random.Range(1000f, 2000f) - transform.GetInstanceID();
    }

    private void Update()
    {
        if (body != null)
        {
            body.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Вычисляем исходный шум Перлина (от 0 до 1)
        float rawNoiseX = Mathf.PerlinNoise(Time.time * hoverSpeed + noiseSeedX, 0f);
        float rawNoiseY = Mathf.PerlinNoise(0f, Time.time * hoverSpeed + noiseSeedY);

        // Переводим в диапазон от -1 до 1
        float noiseX = rawNoiseX * 2f - 1f;
        float noiseY = rawNoiseY * 2f - 1f;

        // Усиливаем шум с помощью контраста и ограничиваем (Clamping), чтобы вытолкнуть объект к краям
        noiseX = Mathf.Clamp(noiseX * noiseContrast, -1f, 1f);
        noiseY = Mathf.Clamp(noiseY * noiseContrast, -1f, 1f);

        Vector3 currentRandomOffset = new Vector3(noiseX * hoverRadiusX, noiseY * hoverRadiusY, 0);

        // 2. Базовая позиция следования
        float targetX = target.position.x;
        float targetY = target.position.y + yOffset;

        Vector3 baseDesiredPosition;

        if (targetY > transform.position.y)
        {
            baseDesiredPosition = new Vector3(targetX, targetY, 0);
        }
        else
        {
            baseDesiredPosition = new Vector3(targetX, transform.position.y, 0);
        }

        // 3. Финальная позиция
        Vector3 finalDesiredPosition = baseDesiredPosition + currentRandomOffset;

        // 4. Движение
        transform.position = Vector3.Lerp(transform.position, finalDesiredPosition, smoothSpeed * Time.deltaTime);
    }
}
