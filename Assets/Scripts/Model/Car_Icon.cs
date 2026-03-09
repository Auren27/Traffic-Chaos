using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Car_Icon : MonoBehaviour
{
    private Model_car model_car;
    private MODEL_WORLD model;
    public int id;
    [SerializeField] private GameObject car;
    public GameObject bak;
    public GameObject[] conditions;

    private void Awake()
    {
        model_car = GameObject.FindWithTag("MenuController").GetComponent<Model_car>();
        model = GameObject.FindWithTag("MenuController").GetComponent<MODEL_WORLD>();
        car.GetComponent<Button>().onClick.AddListener(Click);
        bak.SetActive(false);
    }

    public void ICon_Start()
    {
        car.GetComponent<Image>().sprite = Resources.Load<Sprite>(model.GetVehicleByIdCar(id).imagePath);

        if (model.GetVehicleByIdCar(id).purchase == false) // не куплена
        {
            switch (model.GetVehicleByIdCar(id).purchase_method) // ставим значек
            {
                case "coin":
                    conditions[2].SetActive(true);
                    break;
                case "cristall":
                    conditions[3].SetActive(true);
                    break;
                case "castle":
                    conditions[1].SetActive(true);
                    break;

            }
        }

    }

    private void Click()
    {
        model_car.Click_Icon_Car(id, bak, conditions);
    }
}
