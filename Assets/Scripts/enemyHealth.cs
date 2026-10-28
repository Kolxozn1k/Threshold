using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth;

    private Animator animator;
    [SerializeField] private Material enemyMaterial;
    [ColorUsage(true, true)]
    [SerializeField] private Color dashEmissionColor;
    private MaterialPropertyBlock propBlock;
    private SkinnedMeshRenderer meshRenderer;

    private static readonly int HitColorID = Shader.PropertyToID("_EmissionColor");
    private NavMeshAgent agent;
    private Wandering enemyMainScript;
    private Collider enemyCollider;
   
    public bool isDead { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();
        enemyMainScript = GetComponent<Wandering>();

        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        propBlock = new MaterialPropertyBlock();

        if (meshRenderer != null && meshRenderer.sharedMaterial != null)
        {
            meshRenderer.sharedMaterial.EnableKeyword("_EMISSION");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeDamage(float damage)
    {
        if (isDead == true) return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0f);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if(animator != null)
            {
               

                enemyMainScript.StunMonster();
            }
        }
    }


    private void Die()
    {
        isDead = true;

        if (enemyMainScript != null) enemyMainScript.enabled = false;
        if (agent != null) agent.enabled = false;
        if (enemyCollider != null) enemyCollider.enabled = false;

        if(animator != null)
        {
            animator.SetBool("isDead", true);
        }

        Debug.Log("Враг погиб");

        Destroy(gameObject, 5f);
    }

    private IEnumerator PlayHitAnimation( Color targetColor, float duration)
    {
        if (enemyMaterial == null)
            yield break;


        Color startColor = Color.black;

        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetTexture("_EmissionMap", null);


        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            // Передаем startColor в качестве первого аргумента
            Color currentColor = Color.Lerp(startColor, targetColor, time / duration);

            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", currentColor); // Используй DashColorID, если это инт-идентификатор свойства
            meshRenderer.SetPropertyBlock(propBlock);

            yield return null;
        }

        yield return new WaitForSeconds(0.3f); // Ждем немного перед возвратом к исходному цвету

        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            Color currentColor = Color.Lerp(targetColor, startColor, time / duration);

            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", currentColor); 
            meshRenderer.SetPropertyBlock(propBlock);

            yield return null;
        }
        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(HitColorID, targetColor);
        meshRenderer.SetPropertyBlock(propBlock);
    }

}


