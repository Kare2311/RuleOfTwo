using UnityEngine;

public class MirrorMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float strafeSpeed = 5f;
    public float jumpForce = 12f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;
    public float crouchHeight = 0.5f;
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchTransitionSpeed = 5f;

    [Header("Collision")]
    public float skinWidth = 0.1f;
    public LayerMask wallLayer;
    public Vector3 boxColliderSize = new Vector3(0.5f, 1.8f, 0.5f);
    public Vector3 crouchColliderSize = new Vector3(0.5f, 0.9f, 0.5f);

    [Header("Player References")]
    public Transform player1;
    public Transform player2;
    public BoxCollider player1Collider;
    public BoxCollider player2Collider;

    [Header("Animation")]
    public Animator animator1;
    public Animator animator2;
    private float _movementMagnitude;
    private float _crouchBlend;

    private Rigidbody rb1, rb2;
    private bool isGrounded1, isGrounded2;
    private bool isCrouching = false;
    private Vector3 originalColliderSize1, originalColliderSize2;
    private Vector3 originalColliderCenter1, originalColliderCenter2;

    void Start()
    {
        rb1 = player1.GetComponent<Rigidbody>();
        rb2 = player2.GetComponent<Rigidbody>();

        // Initialize collider references if not set
        if (player1Collider == null) player1Collider = player1.GetComponent<BoxCollider>();
        if (player2Collider == null) player2Collider = player2.GetComponent<BoxCollider>();

        // Store original collider values
        originalColliderSize1 = player1Collider.size;
        originalColliderSize2 = player2Collider.size;
        originalColliderCenter1 = player1Collider.center;
        originalColliderCenter2 = player2Collider.center;

        // Configure rigidbodies
        ConfigureRigidbody(rb1);
        ConfigureRigidbody(rb2);
    }

    void ConfigureRigidbody(Rigidbody rb)
    {
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        // Ground check
        isGrounded1 = Physics.Raycast(player1.position, Vector3.down, groundCheckDistance, groundLayer);
        isGrounded2 = Physics.Raycast(player2.position, Vector3.down, groundCheckDistance, groundLayer);

        // Crouch input
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
        }

        // Smooth crouch transition
        _crouchBlend = Mathf.MoveTowards(_crouchBlend, isCrouching ? 1 : 0, crouchTransitionSpeed * Time.deltaTime);

        // Movement
        HandleMovement();
        UpdateAnimatorParameters();
    }

    void HandleMovement()
    {
        float currentMoveSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        float currentStrafeSpeed = isCrouching ? strafeSpeed * crouchSpeedMultiplier : strafeSpeed;

        // Get input
        Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 moveDirection = player1.TransformDirection(moveInput).normalized;

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
        Vector3 velocity = moveDirection * currentMoveSpeed;
        velocity.y = rb1.linearVelocity.y; // Preserve gravity
        rb1.linearVelocity = velocity;

        // Mirror movement for Player 2 (inverted X)
        Vector3 mirroredVelocity = new Vector3(-velocity.x, velocity.y, velocity.z);
        rb2.linearVelocity = mirroredVelocity;

        // Update collider size for crouching
        UpdateColliderSize();

        // Jump if grounded and not crouching
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded1 && isGrounded2 && !isCrouching)
        {
            rb1.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            rb2.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        _movementMagnitude = Mathf.Clamp01(moveInput.magnitude);
    }

    void UpdateColliderSize()
    {
        // Smooth transition between standing and crouching collider sizes
        player1Collider.size = Vector3.Lerp(
            originalColliderSize1,
            crouchColliderSize,
            _crouchBlend);

        player2Collider.size = Vector3.Lerp(
            originalColliderSize2,
            crouchColliderSize,
            _crouchBlend);

        // Adjust center position when crouching
        player1Collider.center = Vector3.Lerp(
            originalColliderCenter1,
            new Vector3(0, crouchHeight / 2, 0),
            _crouchBlend);

        player2Collider.center = Vector3.Lerp(
            originalColliderCenter2,
            new Vector3(0, crouchHeight / 2, 0),
            _crouchBlend);
    }

    void UpdateAnimatorParameters()
    {
        animator1.SetFloat("CrouchBlend", _crouchBlend);
        animator2.SetFloat("CrouchBlend", _crouchBlend);
        animator1.SetFloat("Speed", _movementMagnitude);
        animator2.SetFloat("Speed", _movementMagnitude);
        animator1.SetBool("IsGrounded", isGrounded1);
        animator2.SetBool("IsGrounded", isGrounded2);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (player1) Gizmos.DrawLine(player1.position, player1.position + Vector3.down * groundCheckDistance);
        if (player2) Gizmos.DrawLine(player2.position, player2.position + Vector3.down * groundCheckDistance);

        // Draw box collider outlines
        if (player1Collider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = player1.localToWorldMatrix;
            Gizmos.DrawWireCube(player1Collider.center, player1Collider.size);
        }
    }
}