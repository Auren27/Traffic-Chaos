using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_Enemy : MonoBehaviour
{
    [SerializeField] private GameObject[] enemys;

    private void Start()
    {
        int randomRoad = Random.Range(0, enemys.Length); // Генерирует случайное
        GameObject enemy = Instantiate(enemys[randomRoad]);
        enemy.transform.position = transform.position;
        Destroy(gameObject);
    }
}
