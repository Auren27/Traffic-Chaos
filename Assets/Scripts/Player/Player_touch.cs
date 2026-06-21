using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_touch : MonoBehaviour
{
    private GameManager mc;

    [SerializeField] private GameObject player;

    private void Awake()
    {
        mc = GameObject.FindWithTag("MenuController").GetComponent<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            DataManager.Instance.CoinAdd(1);
            collision.gameObject.GetComponent<Coin>().CoinDestroy();
        }
        if (collision.CompareTag("Crystal"))
        {
            DataManager.Instance.CrystalAdd(1);
            collision.gameObject.GetComponent<Coin>().CoinDestroy();
        }
        if (collision.CompareTag("Petrol"))
        {
            player.GetComponent<Player>().PetrolAdd(1);
            collision.gameObject.GetComponent<Coin>().CoinDestroy();
        }
        if (collision.CompareTag("Meal"))
        {
            player.GetComponent<Player>().AddHp();
            collision.gameObject.GetComponent<Coin>().CoinDestroy();
        }
        if (collision.CompareTag("Bomb"))
        {
            player.GetComponent<Player>().Attack();
            collision.gameObject.GetComponent<Coin>().CoinDestroy();
        }
        if (collision.CompareTag("Booster"))
        {

            //player.GetComponent<Player>().Attack();

            // Ищем SpriteRenderer на самом объекте или его дочерних элементах
            SpriteRenderer sr = collision.gameObject.GetComponent<Booster>().boosterSpriteRenderer;

            if (sr != null && sr.sprite != null)
            {
                // Берем имя спрайта
                string spriteName = sr.sprite.name;

                // Различаем логику по названию файла спрайта в Unity
                switch (spriteName)
                {
                    case "инверсия": // Замените на точное имя вашего файла спрайта ускорения
                        player.GetComponent<Player>().BoostInverted();
                        break;
                    case "ковш": // Замените на точное имя вашего файла спрайта ускорения
                        break;
                    case "магнит": // Замените на точное имя вашего файла спрайта ускорения
                        break;
                    case "пружина": // Замените на точное имя вашего файла спрайта ускорения
                        break;
                    case "танк": // Замените на точное имя вашего файла спрайта ускорения
                        break;
                    case "ускорение": // Замените на точное имя вашего файла спрайта ускорения
                        break;
                    case "щит": // Замените на точное имя вашего файла спрайта ускорения
                        break;

                    default:
                        Debug.LogWarning("Подобран бустер с неизвестным спрайтом: " + spriteName);
                        break;
                }
            }

            collision.gameObject.GetComponent<Coin>().CoinDestroy();
        }
    }
}
