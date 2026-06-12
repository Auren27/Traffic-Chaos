using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_NLO : MonoBehaviour
{
    public GameObject nloPrefabs;
    [SerializeField] public GameObject spawn_point;

    public GameObject nlo;

    public void Spawn_nlo(Transform target)
    {
        nlo = Instantiate(nloPrefabs, spawn_point.transform.position, Quaternion.identity, gameObject.transform);
        nlo.GetComponent<NLO_Move>().target = target;
    }

}
