using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Move : MonoBehaviour
{
    //public Transform pointA; // Первая точка (верхняя)
    //public Transform pointB; // Вторая точка (нижняя)
    [SerializeField] private float speed = 2.0f; // Скорость движения
    //public bool ne_povorot;
    public bool Flag_activ = false;

    private Vector3 target;

    public int vector;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //rb.mass = 10000;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Activ()
    {
        Flag_activ = true;
        Debug.Log("поехали");
    }

    public void Exit_zone()
    {
        Destroy(gameObject.transform.parent.gameObject);
    }

    //void Awake()
    //{
    //    target = pointA.position; // Начальная цель
    //}

    void LateUpdate()
    {
        if (Flag_activ)
        {
            // Движение к цели
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y + vector, transform.position.z), speed * Time.deltaTime);

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Flag_activ && collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy_Move>().Flag_activ = false; // отключаем движение у противника
        }
    }
}
