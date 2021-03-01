using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnstuckMouse : MonoBehaviour
{
    void Awake()
    {
        Cursor.visible = true;

    }

    // Update is called once per frame
    void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
