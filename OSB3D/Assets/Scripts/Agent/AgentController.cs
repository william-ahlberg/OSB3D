using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float maxSpeed;

    [SerializeField] float gravityScale = 1f;
    [SerializeField] float fallGravityMultiplier = 2f;


    [SerializeField] float groundDrag;

    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMultiplier;
    bool readyToJump;

    public LayerMask groundLayer;
    bool onGround;

    [SerializeField] Transform forwardFace;

    float horizontalInput;
    float verticalInput;


    bool flyMode = false;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
    }

    private void Update()
    {
        // ground check
        onGround = Physics.Raycast(transform.position, Vector3.down, 1f + 0.3f, groundLayer);

        PlayerInput();
        SpeedControl();
        // handle drag
        if (onGround)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        //Debug.Log("Ready to jump:" + readyToJump);                 
        //Debug.Log("On the ground:" + onGround);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (!flyMode)
        {
            if (Input.GetKey(KeyCode.Space) && readyToJump && onGround)
            {
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }
        else if (Input.GetKey(KeyCode.Space))            
        {
            Fly(1);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            Fly(-1);
        }
           
        if (Input.GetKeyDown("t")) 
        {

            switch (flyMode)
            {
                case true:
                    flyMode = false;
                    rb.useGravity = true;
                    Debug.Log("Fly mode off");
                    break;
                case false:
                    Debug.Log("Fly mode on");
                    flyMode = true;
                    rb.useGravity = false;
                    ResetVelocity();
                    break;
                default:
				    break;
            }
            

        }
    }

    public void MovePlayer()
    {
        // calculate movement direction
        moveDirection = forwardFace.forward * verticalInput + forwardFace.right * horizontalInput;

        // on ground
        if(onGround)
            rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Force);

        // in air
        else if(!onGround)
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Force);

        if (rb.velocity.y < 0)
        {
             rb.AddForce(Vector3.down * gravityScale * fallGravityMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void ResetVelocity()
    {
        rb.velocity = Vector3.zero;
    }

    private void Fly(int direction)

    {
        transform.position += direction * Vector3.up * 10f * Time.deltaTime;
    }

}