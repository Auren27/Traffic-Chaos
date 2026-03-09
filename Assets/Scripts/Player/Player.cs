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

    [SerializeField] private bool invertSteering = true; // Инверсия управления (включена по умолчанию)

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
    private float rotationSpeed;
    public GameObject sprite_obj;
    [SerializeField] private float min_rotate;
    [SerializeField] private float max_rotate;

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

    private bool flag_ground = false;// если мы были на траве и переходим на дорогу

    private void Awake()
    {
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
        speed_min = MODEL_WORLD.Instance.GetVehicleByIdCar(id).speed_min;
        speed_max = MODEL_WORLD.Instance.GetVehicleByIdCar(id).speed_max;
        grounded_speed_min = MODEL_WORLD.Instance.GetVehicleByIdCar(id).grounded_speed_min;
        grounded_speed_max = MODEL_WORLD.Instance.GetVehicleByIdCar(id).grounded_speed_max;
        transfer_time = MODEL_WORLD.Instance.GetVehicleByIdCar(id).transfer_time;
        rotationSpeed = MODEL_WORLD.Instance.GetVehicleByIdCar(id).rotationSpeed;
        petrol_rashod = MODEL_WORLD.Instance.GetVehicleByIdCar(id).petrol_rashod;
        sprite_obj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(MODEL_WORLD.Instance.GetVehicleByIdCar(id).imagePath);
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

    private void LateUpdate()
    {
        if (active_game)
        {
            if (Input.GetKey(KeyCode.Space)) { GasFlag = true; /*Debug.Log("Space");*/ }
            if (Input.GetKeyUp(KeyCode.Space)) { GasFlag = false; }

            UpMoving();

            // Управление в зависимости от выбранного режима
            //bool isControlling = false;

            switch (currentControlMode)
            {
                case ControlMode.Joystick:
                    MoveCarWithJoystick();
                    break;

                case ControlMode.SteeringWheel:
                    MoveCarWithSteeringWheel();
                    break;
            }

            // Ускорение/замедление в зависимости от управления
            if (GasFlag)
            {
                speed_timer += Time.deltaTime;
                if (speed_timer >= transfer_time)
                {
                    if (active_speed < 10) active_speed += 0.5f;
                    Interpolizion_speed();
                    speed_timer = 0f;
                }

                //подсветка педали (затемнение)
                GasPedal.gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
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

                //подсветка педали (возвращаем)
                GasPedal.gameObject.GetComponent<Image>().color = Color.white;
            }

            // Расход бензина
            timer += Time.deltaTime;
            if (timer >= petrol_rashod)
            {
                petrol -= 1;
                GameManager.Instance.hp.Petrol(petrol);
                if (petrol == 0) GameManager.Instance.Dead_car();
                timer = 0f;
            }

        }
    }

    private void UpMoving()
    {
        // Движение вперед/назад (одинаково для обоих режимов)
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

    private void Interpolizion_speed()
    {
        speed = speed_min + (active_speed / 10 * (speed_max - speed_min));
        grounded_speed = grounded_speed_min + (active_speed / 10 * (grounded_speed_max - grounded_speed_min));
    }

    // Метод для управления с помощью джойстика
    private void MoveCarWithJoystick()
    {
        if (joystick == null || joystickUI == null || !joystickUI.activeSelf)
            return;

        // Получаем ввод от джойстика
        turnInput = joystick.Horizontal;

        // Поворот спрайта автомобиля
        float targetRotation = max_rotate + ((min_rotate - max_rotate) / (1 + 1)) * (turnInput + 1);
        sprite_obj.transform.rotation = Quaternion.Lerp(
            sprite_obj.transform.rotation,
            Quaternion.Euler(0, 0, targetRotation),
            Time.deltaTime * 11f
        );

        // Двигаемся вправо/влево
        Vector3 turnDirection = transform.right * turnInput * rotationSpeed * Time.deltaTime;
        transform.position += turnDirection;
    }

    // Метод для управления с помощью руля
    private void MoveCarWithSteeringWheel()
    {
        if (steeringWheel == null || steeringWheelUI == null || !steeringWheelUI.activeSelf)
            return;

        // Получаем нормализованный ввод от руля (-1 до 1) с учетом оборотов
        float rawInput = GetSteeringInputWithRotations();

        // Применяем инверсию если нужно
        if (invertSteering)
        {
            rawInput = -rawInput;
        }

        // Применяем мертвую зону
        if (Mathf.Abs(rawInput) < steeringDeadZone)
        {
            rawInput = 0f;
        }

        // Плавное изменение значения поворота
        currentSteeringValue = Mathf.Lerp(currentSteeringValue, rawInput, steeringSmoothSpeed * Time.deltaTime);
        turnInput = currentSteeringValue;

        // Поворот спрайта автомобиля
        float targetRotation = max_rotate + ((min_rotate - max_rotate) / (1 + 1)) * (turnInput + 1);
        sprite_obj.transform.rotation = Quaternion.Lerp(
            sprite_obj.transform.rotation,
            Quaternion.Euler(0, 0, targetRotation),
            Time.deltaTime * 11f
        );

        // Движение вправо/влево с учетом коэффициента передачи и скорости
        float speedFactor = Mathf.Clamp(1f - (speed / speed_max * 0.5f), 0.5f, 1f);
        float effectiveRatio = wheelToCarRatio * speedFactor;

        Vector3 turnDirection = transform.right * turnInput * rotationSpeed * effectiveRatio * Time.deltaTime;
        transform.position += turnDirection;
    }

    // метод для получения ввода от рулевого колеса с учетом оборотов
    private float GetSteeringInputWithRotations()
    {
        if (steeringWheel == null) return 0f;

        // Используем общее вращение (totalRotation) вместо текущего угла
        float totalRotation = steeringWheel.GetTotalRotation();

        // Получаем максимальный возможный угол поворота (в градусах)
        float maxTotalRotation = steeringWheel.maxRotations * 360f;

        // Нормализуем от -1 до 1
        float normalizedInput = Mathf.Clamp(totalRotation / maxTotalRotation, -1f, 1f);

        // Применяем кривую отклика для более точного управления в центре
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
        float groundCheckDistance = 1f;
        LayerMask groundLayer = LayerMask.GetMask("Ground");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    public void Attack()
    {
        hp -= 1;
        if (hp > 0)
        {
            GameManager.Instance.hp.Hp(hp);
            sprite_obj.GetComponent<SpriteRenderer>().color = Color.red;
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
        sprite_obj.GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void PetrolAdd(int count)
    {
        if (petrol < 10)
        {
            petrol += count;
            GameManager.Instance.hp.Petrol(petrol);
        }
    }

    //// Метод для получения текущего ввода (может пригодиться для UI)
    //public float GetCurrentSteeringInput()
    //{
    //    return turnInput;
    //}

    //// Метод для получения текущего режима управления
    //public ControlMode GetCurrentControlMode()
    //{
    //    return currentControlMode;
    //}

    //// Метод для получения количества оборотов руля (для отладки)
    //public int GetSteeringWheelRotations()
    //{
    //    if (steeringWheel != null)
    //        return steeringWheel.GetFullRotations();
    //    return 0;
    //}
}