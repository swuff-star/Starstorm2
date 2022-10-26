using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhatTheFuckContinued : MonoBehaviour
{
    private RectTransform rectTransform;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(1.2f, 1f, 0);
    }
 
    private void FixedUpdate()
    {
        rectTransform.localScale = new Vector3(1.2f, 1f, 0);
    }
}
