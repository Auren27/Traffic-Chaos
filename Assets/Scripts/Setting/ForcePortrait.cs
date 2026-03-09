using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcePortrait : MonoBehaviour
{
    void Awake()
    {
        // Если игра запущена на ПК или в WebGL
        if (!Application.isMobilePlatform)
        {
            // Устанавливаем портретное разрешение (например, 1080x1920)
            Screen.SetResolution(1080, 1920, false);
            // Блокируем изменение ориентации
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }
}
