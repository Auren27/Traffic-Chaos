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
            Debug.Log("бензин");
            player.GetComponent<Player>().PetrolAdd(1);
            collision.gameObject.GetComponent<Coin>().CoinDestroy();
        }
    }
}
