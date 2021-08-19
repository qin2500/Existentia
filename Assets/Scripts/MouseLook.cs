using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    //Camera rotation
    [Range(0f,200f)]
    public float sensitivity;
    public Transform playerTransform;
    private float _xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X")*sensitivity*Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y")*sensitivity*Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation=Mathf.Clamp(_xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_xRotation,0,0);
        playerTransform.Rotate(Vector3.up*mouseX);
        
    }
}
