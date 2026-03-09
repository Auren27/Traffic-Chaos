using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    public Transform target; // объект, за которым будет следить камера

    void LateUpdate()
    {
        if (target != null)
        {
            // Позиционирование камеры
            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y + 2f, -8f); // Управляемая позиция
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, 5 * Time.deltaTime); // применение сглаживания 20

            transform.position = smoothedPosition; // установка новой позиции камеры
        }
    }
}
