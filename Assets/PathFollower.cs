using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [Header("Path Reference")]
    public PathNode pathSource; // Reference to ANY path node in the system

    [Header("Movement Settings")]
    public float movementSpeed = 3f;
    public float rotationSpeed = 2f;
    public float arrivalDistance = 0.1f;
    //public bool startOnAwake = true;

    //[Header("Path Behavior")]
    //public bool loopPath = true;
    //public bool pingPong = false;
    //public bool reversePath = false;

    //[Header("Debug Settings")]
    //public bool showPathInGame = true;
    //public Color pathColor = Color.cyan;

    // private List<PathNode> pathNodes = new List<PathNode>();
    public int currentTargetIndex = 0;
    //private bool isMoving = false;
    //private bool movingForward = true;
    public float delayStart;
    public bool useRandomNode = false;
    public bool useOriginalHeight = false;

    void Start()
    {
        // Get the path from the referenced node

        //pathNodes = pathSource.GetPath();

        //if (reversePath)
        //{
        //    pathNodes.Reverse();
        //}

        //if (startOnAwake && pathNodes.Count > 0)
        //{
        //    StartFollowingPath();
        //}]

        currentTargetIndex = 0;



    }

    void Update()
    {
        delayStart -= Time.deltaTime;
        if (delayStart > 0)
            return;
        //if (isMoving && pathNodes.Count > 0 && currentTargetIndex < pathNodes.Count)
        //{
        MoveToTarget();
        //}
    }

    void MoveToTarget()
    {
        // PathNode targetNode = pathNodes[currentTargetIndex];
        //if (targetNode == null) return;

        Vector3 targetPosition = pathSource.GetNode(currentTargetIndex).transform.position;
        if (useOriginalHeight)
            targetPosition.y=transform.position.y;

        // Move towards target
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            movementSpeed * Time.deltaTime
        );

        // Rotate towards target (only if moving significantly)
        if ((targetPosition - transform.position).sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Check if arrived at target
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget <= arrivalDistance)
        {
            // StartCoroutine(ArriveAtNode(targetNode));
            if (!useRandomNode)
                currentTargetIndex = pathSource.GetNextNodeIndex(currentTargetIndex);
            else
                currentTargetIndex = pathSource.GetRandomNode(currentTargetIndex);
        }
    }



    //IEnumerator ArriveAtNode(PathNode node)
    //{
    //    // Wait at node if specified
    //    if (node.waitTime > 0)
    //    {
    //        yield return new WaitForSeconds(node.waitTime);
    //    }

    //    // Move to next node based on movement mode
    //    if (pingPong)
    //    {
    //        if (movingForward)
    //        {
    //            currentTargetIndex++;
    //            if (currentTargetIndex >= pathNodes.Count)
    //            {
    //                if (loopPath)
    //                {
    //                    currentTargetIndex = pathNodes.Count - 2;
    //                    movingForward = false;
    //                }
    //                else
    //                {
    //                    isMoving = false;
    //                }
    //            }
    //        }
    //        else
    //        {
    //            currentTargetIndex--;
    //            if (currentTargetIndex < 0)
    //            {
    //                if (loopPath)
    //                {
    //                    currentTargetIndex = 1;
    //                    movingForward = true;
    //                }
    //                else
    //                {
    //                    isMoving = false;
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        currentTargetIndex++;

    //        if (currentTargetIndex >= pathNodes.Count)
    //        {
    //            if (loopPath)
    //            {
    //                currentTargetIndex = 0;
    //            }
    //            else
    //            {
    //                isMoving = false;
    //                Debug.Log("Path completed!");
    //            }
    //        }
    //    }
    //}

    //[ContextMenu("Start Following Path")]
    //public void StartFollowingPath()
    //{
    //    if (pathNodes.Count > 0)
    //    {
    //        currentTargetIndex = 0;
    //        movingForward = true;
    //        isMoving = true;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("No path nodes available to follow!");
    //    }
    //}

    //[ContextMenu("Stop Following Path")]
    //public void StopFollowingPath()
    //{
    //    isMoving = false;
    //}

    //[ContextMenu("Pause Following Path")]
    //public void PauseFollowingPath()
    //{
    //    isMoving = false;
    //}

    //[ContextMenu("Resume Following Path")]
    //public void ResumeFollowingPath()
    //{
    //    if (pathNodes.Count > 0)
    //    {
    //        isMoving = true;
    //    }
    //}

    //[ContextMenu("Refresh Path")]
    //public void RefreshPath()
    //{
    //    if (pathSource != null)
    //    {
    //        pathNodes = pathSource.GetPath();

    //        if (reversePath)
    //        {
    //            pathNodes.Reverse();
    //        }

    //        Debug.Log($"Refreshed path with {pathNodes.Count} nodes");
    //    }
    //}

    //public void SetPathSource(PathNode newPathSource)
    //{
    //    pathSource = newPathSource;
    //    RefreshPath();
    //}

    //void OnDrawGizmos()
    //{
    //    if (!showPathInGame || pathNodes == null || pathNodes.Count < 2) return;

    //    // Draw path lines
    //    Gizmos.color = pathColor;
    //    for (int i = 0; i < pathNodes.Count - 1; i++)
    //    {
    //        if (pathNodes[i] != null && pathNodes[i + 1] != null)
    //        {
    //            Gizmos.DrawLine(pathNodes[i].transform.position, pathNodes[i + 1].transform.position);
    //        }
    //    }

    //    // Draw loop connection if enabled
    //    if (loopPath && !pingPong && pathNodes.Count > 1 &&
    //        pathNodes[0] != null && pathNodes[pathNodes.Count - 1] != null)
    //    {
    //        Gizmos.color = new Color(pathColor.r, pathColor.g, pathColor.b, 0.5f);
    //        Gizmos.DrawLine(pathNodes[pathNodes.Count - 1].transform.position, pathNodes[0].transform.position);
    //    }

    //    // Draw current target if moving
    //    if (isMoving && currentTargetIndex < pathNodes.Count && pathNodes[currentTargetIndex] != null)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawWireSphere(pathNodes[currentTargetIndex].transform.position, 0.7f);
    //    }
    //}

    // Public properties for external access
    //public bool IsMoving => isMoving;
    //public int CurrentNodeIndex => currentTargetIndex;
    //public int TotalNodes => pathNodes.Count;
    //public PathNode CurrentTargetNode => (currentTargetIndex < pathNodes.Count) ? pathNodes[currentTargetIndex] : null;
}