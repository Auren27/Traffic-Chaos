using TMPro;
using Unity.VisualScripting;
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

    [Header("Меню комиксов")]
    public GameObject comic_menu;
    public Image Slides_Image;
    public Sprite[] Slides_0;
    public Sprite[] Slides_1;
    public Sprite[] Slides_2;
    private int currentComicNumber; // Какой комикс открыт (0, 1, 2)
    private int currentSlideIndex;  // Какая страница комикса сейчас показывается

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
        menuTabs = new GameObject[11];
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
        menuTabs[10] = comic_menu;
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

        // Меню поражения
        dead_menu_menu[0].onClick.AddListener(() => GameManager.Instance.Menu_Button());
        dead_menu_menu[1].onClick.AddListener(() => GameManager.Instance.Menu_Button());

        // Меню настроек
        settings_menu_menu.onClick.AddListener(Levels_menu_button);
        settings_player_wheel.onClick.AddListener(SettingsOption_PlayerWheel);
        settings_player_joystik.onClick.AddListener(SettingsOption_PlayerJoystik);

        // Меню доната
        donation_menu_menu.onClick.AddListener(Levels_menu_button);

        first_start_menu_menu.onClick.AddListener(() => GameManager.Instance.Start_menu());

        Slides_Image.gameObject.GetComponent<Button>().onClick.AddListener(Comic_touch);

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

    private void Comic_menu_Start(int number)
    {
        ShowTab(10);

        currentComicNumber = number; // Запоминаем, какой комикс открыли
        currentSlideIndex = 0;       // Начинаем с первого слайда

        SetComicSprite(currentComicNumber, currentSlideIndex);
    }

    // Этот метод вызывается при клике/касании по экрану комикса
    public void Comic_touch()
    {
        currentSlideIndex++; // Переходим к следующему слайду

        // Проверяем, не закончились ли слайды в текущем комиксе
        if (IsComicFinished(currentComicNumber, currentSlideIndex))
        {
            Comic_menu_End(); // Если закончились, закрываем комикс
        }
        else
        {
            // Если слайды есть, показываем следующий
            SetComicSprite(currentComicNumber, currentSlideIndex);
        }
    }

    // Вспомогательный метод для отображения нужного спрайта
    private void SetComicSprite(int comicNum, int slideNum)
    {
        switch (comicNum)
        {
            case 0:
                Slides_Image.sprite = Slides_0[slideNum];
                break;
            case 1:
                Slides_Image.sprite = Slides_1[slideNum];
                break;
            case 2:
                Slides_Image.sprite = Slides_2[slideNum];
                break;
        }
    }

    // Вспомогательный метод для проверки окончания массива слайдов
    private bool IsComicFinished(int comicNum, int slideNum)
    {
        switch (comicNum)
        {
            case 0: return slideNum >= Slides_0.Length;
            case 1: return slideNum >= Slides_1.Length;
            case 2: return slideNum >= Slides_2.Length;
            default: return true;
        }
    }

    private void Comic_menu_End()
    {
        GameManager.Instance.Menu_Button();
    }

    // Методы уровней
    private void Level_1_Button()
    {
        Architecture.Instance.activeNumberLevel = 1;
        Comic_menu_Start(0);
    }

    public void Level_1_Vin()
    {
        Architecture.Instance.activeNumberLevel = 2;
        DataManager.Instance.CompletedLevels(1);
        Comic_menu_Start(1);
    }

    public void Level_2_Vin()
    {
        Architecture.Instance.activeNumberLevel = 2;
        DataManager.Instance.CompletedLevels(2);
        Comic_menu_Start(2);
    }

    private void Level_2_Button()
    {
        if (DataManager.Instance.completed_levels >= 1)
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
                    //if (DataManager.Instance.menu_km >= MODEL_WORLD.Instance.GetLevelsById(1).currency)
                    //    levels_Imagelock[i].gameObject.SetActive(false);
                    //else
                    //    levels_Imagelock[i].gameObject.SetActive(true);
                    if (DataManager.Instance.completed_levels >= 1)
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