using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    public Transform target;
    [Tooltip("Скорость подъема")]
    public float smoothSpeed = 5f;
    [Tooltip("Смещение от игрока по Y")]
    public float yOffset = -8f;

    private bool isLocked = false;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target == null || isLocked) return;

        // Вычисляем желаемую позицию (где должен быть дым относительно игрока)
        float targetY = target.position.y + yOffset;

        // УСЛОВИЕ: Двигаемся только если цель выше текущего положения дыма
        if (targetY > transform.position.y)
        {
            Vector3 desiredPosition = new Vector3(0, targetY, 0);

            // Двигаемся вверх
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isLocked = true; // Блокируем дальнейшее движение
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isLocked = true; // Блокируем дальнейшее движение
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isLocked = false; // Блокируем дальнейшее движение
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isLocked = false; // Блокируем дальнейшее движение
        }
    }

    public void StartPosition()
    {
        transform.position = startPosition;
    }
}

