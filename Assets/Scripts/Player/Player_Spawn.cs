using UnityEngine;
using UnityEngine.EventSystems;
using static Player;

public class Player_Spawn : MonoBehaviour
{
    public GameObject car;
    [SerializeField] public GameObject spawn_point;
    [SerializeField] public Camera_Follow my_camera;
    [SerializeField] public Smoke smoke;
    public SpawnMeal spawnMeal;
    public Spawn_NLO spawnNlo;
    public GameObject wheel; // ссылка на руль
    public Joystick joystick; // ссылка на джойстик
    public GameObject joystickUI;
    public ControlMode currentControlMode;

    // Переменные для педалей
    public EventTrigger GasPedal;
    public EventTrigger StopPedal;

    public GameObject player;


    public void Spawn_car()
    {
        player = Instantiate(car, gameObject.transform);
        player.GetComponent<Player>().Characteristics();

        player.transform.position = spawn_point.transform.position;
        player.GetComponent<Player>().joystick = joystick;
        player.GetComponent<Player>().joystickUI = joystickUI;
        player.GetComponent<Player>().steeringWheel = wheel.GetComponent<SteeringWheel>();
        player.GetComponent<Player>().steeringWheel.ResetRotations();
        player.GetComponent<Player>().steeringWheelUI = wheel;
        player.GetComponent<Player>().GasPedal = GasPedal;
        player.GetComponent<Player>().StopPedal = StopPedal;
        player.GetComponent<Player>().OptionsTriggers();
        player.GetComponent<Player>().SetControlMode(currentControlMode);

        my_camera.target = player.GetComponent<Player>().sprite_obj.transform;

        smoke.target = player.GetComponent<Player>().sprite_obj.transform;
        smoke.StartPosition();

        spawnMeal.activeMeal = false;

        spawnNlo.target = player.GetComponent<Player>().sprite_obj.transform;
    }

    public void ActivSpawnMeal1()
    {
        spawnMeal.activeMeal = true;
        spawnMeal.Stage1();
    }

    public void ActivSpawnMeal2()
    {
        spawnMeal.activeMeal = true;
        spawnMeal.Stage2();
    }

    public void OffSpawnMeal()
    {
        spawnMeal.activeMeal = false;
        Destroy_meal();
    }

    public void ActivNLO()
    {
        spawnNlo.Spawn_nlo(player.GetComponent<Player>().sprite_obj.transform);
    }

    public void Destroy_car()
    {
        Destroy(player);
    }

    public void Destroy_NLO()
    {
        if (spawnNlo.nlo != null)
        {
            spawnNlo.nlo.GetComponent<NLO_Spawner>().ClearAllSpawnedObjects();
            Destroy(spawnNlo.nlo);
        }
    }

    public void Destroy_meal()
    {
        spawnMeal.OFFMassMeal();
    }
}
