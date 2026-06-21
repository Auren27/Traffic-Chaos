using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NLO_Move : MonoBehaviour
{
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject tower;
    [SerializeField] private float rotationSpeed = 90f; // градусов в секунду

    public Transform target;

    [Tooltip("Скорость следования за игроком (чем выше, тем быстрее догоняет)")]
    [SerializeField] private float followSpeed = 5f;

    [Tooltip("Смещение от игрока по Y (чтобы НЛО было вверху экрана)")]
    [SerializeField] private float yOffset = 5f;

    [Header("Настройки парения (Шум Перлина)")]
    [SerializeField] private float hoverRadiusX = 3f;
    [SerializeField] private float hoverRadiusY = 1f;
    [Tooltip("Скорость изменения направления парения")]
    [SerializeField] private float hoverSpeed = 1f;

    private float noiseSeedX;
    private float noiseSeedY;

    public float CurrentRotationSpeed { get; set; }
    public float CurrentFollowSpeed { get; set; }

    private void Start()
    {
        // Разные сиды для плавного 2D сдвига
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(1000f, 2000f);

        CurrentRotationSpeed = rotationSpeed;
        CurrentFollowSpeed = followSpeed;
    }

    private void Update()
    {
        if (body != null)
        {
            body.transform.Rotate(0, 0, CurrentRotationSpeed * Time.deltaTime);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Плавный шум без всяких Clamp (чтобы не было рывков и зависаний)
        float rawNoiseX = Mathf.PerlinNoise(Time.time * hoverSpeed + noiseSeedX, 0f);
        float rawNoiseY = Mathf.PerlinNoise(0f, Time.time * hoverSpeed + noiseSeedY);

        // Переводим из [0, 1] в диапазон [-1, 1]
        float noiseX = (rawNoiseX * 2f) - 1f;
        float noiseY = (rawNoiseY * 2f) - 1f;

        Vector3 currentRandomOffset = new Vector3(noiseX * hoverRadiusX, noiseY * hoverRadiusY, 0);

        // 2. Целевая позиция строго привязана к игроку + смещение вверх
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y + yOffset, 0);

        // Финальная точка, куда НЛО стремится
        Vector3 finalDesiredPosition = targetPosition + currentRandomOffset;

        // 3. Линейная интерполяция с адекватной скоростью
        // Используем более отзывчивый Lerp, чтобы НЛО не отставало от машины
        transform.position = Vector3.Lerp(transform.position, finalDesiredPosition, CurrentFollowSpeed * Time.deltaTime);
    }
}
