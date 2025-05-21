using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 15f;
    public float dashForce = 40f;
    public float dashDuration = 1.5f;
    public float stunDuration = 3f;
    public float killDistance = 1.5f;
    public float wanderRadius = 20f;
    public float restDuration = 5f;
    public float chaseTimeLimit = 30f;
    public float seekAfterSeconds = 2f;

    public Transform player;

    private NavMeshAgent agent;
    private Rigidbody rb;

    private enum State { Wandering, Seeking, Chasing, Dashing, Stunned, Resting }
    private State currentState = State.Wandering;

    private float chaseTimer;
    private float timeSinceLastSeen;
    private Vector3 dashTarget;
    private float dashTimer;
    public Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        EnterState(State.Wandering);
    }

    void Update()
    {
        timeSinceLastSeen += Time.deltaTime;

        switch (currentState)
        {
            case State.Wandering:   UpdateWander();   break;
            case State.Seeking:     UpdateSeek();     break;
            case State.Chasing:     UpdateChase();    break;
            case State.Dashing:     UpdateDash();     break;
        }

        if (currentState == State.Wandering && timeSinceLastSeen >= seekAfterSeconds)
        {
            EnterState(State.Seeking);
        }
    }

    // --- STATE MACHINE HELPERS ---

    void EnterState(State newState)
    {
        currentState = newState;
        Debug.Log($"State → {newState}");
        agent.isStopped = true;
        rb.linearVelocity = Vector3.zero;

        switch (newState)
        {
            case State.Wandering:
                agent.isStopped = false;
                GoToRandomPoint();
                Debug.Log("Wander");
                break;

            case State.Seeking:
                if (timeSinceLastSeen < Mathf.Infinity)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                    Debug.Log("Seek");
                }
                break;

            case State.Chasing:
                chaseTimer = 0f;
                Invoke(nameof(StartDash), 2f);
                agent.isStopped = false;
                agent.SetDestination(player.position);
                Debug.Log("Chase");
                break;

            case State.Dashing:
                dashTimer = 0f;
                dashTarget = player.position;
                Vector3 dashDirection = (dashTarget - transform.position).normalized;
                rb.linearVelocity = dashDirection * dashForce;
                agent.isStopped = true;
                Debug.Log("Dash");
                break;

            case State.Resting:
                StartCoroutine(RestCoroutine());
                break;

            case State.Stunned:
                StartCoroutine(StunCoroutine());
                break;
        }
    }

    void TransitionToWanderIfLost()
    {
        if (!PlayerInSight() && chaseTimer >= chaseTimeLimit)
            EnterState(State.Resting);
    }

    // --- STATE UPDATES ---

    void UpdateWander()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToRandomPoint();

            Debug.Log("Wander");

        if (PlayerInSight())
            EnterState(State.Chasing);
    }

    void UpdateSeek()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            EnterState(State.Wandering);

            Debug.Log("Seek");

        if (PlayerInSight())
            EnterState(State.Chasing);
    }

    void UpdateChase()
    {
        chaseTimer += Time.deltaTime;
        agent.SetDestination(player.position);
                  Debug.Log("Chase");
        animator.SetBool("IsAttacking", true);

        if (!PlayerInSight())
            TransitionToWanderIfLost();
    }

    void UpdateDash()
    {
        dashTimer += Time.deltaTime;
                  Debug.Log("Dash");

        if (dashTimer >= dashDuration)
        {
            rb.linearVelocity = Vector3.zero;
            EnterState(State.Chasing);
        }
    }

    void StartDash()
    {
        if (currentState == State.Chasing)
        {
            animator.SetBool("IsAttacking", false);
            EnterState(State.Dashing);
        }
    }

    // --- COROUTINES ---

    IEnumerator RestCoroutine()
    {
        yield return new WaitForSeconds(restDuration);
        timeSinceLastSeen = Mathf.Infinity;
        EnterState(PlayerInSight() ? State.Chasing : State.Wandering);
    }

    IEnumerator StunCoroutine()
    {
        yield return new WaitForSeconds(stunDuration);
        EnterState(State.Chasing);
    }

    // --- NAVIGATION & SENSES ---

    void GoToRandomPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * wanderRadius + transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    bool PlayerInSight()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        var origin = transform.position + Vector3.up * 1.5f;
        var target = player.position + Vector3.up * 1.0f;

        if (Physics.Linecast(origin, target, out RaycastHit hit))
            return hit.transform == player;

        return false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Collided with Player → Killing");
            SceneManager.LoadScene("MenuScene");
            player.gameObject.SetActive(false);
        }
        else if (currentState == State.Dashing)
        {
            rb.linearVelocity = Vector3.zero;
            EnterState(State.Chasing);
        }
    }
}
