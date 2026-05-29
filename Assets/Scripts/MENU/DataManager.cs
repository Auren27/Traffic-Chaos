using System;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    // Singleton
    public static DataManager Instance { get; private set; }

    // Игровые данные
    public string[] scenesToLoad;
    public int scene_active = 0;

    [Header("Валюты")]
    public int menu_coin = 0;
    public int menu_crystal = 0;
    public float menu_km = 0;

    [Header("Игровые данные текущей сессии")]
    public int coin = 0;
    public int crystal = 0;
    public float km = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Методы для работы с валютой
    public void KMAdd()
    {
        km += 0.1f;
        menu_km += 0.1f;
        UIManager.Instance.UpdateCurrencyDisplay();
    }

    public void CoinAdd(int count)
    {
        coin += count;
        menu_coin += count;
        UIManager.Instance.UpdateCurrencyDisplay();
    }

    public void CrystalAdd(int count)
    {
        crystal += count;
        menu_crystal += count;
        UIManager.Instance.UpdateCurrencyDisplay();
    }

    public bool Buy_car_coin(int coin)
    {
        if (menu_coin >= coin)
        {
            menu_coin -= coin;
            UIManager.Instance.UpdateCurrencyDisplay();
            return true;
        }
        return false;
    }

    public bool Buy_car_crystal(int crystal)
    {
        if (menu_crystal >= crystal)
        {
            menu_crystal -= crystal;
            UIManager.Instance.UpdateCurrencyDisplay();
            return true;
        }
        return false;
    }

    public void ResetSessionData()
    {
        coin = 0;
        crystal = 0;
        km = 0;
        UIManager.Instance.UpdateCurrencyDisplay();
    }

    public void Levels_Download()
    {
        //foreach (var item in UIManager.Instance.levels_bac)
        //{
        //    item.SetActive(false);
        //}
        //UIManager.Instance.levels_bac[scene_active].SetActive(true);

        Model_car.Instance.Download_model_car();
        UIManager.Instance.first_start_menu_menu.gameObject.SetActive(true);
    }
}