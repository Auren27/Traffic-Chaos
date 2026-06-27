using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class HP : MonoBehaviour
{
    // Singleton
    public static HP Instance { get; private set; }

    [SerializeField] private Image[] hp;
    [SerializeField] private Sprite heart;
    [SerializeField] private Sprite dead_heart;

    [SerializeField] private Image petrol;
    [SerializeField] private Sprite[] sprite_petrol;

    // полоска прохождения уровня
    [SerializeField] private Image Strip;
    [SerializeField] private Sprite[] sprite_strip;

    // диапазон км
    int mmin = 0;
    int mmax = 5;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Hp(int h)
    {
        for (int i = 0; i < hp.Length; i++)
        {
            if(i < h) // активная жизнь
            {
                hp[i].sprite = heart;
            }
            else
            {
                hp[i].sprite = dead_heart; break;
            }
        }
    }

    public void Petrol(int p)
    {
        petrol.sprite = sprite_petrol[p];
    }

    public int LevelLane(float i)
    {
        if (i < mmax)
        {
            float progress = (i - mmin) / (mmax - mmin);

            int m = Mathf.Clamp((int)(progress * 19), 0, 19);

            if (sprite_strip != null && m < sprite_strip.Length)
            {
                Strip.sprite = sprite_strip[m];
                return m;
            }
        }

        return 0;
    }
}
