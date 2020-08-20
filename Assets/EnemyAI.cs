using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{

    public Transform target;

    public float speed = 5;
    public float nextWaypointDistance = 3f;

    Path[] paths = new Path[3];
    int currentPath = 0;
    string[] graphNames = { "GraphXY", "GraphXZ", "GraphYZ" };
    int[] currentWaypoint = { 0, 0, 0 };
    bool[] reachedEndOfPath = { false, false, false };

    Seeker seeker;
    Rigidbody rb;
    bool generatingPaths = false;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody>();

        InvokeRepeating("RegenPaths", 0f, 1f);
        //RegenPaths();
        
        // seeker.tra

    }

    private void RegenPaths()
    {
        currentPath = 0;
        generatingPaths = true;
        seeker.graphMask = GraphMask.FromGraphName(graphNames[currentPath]);
        seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    public void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            paths[currentPath] = p;
            currentWaypoint[currentPath] = 0;
        } else
        {
            //Debug.ErrorLog(p.error);
            Debug.LogError(p.errorLog);
        }
        currentPath++;
        if (currentPath < paths.Length)
        {
            seeker.graphMask = GraphMask.FromGraphName(graphNames[currentPath]);
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
        else
            generatingPaths = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 finalDirection = Vector3.zero;

        if (generatingPaths)
            return;
        for (int i = 0; i < paths.Length; i++)
        {
            if (paths[i] == null)
                continue;
            if (reachedEndOfPath[i])
                continue;

            if (currentWaypoint[i] >= paths[i].vectorPath.Count)
            {
                reachedEndOfPath[i] = true;
                continue;
            } else
            {
                reachedEndOfPath[i] = false;
            }

            finalDirection += (paths[i].vectorPath[currentWaypoint[i]] - rb.position).normalized; ;

            // Check if we need to use the next waypoint
            float distance = Vector3.Distance(rb.position, paths[i].vectorPath[currentWaypoint[i]]);
            if (distance < nextWaypointDistance && currentWaypoint[i] < paths[i].vectorPath.Count)
            {
                currentWaypoint[i]++;
            }

        }
        
        Vector3 force = finalDirection.normalized * speed * Time.deltaTime;
        rb.AddForce(force);
    }
}
