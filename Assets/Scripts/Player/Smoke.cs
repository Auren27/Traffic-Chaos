using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    public Transform target; // Объект, за которым следим (игрок)
    public float smoothSpeed = 1f; // Скорость сглаживания

    private float lastTargetY; // Переменная для хранения позиции игрока в прошлом кадре
    private bool isLocked = false; // Флаг блокировки движения
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;

        if (target != null)
        {
            lastTargetY = target.position.y;
        }
    }

    void LateUpdate()
    {
        if (target != null && !isLocked) // Двигаем только если не заблокирован
        {
            if (target.position.y > lastTargetY)
            {
                Vector3 desiredPosition = new Vector3(0, target.position.y - 8, 0);
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                transform.position = smoothedPosition;

                lastTargetY = target.position.y;
            }
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

