using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    [SerializeField] private GameObject boost;
    [SerializeField] private GameObject platform;

    public SpriteRenderer boosterSpriteRenderer;

    private float rotationSpeed = 40f; // градусов в секунду

    private void Awake()
    {
        boosterSpriteRenderer = boost.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (boost != null)
        {
            boost.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    // Метод, который будет вызывать менеджер после спавна
    public void SetSprite(Sprite newSprite)
    {
        if (boosterSpriteRenderer != null && newSprite != null)
        {
            boosterSpriteRenderer.sprite = newSprite;
        }
    }
}
