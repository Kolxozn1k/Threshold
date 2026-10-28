using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class Player_main : MonoBehaviour
{
    public static Player_main Instance { get; private set; }

    private SwordDamage SwordDamageScript;


    [SerializeField] float currentspeed;
    public Transform cameraTransform;
    public CharacterController controller;


    [SerializeField] private CinemachineCamera cincam;
    private Vector2 moveInput; // 

    public bool squat = false;
    private float originalHeight = 2f;
    private Vector3 originalModelScale;

    [SerializeField] private Material playerMaterial;
    private Vector3 originalCameraPos;
    private Vector3 moveDirection;

    [Header("DashSettings")]
    private bool isDashing = false;

    [SerializeField] private float DashTime = 0.2f;
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private float dashCooldown = 0.8f;

    [SerializeField] private float attackRange = 1.5f;

    [SerializeField] private float attackRadius = 2f;

    [SerializeField] private float attackDamage = 35f;

    [SerializeField] private LayerMask enemyLayer;

    private float nextDashTime = 0f;
    public bool isInVisible = false;
    private bool isAttacking = false;

    public float iFrameDuration = 0.2f;

    private Animator animator;

    [Header("Dash VFX Settings")]
    [SerializeField] private TrailRenderer[] dashTrails; // Перетащишь сюда компонент из инспектора
    [SerializeField] private ParticleSystem dashParticles; // Опционально: партиклы пыли

    [ColorUsage(true, true)] // Этот атрибут включает HDR-палитру прямо в инспекторе Unity!
    [SerializeField] private Color dashEmissionColor;

    private static readonly int DashColorID = Shader.PropertyToID("_EmissionColor");
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    void Awake()
    {
        // Инициализируем синглтон
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalHeight = controller.height;

        originalCameraPos = cameraTransform.localPosition;

        animator = GetComponentInChildren<Animator>();

        dashTrails = GetComponentsInChildren<TrailRenderer>();

        SwordDamageScript = GetComponentInChildren<SwordDamage>();

    }


    public void OnMove(InputValue val) // 
    {
        moveInput = val.Get<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {

        moveDirection = (GetForward() * moveInput.y) + (GetRight() * moveInput.x); // вычисляем направление движения игрока относительно камеры

        if (isDashing == false)
        {
            controller.Move(moveDirection * currentspeed * Time.deltaTime);
        }
        else
            return;

        HandleRotation(moveDirection);

        CheckSpace();

        if (!controller.isGrounded)
        {
            controller.Move(Vector3.down * 9.81f * Time.deltaTime);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            animator.SetTrigger("attack");
        }

        bool isMoving = moveInput.magnitude > 0.1f;

        animator.SetBool("isMoving", isMoving);


    }

    private Vector3 GetForward() // метод для получения направления вперед относительно камеры
    {
        Vector3 forward = cincam.transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    private Vector3 GetRight() // метод для получения направления вправо относительно камеры
    {
        Vector3 right = cincam.transform.right;
        right.y = 0f;
        return right.normalized;
    }

    void HandleRotation(Vector3 moveDirection) // метод для поворота игрока в направлении курсора мыши
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetDirection = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetDirection, Time.deltaTime * 15f);
        }
    }

    private void CheckSpace()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.time >= nextDashTime)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;

        animator.SetTrigger("dash");

        StartCoroutine(AnimateDashMaterial(dashEmissionColor, 0.1f));

        foreach (TrailRenderer trail in dashTrails)
        {
            trail.emitting = true;
        }

        nextDashTime = Time.time + dashCooldown;

        Vector3 dashDirection = moveDirection != Vector3.zero ? moveDirection : transform.forward;

        StartCoroutine(IsInVisible());

        float currentTime = 0f;

        while (currentTime < DashTime)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null; // Ждем до следующего кадра
        }
        currentTime = 0f;

        StartCoroutine(AnimateDashMaterial(Color.black, 0.3f));

        foreach (TrailRenderer trail in dashTrails)
        {
            trail.emitting = false;
        }
        isDashing = false;
    }

    IEnumerator AnimateDashMaterial(Color targetColor, float duration)
    {
        if (playerMaterial == null)
            yield break;

        Color startColor = playerMaterial.GetColor(DashColorID);

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            Color currentColor = Color.Lerp(startColor, targetColor, time / duration);
            playerMaterial.SetColor(DashColorID, currentColor);
            yield return null;
        }
        playerMaterial.SetColor(DashColorID, targetColor);
    }

    IEnumerator IsInVisible()
    {
        isInVisible = true;

        yield return new WaitForSeconds(iFrameDuration);

        isInVisible = false;
    }


    public void PlayHitFeedback(Vector3 AttackerPosition)
    {
        Vector3 pushDirection = (transform.position - AttackerPosition).normalized;

        pushDirection.y = 0f;

        controller.Move(pushDirection * 0.5f);

    }



    public void EnableWeaponDamage()
    {
        if (SwordDamageScript != null)
            SwordDamageScript.EnableDamage();
    }

    public void DisableWeaponDamage()
    {
        if (SwordDamageScript != null)
            SwordDamageScript.DisableDamage();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackCenter = transform.position + transform.forward * attackRange;
        Gizmos.DrawWireSphere(attackCenter, attackRadius);
    }


}