using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Zone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            //Debug.Log("нашел");
            E_Car enemyCar = collision.gameObject.transform.parent.GetComponent<E_Car>();
            if (enemyCar != null)
            {
                enemyCar.ActivateEnemy();
            }
        }
    }

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Meal"))
    //    {
    //        Meal meal = collision.gameObject.transform.parent.GetComponent<Meal>();
    //        if (meal != null)
    //        {
    //            meal.Evaporate();
    //        }
    //    }
    //}


    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Enemy"))
    //    {
    //        Debug.Log("удалил");
    //        //collision.gameObject.GetComponent<Enemy_Move>().Exit_zone();
    //        E_Car enemyCar = collision.gameObject.transform.parent.GetComponent<E_Car>();
    //        if (enemyCar != null)
    //        {
    //            enemyCar.ExitPlayerZone();
    //        }
    //    }
    //    //if (collision.CompareTag("Coin"))
    //    //{
    //    //    Debug.Log("bcxtpkf vjytnrf");
    //    //    collision.gameObject.GetComponent<Coin>().CoinDestroy();
    //    //}
    //    //if (collision.CompareTag("Crystal"))
    //    //{
    //    //    collision.gameObject.GetComponent<Coin>().CoinDestroy();
    //    //}
    //    //if (collision.CompareTag("Petrol"))
    //    //{
    //    //    collision.gameObject.GetComponent<Coin>().CoinDestroy();
    //    //}
    //}

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Coin"))
    //    {
    //        Debug.Log("bcxtpkf vjytnrf");
    //        collision.gameObject.GetComponent<Coin>().CoinDestroy();
    //    }
    //    if (collision.gameObject.CompareTag("Crystal"))
    //    {
    //        collision.gameObject.GetComponent<Coin>().CoinDestroy();
    //    }
    //    if (collision.gameObject.CompareTag("Petrol"))
    //    {
    //        collision.gameObject.GetComponent<Coin>().CoinDestroy();
    //    }
    //}
}
