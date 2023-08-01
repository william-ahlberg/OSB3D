using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 100f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float gravityScale = 1f;
    [SerializeField] float groundDrag = 5f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float jumpCooldown = 0.25f;
    [SerializeField] float airMultiplier = 0.01f;
    [SerializeField] float sensitivityX = 100f;
    [SerializeField] float sensitivityY = 100f;

    private float agentHeight = 1.8f;

    bool readyToJump;
    bool onGround;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] bool flyMode = false;
    [SerializeField] bool manualInput = true;

    [SerializeField] Transform forwardFace;
    [SerializeField] Transform head;
    [SerializeField] Transform body;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    PlayerController playerController;
    Rigidbody rb;

    float[] actions;

    private float rotationX;
    private float rotationY;


    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerController = GetComponent<PlayerController>();
        readyToJump = true;

    }

    private void Update()
    {
        // ground check
        onGround = CheckGround();
        if (manualInput)
        {
            actions = playerController.PlayerInput();
        }
        else
        {
            Debug.Log("Agent actionbuffer not implemented.");
        }

        SpeedControl();
        // handle drag

        if (onGround)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

    }

    public void MoveAgent(float[] actions)
    {
        // calculate movement direction
        moveDirection = forwardFace.forward * actions[0] + forwardFace.right * actions[1];

        // Rotate
        Rotate(actions);

        // on ground
        if (onGround)
        {
            //Debug.Log("On the ground");
            rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Force);
        }
        // in air
        else if (!onGround)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Force);
        }

        if (rb.velocity.y <= 0)
        {
            rb.AddForce(Vector3.down * gravityScale, ForceMode.Force);
        }

        if (actions[4] > 0.5f && readyToJump && onGround)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);

        }
        CheckEdge();

    }

    private bool CheckEdge()
    {
        //Debug.Log(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer));
        return true;
    }

    private bool CheckGround()
    {
        return Physics.Raycast(transform.position, Vector3.down, agentHeight, groundLayer);
    }

    private void FixedUpdate()
    {
        actions = playerController.PlayerInput();
        MoveAgent(actions);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
    private void Rotate(float[] actions)
    {
        float mouseX = actions[2] * Time.deltaTime * sensitivityX;
        float mouseY = actions[3] * Time.deltaTime * sensitivityY;
        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        rotationY = rotationY % 360f;

        head.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        forwardFace.rotation = Quaternion.Euler(0, rotationY, 0);
        body.rotation = forwardFace.rotation;
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