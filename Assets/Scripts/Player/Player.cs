using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private bool active_game = false;

    // Режимы управления
    public enum ControlMode
    {
        SteeringWheel,
        Joystick
    }

    public ControlMode currentControlMode = ControlMode.SteeringWheel;

    // Переменные для джойстика
    public Joystick joystick; // ссылка на джойстик
    public GameObject joystickUI; // UI элемент джойстика

    // Переменные для рулевого колеса
    public SteeringWheel steeringWheel; // ссылка на рулевое колесо
    public GameObject steeringWheelUI; // UI элемент руля

    // Переменные для педалей
    public EventTrigger GasPedal;
    private bool GasFlag = false;
    public EventTrigger StopPedal;

    private bool invertSteering = false; // Инверсия управления

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
    private float transfer_time; // время смены передачи
    [SerializeField] private float active_speed = 1; // скорость от 0 до 10

    private float turnInput;
    [SerializeField] private float rotationSpeed = 180f; // Базовая скорость поворота (градусы в секунду)
    public GameObject sprite_obj;
    private float min_rotate = -30f; // Ограничение поворота колес (вправо)
    private float max_rotate = 30f;  // Ограничение поворота колес (влево)

    // Параметры для рулевого управления
    [SerializeField] private float maxSteeringInput = 1f; // Максимальное значение поворота
    [SerializeField] private float steeringDeadZone = 0.1f; // Мертвая зона руля
    [SerializeField] private float wheelToCarRatio = 1f; // коэффициент передачи

    // переменные для плавного управления рулем
    private float currentSteeringValue = 0f;
    [SerializeField] private float steeringSmoothSpeed = 5f; // Скорость сглаживания

    private float smoothTime = 0.1f;

    private int hp = 3;
    private int petrol = 10;
    [SerializeField] private float petrol_rashod; // время через которое сбрасывается 1 еденица бензина
    private float timer = 0f;

    private bool flag_ground = false;// если мы были на траве и переходим на дорогуя

    private SpriteRenderer spriteRenderer;
    private int groundLayerMask;

    private void Awake()
    {

        if (sprite_obj != null)
            spriteRenderer = sprite_obj.GetComponent<SpriteRenderer>();

        // Кэшируем маску слоя (работает гораздо быстрее целочисленный сдвиг битов)
        groundLayerMask = LayerMask.GetMask("Ground");

        GameManager.Instance.hp.Hp(hp);//берем в скрипте MenuController ссылку на скрипт HP, в котором вызываем функцию отображения жизней
        id = MODEL_WORLD.Instance.active_car_id;
    }

    public void OptionsTriggers()
    {
        // Создаем запись для Drag события
        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.PointerDown;
        dragEntry.callback = new EventTrigger.TriggerEvent();
        dragEntry.callback.AddListener((data) => GasFlag = true);
        // Добавляем запись в триггеры
        GasPedal.triggers.Add(dragEntry);

        // Создаем запись для Drag события
        dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.PointerUp;
        dragEntry.callback = new EventTrigger.TriggerEvent();
        dragEntry.callback.AddListener((data) => GasFlag = false);
        // Добавляем запись в триггеры
        GasPedal.triggers.Add(dragEntry);
    }

    void Start()
    {
        Characteristics();
        // Убедимся, что активен только нужный UI
        UpdateControlUI();
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

    public void Active_Game(bool active)
    {
        active_game = active;
    }

    // Метод для переключения режимов управления
    public void SetControlMode(ControlMode mode)
    {
        currentControlMode = mode;
        UpdateControlUI();
    }

    // Переключить на следующий режим
    public void ToggleControlMode()
    {
        currentControlMode = currentControlMode == ControlMode.Joystick ?
            ControlMode.SteeringWheel : ControlMode.Joystick;
        UpdateControlUI();
    }

    // Обновление UI элементов управления
    private void UpdateControlUI()
    {
        if (joystickUI != null)
            joystickUI.SetActive(currentControlMode == ControlMode.Joystick);

        if (steeringWheelUI != null)
            steeringWheelUI.SetActive(currentControlMode == ControlMode.SteeringWheel);
    }

    private void Update()
    {
        if (!active_game) return;

        // Ввод газа (Клавиатура W)
        if (Input.GetKey(KeyCode.Space)) { GasFlag = true; }
        if (Input.GetKeyUp(KeyCode.Space)) { GasFlag = false; }

        // Расчет направления поворота
        UpdateTurnInput();

        // Поворот физического тела машины
        RotateCarBody();

        // Визуальный поворот колес/спрайта
        RotateSpriteVisual();

        // Движение вперед
        UpMoving();

        // Набор/Сброс скорости
        UpdateSpeedInterpolation();

        // Бензин
        UpdatePetrol();
    }

    private void UpdateTurnInput()
    {
        float targetInput = 0f;

        // 1. Проверяем клавиатуру (A/D)
        float keyboardInput = Input.GetAxisRaw("Horizontal"); // A = -1, D = 1

        if (Mathf.Abs(keyboardInput) > 0.01f)
        {
            // Нажата клавиатура: в Unity 2D (ось Z) поворот ВЛЕВО — это плюс, ВПРАВО — минус.
            // При нажатии A (-1) мы хотим ехать влево (+1), поэтому меняем знак.
            targetInput = -keyboardInput;
        }
        else
        {
            // 2. Клавиатура не нажата — проверяем выбранный мобильный UI
            switch (currentControlMode)
            {
                case ControlMode.Joystick:
                    if (joystick != null && joystickUI != null && joystickUI.activeSelf)
                    {
                        if (invertSteering) targetInput = joystick.Horizontal;
                        else targetInput = -joystick.Horizontal;
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

        // Единое плавное сглаживание для ЛЮБОГО типа ввода (убирает «эффект льда» и резкие рывки)
        currentSteeringValue = Mathf.Lerp(currentSteeringValue, targetInput, steeringSmoothSpeed * Time.deltaTime);
        turnInput = currentSteeringValue;
    }

    private void RotateCarBody()
    {
        // Если машина стоит на месте, она не должна разворачиваться
        if (active_speed <= 0.1f) return;

        float currentMovingSpeed = IsGrounded() ? grounded_speed : speed;
        float speedFactor = Mathf.Clamp(1f - (currentMovingSpeed / speed_max * 0.3f), 0.7f, 1f);

        // Переводим ввод в угол вращения. 
        // Добавлено умножение на Mathf.Sign(active_speed), чтобы при движении назад (если добавишь) реверсировался руль.
        float rotationAmount = turnInput * rotationSpeed * speedFactor * Time.deltaTime;

        transform.Rotate(0, 0, rotationAmount);
    }

    private void RotateSpriteVisual()
    {
        if (sprite_obj == null) return;

        // Плавный наклон колес/спрайта относительно текущего turnInput
        // turnInput меняется от -1 (вправо) до 1 (влево)
        float targetRotation = turnInput * max_rotate;

        sprite_obj.transform.localRotation = Quaternion.Lerp(
            sprite_obj.transform.localRotation,
            Quaternion.Euler(0, 0, targetRotation),
            Time.deltaTime * 12f
        );
    }

    private void UpMoving()
    {
        // Движение строго вперед относительно ориентации машины
        if (IsGrounded())
        {
            Vector3 moveDirection = transform.up * grounded_speed * Time.deltaTime;
            transform.position += moveDirection;
            flag_ground = true;
        }
        else
        {
            if (flag_ground)
            {
                active_speed = grounded_speed;
                Interpolizion_speed();
                flag_ground = false;
            }
            Vector3 moveDirection = transform.up * speed * Time.deltaTime;
            transform.position += moveDirection;
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
        // Исправлено: пускаем луч по направлению КУЗОВА (transform.up), а не глобально вверх (Vector2.up)
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

    IEnumerator Red_Color()
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
}