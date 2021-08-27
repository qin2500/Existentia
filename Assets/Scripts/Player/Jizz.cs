using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jizz : MonoBehaviour
{
    public Transform playerCam;
    public Transform orientation;
    
    private Rigidbody rb;

    private float xRotation;
    public float sensitivity = 175f;
    private float sensMultiplier = 1f;
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public bool grounded;
    public LayerMask whatIsGround;
    
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;
    float x, y;
    private bool jumping;

    //Wallrunning variables
    public LayerMask whatIsWall;
    public float wallrunForce, maxWallrunTime, maxWallrunSpeed;
    public bool wallRight, wallLeft, onWall;
    public bool wallrunning, forwardPressed;
    public float maxWallrunCamTilt, wallrunCamTilt;
    
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }
    
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    
    private void FixedUpdate() {
        Movement();
    }

    private void Update() {
        MyInput();
        Look();

        //CheckForWall();
        WallrunInput();
    }

    private void MyInput() {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
    }


    private void Movement() {

        rb.AddForce(Vector3.down * Time.deltaTime * 10);
        
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        CounterMovement(x, y, mag);
        
        if (readyToJump && jumping) Jump();

        float maxSpeed = this.maxSpeed;
        
        
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        float multiplier = 1f, multiplierV = 1f;
        
        if (!grounded) {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }
        
        rb.AddForce(this.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(this.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump() {
        if (grounded && readyToJump) {
            readyToJump = false;

            if(!wallrunning)
            {
                rb.AddForce(Vector2.up * jumpForce * 1.5f);
                rb.AddForce(normalVector * jumpForce * 0.5f);
                
                Vector3 vel = rb.velocity;
                if (rb.velocity.y < 0.5f)
                    rb.velocity = new Vector3(vel.x, 0, vel.z);
                else if (rb.velocity.y > 0) 
                    rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            }

            else 
            {
                if(wallRight) rb.AddForce(-orientation.right * jumpForce);
                else rb.AddForce(orientation.right * jumpForce);
            }

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    private void ResetJump() {
        readyToJump = true;
    }

    private float _xRotation;
    private void Look() {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        playerCam.localRotation = Quaternion.Euler(_xRotation,0f,0f);
        this.transform.Rotate(Vector3.up * mouseX);
    }

    private void CounterMovement(float x, float y, Vector2 mag) {
        if (!grounded || jumping) return;

        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0)) {
            rb.AddForce(moveSpeed * this.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0)) {
            rb.AddForce(moveSpeed * this.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }
        
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed) {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }
    public Vector2 FindVelRelativeToLook() {
        float lookAngle = this.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v) {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;
    
    private void OnCollisionStay(Collision other) {
        int layer = other.gameObject.layer;

        if(whatIsWall == (whatIsWall | (1 << layer))) onWall = true;
        else if(whatIsWall != (whatIsWall | (1 << layer))) onWall = false;

        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal)) {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        float delay = 3f;
        if (!cancellingGrounded) {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded() {
        grounded = false;
    }

    //Wallrun functions
    public void WallrunInput()
    {
        if(Input.GetKeyDown(KeyCode.W)) forwardPressed = true;
        if(Input.GetKeyUp(KeyCode.W)) forwardPressed = false;

        if(forwardPressed && onWall) StartWallrun();
        else StopWallrun();
    }

    public void StartWallrun()
    {
        rb.useGravity = false;
        wallrunning = true;
    }

    public void StopWallrun()
    {
        rb.useGravity = true;
        wallrunning = false;

        if(rb.velocity.magnitude <= maxWallrunSpeed)
        {
            rb.AddForce(orientation.forward * wallrunForce * Time.deltaTime);

            if(wallRight)
                rb.AddForce(orientation.right * wallrunForce/5 * Time.deltaTime);

            else if (wallLeft)
                rb.AddForce(-orientation.right * wallrunForce * Time.deltaTime);   
        }
    }

    public void CheckForWall()
    {
        if(!onWall) StopWallrun();

        else 
        {
            wallRight = Physics.Raycast(transform.position, orientation.right, 1f, whatIsWall);
            wallLeft = Physics.Raycast(transform.position, -orientation.right, 1f, whatIsWall);
        }
    }
}
