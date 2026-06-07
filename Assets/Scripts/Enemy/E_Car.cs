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

    [Header("Настройки траектории и дистанции")]
    [SerializeField] private float minSafeDistance = 1.2f;   // Минимальный зазор между центрами машин в пробке
    [SerializeField] private float reactionTime = 0.4f;      // Влияние скорости на тормозной путь
    [SerializeField] private float scanRadius = 3.0f;        // Радиус поиска машин впереди по траектории
    [SerializeField] private LayerMask enemyLayer;

    // Эти переменные оставляем для внутренних расчетов, убираем [SerializeField]
    private float rayDistance;
    private float safeDistance;
    public float originalSpeed;
    private bool hasSavedSpeed = false;

    private bool flagtryActivateAgain = false;

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
        // Если машина уже едет, ничего делать не нужно
        if (isActive) return;

        // ПРОВЕРКА: Если прямо сейчас на нашей полосе рядом есть движущаяся машина — стоим!
        if (IsMovingCarNearby())
        {
            if (!flagtryActivateAgain)
            {
                flagtryActivateAgain = true;
                // Перезапускаем корутину ожидания
                StartCoroutine(WaitAndTryActivateAgain());
                return;
            }
            else 
            {
                HandleRouteCompleted();
            }
        }

        // Если путь чист — активируем
        isActive = true;

        if (isInitialized && isActive && currentTargetPosition == Vector3.zero)
        {
            NextWaypoint();
        }
    }

    private IEnumerator WaitAndTryActivateAgain()
    {
        // Ждем 0.15 секунды и пробуем снова
        yield return new WaitForSeconds(0.15f);
        ActivateEnemy();
    }

    private bool IsMovingCarNearby()
    {
        Vector2 myPosition = new Vector2(transform.position.x, transform.position.y);

        // Берем увеличенный радиус (minSafeDistance * 2.5f), чтобы машина точно успела проехать мимо нас
        float checkRadius = minSafeDistance * 2.5f;
        Collider2D[] nearbyCars = Physics2D.OverlapCircleAll(myPosition, checkRadius, enemyLayer);

        foreach (Collider2D carCollider in nearbyCars)
        {
            if (carCollider.gameObject != this.gameObject)
            {
                E_Car otherCar = carCollider.GetComponent<E_Car>();

                // Если машина рядом АКТИВНА и движется в ТУ ЖЕ сторону (флаг совпадает)
                if (otherCar != null && otherCar.isActive && otherCar.flag_up == this.flag_up)
                {
                    // Нам не важен вектор сближения. Если попутная активная машина находится 
                    // в радиусе нашей позиции — мы обязаны подождать, пока она уедет вперед!
                    return true;
                }
            }
        }
        return false;
    }

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

        // Проверяем, что у нас есть валидная целевая точка
        if (currentWaypointIndex >= waypoints.Count || currentWaypointIndex < 0 || waypoints[currentWaypointIndex] == null)
        {
            // Пытаемся найти новую точку
            currentWaypointIndex = FindNearestWaypointIndex();
            if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex] != null)
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

        // Проверяем дистанцию до передних машин и корректируем скорость
        AvoidCollisionsAlongPath();

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

    private void AvoidCollisionsAlongPath()
    {
        // Рассчитываем динамическую безопасную дистанцию торможения от текущей скорости
        safeDistance = minSafeDistance + (moveSpeed * reactionTime);

        Vector2 myPosition = transform.position;

        // Находим все машины в радиусе сканирования
        Collider2D[] hitCars = Physics2D.OverlapCircleAll(myPosition, scanRadius, enemyLayer);

        E_Car carToFollow = null;
        float minDistanceToTargetCar = float.MaxValue;

        foreach (Collider2D col in hitCars)
        {
            if (col.gameObject == this.gameObject) continue;

            E_Car otherCar = col.GetComponent<E_Car>();

            // Нас интересуют только активные попутные машины на той же трассе
            if (otherCar != null && otherCar.isActive && otherCar.flag_up == this.flag_up)
            {
                // Проверяем положение по Waypoints: машина впереди должна иметь индекс точки 
                // равный нашему ИЛИ быть на следующие точки впереди.
                int indexDiff = otherCar.currentWaypointIndex - this.currentWaypointIndex;

                // Обработка зацикливания (если одна машина уже на 0-й точке, а мы на последней)
                if (indexDiff < -waypoints.Count / 2) indexDiff += waypoints.Count;
                if (indexDiff > waypoints.Count / 2) indexDiff -= waypoints.Count;

                // Если машина делит с нами целевую точку или едет к следующей — она официально ПЕРЕР НАМИ
                if (indexDiff >= 0)
                {
                    float dist = Vector2.Distance(myPosition, otherCar.transform.position);
                    if (dist < minDistanceToTargetCar)
                    {
                        minDistanceToTargetCar = dist;
                        carToFollow = otherCar;
                    }
                }
            }
        }

        // Если перед нами по траектории обнаружена машина
        if (carToFollow != null)
        {
            // Ориентируемся строго на текущую скорость лидера
            float targetSpeed = carToFollow.moveSpeed;

            if (minDistanceToTargetCar <= safeDistance)
            {
                // Если дистанция критическая (внутри радиуса поворота) или лидер стоит
                if (minDistanceToTargetCar < safeDistance * 0.75f || targetSpeed <= 0.1f)
                {
                    moveSpeed = 0f; // Полный стоп, чтобы не въехать в зад или бок на вираже
                }
                else
                {
                    // Идеально копируем скорость переднего на повороте
                    moveSpeed = targetSpeed;
                }
            }
            else
            {
                // Заблаговременное плавное притормаживание по Lerp при приближении
                moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, Time.deltaTime * 10f);
            }

            // Барьер безопасности: не ехать быстрее лидера, если мы в зоне контроля
            if (moveSpeed > carToFollow.moveSpeed && minDistanceToTargetCar < safeDistance)
            {
                moveSpeed = carToFollow.moveSpeed;
            }
            return;
        }

        // Если траектория впереди чистая — плавно возвращаем оригинальную скорость
        if (hasSavedSpeed)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, originalSpeed, Time.deltaTime * 3f);
        }
    }
}