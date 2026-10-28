using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth;

    private bool isDefend;
    private Animator animator;
    public bool isDead { get; private set; } 

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isMoving = animator.GetBool("isMoving");

        if (Mouse.current.rightButton.isPressed && !isMoving) 
        {
            isDefend = true;
            animator.SetBool("defend", true);
        }
        else
        {
            isDefend = false;
            animator.SetBool("defend", false);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDefend) return;

        if (isDead == true) return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0f);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("TakeDamage");
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");

        if(TryGetComponent<Player_main>(out var mainScript)) mainScript.enabled = false;
        if(TryGetComponent<CharacterController>(out var controller)) controller.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


}
