using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Model_car : MonoBehaviour
{
    // Singleton
    public static Model_car Instance { get; private set; }

    [SerializeField] private GameObject active_bac;
    [SerializeField] private GameObject[] jac;
    [SerializeField] private GameObject[] jackdaw_active;
    [SerializeField] private Button button_GO;
    [SerializeField] private GameObject[] button_status;// играть, выбрать, купить (, кнопка заблокирована)
    [SerializeField] private TextMeshProUGUI[] text_buy;
    private int status = 0;
    [SerializeField] private int n_id=0;

    [SerializeField] private Image[] Strip;
    [SerializeField] private Sprite[] sprite_strip;

    [SerializeField] private GameObject[] car_points;
    [SerializeField] private GameObject[] obj_car_icons;
    [SerializeField] private Car_Icon[] car_icons;
    [SerializeField] private GameObject Prefab_car_icon;

    //кнопки сортировки
    [SerializeField] private GameObject[] Object_sort;// кнопки (сдвигаются для наглядности)
    [SerializeField] private GameObject[] point_sort1;// 1-е положение кнопок (статик)
    [SerializeField] private GameObject[] point_sort2;// 2-е положение кнопок
    private bool flag_sort = false;
    private int sort_i = 0;

    private MODEL_WORLD model;

    public void Start_Model()
    {
        if (flag_sort)
        {
            // зачищаем массив
            for (int i = 1; i <= obj_car_icons.Length; i++)
            {
                Destroy(obj_car_icons[i - 1]);
                Destroy(car_icons[i - 1]);
            }

            Download_model_car();

            flag_sort = false;
        }

        //gameObject.GetComponent<MenuController>().Start_menu();
        button_GO.onClick.AddListener(Click_Go_Car);
        new_Specifications(model.active_car_id);

        foreach (var item in car_icons)
        {
            item.ICon_Start();
        }

        Click_Icon_Car(model.active_car_id, active_bac, jackdaw_active);
    }

    private void Awake()
    {
        model = GetComponent<MODEL_WORLD>();

        Object_sort[0].GetComponent<Button>().onClick.AddListener(Sorted_castle);// уровни
        Object_sort[1].GetComponent<Button>().onClick.AddListener(Sorted_coin);// уровни
        Object_sort[2].GetComponent<Button>().onClick.AddListener(Sorted_cristall);// уровни

        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void Download_model_car()
    {
        obj_car_icons = new GameObject[car_points.Length];
        car_icons = new Car_Icon[car_points.Length];

        for (int i = 1; i <= car_points.Length; i++) // спавним на место точек иконки машин
        {
            obj_car_icons[i - 1] = Instantiate(Prefab_car_icon, car_points[i - 1].gameObject.transform); // создаем и сохраняем объект
            car_icons[i - 1] = obj_car_icons[i - 1].GetComponent<Car_Icon>(); // компонент Car_Icon отдельно
            car_icons[i - 1].id = i; // устанавливаем свой id для каждой машины

            if (i == model.active_car_id) // указываем, что выбрана первая
            {
                active_bac = car_icons[i - 1].bak;
                jackdaw_active = car_icons[i - 1].conditions;
                jackdaw_active[0].SetActive(true);
            }
        }
    }

    public void Click_Icon_Car(int new_id, GameObject bac, GameObject[] jackdaw) // нажал на машину
    {
        n_id = new_id;

        //показываем фон выбранной машины
        if (active_bac != null) active_bac.SetActive(false);
        active_bac = bac;
        active_bac.SetActive(true);
        //берем новый флажок (галочка)
        jac = jackdaw;

        new_Specifications(new_id);

        foreach (var bs in button_status)
        {
            bs.SetActive(false);
        }

        if (!model.GetVehicleByIdCar(new_id).purchase) // ещё не куплена
        {
            foreach (var tb in text_buy)
            {
                tb.gameObject.SetActive(false);
            }
            switch (model.GetVehicleByIdCar(new_id).purchase_method) //методы покупки
            {
                case "coin":
                    button_status[2].SetActive(true);
                    status = 2;
                    text_buy[0].gameObject.SetActive(true);
                    text_buy[0].text = model.GetVehicleByIdCar(new_id).currency.ToString();
                    ///
                    break;
                case "cristall":
                    button_status[2].SetActive(true);
                    status = 3;
                    text_buy[1].gameObject.SetActive(true);
                    text_buy[1].text = model.GetVehicleByIdCar(new_id).currency.ToString();
                    ///
                    break;
                case "castle":
                    button_status[2].SetActive(true);
                    status = 4;
                    text_buy[2].gameObject.SetActive(true);
                    text_buy[2].text = "level " + model.GetVehicleByIdCar(new_id).currency.ToString();
                    ///
                    break;
            }
        }
        else // уже куплена
        {

            if (model.active_car_id == new_id) // выбрана повторно
            {
                button_status[0].SetActive(true);
                status = 0;
            }
            else // выбрана в асартименте
            {
                button_status[1].SetActive(true);
                status = 1;
            }

            //берем новый id
            model.active_car_id = new_id;
        }
    }

    private void Click_Go_Car() //выбрал машину (нажал на кнопку start)
    {
        if (status > 1)
        {
            //foreach (var tb in text_buy)
            //{
            //    tb.gameObject.SetActive(false);
            //}
            if (status == 2)
            {
                if (DataManager.Instance.Buy_car_coin(model.GetVehicleByIdCar(n_id).currency)) // если купил
                {
                    model.GetVehicleByIdCar(n_id).purchase = true;
                    foreach (var item in jac)
                    {
                        item.gameObject.SetActive(false);
                    }
                    Click_Icon_Car(n_id, active_bac, jac);
                    Click_Go_Car();
                }
            }
            if (status == 3)
            {
                if (DataManager.Instance.Buy_car_crystal(model.GetVehicleByIdCar(n_id).currency)) // если купил
                {
                    model.GetVehicleByIdCar(n_id).purchase = true;
                    foreach (var item in jac)
                    {
                        item.gameObject.SetActive(false);
                    }
                    Click_Icon_Car(n_id, active_bac, jac);
                    Click_Go_Car();
                }
            }
            if (status == 4)
            {
                // смена уровня на нужный
            }
        }
        else
        {
            if (status == 1)
            {
                if (jackdaw_active[0] != null) jackdaw_active[0].SetActive(false);
                jackdaw_active = jac;
                jackdaw_active[0].SetActive(true);

                Click_Icon_Car(model.active_car_id, active_bac, jackdaw_active);
            }
            else if (status == 0)
            {
                //если была сортировка, то возвращаем кнопки на место
                if (sort_i == 1) Sorted_castle();
                else if (sort_i == 2) Sorted_coin();
                else if (sort_i == 3) Sorted_cristall();

                GameManager.Instance.Menu_Button();
            }
        }
    }


    private void new_Specifications(int car_id)
    {
        // strip 0-14 (0 - минимум, 14 - максимум)

        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0://скорость
                    int mmin = 1;
                    int mmax = 14;
                    float m = 0 + ((model.GetVehicleByIdCar(car_id).speed_max - mmin) / (mmax - mmin) * (14 - 0));
                    int mm = (int)m;
                    float mmin2 = 1;
                    float mmax2 = 0.1f;
                    float m2 = 0 + ((model.GetVehicleByIdCar(car_id).transfer_time - mmin2) / (mmax2 - mmin2) * (14 - 0));
                    int mm2 = (int)m2;
                    int mmm = (mm + mm2) / 2;
                    Strip[i].sprite = sprite_strip[mmm];
                    break;
                case 1://управляемость
                    mmin = 150;
                    mmax = 250;
                    m = 0 + ((model.GetVehicleByIdCar(car_id).rotationSpeed - mmin) / (mmax - mmin) * (14 - 0));
                    mm = (int)m;
                    Strip[i].sprite = sprite_strip[mm];
                    break;
                case 2://проходимость
                    mmin = 1;
                    mmax = 14;
                    m = 0 + ((model.GetVehicleByIdCar(car_id).grounded_speed_max - mmin) / (mmax - mmin) * (14 - 0));
                    mm = (int)m;
                    Strip[i].sprite = sprite_strip[mm];
                    break;
                case 3://топливо
                    mmin = 20;
                    mmax = 2;
                    m = 0 + ((model.GetVehicleByIdCar(car_id).petrol_rashod - mmin) / (mmax - mmin) * (14 - 0));
                    mm = (int)m;
                    Strip[i].sprite = sprite_strip[mm];
                    break;
            }
        }
    }

    private void Sorted_castle()
    {
        Color color;

        if (!flag_sort || sort_i != 1)
        {
            Sorted("castle");

            StartCoroutine(MoveToPoint(Object_sort[0], point_sort2[0].transform.position));
            color = Object_sort[0].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[0].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[1], point_sort1[1].transform.position));
            color = Object_sort[1].GetComponent<Image>().color;
            color.a = 0.5f;
            Object_sort[1].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[2], point_sort1[2].transform.position));
            color = Object_sort[2].GetComponent<Image>().color;
            color.a = 0.5f;
            Object_sort[2].GetComponent<Image>().color = color;
            sort_i = 1;
        }
        else
        {
            Start_Model();

            StartCoroutine(MoveToPoint(Object_sort[0], point_sort1[0].transform.position));
            color = Object_sort[0].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[0].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[1], point_sort1[1].transform.position));
            color = Object_sort[1].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[1].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[2], point_sort1[2].transform.position));
            color = Object_sort[2].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[2].GetComponent<Image>().color = color;
            sort_i = 0;
        }
    }

    private void Sorted_coin()
    {
        if (!flag_sort || sort_i != 2)
        {
            Sorted("coin");

            StartCoroutine(MoveToPoint(Object_sort[0], point_sort2[0].transform.position));
            Color color = Object_sort[0].GetComponent<Image>().color;
            color.a = 0.5f;
            Object_sort[0].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[1], point_sort2[1].transform.position));
            color = Object_sort[1].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[1].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[2], point_sort1[2].transform.position));
            color = Object_sort[2].GetComponent<Image>().color;
            color.a = 0.5f;
            Object_sort[2].GetComponent<Image>().color = color;
            sort_i = 2;
        }
        else
        {
            Start_Model();

            StartCoroutine(MoveToPoint(Object_sort[0], point_sort1[0].transform.position));
            Color color = Object_sort[0].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[0].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[1], point_sort1[1].transform.position));
            color = Object_sort[1].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[1].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[2], point_sort1[2].transform.position));
            color = Object_sort[2].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[2].GetComponent<Image>().color = color;
            sort_i = 0;
        }
    }

    private void Sorted_cristall()
    {
        if (!flag_sort || sort_i < 3)
        {
            Sorted("cristall");

            StartCoroutine(MoveToPoint(Object_sort[0], point_sort2[0].transform.position));
            Color color = Object_sort[0].GetComponent<Image>().color;
            color.a = 0.5f;
            Object_sort[0].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[1], point_sort2[1].transform.position));
            color = Object_sort[1].GetComponent<Image>().color;
            color.a = 0.5f;
            Object_sort[1].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[2], point_sort2[2].transform.position));
            color = Object_sort[2].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[2].GetComponent<Image>().color = color;
            sort_i = 3;
        }
        else
        {
            Start_Model();

            StartCoroutine(MoveToPoint(Object_sort[0], point_sort1[0].transform.position));
            Color color = Object_sort[0].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[0].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[1], point_sort1[1].transform.position));
            color = Object_sort[1].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[1].GetComponent<Image>().color = color;
            StartCoroutine(MoveToPoint(Object_sort[2], point_sort1[2].transform.position));
            color = Object_sort[2].GetComponent<Image>().color;
            color.a = 1f;
            Object_sort[2].GetComponent<Image>().color = color;
            sort_i = 0;
        }
    }

    private void Sorted(string name_val)
    {
        flag_sort = true;

        int k = 0;
        int k_id = 0;

        // зачищаем массив
        for (int i = 1; i <= obj_car_icons.Length; i++)
        {
            Destroy(obj_car_icons[i - 1]);
            Destroy(car_icons[i - 1]);
        }

        for (int i = 1; i <= car_points.Length; i++)
        {
            if (model.GetVehicleByIdCar(i).purchase_method == name_val)
            {
                k++;
            }
        }

        obj_car_icons = new GameObject[k];
        car_icons = new Car_Icon[k];

        for (int i = 1; i <= k; i++)
        {
            obj_car_icons[i - 1] = Instantiate(Prefab_car_icon, car_points[i - 1].gameObject.transform); // создаем и сохраняем объект
            car_icons[i - 1] = obj_car_icons[i - 1].GetComponent<Car_Icon>(); // компонент Car_Icon отдельно
            for (int j = 1; j <= car_points.Length; j++)
            {
                if (j <= k_id) continue;

                if (model.GetVehicleByIdCar(j).purchase_method == name_val)
                {
                    car_icons[i - 1].id = j; // устанавливаем свой id для каждой машины
                    k_id = j;

                    break;
                }
            }

            if (car_icons[i - 1].id == model.active_car_id) // указываем, что выбрана первая
            {
                active_bac = car_icons[i - 1].bak;
                active_bac.SetActive(true);
                jackdaw_active = car_icons[i - 1].conditions;
                jackdaw_active[0].SetActive(true);
            }
        }

        foreach (var item in car_icons)
        {
            item.ICon_Start();
        }


        ///
    }

    /// <summary>
    /// /
    /// </summary>
    // Корутина для плавного перемещения
    private IEnumerator MoveToPoint(GameObject obj, Vector2 targetPoint)
    {
        Vector3 startPosition = obj.transform.position; // Берём Vector3 (с учётом Z)
        float duration = Vector2.Distance(startPosition, targetPoint) / 100; // Ускоряем в 3 раза

        for (float t = 0; t < 1; t += Time.deltaTime / duration)
        {
            // Lerp между Vector2, но сохраняем Z из startPosition
            obj.transform.position = new Vector3(
                Mathf.Lerp(startPosition.x, targetPoint.x, t),
                Mathf.Lerp(startPosition.y, targetPoint.y, t),
                startPosition.z // Сохраняем исходный Z
            );
            yield return null;
        }

        // Фиксируем конечную позицию (с сохранением Z)
        obj.transform.position = new Vector3(targetPoint.x, targetPoint.y, startPosition.z);
    }
}
