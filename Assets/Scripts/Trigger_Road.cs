using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Road : MonoBehaviour
{
    //[SerializeField] private Architecture arh;
    [SerializeField] private Road_Chareacter r_c;

    private void Awake()
    {
        //arh = GameObject.FindWithTag("Architecture").GetComponent<Architecture>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Architecture.Instance.SpawnNewRoad(r_c.type_road);
            DataManager.Instance.KMAdd();
        }
    }
}
