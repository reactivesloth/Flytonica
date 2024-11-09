using System;
using UnityEngine;

public class CopyTransform : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Transform myTransform;

    private void Awake()
    {
        myTransform = transform;
    }

    private void Update()
    {
        if (target != null && myTransform != null)
        {
            myTransform.position = target.position;
            myTransform.rotation = target.rotation;
        }
    }
}
