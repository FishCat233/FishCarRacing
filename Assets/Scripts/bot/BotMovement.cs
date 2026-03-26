using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class BotMovement : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [SerializeField] private float reachDistance = 0.8f;
    [SerializeField] private float rotateSpeed = 12f;

    [Header("Move Tuning")]
    [SerializeField] private Vector2 speedRange = new Vector2(8.5f, 11f);
    [SerializeField] private float agentAcceleration = 30f;
    [SerializeField] private float agentAngularSpeed = 720f;
    [SerializeField] private float stoppingDistance = 0.2f;

    [Header("Finish Behavior")]
    [SerializeField] private bool stopOnFinish = true;
    [SerializeField] private float finishSlideTime = 0.35f;
    [SerializeField] private float finishSlideDamping = 10f;
    [SerializeField] private float minSlideSpeed = 0.5f;

    private int currentWaypointIndex;
    private NavMeshAgent agent;
    private RaceRuleSystem raceRuleSystem;
    private RacerProgress racerProgress;
    private bool hasFinished;
    private bool wasRaceActive;
    private Coroutine slideCoroutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        racerProgress = GetComponentInParent<RacerProgress>();
        raceRuleSystem = FindFirstObjectByType<RaceRuleSystem>();

        if (agent == null)
        {
            Debug.LogWarning("[BotMovement] Missing NavMeshAgent.", this);
            enabled = false;
            return;
        }

        float minSpeed = Mathf.Min(speedRange.x, speedRange.y);
        float maxSpeed = Mathf.Max(speedRange.x, speedRange.y);
        agent.speed = Random.Range(minSpeed, maxSpeed);
        agent.acceleration = agentAcceleration;
        agent.angularSpeed = agentAngularSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.updateRotation = false;
    }

    private void OnEnable()
    {
        if (raceRuleSystem != null)
        {
            raceRuleSystem.RacerFinished -= OnRacerFinished;
            raceRuleSystem.RacerFinished += OnRacerFinished;
        }
    }

    private void OnDisable()
    {
        if (raceRuleSystem != null)
        {
            raceRuleSystem.RacerFinished -= OnRacerFinished;
        }
    }

    private void Update()
    {
        if (hasFinished || agent == null)
        {
            return;
        }

        bool raceActive = raceRuleSystem != null && raceRuleSystem.RaceActive;
        if (!raceActive)
        {
            if (agent.isOnNavMesh)
            {
                if (!agent.isStopped)
                {
                    agent.isStopped = true;
                }

                if (wasRaceActive)
                {
                    agent.ResetPath();
                }
            }

            wasRaceActive = false;
            return;
        }

        if (agent.isOnNavMesh && agent.isStopped)
        {
            agent.isStopped = false;
        }

        wasRaceActive = true;

        if (waypoints == null || waypoints.Count == 0)
        {
            return;
        }

        if (!agent.isOnNavMesh)
        {
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        if (target == null)
        {
            AdvanceToNextWaypoint();
            return;
        }

        Vector3 lookDirection = agent.desiredVelocity.sqrMagnitude > 0.01f
            ? agent.desiredVelocity
            : target.position - transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(target.position, transform.position) <= reachDistance)
        {
            AdvanceToNextWaypoint();
            target = waypoints[currentWaypointIndex];
            if (target == null)
            {
                return;
            }
        }

        agent.SetDestination(target.position);
    }

    private void AdvanceToNextWaypoint()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            return;
        }

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
    }

    private void OnRacerFinished(RacerProgress finishedRacer)
    {
        if (!stopOnFinish || hasFinished || finishedRacer == null)
        {
            return;
        }

        if (racerProgress == null)
        {
            racerProgress = GetComponentInParent<RacerProgress>();
        }

        if (finishedRacer != racerProgress)
        {
            return;
        }

        hasFinished = true;

        float startSlideSpeed = minSlideSpeed;
        if (agent != null)
        {
            startSlideSpeed = Mathf.Max(minSlideSpeed, agent.velocity.magnitude);
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }

        slideCoroutine = StartCoroutine(SlideForward(startSlideSpeed));
    }

    private IEnumerator SlideForward(float startSpeed)
    {
        float elapsed = 0f;
        float speed = startSpeed;

        while (elapsed < finishSlideTime)
        {
            float dt = Time.deltaTime;
            transform.position += transform.forward * speed * dt;
            speed = Mathf.Lerp(speed, 0f, finishSlideDamping * dt);
            elapsed += dt;
            yield return null;
        }
    }
}
