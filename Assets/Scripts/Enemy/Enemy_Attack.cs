using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Enemy_Attack : MonoBehaviour
{
    [SerializeField] private Player player;

    private void Awake()
    {
        player = this.GetComponent<Player>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //collision.gameObject.GetComponent<Enemy_Move>().Flag_activ = false; // отключаем движение у противника
            Dead();
        }
    }

    private void Dead()
    {
        Debug.Log("авария");
        player.Attack();
    }
}
