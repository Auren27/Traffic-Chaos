using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_touch : MonoBehaviour
{
    private GameManager mc;

    [SerializeField] private Player player;

    private void Awake()
    {
        mc = GameObject.FindWithTag("MenuController").GetComponent<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Если игрок в полете, полностью игнорируем опасные наземные объекты
        if (player.IsFlying)
        {
            if (collision.CompareTag("Bomb"))
            {
                return; // Просто выходим из метода, бомба не нанесет урона и не уничтожится
            }

            // Если вы хотите, чтобы еда (Meal) тоже оставалась на земле и не бралась в полете:
            if (collision.CompareTag("Meal"))
            {
                return;
            }
        }

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
            player.PetrolAdd(1);
            collision.gameObject.GetComponent<Coin>().CoinDestroy();
        }
        if (collision.CompareTag("Meal"))
        {
            player.AddHp();
            collision.gameObject.GetComponent<Coin>().CoinDestroy();
        }
        if (collision.CompareTag("Bomb"))
        {
            player.Attack();
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
                    case "инверсия":
                        player.ApplyTimedBooster(spriteName, player.StartBoostInverted, player.EndBoostInverted, 15f);
                        break;
                    case "ковш":
                        player.ApplyTimedBooster(spriteName, player.StartBoostBucket, player.EndBoostBucket, 15f);
                        break;
                    case "магнит":
                        player.ApplyTimedBooster(spriteName, player.StartBoostMagnet, player.EndBoostMagnet, 15f);
                        break;
                    case "пружина":
                        player.ApplyTimedBooster(spriteName, player.StartBoostSpring, player.EndBoostSpring, 5f);
                        break;
                    case "танк":
                        player.ApplyTimedBooster(spriteName, player.StartBoostTank, player.EndBoostTank, 15f);
                        break;
                    case "ускорение":
                        player.ApplyTimedBooster(spriteName, player.StartNitro, player.EndNitro,15f);
                        break;
                    case "щит":
                        player.ApplyTimedBooster(spriteName, player.StartBoostShield, player.EndBoostShield, 15f);
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
