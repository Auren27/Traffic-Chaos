using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class End_Level : MonoBehaviour
{
    [SerializeField] private GameObject Menu;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Menu_Open();
        }
    }

    private void Menu_Open()
    {
        Menu.SetActive(true);
        Time.timeScale = 0f;
    }
}
