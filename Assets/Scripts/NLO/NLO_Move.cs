using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NLO_Move : MonoBehaviour
{
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject tower;
    [SerializeField] private float rotationSpeed = 90f; // градусов в секунду

    public Transform target;
    [Tooltip("Скоростm")]
    public float smoothSpeed = 5f;
    [Tooltip("Смещение от игрока по Y")]
    public float yOffset = +4f;


    private void Update()
    {
        body.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Вычисляем желаемую позицию (где должен быть дым относительно игрока)
        float targetX = target.position.x;
        float targetY = target.position.y + yOffset;

        Vector3 desiredPosition = new Vector3(0, 0, 0);

        // УСЛОВИЕ: Двигаемся только если цель выше текущего положения дыма
        if (targetY > transform.position.y)
        {
            desiredPosition = new Vector3(targetX, targetY, 0);

        }
        else
        {
            desiredPosition = new Vector3(targetX, 0, 0);
        }

        // Двигаемся
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
