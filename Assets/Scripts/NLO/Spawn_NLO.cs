using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_NLO : MonoBehaviour
{
    public Transform target;
    public GameObject nloPrefabs;

    public GameObject nlo;

    public void Spawn_nlo(Transform target)
    {
        float targetY = target.transform.position.y + 10f;
        Vector3 desiredPosition = new Vector3(target.transform.position.x, targetY, target.transform.position.z);

        nlo = Instantiate(nloPrefabs, desiredPosition, Quaternion.identity, gameObject.transform);
        nlo.GetComponent<NLO_Move>().target = target;
    }

}
