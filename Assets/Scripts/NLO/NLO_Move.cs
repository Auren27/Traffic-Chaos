using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NLO_Move : MonoBehaviour
{
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject tower;
    [SerializeField] private float rotationSpeed = 90f; // градусов в секунду

    private void Update()
    {
        body.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
