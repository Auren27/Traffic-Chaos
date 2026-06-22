using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private bool active_game = false;

    public enum ControlMode { Joystick, SteeringWheel }
    public ControlMode currentControlMode = ControlMode.SteeringWheel;

    public Joystick joystick;
    public GameObject joystickUI;
    public SteeringWheel steeringWheel;
    public GameObject steeringWheelUI;

    public EventTrigger GasPedal;
    private bool GasFlag = false;
    public EventTrigger StopPedal;

    private bool invertSteering = false;

    [SerializeField] private int id;

    private int broadcast = 1;
    private float time_broadcast = 0;
    private float speed_min;
    [SerializeField] private float speed = 1f;
    private float speed_max;
    private float grounded_speed_min;
    private float grounded_speed = 1f;
    private float grounded_speed_max;
    private float speed_timer = 0f;
    private float transfer_time;
    [SerializeField] private float active_speed = 0;

    private float turnInput;
    [SerializeField] private float rotationSpeed = 500f;
    public GameObject sprite_obj;
    private float min_rotate = -30f;
    private float max_rotate = 30f;

    [SerializeField] private float maxSteeringInput = 1f;
    [SerializeField] private float steeringDeadZone = 0.1f;
    [SerializeField] private float wheelToCarRatio = 1f;

    private float currentSteeringValue = 0f;

    [Header("СТАБИЛИЗАЦИЯ И ВВОД")]
    [Tooltip("Скорость возврата руля в центр. Чем выше, тем послушнее машина.")]
    [SerializeField] private float steeringReturnSpeed = 40f; // Немного увеличили для отзывчивости
    [Tooltip("Скорость плавного поворота руля.")]
    [SerializeField] private float steeringSmoothSpeed = 25f; // Сделали реакцию быстрее

    private int hp = 3;
    private int petrol = 10;
    [SerializeField] private float petrol_rashod;
    private float timer = 0f;

    private bool flag_ground = false;

    private SpriteRenderer spriteRenderer;
    private int groundLayerMask;

    [Header("ФИЗИКА ЖЕСТКОГО СЦЕПЛЕНИЯ")]
    [Tooltip("Сцепление (0 - лед, 1 - рельсы). Рекомендуется 0.9 - 1.0")]
    [Range(0f, 1f)]
    [SerializeField] private float tyreGrip = 1.0f;

    private Vector2 currentVelocity = Vector2.zero;

    [Header("НАСТРОЙКИ МАГНИТА")]
    private float magnetRadius = 5f;       // Радиус подсасывания
    private float magnetSpeed = 6f;       // Скорость полета бонуса к игроку
    private bool isMagnetActive = false;                    // Флаг работы магнита
    private Coroutine magnetCoroutine;                      // Ссылка на корутину магнита

    private Dictionary<string, Coroutine> activeBoosters = new Dictionary<string, Coroutine>();

    [Header("НАСТРОЙКИ ПРЫЖКА (ПРУЖИНЫ)")]
    [SerializeField] private float jumpDuration = 5f;       // Длительность одного прыжка
    [SerializeField] private float maxScaleMultiplier = 2f;  // Максимальное увеличение (высота полета)
    [SerializeField] private AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 0); // Кривая прыжка (парабола)

    [SerializeField] private bool isFlying = false;                             // Флаг: летит ли машина сейчас
    private Coroutine springCoroutine;

    // Публичный геттер, чтобы скрипт касаний знал, летим мы или нет
    public bool IsFlying => isFlying;

    private void Awake()
    {
        if (sprite_obj != null)
            spriteRenderer = sprite_obj.GetComponent<SpriteRenderer>();

        groundLayerMask = LayerMask.GetMask("Ground");
        GameManager.Instance.hp.Hp(hp);
        id = MODEL_WORLD.Instance.active_car_id;
    }

    public void OptionsTriggers()
    {
        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.PointerDown;
        dragEntry.callback = new EventTrigger.TriggerEvent();
        dragEntry.callback.AddListener((data) => GasFlag = true);
        GasPedal.triggers.Add(dragEntry);

        dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.PointerUp;
        dragEntry.callback = new EventTrigger.TriggerEvent();
        dragEntry.callback.AddListener((data) => GasFlag = false);
        GasPedal.triggers.Add(dragEntry);
    }

    void Start()
    {
        Characteristics();
        UpdateControlUI();
        currentVelocity = transform.up;
    }

    public void Characteristics()
    {
        var carData = MODEL_WORLD.Instance.GetVehicleByIdCar(id);
        if (carData == null) return;

        speed_min = carData.speed_min;
        speed_max = carData.speed_max;
        grounded_speed_min = carData.grounded_speed_min;
        grounded_speed_max = carData.grounded_speed_max;
        transfer_time = carData.transfer_time;
        rotationSpeed = carData.rotationSpeed;
        petrol_rashod = carData.petrol_rashod;
        spriteRenderer.sprite = Resources.Load<Sprite>(carData.imagePath);
    }

    public void Active_Game(bool active) { active_game = active; }
    public void SetControlMode(ControlMode mode) { currentControlMode = mode; UpdateControlUI(); }
    public void ToggleControlMode() { currentControlMode = currentControlMode == ControlMode.Joystick ? ControlMode.SteeringWheel : ControlMode.Joystick; UpdateControlUI(); }

    private void UpdateControlUI()
    {
        if (joystickUI != null) joystickUI.SetActive(currentControlMode == ControlMode.Joystick);
        if (steeringWheelUI != null) steeringWheelUI.SetActive(currentControlMode == ControlMode.SteeringWheel);
    }

    private void Update()
    {
        if (!active_game) return;

        if (Input.GetKey(KeyCode.Space)) { GasFlag = true; }
        if (Input.GetKeyUp(KeyCode.Space)) { GasFlag = false; }

        UpdateTurnInput();
        RotateCarBody();
        RotateSpriteVisual();
        UpMoving();
        UpdateSpeedInterpolation();
        UpdatePetrol();
    }

    private void UpdateTurnInput()
    {
        float targetInput = 0f;
        float keyboardInput = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(keyboardInput) > 0.01f)
        {
            // КЛАВИАТУРА: Задаем цель для плавного, но быстрого смещения
            targetInput = -keyboardInput;
        }
        else
        {
            // МОБИЛЬНОЕ УПРАВЛЕНИЕ
            switch (currentControlMode)
            {
                case ControlMode.Joystick:
                    if (joystick != null && joystickUI != null && joystickUI.activeSelf)
                    {
                        targetInput = invertSteering ? joystick.Horizontal : -joystick.Horizontal;
                    }
                    break;

                case ControlMode.SteeringWheel:
                    if (steeringWheel != null && steeringWheelUI != null && steeringWheelUI.activeSelf)
                    {
                        float rawInput = GetSteeringInputWithRotations();
                        if (invertSteering) rawInput = -rawInput;
                        if (Mathf.Abs(rawInput) < steeringDeadZone) rawInput = 0f;
                        targetInput = rawInput;
                    }
                    break;
            }
        }

        // Общий отзывчивый интерполятор для всех типов ввода
        if (Mathf.Abs(targetInput) < 0.01f)
        {
            currentSteeringValue = Mathf.MoveTowards(currentSteeringValue, 0f, steeringReturnSpeed * Time.deltaTime);
        }
        else
        {
            currentSteeringValue = Mathf.MoveTowards(currentSteeringValue, targetInput, steeringSmoothSpeed * Time.deltaTime);
        }

        turnInput = currentSteeringValue;
    }

    private void RotateCarBody()
    {
        if (active_speed <= 0.1f) return;

        float currentMovingSpeed = IsGrounded() ? grounded_speed : speed;
        // Коэффициент зависимости поворота от скорости (чтобы на месте не крутилась бешено)
        float speedFactor = Mathf.Clamp(currentMovingSpeed / speed_max, 0.5f, 1f);
        float rotationAmount = turnInput * rotationSpeed * speedFactor * Time.deltaTime;

        transform.Rotate(0, 0, rotationAmount);
    }

    private void RotateSpriteVisual()
    {
        if (sprite_obj == null) return;

        float currentMovingSpeed = IsGrounded() ? grounded_speed : speed;
        float targetRotation = 0f;

        if (active_speed > 0.2f && currentMovingSpeed > 0.2f)
        {
            targetRotation = turnInput * max_rotate;
        }

        sprite_obj.transform.localRotation = Quaternion.Lerp(
            sprite_obj.transform.localRotation,
            Quaternion.Euler(0, 0, targetRotation),
            Time.deltaTime * 15f
        );
    }

    private void UpMoving()
    {
        float targetSpeed = IsGrounded() ? grounded_speed : speed;

        if (active_speed <= 0.1f || targetSpeed <= 0.1f)
        {
            currentVelocity = Vector2.zero;
            return;
        }

        // ИСПРАВЛЕННЫЙ РАСЧЕТ ИНЕРЦИИ (ДРИФТА):
        // Считаем проекции ТЕКУЩЕЙ скорости на направления кузова
        float forwardSpeed = Vector2.Dot(currentVelocity, transform.up);
        float rightSpeed = Vector2.Dot(currentVelocity, transform.right);

        // Гасим боковую скорость в зависимости от tyreGrip (1.0 = мгновенный поворот без заноса)
        rightSpeed *= (1f - tyreGrip);

        // Движение вперед всегда стремится к целевой скорости targetSpeed
        forwardSpeed = Mathf.MoveTowards(forwardSpeed, targetSpeed, targetSpeed * Time.deltaTime * 5f);

        // Собираем итоговую скорость
        currentVelocity = (Vector2)transform.up * forwardSpeed + (Vector2)transform.right * rightSpeed;

        // Ограничиваем общую скорость, чтобы не было "катапультирования" при заносах
        if (currentVelocity.magnitude > targetSpeed)
        {
            currentVelocity = currentVelocity.normalized * targetSpeed;
        }

        // Перемещаем объект
        transform.position += (Vector3)currentVelocity * Time.deltaTime;

        // Логика смены покрытий
        if (IsGrounded())
        {
            flag_ground = true;
        }
        else if (flag_ground)
        {
            active_speed = grounded_speed;
            Interpolizion_speed();
            flag_ground = false;
        }
    }

    private void UpdateSpeedInterpolation()
    {
        if (GasFlag)
        {
            speed_timer += Time.deltaTime;
            if (speed_timer >= transfer_time)
            {
                if (active_speed < 10) active_speed += 0.5f;
                Interpolizion_speed();
                speed_timer = 0f;
            }
            if (GasPedal != null) GasPedal.gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
        else
        {
            speed_timer -= Time.deltaTime;
            if (speed_timer <= 0)
            {
                if (active_speed > 0) active_speed -= 0.5f;
                Interpolizion_speed();
                speed_timer = transfer_time;
            }
            if (GasPedal != null) GasPedal.gameObject.GetComponent<Image>().color = Color.white;
        }
    }

    private void UpdatePetrol()
    {
        timer += Time.deltaTime;
        if (timer >= petrol_rashod)
        {
            petrol -= 1;
            GameManager.Instance.hp.Petrol(petrol);
            if (petrol == 0) GameManager.Instance.Dead_car();
            timer = 0f;
        }
    }

    private void Interpolizion_speed()
    {
        speed = speed_min + (active_speed / 10 * (speed_max - speed_min));
        grounded_speed = grounded_speed_min + (active_speed / 10 * (grounded_speed_max - grounded_speed_min));
    }

    private float GetSteeringInputWithRotations()
    {
        if (steeringWheel == null) return 0f;

        float totalRotation = steeringWheel.GetTotalRotation();
        float maxTotalRotation = steeringWheel.maxRotations * 360f;
        if (maxTotalRotation == 0) return 0f;

        float normalizedInput = Mathf.Clamp(totalRotation / maxTotalRotation, -1f, 1f);
        float steeringResponse = 0f;

        if (normalizedInput >= 0)
        {
            steeringResponse = Mathf.Pow(normalizedInput, 1.5f);
        }
        else
        {
            steeringResponse = -Mathf.Pow(Mathf.Abs(normalizedInput), 1.5f);
        }

        return steeringResponse * maxSteeringInput;
    }

    bool IsGrounded()
    {
        float groundCheckDistance = 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, groundCheckDistance, groundLayerMask);
        return hit.collider != null;
    }

    public void Attack()
    {
        hp -= 1;
        if (hp > 0)
        {
            GameManager.Instance.hp.Hp(hp);
            spriteRenderer.color = Color.red;
            StartCoroutine(Red_Color());
        }
        else if (hp == 0)
        {
            GameManager.Instance.hp.Hp(hp);
            GameManager.Instance.Dead_car();
        }
    }

    public void AddHp()
    {
        if (hp < 3)
        {
            hp += 1;
            GameManager.Instance.hp.Hp(hp);
        }
    }

    private IEnumerator Red_Color()
    {
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = Color.white;
    }

    public void PetrolAdd(int count)
    {
        if (petrol < 10)
        {
            petrol += count;
            GameManager.Instance.hp.Petrol(petrol);
        }
    }

    // Универсальный метод для запуска временного эффекта
    // System.Action — это ссылка на метод (функцию), которую мы хотим передать
    public void ApplyTimedBooster(string boosterId, System.Action startEffect, System.Action endEffect, float duration)
    {
        // Если бустер этого типа уже работает
        if (activeBoosters.ContainsKey(boosterId))
        {
            // Останавливаем старый таймер (при этом endEffect НЕ вызывается, характеристики остаются измененными)
            if (activeBoosters[boosterId] != null)
            {
                StopCoroutine(activeBoosters[boosterId]);
            }
            activeBoosters.Remove(boosterId);
        }
        else
        {
            // Если это первое взятие бустера — активируем его эффект
            startEffect?.Invoke();
        }

        // Запускаем новый таймер обновления
        Coroutine newBoosterCoroutine = StartCoroutine(BoosterTimerCoroutine(boosterId, endEffect, duration));
        activeBoosters.Add(boosterId, newBoosterCoroutine);
    }

    private IEnumerator BoosterTimerCoroutine(string boosterId, System.Action endEffect, float duration)
    {
        // Ждем положенное время
        yield return new WaitForSeconds(duration);

        // Время вышло — выключаем эффект
        endEffect?.Invoke();

        // Удаляем из списка активных
        if (activeBoosters.ContainsKey(boosterId))
        {
            activeBoosters.Remove(boosterId);
        }
    }

    public void StartBoostInverted()
    {
        invertSteering = true;
        if (spriteRenderer != null) spriteRenderer.color = new Color(0.6f, 0.1f, 0.9f, 1f); // подсветим машину фиолетовым
    }

    public void EndBoostInverted()
    {
        invertSteering = false;
        if (spriteRenderer != null) spriteRenderer.color = Color.white; // вернем обычный цвет
    }

    public void StartNitro()
    {
        speed_max *= 1.5f;
        grounded_speed_max *= 1.5f;
        if (spriteRenderer != null) spriteRenderer.color = new Color(0f, 0.8f, 1f, 1f); // подсветим машину синим
        Debug.Log("Нитро активировано!");
    }

    public void EndNitro()
    {
        speed_max /= 1.5f;
        grounded_speed_max /= 1.5f;
        if (spriteRenderer != null) spriteRenderer.color = Color.white; // вернем обычный цвет
        Debug.Log("Нитро завершилось.");
    }

    public void StartBoostBucket()
    {
        if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.5f, 0f, 1f); // подсветим машину оранжевым
    }

    public void EndBoostBucket()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.white; // вернем обычный цвет
    }

    public void StartBoostMagnet()
    {
        if (spriteRenderer != null) spriteRenderer.color = new Color(0.1f, 0.9f, 0.1f, 1f); // подсветим машину зеленым

        if (!isMagnetActive)
        {
            isMagnetActive = true;
            magnetCoroutine = StartCoroutine(MagnetRoutine());
        }
    }

    public void EndBoostMagnet()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.white; // вернем обычный цвет

        isMagnetActive = false;
        if (magnetCoroutine != null)
        {
            StopCoroutine(magnetCoroutine);
            magnetCoroutine = null;
        }
    }

    private IEnumerator MagnetRoutine()
    {
        // Будем собирать объекты на слое "Buns", который используется в BonusManager
        int bonusLayerMask = LayerMask.GetMask("Buns");

        while (isMagnetActive)
        {
            // Находим все коллайдеры в круговом диапазоне вокруг игрока
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, magnetRadius, bonusLayerMask);

            foreach (Collider2D hit in hitColliders)
            {
                // Запускаем плавное притягивание для каждого найденного бонуса отдельно
                if (hit != null)
                {
                    StartCoroutine(AttractBonusRoutine(hit.gameObject));
                }
            }

            // Задержка проверки, чтобы не перегружать процессор каждый кадр
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator AttractBonusRoutine(GameObject bonus)
    {
        // Пока бонус существует и магнит активен — тянем его к игроку
        while (bonus != null && isMagnetActive)
        {
            // Перемещаем бонус в позицию игрока
            bonus.transform.position = Vector3.MoveTowards(
                bonus.transform.position,
                transform.position,
                magnetSpeed * Time.deltaTime
            );

            // Если бонус подлетел достаточно близко — корутина завершается.
            // Скрипт сбора на самом бонусе (через OnTrigerEnter или дистанцию) сработает и уничтожит объект.
            if (Vector3.Distance(bonus.transform.position, transform.position) < 0.2f)
            {
                yield break;
            }

            yield return null;
        }
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawWireSphere(transform.position, magnetRadius);
    //}

    public void StartBoostSpring()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.cyan; // подсветим машину голубым

        // Если корутина уже активна (повторный бустер), сбрасываем масштаб в дефолт перед новым прыжком
        if (springCoroutine != null)
        {
            StopCoroutine(springCoroutine);
        }

        springCoroutine = StartCoroutine(SpringJumpRoutine());
    }

    public void EndBoostSpring()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.white; // вернем обычный цвет

        isFlying = false;
        if (springCoroutine != null)
        {
            StopCoroutine(springCoroutine);
            springCoroutine = null;
        }

        // Гарантированно возвращаем машине исходный размер принудительно
        if (sprite_obj != null)
        {
            sprite_obj.transform.localScale = Vector3.one;
        }
    }

    private IEnumerator SpringJumpRoutine()
    {
        isFlying = true;
        float timer = 0f;
        Vector3 baseScale = Vector3.one; // Исходный масштаб (1, 1, 1)

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / jumpDuration;

            // Получаем текущую "высоту" из кривой (от 0 до 1 и обратно до 0)
            float heightValue = jumpCurve.Evaluate(progress);

            // Вычисляем новый масштаб машинки
            float currentScale = Mathf.Lerp(1f, maxScaleMultiplier, heightValue);

            if (sprite_obj != null)
            {
                sprite_obj.transform.localScale = baseScale * currentScale;
            }

            yield return null;
        }

        // По окончании прыжка возвращаем все в исходное состояние
        EndBoostSpring();
    }

    public void StartBoostTank()
    {
    }

    public void EndBoostTank()
    {
    }

    public void StartBoostShield()
    {
        if (spriteRenderer != null) spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f); // подсветим машину серым
    }

    public void EndBoostShield()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.white; // вернем обычный цвет
    }
}