using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IHateMath : MonoBehaviour
{
    private float amplitude = 0.8f;
    public float frequency = 2.4f;

    Vector3 currentPos = new Vector3();
    Vector3 tempPos = new Vector3();

    public void Start()
    {
        currentPos = transform.position;
    }
    public void Update()
    {
        tempPos = currentPos;
        tempPos.y += Mathf.Sin(Time.fixedDeltaTime * Mathf.PI * frequency) * amplitude;

        transform.position = tempPos;
    }
}
