using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Car : MonoBehaviour
{
    [Header("Movement Settings")]
    public bool flag_up = false;
    [SerializeField] private bool isActive = false;
    public int number_car;
    private float moveSpeed = 2f;
    private float arrivalThreshold = 0.1f;
    public float rotationSpeed = 2f;

    [Header("Waypoints")]
    [SerializeField] private List<GameObject> waypoints = new List<GameObject>();
    [SerializeField] private int currentWaypointIndex = 0;
    private Vector3 currentTargetPosition;

    // Добавим переменную для отслеживания, инициализирована ли машина
    [SerializeField] private bool isInitialized = false;

    [Header("Настройки предотвращения столкновений")]
    [SerializeField] private float rayDistance = 6.0f;     // Дистанция, на которой машина замечает препятствие
    [SerializeField] private float safeDistance = 4.0f;    // Минимальная безопасная дистанция до передней машины
    [SerializeField] private LayerMask enemyLayer;         // Слой, на котором находятся машины (Enemy)

    private float originalSpeed;                           // Здесь сохраним начальную скорость машины
    private bool hasSavedSpeed = false;

    private void Awake()
    {
        if (Random.Range(0, 2) == 0) // определяем в какую сторону движется машина
        {
            flag_up = true;
        }

        this.gameObject.GetComponentInChildren<Enemy_JSON>().StartJSON_Car(); // загружаем ресурсы машины

        //DelayedInitialize();
    }

    private void Start()
    {
        // Добавим небольшую задержку, чтобы менеджер успел инициализироваться
        StartCoroutine(DelayedInitialize());
    }

    private IEnumerator DelayedInitialize()
    {
        yield return null; // Ждем один кадр
        waypoints = GetWaypointsForDirection(); // определение точек
        InitializeCar();
        isInitialized = true;

       // if(number_car >=0 && number_car < 3) ActivateEnemy();
    }

    public void New_Speed(float speed, float rspeed)
    {
        moveSpeed = speed;
        rotationSpeed = rspeed;

        originalSpeed = speed;
        hasSavedSpeed = true;
    }

    public void ActivateEnemy()
    {
        isActive = true;
        if (isInitialized && isActive && currentTargetPosition == Vector3.zero)
        {
            NextWaypoint();
        }
    }

    // Метод для обновления точек маршрута
    //public void UpdateWaypoints()
    //{
    //    List<GameObject> newWaypoints = GetWaypointsForDirection();

    //    if (newWaypoints == null || newWaypoints.Count == 0)
    //    {
    //        HandleRouteCompleted();
    //        return;
    //    }

    //    waypoints = newWaypoints;

    //    if (flag_up)
    //    {
    //        // Для машин ВВЕРХ: массив сдвинулся назад, уменьшаем индекс на 4
    //        currentWaypointIndex -= 4;

    //        if (currentWaypointIndex < 0)
    //        {
    //            currentWaypointIndex = 0;
    //        }
    //    }
    //    else
    //    {
    //        // Для машин ВНИЗ: из-за реверса массива новые точки встали в НАЧАЛО.
    //        // Физическая точка сместилась вперед по индексу, поэтому УВЕЛИЧИВАЕМ индекс на 4.
    //        currentWaypointIndex += 4;
    //    }

    //    // Проверяем, чтобы индекс не вылетел за пределы новой длины массива
    //    if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Count)
    //    {
    //        if (waypoints[currentWaypointIndex] != null)
    //        {
    //            currentTargetPosition = waypoints[currentWaypointIndex].transform.position;
    //        }
    //    }
    //    else
    //    {
    //        HandleRouteCompleted();
    //    }
    //}

    public void UpdateWaypoints()
    {
        // 1. Запоминаем объект точки, к которой машина ехала ДО обновления карты
        GameObject previousTargetObject = null;
        if (waypoints != null && currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Count)
        {
            previousTargetObject = waypoints[currentWaypointIndex];
        }

        // 2. Получаем обновленный список точек
        List<GameObject> newWaypoints = GetWaypointsForDirection();

        if (newWaypoints == null || newWaypoints.Count == 0)
        {
            HandleRouteCompleted();
            return;
        }

        waypoints = newWaypoints;

        // 3. Ищем, под каким индексом наша старая цель находится в НАШЕМ НОВОМ списке
        int foundIndex = -1;
        if (previousTargetObject != null)
        {
            foundIndex = waypoints.IndexOf(previousTargetObject);
        }

        // 4. Если точка успешно найдена в новом списке — просто присваиваем её индекс!
        if (foundIndex != -1)
        {
            currentWaypointIndex = foundIndex;
        }
        else
        {
            // Если старая точка была удалена (машина ехала по самой старой дороге, которую стерли),
            // тогда применяем математический сдвиг как запасной вариант
            if (flag_up)
            {
                currentWaypointIndex -= 4;
            }
            else
            {
                currentWaypointIndex += 4;
            }
        }

        // 5. Финальная проверка границ и обновление позиции
        if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Count)
        {
            if (waypoints[currentWaypointIndex] != null)
            {
                currentTargetPosition = waypoints[currentWaypointIndex].transform.position;

                // КРИТИЧЕСКИЙ СЕКРЕТ: Сбрасываем поворот машины в сторону новой (правильной) цели прямо сейчас,
                // чтобы убрать визуальный «кивок» или дерганье на один кадр.
                // Явно приводим оба значения к Vector2 внутри операции, чтобы избежать CS0034
                Vector2 targetPos2D = new Vector2(currentTargetPosition.x, currentTargetPosition.y);
                Vector2 myPos2D = new Vector2(transform.position.x, transform.position.y);

                Vector2 direction = (targetPos2D - myPos2D).normalized;
                if (direction != Vector2.zero)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle - 90); // Скорректируйте -90 в зависимости от спрайта машины
                }
            }
        }
        else
        {
            HandleRouteCompleted();
        }
    }

    private List<GameObject> GetWaypointsForDirection() // загрузка точек
    {
        if (Architecture.Instance == null || Architecture.Instance.GetEnemyWaypointManager() == null)
        {
            //Debug.LogWarning("Architecture или EnemyWaypointManager не найден");
            return new List<GameObject>();
        }

        if (flag_up)
        {
            List<GameObject> points = Architecture.Instance.GetEnemyWaypointManager().GetAllEnemyPointsUp();
            if (points == null)
            {
                //Debug.LogWarning("Список точек Up равен null");
                return new List<GameObject>();
            }

            List<GameObject> filteredPoints = new List<GameObject>();
            foreach (GameObject point in points)
            {
                //if (point != null)
                //{
                    filteredPoints.Add(point);
                //}
            }
            return filteredPoints;
        }
        else
        {
            List<GameObject> original = Architecture.Instance.GetEnemyWaypointManager().GetAllEnemyPointsDown();
            if (original == null)
            {
                //Debug.LogWarning("Список точек Down равен null");
                return new List<GameObject>();
            }

            // Фильтруем null точки и создаем реверсивный список
            List<GameObject> reversed = new List<GameObject>();
            for (int i = original.Count - 1; i >= 0; i--)
            {
                //if (original[i] != null)
                //{
                    reversed.Add(original[i]);
                //}
            }
            return reversed;
        }
    }

    private void InitializeCar()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning($"Машина {number_car} не имеет точек маршрута");
            Destroy(gameObject);
            return;
        }

        // Находим ближайшую точку
        currentWaypointIndex = FindNearestWaypointIndex();

        if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Count)
        {
            isActive = false;
            TeleportToStartPosition();
            //Debug.Log($"Машина {number_car} инициализирована на точке {currentWaypointIndex}");
        }
        else
        {
            //Debug.LogWarning($"Машина {number_car} не нашла ближайшую точку");
            Destroy(gameObject);
        }
    }

    private void TeleportToStartPosition()
    {
        if (currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex] != null)
        {
            transform.position = waypoints[currentWaypointIndex].transform.position;

            // Если есть следующая точка - поворачиваем к ней
            int nextIndex = GetNextValidWaypointIndex(currentWaypointIndex);
            if (nextIndex >= 0 && nextIndex < waypoints.Count)
            {
                Vector3 nextTargetPosition = waypoints[nextIndex].transform.position;
                Vector2 direction = (nextTargetPosition - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    // Найти следующий валидный индекс точки
    private int GetNextValidWaypointIndex(int startIndex)
    {
        for (int i = startIndex + 1; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
                return i;
        }
        return -1;
    }

    // Найти ближайшую точку - ИСПРАВЛЕННАЯ ВЕРСИЯ
    private int FindNearestWaypointIndex()
    {
        if (waypoints == null || waypoints.Count == 0)
            return -1;

        int nearestIndex = -1;
        float minDistance = float.MaxValue;
        Vector3 carPosition = transform.position;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            float distance = Vector3.Distance(carPosition, waypoints[i].transform.position);

            //// Логирование для отладки
            //if (number_car == 0) // Логируем только для первой машины
            //{
            //    Debug.Log($"Точка {i}: {waypoints[i].transform.position}, расстояние: {distance:F2}, мин. расстояние: {minDistance:F2}");
            //}

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        if (nearestIndex == -1)
        {
            // Если не нашли ни одной валидной точки, ищем первую не-null
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null)
                {
                    nearestIndex = i;
                    //Debug.LogWarning($"Машина {number_car}: ближайшая точка не найдена, используется первая валидная {nearestIndex}");
                    break;
                }
            }
        }
        else
        {
            //Debug.Log($"Машина {number_car}: ближайшая точка {nearestIndex} на расстоянии {minDistance:F2}");
        }

        return nearestIndex;
    }

    private void NextWaypoint()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            //Debug.LogWarning($"Машина {number_car}: нет точек маршрута");
            isActive = false;
            return;
        }

        // Увеличиваем индекс и ищем следующую валидную точку
        currentWaypointIndex++;

        // Пропускаем null точки
        while (currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex] == null)
        {
            currentWaypointIndex++;
        }

        // Если достигли конца списка
        if (currentWaypointIndex >= waypoints.Count && isActive)
        {
            Debug.Log(currentWaypointIndex + " достигла конца списка");
            HandleRouteCompleted();
            return;
        }

        if (isActive)
        {
            if (flag_up)
            {
                if (currentTargetPosition.y < waypoints[0].transform.position.y)
                {
                    HandleRouteCompleted();
                    return;
                }
            }
            else
            {
                if (currentTargetPosition.y < waypoints[waypoints.Count - 1].transform.position.y)
                {
                    HandleRouteCompleted();
                    return;
                }
            }
        }
        

        // Устанавливаем новую целевую позицию
        currentTargetPosition = waypoints[currentWaypointIndex].transform.position;
        //Debug.Log($"Машина {number_car} движется к точке {currentWaypointIndex} ({currentTargetPosition})");
        
    }

    private void HandleRouteCompleted()
    {
        Debug.Log($"Машина {number_car} завершила маршрут");

        // Уничтожаем машину
        if (Architecture.Instance != null && Architecture.Instance.GetEnemyWaypointManager() != null)
        {
            Architecture.Instance.GetEnemyWaypointManager().DestroyEnemyCar(number_car);
        }
        Destroy(gameObject);
    }

    private void Update()
    {

        if (!isActive || !isInitialized) return;

        // 1. Проверяем дистанцию до передних машин и корректируем скорость
        AvoidCollisions();

        // Проверяем, что у нас есть валидная целевая точка
        if (currentWaypointIndex >= waypoints.Count || currentWaypointIndex < 0 ||
            waypoints[currentWaypointIndex] == null)
        {
            // Пытаемся найти новую точку
            currentWaypointIndex = FindNearestWaypointIndex();
            if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Count &&
                waypoints[currentWaypointIndex] != null)
            {
                currentTargetPosition = waypoints[currentWaypointIndex].transform.position;
            }
            else
            {
                isActive = false;
                Debug.LogWarning($"Машина {number_car}: не удалось найти валидную точку");
                return;
            }
        }

        // Движение к точке
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTargetPosition,
            moveSpeed * Time.deltaTime
        );

        // Плавный поворот
        if (currentTargetPosition != transform.position)
        {
            Vector2 direction = (currentTargetPosition - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Проверка достижения точки
        float distanceToTarget = Vector3.Distance(transform.position, currentTargetPosition);
        if (distanceToTarget <= arrivalThreshold)
        {
            //Debug.Log($"Машина {number_car} достигла точки {currentWaypointIndex}");
            NextWaypoint();
        }

    }

    // Метод для получения текущего состояния машины
    public string GetCarStatus()
    {
        string status = $"Машина {number_car}: ";
        status += isActive ? "Активна" : "Неактивна";
        status += $", Точка: {currentWaypointIndex}/{waypoints.Count}";
        status += $", Направление: {(flag_up ? "Вверх" : "Вниз")}";
        if (currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex] != null)
        {
            status += $", Позиция цели: {waypoints[currentWaypointIndex].transform.position}";
        }
        return status;
    }

    // Метод для принудительной активации машины с указанной точки
    public void ActivateFromWaypoint(int waypointIndex)
    {
        if (waypointIndex >= 0 && waypointIndex < waypoints.Count && waypoints[waypointIndex] != null)
        {
            currentWaypointIndex = waypointIndex;
            currentTargetPosition = waypoints[waypointIndex].transform.position;
            transform.position = currentTargetPosition;
            isActive = true;
            Debug.Log($"Машина {number_car} активирована с точки {waypointIndex}");
        }
    }

    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || waypoints == null || !isInitialized) return;

        Gizmos.color = flag_up ? Color.green : Color.red;

        // Рисуем путь
        GameObject prevPoint = null;
        foreach (var point in waypoints)
        {
            if (point != null)
            {
                if (prevPoint != null)
                {
                    Gizmos.DrawLine(prevPoint.transform.position, point.transform.position);
                }
                prevPoint = point;
            }
        }

        // Подсвечиваем текущую целевую точку
        if (currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(waypoints[currentWaypointIndex].transform.position, 0.3f);
            Gizmos.DrawLine(transform.position, waypoints[currentWaypointIndex].transform.position);
        }
    }

    void OnDestroy()
    {
        
        Debug.Log($"Машина {number_car} уничтожена. Причина: {GetDestroyReason()}");
    }

    private string GetDestroyReason()
    {
        // Проверьте текущее состояние для определения причины
        if (waypoints == null || waypoints.Count == 0)
            return "Нет точек маршрута";

        if (currentWaypointIndex < 0 || currentWaypointIndex >= waypoints.Count)
            return "Неверный индекс точки";

        if (!isInitialized)
            return "Не инициализирована";

        return "Неизвестно (возможно HandleRouteCompleted)";
    }

    private void AvoidCollisions()
    {
        Vector2 direction = transform.up;
        Vector2 myPosition = new Vector2(transform.position.x, transform.position.y);

        // Смещаем старт проверки чуть вперед
        Vector2 rayStart = myPosition + direction * 0.5f;

        // Временно выключаем свой коллайдер
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null) myCollider.enabled = false;

        // ИСПОЛЬЗУЕМ CIRCLECAST вместо обычного Raycast. 
        // Радиус 0.4f создает "толстый" луч шириной с машину, который не теряет цель на поворотах.
        RaycastHit2D hit = Physics2D.CircleCast(rayStart, 0.4f, direction, rayDistance, enemyLayer);

        // Включаем коллайдер обратно
        if (myCollider != null) myCollider.enabled = true;

        // Рисуем линию для отладки
        Debug.DrawRay(rayStart, direction * rayDistance, hit.collider != null ? Color.red : Color.green);

        if (hit.collider != null)
        {
            E_Car frontCar = hit.collider.GetComponent<E_Car>();

            if (frontCar != null && frontCar != this)
            {
                // Считаем точное расстояние между центрами машин для защиты от слияния
                float absoluteDistance = Vector2.Distance(myPosition, new Vector2(frontCar.transform.position.x, frontCar.transform.position.y));

                // КРИТИЧЕСКАЯ ПРОВЕРКА: Если мы ОЧЕНЬ близко (наехали или почти наехали)
                if (absoluteDistance <= safeDistance)
                {
                    // Жестко приравниваем скорость. Если передняя машина замедлилась на повороте, 
                    // мы мгновенно сбрасываем скорость без Lerp, чтобы не въехать по инерции.
                    moveSpeed = frontCar.moveSpeed;

                    // Дополнительный барьер: если дистанция критическая, принудительно притормаживаем сильнее передней
                    if (absoluteDistance < safeDistance * 0.8f)
                    {
                        moveSpeed = frontCar.moveSpeed * 0.5f;
                    }
                }
                else
                {
                    // Если мы еще на безопасном расстоянии, но догоняем — плавно подстраиваемся (коэффициент увеличен до 12f для резкости)
                    moveSpeed = Mathf.Lerp(moveSpeed, frontCar.moveSpeed, Time.deltaTime * 12f);
                }
                return; // Препятствие обработано, выходим
            }
        }

        // Если впереди никого нет — плавно возвращаем исходную скорость
        if (hasSavedSpeed)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, originalSpeed, Time.deltaTime * 3f);
        }
    }
}