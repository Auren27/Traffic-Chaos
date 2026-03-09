using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheel : MonoBehaviour
{
    public Joystick joystick; // Префаб джойстика
    public float x, y; // Переменные для определения направления
    public float rotationSpeed = 1000f; // Скорость поворота
    public bool reverseRotation = false; // Инвертировать направление вращения
    public float maxRotations = 3; // Максимальное количество оборотов в каждую сторону
    public bool returnToCenter = false; // Возвращать ли руль в центр при отпускании

    [SerializeField] private float totalRotation = 0f; // Накопленный угол поворота (в градусах)
    [SerializeField] private float previousAngle = 0f; // Угол на предыдущем кадре
    [SerializeField] private int fullRotations = 0; // Количество полных оборотов
    [SerializeField] private bool isLocked = false; // Флаг блокировки вращения
    [SerializeField] private float lockThreshold = 5f; // Порог для блокировки (в градусах)
    [SerializeField] private float centerReturnSpeed = 500f; // Скорость возврата в центр (если включено)
    [SerializeField] private float deadZone = 0.1f; // Мертвая зона джойстика

    void Start()
    {
        previousAngle = NormalizeAngle(transform.eulerAngles.z);
    }

    void LateUpdate()
    {
        x = joystick.Horizontal;
        y = joystick.Vertical;

        // Проверяем, находится ли джойстик в мертвой зоне
        bool isJoystickActive = Mathf.Abs(x) > deadZone || Mathf.Abs(y) > deadZone;

        if (isJoystickActive)
        {
            // Вычисляем целевой угол по оси Z
            float targetAngle = Mathf.Atan2(-x, y) * Mathf.Rad2Deg;

            // Инвертируем при необходимости
            if (reverseRotation) targetAngle = -targetAngle;

            // Проверяем, достигли ли мы максимальных оборотов
            float maxAngle = maxRotations * 360f;
            float currentTotalRotation = Mathf.Abs(totalRotation);

            // Определяем направление вращения по целевой позиции джойстика
            float currentAngle = NormalizeAngle(transform.eulerAngles.z);
            float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

            // Проверяем, пытаемся ли мы вращать в сторону предела
            bool tryingToExceedLimit = false;

            if (currentTotalRotation >= maxAngle - lockThreshold)
            {
                if (angleDifference > 0 && totalRotation > 0) // Поворот в положительном направлении
                    tryingToExceedLimit = true;
                else if (angleDifference < 0 && totalRotation < 0) // Поворот в отрицательном направлении
                    tryingToExceedLimit = true;
            }

            // Если достигли предела и пытаемся вращаться дальше - блокируем
            if (currentTotalRotation >= maxAngle - lockThreshold && tryingToExceedLimit)
            {
                isLocked = true;

                // Визуальный эффект блокировки - небольшое сопротивление
                // Можно добавить вибрацию или звук здесь

                // Вычисляем максимально допустимый угол
                float clampedAngle = Mathf.Sign(totalRotation) * maxAngle;
                float clampedNormalizedAngle = clampedAngle % 360f;

                // Плавно возвращаем руль к максимальному углу
                Quaternion clampedRotation = Quaternion.Euler(0, 0, clampedNormalizedAngle);
                transform.rotation = Quaternion.Lerp(transform.rotation, clampedRotation, rotationSpeed * Time.deltaTime);

                // Обновляем угол для корректного расчета оборотов
                currentAngle = NormalizeAngle(transform.eulerAngles.z);
            }
            else
            {
                isLocked = false;

                // Нормальное вращение
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Считаем обороты (только если не заблокировано)
            if (!isLocked)
            {
                CalculateRotations();
            }

            // Дополнительная проверка и корректировка угла
            if (currentTotalRotation > maxAngle)
            {
                totalRotation = Mathf.Sign(totalRotation) * maxAngle;
            }
        }
        else if (returnToCenter)
        {
            // Если включен возврат в центр и джойстик не активен
            float currentAngle = NormalizeAngle(transform.eulerAngles.z);

            // Плавно возвращаем руль к нулевому углу
            if (Mathf.Abs(currentAngle) > 0.5f)
            {
                float targetAngle = 0f;
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, centerReturnSpeed * Time.deltaTime);

                // Обновляем расчет оборотов при возврате в центр
                CalculateRotations();
            }
        }
        // Если returnToCenter = false и джойстик не активен, руль остается на месте
    }

    void CalculateRotations()
    {
        // Текущий угол от -180 до 180
        float currentAngle = NormalizeAngle(transform.eulerAngles.z);

        // Разница между текущим и предыдущим углом
        float delta = Mathf.DeltaAngle(previousAngle, currentAngle);

        // Добавляем к накопленному вращению
        totalRotation += delta;

        // Вычисляем количество полных оборотов
        fullRotations = Mathf.FloorToInt(totalRotation / 360f);

        // Сохраняем текущий угол для следующего кадра
        previousAngle = currentAngle;
    }

    // Нормализация угла в диапазон [-180, 180]
    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    // Методы для получения данных об оборотах
    public float GetTotalRotation()
    {
        return totalRotation;
    }

    public int GetFullRotations()
    {
        return fullRotations;
    }

    public float GetCurrentAngle()
    {
        return NormalizeAngle(transform.eulerAngles.z);
    }

    // обнуление
    public void ResetRotations()
    {
        totalRotation = 0f;
        fullRotations = 0;
        isLocked = false;
        previousAngle = NormalizeAngle(transform.eulerAngles.z);
    }

    // Новые публичные методы для управления поведением
    public void SetReturnToCenter(bool shouldReturn)
    {
        returnToCenter = shouldReturn;
    }

    public void SetCenterReturnSpeed(float speed)
    {
        centerReturnSpeed = Mathf.Max(0, speed);
    }

    public void SetDeadZone(float zone)
    {
        deadZone = Mathf.Clamp01(zone);
    }
}