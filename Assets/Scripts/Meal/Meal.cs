using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meal : MonoBehaviour
{
    //public Player player;

    [Header("Настройки движения")]
    private float fallSpeed = 5f;       // Скорость падения вниз
    private float rotationSpeed = 10f; // Скорость вращения (градусов в секунду)

    [Header("Настройки спрайтов")]
    [SerializeField] private Sprite[] SpriteMeal;

    private void Awake()
    {
        RandomMeal();
    }

    public void RandomMeal()
    {
        int randomIndex = Random.Range(0, SpriteMeal.Length);
        gameObject.GetComponent<SpriteRenderer>().sprite = SpriteMeal[randomIndex];
    }

    void Update()
    {
        // 1. Движение вниз
        // Vector3.down — это вектор (0, -1, 0)
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // 2. Вращение вокруг оси Z (для 2D)
        // Space.Self крутит объект относительно его собственного центра
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime, Space.Self);

    }
}
