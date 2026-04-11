using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    // ── Tunables ─────────────────────────────────────────────────────────────
    [Header("Jump")]
    public float jumpForce          = 14f;
    public float jumpCutMultiplier  = 0.4f;   // applied to upward vel on early release
    public float fallGravityScale   = 3f;
    public float baseGravityScale   = 1.5f;
    public float coyoteTime         = 0.1f;
    public int   maxJumps           = 1;      // perks can increase to 2

    [Header("Ground Check")]
    public Transform groundCheck;
    public float     groundCheckRadius = 0.15f;
    public LayerMask groundLayers;

    // ── Internal state ────────────────────────────────────────────────────────
    private Rigidbody2D  _rb;
    private PlayerStats  _stats;
    private PlayerCombat _combat;
    private Animator     _animator;

    private Vector2 _moveInput;
    private bool    _jumpPressed;
    private bool    _jumpHeld;

    private bool  _isGrounded;
    private int   _groundContactCount;   // collision-based ground detection
    private float _coyoteTimer;
    private int   _jumpsLeft;

    private bool  _isDashing;
    private bool  _dashOnCooldown;
    private float _dashCooldownTimer;
    public  bool  DashIFrameActive { get; private set; }

    [Header("Dash")]
    public float dashSpeed     = 20f;
    public float dashDuration  = 0.15f;
    public float iFrameDuration = 0.15f;

    // Aim direction (world-space), used by Ranger class
    public Vector2 AimDirection { get; private set; } = Vector2.right;
    private Camera _mainCamera;
    private bool   _isKeyboardMouse;

    // Facing direction (+1 right, -1 left)
    public int FacingDir { get; private set; } = 1;

    // Drop-through flag
    private bool _dropThrough;

    private void Awake()
    {
        _rb       = GetComponent<Rigidbody2D>();
        _stats    = GetComponent<PlayerStats>();
        _combat   = GetComponent<PlayerCombat>();
        _animator = GetComponent<Animator>();
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        // OnControlsChanged only fires on scheme transitions, not at startup —
        // read the current scheme directly so keyboard+mouse aim works from the first frame.
        var pi = GetComponent<PlayerInput>();
        if (pi != null)
            _isKeyboardMouse = pi.currentControlScheme == "KeyboardMouse";
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Wave) return;
        if (_stats.IsDead) return;

        UpdateGrounded();
        UpdateCoyote();
        HandleJump();
        HandleDashCooldown();
        UpdateAim();
        UpdateFacing();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Wave) return;
        if (_stats.IsDead || _isDashing) return;

        float speed = _stats.moveSpeed;
        _rb.linearVelocity = new Vector2(_moveInput.x * speed, _rb.linearVelocity.y);

        // Variable gravity
        if (_rb.linearVelocity.y < 0)
            _rb.gravityScale = fallGravityScale;
        else if (_rb.linearVelocity.y > 0 && !_jumpHeld)
            _rb.gravityScale = fallGravityScale;
        else
            _rb.gravityScale = baseGravityScale;
    }

    // ── Ground check ─────────────────────────────────────────────────────────

    private void UpdateGrounded()
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = _groundContactCount > 0;
        if (_isGrounded && !wasGrounded)
        {
            Debug.Log($"[Jump] Landed! jumpsLeft reset to {maxJumps}");
            OnLanded();
        }
        else if (!_isGrounded && wasGrounded)
        {
            Debug.Log("[Jump] Left ground.");
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayers.value) == 0) return;
        foreach (var c in col.contacts)
            if (c.normal.y > 0.5f) { _groundContactCount++; break; }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayers.value) == 0) return;
        _groundContactCount = Mathf.Max(0, _groundContactCount - 1);
    }

    private void OnLanded()
    {
        _jumpsLeft       = maxJumps;
        _dashOnCooldown  = false;
        _dashCooldownTimer = 0f;
        _rb.gravityScale = baseGravityScale;
    }

    private void UpdateCoyote()
    {
        if (_isGrounded)
            _coyoteTimer = coyoteTime;
        else
            _coyoteTimer -= Time.deltaTime;
    }

    // ── Jump ─────────────────────────────────────────────────────────────────

    private void HandleJump()
    {
        if (_jumpPressed)
        {
            _jumpPressed = false;

            bool canJump = _isGrounded || _coyoteTimer > 0f || _jumpsLeft > 0;
            if (canJump)
            {
                if (!_isGrounded && _coyoteTimer <= 0f) _jumpsLeft--;
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _coyoteTimer = 0f;
            }
        }
    }

    private IEnumerator DropThroughPlatform()
    {
        // Temporarily switch to the DropThrough layer so PlatformEffector2D ignores us
        int originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("DropThrough");
        yield return new WaitForSeconds(0.3f);
        gameObject.layer = originalLayer;
    }

    // ── Dash ─────────────────────────────────────────────────────────────────

    private void HandleDashCooldown()
    {
        if (_dashOnCooldown)
        {
            _dashCooldownTimer -= Time.deltaTime;
            if (_dashCooldownTimer <= 0f)
                _dashOnCooldown = false;
        }
    }

    private IEnumerator DoDash(Vector2 direction)
    {
        _isDashing       = true;
        DashIFrameActive = true;
        _dashOnCooldown  = true;
        _dashCooldownTimer = _stats.dashCooldown;

        CombatEventSystem.RaisePlayerDash(this);

        Vector2 dashVel = direction.normalized * dashSpeed;
        _rb.gravityScale = 0f;
        _rb.linearVelocity      = dashVel;

        yield return new WaitForSeconds(dashDuration);

        _isDashing       = false;
        _rb.gravityScale = baseGravityScale;

        yield return new WaitForSeconds(iFrameDuration - dashDuration);
        DashIFrameActive = false;
    }

    // ── Aim ──────────────────────────────────────────────────────────────────

    private void UpdateAim()
    {
        if (_isKeyboardMouse && _mainCamera != null)
        {
            // Convert mouse screen pos to world direction from player
            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorld.z = 0f;
            Vector2 dir = (mouseWorld - transform.position);
            if (dir.sqrMagnitude > 0.01f)
                AimDirection = dir.normalized;
        }
        // Gamepad aim is set via OnAim callback from PlayerInput
    }

    private void UpdateFacing()
    {
        if (_moveInput.x > 0.1f)       FacingDir =  1;
        else if (_moveInput.x < -0.1f) FacingDir = -1;
        // Flip sprite — TODO: replace with animator when art is added
        transform.localScale = new Vector3(FacingDir, 1f, 1f);
    }

    // ── Input callbacks (wired by PlayerInput component) ─────────────────────

    public void OnMove(InputValue value)          => _moveInput = value.Get<Vector2>();
    public void OnJump(InputValue value)          { if (value.isPressed) _jumpPressed = true; _jumpHeld = value.isPressed; }
    public void OnDropThrough(InputValue value)
    {
        _dropThrough = value.isPressed;
        // Trigger drop-through immediately on press — no Jump needed
        if (value.isPressed && _isGrounded)
            StartCoroutine(DropThroughPlatform());
    }
    public void OnAim(InputValue value)
    {
        Vector2 v = value.Get<Vector2>();
        if (!_isKeyboardMouse && v.sqrMagnitude > 0.1f)
            AimDirection = v.normalized;
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed || _dashOnCooldown || _isDashing) return;
        Vector2 dir = _moveInput.sqrMagnitude > 0.1f ? _moveInput : new Vector2(FacingDir, 0f);
        StartCoroutine(DoDash(dir));
    }

    public void OnControlsChanged(PlayerInput pi)
    {
        _isKeyboardMouse = pi.currentControlScheme == "KeyboardMouse";
    }

    // ── Animator ─────────────────────────────────────────────────────────────

    private void UpdateAnimator()
    {
        if (_animator == null) return;
        _animator.SetFloat("Speed",         Mathf.Abs(_moveInput.x));
        _animator.SetFloat("VerticalSpeed", _rb.linearVelocity.y);
        _animator.SetBool ("IsGrounded",    _isGrounded);
    }

    public void TriggerAttackAnim() => _animator?.SetTrigger("AttackTrigger");
    public void TriggerHurtAnim()   => _animator?.SetTrigger("HurtTrigger");
    public void TriggerDieAnim()    => _animator?.SetTrigger("DieTrigger");

    // ── Public helpers ────────────────────────────────────────────────────────

    public bool IsGrounded    => _isGrounded;
    public bool IsDashing     => _isDashing;
    public Vector2 MoveInput  => _moveInput;
}
