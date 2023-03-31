using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCMove : MonoBehaviour
{


    public Transform[] randomPoints;


    private NavMeshAgent agent;

    private Transform currTarget;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        currTarget = RandomTarget();
    }
    private void Update()
    {
        if(!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currTarget = RandomTarget();
        }
        
        agent.SetDestination(currTarget.position);
    }

    private Transform RandomTarget()
    {
        var t = randomPoints[Random.Range(0, randomPoints.Length)];

        if (t != currTarget)
            return t;
        return RandomTarget();
    }
}
