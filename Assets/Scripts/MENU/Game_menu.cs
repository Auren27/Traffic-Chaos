using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class Game_menu : MonoBehaviour
{
    [SerializeField] private GameObject Menu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!Menu.activeSelf) { Menu_Open(); }
            else { Menu_Exit(); };
        }
    }

    public void Menu_Open()
    {
        Menu.SetActive(true);
        YandexGame.GameplayStop(); // стоп
        Time.timeScale = 0f;
    }

    public void Menu_Exit()
    {
        Menu.SetActive(false);
        YandexGame.GameplayStart(); // старт
        Time.timeScale = 1f;
    }

    public void Start_Menu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

}
