using UnityEngine;

public class Enemy_JSON : MonoBehaviour
{
    private int id;
    GameObject car;

    public void StartJSON_Car()
    {
        car = this.gameObject;
        Car_Randomizer();
    }

    private void Car_Randomizer()
    {
        id = Random.Range(1, MODEL_WORLD.Instance.GetAllVehiclesCar().Count);

        Car_Change();
    }

    private void Car_Change()
    {
        //Debug.Log("ошибка "+id);
        car.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(MODEL_WORLD.Instance.GetVehicleByIdCar(id).imagePath);

        float newSpeed = MODEL_WORLD.Instance.GetVehicleByIdCar(id).speed_max * 40 / 100;
        //Debug.Log(newSpeed);

        float newRSpeed = MODEL_WORLD.Instance.GetVehicleByIdCar(id).rotationSpeed;

        car.transform.parent.GetComponent<E_Car>().New_Speed(newSpeed, newRSpeed);
    }
}
