using System.Collections;
using UnityEngine;

public class Spawn_Obj : MonoBehaviour
{
    [SerializeField] private int count_enemy;
    [SerializeField] private GameObject objectsToSpawn_Up; // Массив объектов для спавна
    //[SerializeField] private int count_Ground_Up;
    [SerializeField] private GameObject objectsToSpawn_Ground_Up; // Массив объектов для спавна
    //[SerializeField] private int count_Down;
    [SerializeField] private GameObject objectsToSpawn_Down; // Массив объектов для спавна
    //[SerializeField] private int count_Ground_Down;
    [SerializeField] private GameObject objectsToSpawn_Ground_Down; // Массив объектов для спавна

    [SerializeField] private int count_currency;
    [SerializeField] private GameObject[] currency; // Массив объектов для спавна

    private float spawnRadius = 16f; // Радиус, в который будут спауниться объекты
    private bool flag_up = true;

    [SerializeField] private GameObject road; // монетки будем привязывать к road, чтобы удалялись вместе с дорогой 
    

    private void Awake()
    {
        SpawnObject();
    }

    private void SpawnObject()
    {
        
        for (int i = 0; i < count_enemy; i++)
        {
            Vector2 randomPosition = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(randomPosition, 1.0f); // Поиск коллайдеров в указанном круге
            bool touch = false;
            foreach (Collider2D col in colliders)
            {
                if (col.gameObject.tag == "Enemy")
                {
                    break;
                }
                else if (col.gameObject.tag == "Road")
                {
                    touch = true;
                    Debug.Log("Соприкоснулся с дорогой");
                    if (flag_up)
                    {
                        // Добавляем новый объект в конец
                        Instantiate(objectsToSpawn_Up, randomPosition, Quaternion.identity);
                        flag_up = !flag_up;
                    }
                    else
                    {
                        Instantiate(objectsToSpawn_Down, randomPosition, Quaternion.identity);
                        flag_up = !flag_up;
                    }
                    break;
                }
                else if (col.gameObject.tag == "Ground")
                {
                    touch = true;
                    Debug.Log("Соприкоснулся с травой");
                    if (flag_up)
                    {
                        Instantiate(objectsToSpawn_Ground_Up, randomPosition, Quaternion.identity);
                        flag_up = !flag_up;
                    }
                    else 
                    {
                        Instantiate(objectsToSpawn_Ground_Down, randomPosition, Quaternion.identity);
                        flag_up = !flag_up;
                    }
                    break;
                }
            }
            if (!touch)
            {
                i--;
            }
        }

        for (int i = 0; i < count_currency; i++)
        {
            GameObject spawn_obj = currency[Random.Range(0, currency.Length)];
            Vector2 randomPosition = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(randomPosition, 1.0f); // Поиск коллайдеров в указанном круге
            bool touch = false;
            foreach (Collider2D col in colliders)
            {
                if (col.gameObject.tag == "Coin")
                {
                    break;
                }
                else if (col.gameObject.tag == "Crystal")
                {
                    break;
                }
                else if (col.gameObject.tag == "Petrol")
                {
                    break;
                }
                else if (col.gameObject.tag == "Road")
                {
                    touch = true;
                    
                    GameObject nobj = Instantiate(spawn_obj, randomPosition, Quaternion.identity);
                    nobj.transform.SetParent(road.transform);
                    break;
                }
                else if (col.gameObject.tag == "Ground")
                {
                    touch = true;

                    GameObject nobj = Instantiate(spawn_obj, randomPosition, Quaternion.identity);
                    nobj.transform.SetParent(road.transform);
                    break;
                }
            }
            if (!touch)
            {
                i--;
            }
        }
        Destroy(gameObject);
    }
}
