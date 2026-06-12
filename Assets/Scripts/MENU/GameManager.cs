using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public Player_Spawn player_spawn;
    public HP hp;
    //private GameObject joystick;

    [Header("Game State")]
    public bool active_game = false;

    private bool stage1 = false;
    private bool stage2 = false;
    private bool stage3 = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        //joystick = player_spawn.joystick2;
    }

    void Start()
    {
        UIManager.Instance.ShowTab(9); // Показываем первую вкладку при запуске
        YandexGame.GameplayStart(); // старт
        StartCoroutine(MODEL_WORLD.Instance.Download()); // загружаем все данные с Json
    }

    private void Update()
    {
        if (active_game && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    public void StartGame()
    {
        active_game = true;
        YandexGame.GameplayStart();
        UIManager.Instance.ShowTab(5);

        DataManager.Instance.ResetSessionData();
        hp.Petrol(10);

        player_spawn.player.GetComponent<Player>().Active_Game(true);

        Stage1();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowTab(2);
        active_game = false;
        player_spawn.player.GetComponent<Player>().Active_Game(active_game);
        YandexGame.GameplayStop();
    }

    public void ResumeGame()
    {
        UIManager.Instance.ShowTab(5);
        Time.timeScale = 1f;
        active_game = true;
        player_spawn.player.GetComponent<Player>().Active_Game(active_game);
        YandexGame.GameplayStart();
    }

    public void Start_menu()
    {
        player_spawn.Spawn_car();
        UIManager.Instance.ShowTab(0);
        Time.timeScale = 1f;
        active_game = false;
        Architecture.Instance.SpawnStartRoad();
    }

    public void Menu_Button()
    {
        player_spawn.Destroy_car();
        player_spawn.Spawn_car();
        player_spawn.Destroy_meal();
        UIManager.Instance.ShowTab(0);
        Time.timeScale = 1f;
        active_game = false;
        Architecture.Instance.UnloadAll();
        Architecture.Instance.SpawnStartRoad();
        UIManager.Instance.UpdateCurrencyDisplay();
    }

    public void Dead_car()
    {
        UIManager.Instance.ShowTab(4);
        Time.timeScale = 0f;
        active_game = false;
        player_spawn.player.GetComponent<Player>().Active_Game(active_game);
    }

    // Методы для работы со сценами
    public void LoadScenes(int level_number)
    {
        SceneManager.LoadScene(DataManager.Instance.scenesToLoad[level_number], LoadSceneMode.Additive);
    }

    public void Stage1()
    {
        if(stage1 == false)
        {
            player_spawn.ActivSpawnMeal1();
            stage1 = true;
        }
    }

    public void Stage2()
    {
        if (stage2 == false)
        {
            player_spawn.ActivSpawnMeal2();
            stage2 = true;
        }
    }

    public void Stage3()
    {
        if (stage3 == false)
        {
            player_spawn.OffSpawnMeal();
            player_spawn.ActivNLO();
            stage3 = true;
        }
    }
}