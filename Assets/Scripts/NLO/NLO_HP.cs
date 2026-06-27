using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NLO_HP : MonoBehaviour
{
    [Header("Настройки Здоровья")]
    [SerializeField] private int maxHp = 3;
    private int currentHp;

    [Header("Визуальный эффект урона")]
    [SerializeField] private Renderer[] ufoRenderer; // Ссылки на графику (как в контроллере)

    private void Start()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"НЛО получил урон! Осталось HP: {currentHp}");

        // Эффект мигания при уроне
        StartCoroutine(DamageFlashRoutine());

        if (currentHp <= 0)
        {
            BossDead();
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (ufoRenderer == null) yield break;

        // Быстро мигнем белым или прозрачным при попадании
        for (int i = 0; i < ufoRenderer.Length; i++)
            ufoRenderer[i].material.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < ufoRenderer.Length; i++)
            ufoRenderer[i].material.color = Color.white;
    }

    private void BossDead()
    {
        Debug.Log("БОСС ПОВЕРЖЕН!");
        switch (Architecture.Instance.activeNumberLevel)
        {
            case 1:
                UIManager.Instance.Level_1_Vin();
                break;
            case 2:
                UIManager.Instance.Level_2_Vin();
                break;
            default:
                break;
        }
        // GameManager.Instance.Victory();
        Destroy(gameObject);
    }
}
