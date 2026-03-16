using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class KartDemoMovement : MonoBehaviour
{

    int CurrentWaypoint;
    public List<Transform> Waypoints;

    RaycastHit hit;

    NavMeshAgent agent;

    private void Awake()
    {
        agent = this.GetComponent<NavMeshAgent>();
        agent.speed = Random.Range(6f, 8f);
    }

    void Update()
    {

        Vector3 lookrotation = agent.steeringTarget - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookrotation), 10 * Time.deltaTime);

        if (Vector3.Distance(Waypoints[CurrentWaypoint].position, transform.position) < 1f)
        {
            CurrentWaypoint++;
        }

        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }

        if (CurrentWaypoint >= Waypoints.Count)
        {
            CurrentWaypoint = 0;
        }

        agent.SetDestination(Waypoints[CurrentWaypoint].position);

    }



}
