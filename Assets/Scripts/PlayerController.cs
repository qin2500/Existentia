using System;
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
    public float maxSlopeAngle = 20f;

    public Transform goundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;
    public bool grounded = true;
    public bool canJump = true;
    public float jumpCoolDown;
    public float jumpForce;
    public float dragForce;
    
    
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
        CheckGround();
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
        counterForce(x,y,magnitude);
        
        if (Input.GetButton("Jump")) Jump();
        float mutiplier = 1;

        if (!grounded) mutiplier = 0.5f;
        
        
        
        if (magnitude.magnitude < maxVelocity)
        {
            rb.AddForce(this.transform.forward*y*moveSpeed*Time.deltaTime * mutiplier);
            rb.AddForce(this.transform.right*x*moveSpeed*Time.deltaTime * mutiplier);
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

    public void Jump()
    {
        if (grounded && canJump)
        {
            canJump = false;
            rb.AddForce(Vector3.up*jumpForce);
            Invoke(nameof(resetJump),jumpCoolDown);
        }
    }

    private bool IsFloor(Vector3 v) {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool _cancellingGrounded;

    private void CheckGround()
    {
        grounded = Physics.CheckSphere(goundCheck.position, groundCheckRadius, groundLayer);
    }

    private void StopGrounded() {
        grounded = false;
    }

    private void resetJump()
    {
        canJump = true;
    }

    public void counterForce(float x, float y, Vector2 mag)
    {
        if (!grounded) return;
        
        var threshold = 0.01;
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0)) {
            rb.AddForce(moveSpeed * transform.right * Time.deltaTime * -mag.x * dragForce);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0)) {
            rb.AddForce(moveSpeed * transform.forward * Time.deltaTime * -mag.y * dragForce);
        }

//        if (x == 0 && y == 0)
//        {
//            rb.AddForce(rb.velocity * Time.deltaTime * dragForce * -1);
//        }
    }
    }
