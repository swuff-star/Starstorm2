using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhatTheFuck : MonoBehaviour
{
    private RectTransform rectTransform;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = Vector3.zero;
    }

    private void FixedUpdate()
    {
        rectTransform.localPosition = Vector3.zero;
    }
}
