using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Camera Rotation
    [Range(0f, 300f)] public float mouseSensitivity;
    public Transform cameraTransform;
    private float _xRotation = 0f;
    
    //Movement
    private Rigidbody rb;
    public float extraGravity = 10f;
    public float maxVelocity;
    public float moveSpeed = 400;

    public LayerMask groundLayer;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
           CameraLook();
    }

    private void FixedUpdate()
    {
        Move();
        
    }

    private void CameraLook()
    {
        float mouseX = Input.GetAxis("Mouse X")*mouseSensitivity*Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y")*mouseSensitivity*Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(_xRotation,0f,0f);
        this.transform.Rotate(Vector3.up * mouseX);
    }

    private void Move()
    {
        rb.AddForce(Vector3.down * Time.deltaTime * extraGravity);
        
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 magnitude = FindRelativeVelocity();

        if (magnitude.magnitude < maxVelocity)
        {
            rb.AddForce(this.transform.forward*y*moveSpeed*Time.deltaTime);
            rb.AddForce(this.transform.right*x*moveSpeed*Time.deltaTime);
        }
        
    }

    public Vector2 FindRelativeVelocity()
    {
        float lookAngle = transform.eulerAngles.y;
        float moveDirection = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;
        float u = Mathf.DeltaAngle(lookAngle, moveDirection);
        float v = 90 - u;

        float magnitude = rb.velocity.magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
        
    }
    
    public 
}
