using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meal : MonoBehaviour
{
    //public Player player;

    [Header("Настройки движения")]
    private float fallSpeed = 3f;       // Скорость падения вниз
    private float rotationSpeed = 50f; // Скорость вращения (градусов в секунду)

    [Header("Настройки спрайтов")]
    [SerializeField] private Sprite[] SpriteMeal;

    [Header("Удаление объекта")]
    [SerializeField] private float destroyYThreshold = -6f; // Координата Y, ниже которой объект удаляется

    public GameObject SpriteGO;

    private void Awake()
    {
        RandomMeal();
    }

    public void RandomMeal()
    {
        int randomIndex = Random.Range(0, SpriteMeal.Length);
        SpriteGO.gameObject.GetComponent<SpriteRenderer>().sprite = SpriteMeal[randomIndex];
    }

    void Update()
    {
        // 1. Движение вниз
        // Vector3.down — это вектор (0, -1, 0)
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.Self);

        // 2. Вращение вокруг оси Z (для 2D)
        // Space.Self крутит объект относительно его собственного центра
        SpriteGO.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime, Space.Self);

        // 3. Оптимизация: удаляем объект, если он улетел за нижний экран
        if (transform.position.y < destroyYThreshold)
        {
            Destroy(gameObject);
        }
    }

    public void Evaporate()
    {
        Destroy(gameObject);
    }
}
