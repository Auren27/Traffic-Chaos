using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Player;

public class UIManager : MonoBehaviour
{
    // Singleton
    public static UIManager Instance { get; private set; }

    [Header("Menu Tabs")]
    public GameObject[] menuTabs;

    [Header("Стартовое меню")]
    public GameObject start_menu;//0
    //public GameObject press_to_start;
    public Button[] start_menu_button; // все кнопки в стартовом меню
    public TextMeshProUGUI coin_start_menu;
    public TextMeshProUGUI crystal_start_menu;
    public TextMeshProUGUI km_start_menu;

    [Header("Меню выбора уровней")]
    public GameObject levels_menu;//1
    public Button levels_menu_menu;
    //public Sprite Sprite_Button;
    //public Sprite Sprite_Buttonlock;
    //public GameObject[] levels_bac;// изображение фона (флажок выбора уровня)
    public Button levels_1;
    public Button levels_2;
    public Image[] levels_Imagelock;
    //public Button levels_3;
    //public Button levels_4;
    //public Button levels_5;

    [Header("Меню паузы")]
    public GameObject pause_menu;//2
    public Button[] pause_menu_resume;
    public Button[] pause_menu_menu;

    [Header("Меню победы")]
    public GameObject vin_menu;//3

    [Header("Меню поражения")]
    public GameObject dead_menu;//4
    public Button[] dead_menu_menu;

    [Header("Меню геймплея")]
    public GameObject game_menu;//5
    public Button game_menu_pause;
    public TextMeshProUGUI coin_game_menu;
    public TextMeshProUGUI crystal_game_menu;
    public TextMeshProUGUI km_game_menu;

    [Header("Меню выбоа машин")]
    public GameObject cars_menu;//6
    public Button cars_menu_menu;
    public GameObject[] cars_bac;// изображение фона (флажок выбора машины)

    [Header("Меню настроек")]
    public GameObject settings_menu;//7
    public Button settings_menu_menu;
    public Button settings_player_wheel;
    public GameObject settings_player_wheelImage;
    public Button settings_player_joystik;
    public GameObject settings_player_joystikImage;
    public LanguageButtons settings_language;

    [Header("Меню доната")]
    public GameObject donation_menu;//8
    public Button donation_menu_menu;

    [Header("Меню предзагрузки")]
    public GameObject first_start_menu;//9
    public Button first_start_menu_menu;

    [Header("State")]
    public int currentTabIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        InitializeUI();
        SetupButtonListeners();
    }

    private void InitializeUI()
    {
        // Инициализация массива вкладок меню
        menuTabs = new GameObject[10];
        menuTabs[0] = start_menu;
        menuTabs[1] = levels_menu;
        menuTabs[2] = pause_menu;
        menuTabs[3] = vin_menu;
        menuTabs[4] = dead_menu;
        menuTabs[5] = game_menu;
        menuTabs[6] = cars_menu;
        menuTabs[7] = settings_menu;
        menuTabs[8] = donation_menu;
        menuTabs[9] = first_start_menu;
    }

    private void SetupButtonListeners()
    {
        // Кнопки паузы
        pause_menu_resume[0].onClick.AddListener(() => GameManager.Instance.ResumeGame());
        pause_menu_menu[0].onClick.AddListener(() => GameManager.Instance.Menu_Button());
        pause_menu_resume[1].onClick.AddListener(() => GameManager.Instance.ResumeGame());
        pause_menu_menu[1].onClick.AddListener(() => GameManager.Instance.Menu_Button());

        game_menu_pause.onClick.AddListener(() => GameManager.Instance.PauseGame());

        levels_menu_menu.onClick.AddListener(Levels_menu_button);

        cars_menu_menu.onClick.AddListener(Levels_menu_button);

        // Стартовое меню
        start_menu_button[0].onClick.AddListener(Levels_menu_menu);
        start_menu_button[1].onClick.AddListener(Cars_menu_menu);
        start_menu_button[2].onClick.AddListener(() => GameManager.Instance.StartGame());
        start_menu_button[3].onClick.AddListener(Settings_menu_menu);
        start_menu_button[4].onClick.AddListener(Donation_menu_menu);
        start_menu_button[5].onClick.AddListener(Donation_menu_menu);

        dead_menu_menu[0].onClick.AddListener(() => GameManager.Instance.Menu_Button());
        dead_menu_menu[1].onClick.AddListener(() => GameManager.Instance.Menu_Button());

        settings_menu_menu.onClick.AddListener(Levels_menu_button);
        settings_player_wheel.onClick.AddListener(SettingsOption_PlayerWheel);
        settings_player_joystik.onClick.AddListener(SettingsOption_PlayerJoystik);

        donation_menu_menu.onClick.AddListener(Levels_menu_button);

        first_start_menu_menu.onClick.AddListener(() => GameManager.Instance.Start_menu());

        // Кнопки уровней
        levels_1.onClick.AddListener(Level_1_Button);
        levels_2.onClick.AddListener(Level_2_Button);
        //levels_3.onClick.AddListener(Level_3_Button);
        //levels_4.onClick.AddListener(Level_4_Button);
        //levels_5.onClick.AddListener(Level_5_Button);
    }

    public void ShowTab(int index)
    {
        foreach (GameObject tab in menuTabs)
        {
            tab.SetActive(false);
        }

        if (index >= 0 && index < menuTabs.Length)
        {
            menuTabs[index].SetActive(true);
            if (menuTabs[index].GetComponent<LanguageScene>())
            {
                menuTabs[index].GetComponent<LanguageScene>().isLanguage();
            }

            currentTabIndex = index;
        }
    }

    public void UpdateCurrencyDisplay()
    {
        if (DataManager.Instance == null) return;

        if (currentTabIndex == 5) // game_menu
        {
            coin_game_menu.text = DataManager.Instance.coin.ToString();
            crystal_game_menu.text = DataManager.Instance.crystal.ToString();
            km_game_menu.text = DataManager.Instance.km.ToString("F1") + " km";
        }
        else if (currentTabIndex == 0) // start_menu
        {
            coin_start_menu.text = DataManager.Instance.menu_coin.ToString();
            crystal_start_menu.text = DataManager.Instance.menu_crystal.ToString();
            km_start_menu.text = DataManager.Instance.menu_km.ToString("F1") + " km";
        }
    }

    // UI методы навигации
    private void Levels_menu_menu()
    {
        Level_lock();
        ShowTab(1);
    }
    private void Levels_menu_button() => ShowTab(0);
    private void Cars_menu_menu() 
    {
        ShowTab(6);
        Model_car.Instance.Start_Model();
    }

    private void Settings_menu_menu() 
    {
        ShowTab(7);
        SettingsOption_PlayerWheelUI();
        settings_language.Language_UI();
    }

    private void Donation_menu_menu() => ShowTab(8);


    // Методы уровней
    private void Level_1_Button()
    {
        Architecture.Instance.activeNumberLevel = 1;
        GameManager.Instance.Menu_Button();
    }

    private void Level_2_Button()
    {
        if (DataManager.Instance.menu_km >= MODEL_WORLD.Instance.GetLevelsById(1).currency)
        {
            Architecture.Instance.activeNumberLevel = 2;
            GameManager.Instance.Menu_Button();
        }
    }

    private void Level_3_Button()
    {
        
    }


    public void Level_lock()
    {
        for (int i = 0; i < levels_Imagelock.Length; i++)
        {
            switch (i)
            {
                case 0://2
                    if (DataManager.Instance.menu_km >= MODEL_WORLD.Instance.GetLevelsById(1).currency)
                        levels_Imagelock[i].gameObject.SetActive(false);
                    else
                        levels_Imagelock[i].gameObject.SetActive(true);
                    break;
            }
        }
    }

    private void SettingsOption_PlayerWheel() // настройки контроллера (руль)
    {
        GameManager.Instance.player_spawn.player.GetComponent<Player>().SetControlMode(ControlMode.SteeringWheel);
        GameManager.Instance.player_spawn.currentControlMode = ControlMode.SteeringWheel;

        SettingsOption_PlayerWheelUI();
    }

    private void SettingsOption_PlayerJoystik() // настройки контроллера (джойстик)
    {
        GameManager.Instance.player_spawn.player.GetComponent<Player>().SetControlMode(ControlMode.Joystick);
        GameManager.Instance.player_spawn.currentControlMode = ControlMode.Joystick;

        SettingsOption_PlayerWheelUI();
    }

    private void SettingsOption_PlayerWheelUI()// настройки контроллера (отображение)
    {
        if(GameManager.Instance.player_spawn.player.GetComponent<Player>().currentControlMode == ControlMode.SteeringWheel)
        {
            settings_player_wheelImage.SetActive(true);
            settings_player_joystikImage.SetActive(false);
        }
        else
        {
            settings_player_wheelImage.SetActive(false);
            settings_player_joystikImage.SetActive(true);
        }
    }
}