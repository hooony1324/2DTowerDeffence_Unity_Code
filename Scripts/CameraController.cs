using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 0.001f;

    [SerializeField]
    private float CameraSize = 10;

    [SerializeField]
    private float zoomMin = 6;

    [SerializeField]
    private float zoomMax = 15;

    // Update is called once per frame
    void LateUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // 줌이 클 수록 moveSpeed느리게
        transform.position += new Vector3(x, y, 0) * (moveSpeed * Camera.main.orthographicSize);

        Vector2 scroll = Input.mouseScrollDelta;
        if (scroll.magnitude > 0)
        {
            Camera.main.orthographicSize = Math.Clamp(Camera.main.orthographicSize - scroll.y, zoomMin, zoomMax); 
        }
    }
}
