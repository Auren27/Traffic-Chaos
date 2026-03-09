using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class HP : MonoBehaviour
{
    [SerializeField] private Image[] hp;
    [SerializeField] private Sprite heart;
    [SerializeField] private Sprite dead_heart;

    [SerializeField] private Image petrol;
    [SerializeField] private Sprite[] sprite_petrol;

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
}
