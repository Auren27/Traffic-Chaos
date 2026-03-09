using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road_Chareacter : MonoBehaviour
{
    public GameObject[] background;
    public GameObject[] fence;
    public int type_road; // 0 - прямая
    public GameObject[] Grass_Collisions;
    public BoxCollider2D Trigger_Oblast;
    public E_Car[] enemys_car;
}
