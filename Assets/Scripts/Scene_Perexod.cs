using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Perexod : MonoBehaviour
{
    public string sceneName; // имя сцены для перехода

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName); // загрузка сцены по имени
    }
}
