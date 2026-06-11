using System.Collections;
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
}