using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public Transform player;
    public float detectionRange = 10f;
    public float chargeCooldown = 5f;
    public float chargeForce = 30f;
    public float stunDuration = 3f;
    public float chargeChance = 0.2f;
    public float killDistance = 1.5f; 

    private NavMeshAgent agent;
    private int currentPatrolIndex;
    private bool chasing = false;
    private bool charging = false;
    private bool stunned = false;
    private float chargeTimer = 1f;
    private Rigidbody rb;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        currentPatrolIndex = 0;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (stunned) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        chargeTimer += Time.deltaTime;

        if (charging)
        {
            if (distanceToPlayer < killDistance)
            {
                KillPlayer();
            }
            return;
        }

        if (distanceToPlayer < detectionRange)
        {
            chasing = true;

            if (distanceToPlayer < killDistance)
            {
                KillPlayer();
                return;
            }

            if (chargeTimer >= chargeCooldown && Random.value < chargeChance)
            {
                StartCoroutine(Charge());
                return;
            }

            agent.SetDestination(player.position);
        }
        else
        {
            chasing = false;
            Patrol();
        }
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    System.Collections.IEnumerator Charge()
    {
        charging = true;
        chargeTimer = 0f;
        agent.enabled = false;

        Vector3 direction = (player.position - transform.position).normalized;
        rb.AddForce(direction * chargeForce, ForceMode.VelocityChange);

        yield return new WaitForSeconds(1.5f);

        if (!stunned)
        {
            rb.linearVelocity = Vector3.zero;
            charging = false;
            agent.enabled = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (charging)
        {
            if (collision.transform.CompareTag("Player"))
            {
                KillPlayer();
            }
            else
            {
                StartCoroutine(Stun());
            }
        }
    }

    System.Collections.IEnumerator Stun()
    {
        stunned = true;
        charging = false;
        rb.linearVelocity = Vector3.zero;
        agent.enabled = false;

        // TODO: dodaj animację ogłuszenia

        yield return new WaitForSeconds(stunDuration);

        stunned = false;
        agent.enabled = true;
    }

    void KillPlayer()
    {
        // TODO: Dodaj animację zabicia
        Debug.Log("Gracz zginął!");

        player.gameObject.SetActive(false);
    }
}
