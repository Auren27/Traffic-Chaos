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
        // --- СЛУЧАЙ 1: Врезались в БОССА (НЛО) ---
        if (collision.gameObject.CompareTag("NLO"))
        {
            if (player != null && player.IsFlying)
            {
                // Если игрок в полете — атакуем НЛО!
                NLO_HP bossHP = collision.gameObject.GetComponent<NLO_HP>();
                if (bossHP != null)
                {
                    Debug.Log("Таран босса в полете!");
                    bossHP.TakeDamage(1); // Наносим 1 единицу урона боссу

                }
            }
            else
            {
                Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>(), true);
            }
            return; // Выходим из метода, чтобы не срабатывали проверки ниже
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (player != null && !player.IsFlying)
            {
                //collision.gameObject.GetComponent<Enemy_Move>().Flag_activ = false; // отключаем движение у противника
                Dead();
            }
            else
            {
                Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>(), true);
            }
        }
    }

    private void Dead()
    {
        Debug.Log("авария");
        player.Attack();
    }
}
