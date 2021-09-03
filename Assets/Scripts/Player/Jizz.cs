using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jizz : MonoBehaviour {
    public Transform playerCam;
    private Rigidbody rb;

    public LayerMask whatIsGround;
    private float xRotation;
    public float sensitivity = 175f;
    private float sensMultiplier = 1f;
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    private bool grounded, cancellingGrounded;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;
    private float x, y;
    private bool jumping;

    //Wallrunning variables
    public LayerMask whatIsWall;
    public float wallrunForce, maxWallrunTime, maxWallrunSpeed;
    public float maxWallrunCamTilt, wallrunCamTilt;
    public bool wallRight, onWall;
    public bool wallrunning, forwardPressed;
    
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

        WallrunInput();
    }

    private void MyInput() {                                                                       //Keyboard input
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");

        if (Input.GetKeyDown(KeyCode.W)) forwardPressed = true;
        if (Input.GetKeyUp(KeyCode.W)) forwardPressed = false;
    }

    private void Movement() {

        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        Vector3 forwardVelocity = Vector3.Project(rb.velocity, rb.transform.forward);

        if (readyToJump && jumping) Jump();

        if (wallrunning && rb.velocity.magnitude < maxWallrunSpeed) {                              //Wallrun movement
            rb.AddForce(this.transform.forward * wallrunForce * Time.deltaTime);

            if (wallRight) rb.AddForce(this.transform.right * wallrunForce * Time.deltaTime);
            else rb.AddForce(-this.transform.right * wallrunForce * Time.deltaTime);

            return;
        }

        rb.AddForce(Vector3.down * Time.deltaTime * 10);                                            //Gravity force

        CounterMovement(x, y, mag);

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
        
        if(wallrunning) x = 0;

        rb.AddForce(this.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(this.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void CounterMovement(float x, float y, Vector2 mag) {
        if (!grounded || jumping || wallrunning) return;

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

    private void Jump() {

        if (grounded && readyToJump) {                                                      //Normal jump

            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);
                
            Vector3 vel = rb.velocity;

            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);

            else if (rb.velocity.y > 0) 
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
        }

        else if (wallrunning && readyToJump) {                                              //Wallrun jump
            wallrunning = false;

            if (rb.velocity.magnitude < maxWallrunSpeed) rb.AddForce(this.transform.forward * jumpForce/5 * 7);

            if (wallRight) rb.AddForce(-this.transform.right * jumpForce/5 * 7);
            else rb.AddForce(this.transform.right * jumpForce/5 * 7);

            rb.AddForce(this.transform.up * jumpForce/2 * 3);
        }

        readyToJump = false;
        Invoke(nameof(ResetJump), jumpCooldown);
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

        playerCam.localRotation = Quaternion.Euler(_xRotation, 0f, wallrunCamTilt);
        this.transform.Rotate(Vector3.up * mouseX);
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
    
    private void OnCollisionStay(Collision other) {
        int layer = other.gameObject.layer;

        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.contacts[i].normal;
            //Floor
            if (IsFloor(normal)) {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                wallrunCamTilt = 0;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        float delay = 3f;

        if (!cancellingGrounded) {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void OnCollisionEnter(Collision other) {
        int layer = other.gameObject.layer;

        //Enter collision with runnable wall
        if (whatIsWall == (whatIsWall | (1 << layer))) {
            onWall = true;
            CheckWall(whatIsWall);
        }

        else if (whatIsWall != (whatIsWall | (1 << layer))) onWall = false;
    }

    private void OnCollisionExit(Collision other) {
        int layer = other.gameObject.layer;

        //Exit collision with runnable wall
        if (whatIsWall == (whatIsWall | (1 << layer))) 
            onWall = false;
    }

    private void StopGrounded() {
        grounded = false;
    }

    //Wallrun functions
    public void WallrunInput() {
        if (forwardPressed && onWall && !grounded) StartWallrun();
        else StopWallrun();

        //Tilt camera during wall runs
        if (Math.Abs(wallrunCamTilt) < maxWallrunCamTilt && wallrunning && wallRight)
            wallrunCamTilt += Time.deltaTime * maxWallrunCamTilt * 2;
        else if (Math.Abs(wallrunCamTilt) < maxWallrunCamTilt && wallrunning && !wallRight)
            wallrunCamTilt -= Time.deltaTime * maxWallrunCamTilt * 2;

        if (wallrunCamTilt > 0 && !wallrunning && wallRight)
            wallrunCamTilt -= Time.deltaTime * maxWallrunCamTilt * 2;
        else if (wallrunCamTilt < 0 && !wallrunning && !wallRight)
            wallrunCamTilt += Time.deltaTime * maxWallrunCamTilt * 2;

        if (Math.Abs(wallrunCamTilt) < 1 && !wallrunning) wallrunCamTilt = 0;
    }

    public void StartWallrun() {
        if(wallrunning) return;

        rb.useGravity = false;
        wallrunning = true;
        readyToJump = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void StopWallrun() {
        rb.useGravity = true;
        wallrunning = false;
    }

    public void CheckWall(LayerMask layer) {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out hit, Mathf.Infinity, layer))
            wallRight = true;
        else if (Physics.Raycast(transform.position, transform.TransformDirection(-Vector3.right), out hit, Mathf.Infinity, layer))
            wallRight = false;    
    }
}
