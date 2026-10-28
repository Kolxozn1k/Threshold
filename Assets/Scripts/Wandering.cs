using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Wandering : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    

    [Header("Patrol Settings")]
    private Transform[] wayPoints;
    [SerializeField] private float agentWalkSpeed;
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 4f;
    private int currentWay = -1;

    [Header("Vision (FOV) Settings")]
    [SerializeField] private float viewRadiusWalk = 10f;
    private float viewRadius = 10f;
    [Range(0, 360)]
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float monsterEyesOffset = 1.5f;
    [SerializeField] private float playerCenterOffset = 0.8f;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 35f;
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float nextTimeAttack;
    [SerializeField] private float coolDown;

    [Header("Timers & States")]
    [SerializeField] private float LoosingTime;
    private float timer;
    private float timer2;
    private float currentTime;
    private float idleTimer;
    private bool isWaiting;
    private bool isChasing;
    private bool isAttacking;
    private bool isStuned;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        GameObject[] points = GameObject.FindGameObjectsWithTag("WayPoint");
        wayPoints = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            wayPoints[i] = points[i].transform;
        }

        SetNewWayPointDestination();
        agent.speed = agentWalkSpeed;

        if (Player_main.Instance != null)
        {
            player = Player_main.Instance.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isStuned)
            return;

        if(animator != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("speed", currentSpeed);
        }

        if (player == null)
        {
            if (Player_main.Instance != null) player = Player_main.Instance.transform;
            else return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isAttacking && distanceToPlayer <= attackDistance)
        {
            if(IsAgentReady())
                agent.isStopped = true;
            LookAtPlayer();
            return;
        }

        if (distanceToPlayer <= attackDistance)
        {
            if(IsAgentReady())
                agent.isStopped = true;
            LookAtPlayer();
            if (Time.time >= nextTimeAttack)
            {
                StartCoroutine(AttackPlayer());
            }

            return;
        }


        if (IsAgentReady())
            agent.isStopped = false;

        float agentSpeed = agent.velocity.magnitude;
        animator.SetFloat("speed", agentSpeed);


        if (CanSeePlayer())
        {
            if (!isChasing)
            {
                timer2 = 0f;
                isChasing = true;
                isWaiting = false;
            }
            agent.speed = agentWalkSpeed * 1.2f;
            if(IsAgentReady())
            {
                agent.SetDestination(player.position);
            }
        }
        else
        {
            if (isChasing)
            {
                agent.speed = agentWalkSpeed * 1.2f;
                if(IsAgentReady())
                {
                    agent.SetDestination(player.position);
                }
                timer2 += Time.deltaTime;
                if (timer2 >= LoosingTime)
                {
                    agent.speed = agentWalkSpeed;
                    isChasing = false;
                    agent.ResetPath();
                    isWaiting = true;
                    timer = 0f;
                    currentTime = Random.Range(minWaitTime, maxWaitTime);
                }
            }
            else
            {
                agent.speed = agentWalkSpeed;
                PatrolLogic();
            }
        }
    }

    private bool CanSeePlayer()// метод для проверки видимости игрока монстром
    {
        if (player == null) return false;

        Vector3 monsterEyes = transform.position + Vector3.up * monsterEyesOffset;
        Vector3 playerCenter = player.position + Vector3.up * playerCenterOffset;

        float DistanceToPlayer = Vector3.Distance(monsterEyes, playerCenter);

        if (DistanceToPlayer < viewRadius)
        {
            Vector3 directionToPlayer = (playerCenter - monsterEyes).normalized;
            if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2f)
            {
                if (!Physics.Raycast(monsterEyes, directionToPlayer, DistanceToPlayer, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;

    }

    private void PatrolLogic()// метод для логики патрулирования монстра
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                timer = 0f;
                currentTime = Random.Range(minWaitTime, maxWaitTime);
            }

            if (isWaiting)
            {
                timer += Time.deltaTime;
                if (timer >= currentTime)
                {
                    isWaiting = false;
                    SetNewWayPointDestination();
                }
            }
        }
    }

    private void SetNewWayPointDestination()// метод для установки новой точки назначения для патрулирования
    {
        if (wayPoints.Length == 1)
        {
            currentWay = 0;
            agent.SetDestination(wayPoints[0].position);
            return;
        }

        int NextWay;

        do
        {
            NextWay = Random.Range(0, wayPoints.Length);
        }
        while (NextWay == currentWay);

        currentWay = NextWay;

        agent.SetDestination(wayPoints[NextWay].position);
    }

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetDirection = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetDirection, Time.deltaTime * 5f);
        }

    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;

        // Останавливаем монстра
        if (IsAgentReady())
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        

        // Ставим cooldown
        nextTimeAttack = Time.time + coolDown;

        // Смотрим на игрока
        LookAtPlayer();

        if(animator != null)
        {
            animator.SetTrigger("attack");
        }

        // Задержка перед ударом
        yield return new WaitForSeconds(1f);

        Player_main playerStun = GetComponent<Player_main>();

        if (Player_main.Instance != null)
        {
            Player_main.Instance.PlayHitFeedback(transform.position);
        }

        if (player == null)
        {
            isAttacking = false;
            agent.isStopped = false;
            yield break;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackDistance)
        {
            if (Player_main.Instance != null && !Player_main.Instance.isInVisible)
            {
                

                PlayerHealth health = player.GetComponent<PlayerHealth>();

                if(health != null && !health.isDead)
                {
                    health.TakeDamage(attackDamage);
                }

            }
            else
            {
                
            }
        }

        // Заканчиваем атаку
        isAttacking = false;

        // Разрешаем движение снова
        if(IsAgentReady())
            agent.isStopped = false;

 
    }

    public void StunMonster()
    {
        if (!isStuned)
            StartCoroutine(GetStun());
    }

    IEnumerator GetStun()
    {
        
        isStuned = true;

        if (IsAgentReady())
            agent.isStopped = true;

        animator.SetTrigger("takeDamage");

        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        float animationLength = stateInfo.length;

        float animationLengthBySpeed = animationLength / stateInfo.speed;

        yield return new WaitForSeconds(animationLengthBySpeed); // когда будет анимация, сделаю через ее длительность
        isStuned = false;

        if (IsAgentReady())
            agent.isStopped = false;

    }


    private bool IsAgentReady()
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }

    


    private void OnDrawGizmosSelected()// метод для визуализации поля зрения монстра в редакторе
    {
        Vector3 monsterEyes = transform.position + Vector3.up * monsterEyesOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(monsterEyes, viewRadius);

        Vector3 fovLineRight = Quaternion.AngleAxis(viewAngle / 2, Vector3.up) * transform.forward;
        Vector3 fovLineLeft = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(monsterEyes, fovLineRight * viewRadius);
        Gizmos.DrawRay(monsterEyes, fovLineLeft * viewRadius);
    }
}