using UnityEngine;
using UnityEngine.InputSystem;


public class MirrorMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float strafeSpeed = 5f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    [Range(0.1f, 0.5f)] public float groundCheckDelay = 0.2f; // New: Prevents jump buffering

    [Header("Ground Detection")]
    public float groundCheckRadius = 0.25f; // Radius of the sphere cast
    public float groundCheckOffset = 0.05f; // Small offset from bottom of character
    public float groundCheckDistance = 0.3f;

    [Header("Collision")]
    public float skinWidth = 0.1f;
    public LayerMask wallLayer;
    public Vector3 boxColliderSize = new Vector3(0.5f, 1.8f, 0.5f);

    [Header("Player References")]
    public Transform player1;
    public Transform player2;
    public BoxCollider player1Collider;
    public BoxCollider player2Collider;

    [Header("Animation")]
    public Animator animator1;
    public Animator animator2;
    private float _movementMagnitude;

    private Rigidbody rb1, rb2;
    private bool isGrounded1, isGrounded2;

    private PlayerInputActions inputActions;
    private Vector2 moveInput = Vector2.zero;
    private bool jumpPressed = false;
    private float lastGroundedTime1, lastGroundedTime2; // Track when we were last grounded

    void Awake()
    {
        inputActions = new PlayerInputActions();
        Debug.Log("Input actions initialized");
    }

    void OnEnable()
    {
        inputActions.Gameplay.Enable();
        inputActions.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Gameplay.Jump.performed += ctx => jumpPressed = true;
        Debug.Log("Input actions enabled");
    }

    void OnDisable()
    {
        inputActions.Gameplay.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Gameplay.Move.canceled -= ctx => moveInput = Vector2.zero;
        inputActions.Gameplay.Jump.performed -= ctx => jumpPressed = true;
        inputActions.Gameplay.Disable();
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        Debug.Log($"Move input: {moveInput}");
    }

    void OnMoveCancel(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        jumpPressed = true;
        Debug.Log("Jump pressed!");
    }

    void Start()
    {
        rb1 = player1.GetComponent<Rigidbody>();
        rb2 = player2.GetComponent<Rigidbody>();

        if (player1Collider == null) player1Collider = player1.GetComponent<BoxCollider>();
        if (player2Collider == null) player2Collider = player2.GetComponent<BoxCollider>();

        ConfigureRigidbody(rb1);
        ConfigureRigidbody(rb2);

        Debug.Log($"Rigidbody1 gravity: {rb1.useGravity}, constraints: {rb1.constraints}");
        Debug.Log($"Rigidbody2 gravity: {rb2.useGravity}, constraints: {rb2.constraints}");
    }

    void ConfigureRigidbody(Rigidbody rb)
    {
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        // Improved ground check with coyote time
        Vector3 feetPos1 = player1.position + Vector3.down * (player1Collider.size.y / 2 - groundCheckOffset);
        if (Physics.CheckSphere(feetPos1, groundCheckRadius, groundLayer))
        {
            isGrounded1 = true;
            lastGroundedTime1 = Time.time;
        }
        else
        {
            isGrounded1 = false;
        }

        // Improved ground check with coyote time
        Vector3 feetPos2 = player2.position + Vector3.down * (player1Collider.size.y / 2 - groundCheckOffset);
        if (Physics.CheckSphere(feetPos1, groundCheckRadius, groundLayer))
        {
            isGrounded2 = true;
            lastGroundedTime2 = Time.time;
        }
        else
        {
            isGrounded2 = false;
        }

        Debug.DrawRay(feetPos1, Vector3.down * groundCheckRadius, isGrounded1 ? Color.green : Color.red);
        Debug.DrawRay(feetPos2, Vector3.down * groundCheckRadius, isGrounded2 ? Color.green : Color.red);

        UpdateAnimatorParameters();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleGravity();

        if (jumpPressed)
        {
            Debug.Log($"Attempting jump. Grounded1: {isGrounded1}, Grounded2: {isGrounded2}");
        }

        // Reset jump input after physics update
        jumpPressed = false;
    }

    void HandleMovement()
    {
        // Get input
        Vector3 inputVector = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 moveDirection = player1.TransformDirection(inputVector).normalized;

        // Wall collision check for Player 1
        if (Physics.BoxCast(
            player1.position + player1Collider.center,
            player1Collider.size * 0.5f - Vector3.one * skinWidth,
            moveDirection,
            out RaycastHit hit1,
            player1.rotation,
            skinWidth * 2f,
            wallLayer))
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection, hit1.normal).normalized;
        }

        // Apply movement to Player 1
        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = rb1.linearVelocity.y;
        rb1.linearVelocity = velocity;

        // Mirror movement for Player 2 (inverted X)
        Vector3 mirroredVelocity = new Vector3(-velocity.x, velocity.y, velocity.z);
        rb2.linearVelocity = mirroredVelocity;

        // Jump if grounded
        if (jumpPressed && (isGrounded1 || isGrounded2)) // Can jump if either player is grounded
        {
            // Only jump with the grounded player(s)
            if (isGrounded1)
            {
                rb1.linearVelocity = new Vector3(rb1.linearVelocity.x, 0, rb1.linearVelocity.z); // Reset vertical velocity
                rb1.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded1 = false;
            }

            if (isGrounded2)
            {
                rb2.linearVelocity = new Vector3(rb2.linearVelocity.x, 0, rb2.linearVelocity.z);
                rb2.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded2 = false;
            }
        }

        // Enhanced jump with coyote time and input buffering
        bool canJump1 = Time.time - lastGroundedTime1 <= groundCheckDelay;
        bool canJump2 = Time.time - lastGroundedTime2 <= groundCheckDelay;

        if (jumpPressed && (canJump1 || canJump2))
        {
            if (canJump1)
            {
                rb1.linearVelocity = new Vector3(rb1.linearVelocity.x, 0, rb1.linearVelocity.z);
                rb1.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                lastGroundedTime1 = 0; // Prevent double jumps
            }

            if (canJump2)
            {
                rb2.linearVelocity = new Vector3(rb2.linearVelocity.x, 0, rb2.linearVelocity.z);
                rb2.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                lastGroundedTime2 = 0;
            }
        }

        _movementMagnitude = Mathf.Clamp01(new Vector3(moveInput.x, 0, moveInput.y).magnitude);
    }

    void HandleGravity()
    {
        // Apply stronger gravity when falling
        if (rb1.linearVelocity.y < 0)
        {
            rb1.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Apply lighter gravity when ascending but not holding jump
        else if (rb1.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb1.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        if (rb2.linearVelocity.y < 0)
        {
            rb2.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Apply lighter gravity when ascending but not holding jump
        else if (rb2.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb2.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void UpdateAnimatorParameters()
    {
        animator1.SetFloat("Speed", _movementMagnitude);
        animator2.SetFloat("Speed", _movementMagnitude);
        // Optionally update grounded bools if your animators use them
        // animator1.SetBool("IsGrounded", isGrounded1);
        // animator2.SetBool("IsGrounded", isGrounded2);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw spheres at ground check positions
        Gizmos.color = isGrounded1 ? Color.green : Color.red;
        Vector3 spherePos1 = player1.position + Vector3.down * (player1Collider.size.y / 2 - groundCheckOffset);
        Gizmos.DrawWireSphere(spherePos1, groundCheckRadius);

        Gizmos.color = isGrounded2 ? Color.green : Color.red;
        Vector3 spherePos2 = player2.position + Vector3.down * (player2Collider.size.y / 2 - groundCheckOffset);
        Gizmos.DrawWireSphere(spherePos2, groundCheckRadius);
    }

    
}
