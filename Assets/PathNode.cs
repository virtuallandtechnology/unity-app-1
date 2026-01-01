using System;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    [Header("Path Configuration")]
    public List<GameObject> allNodes = new List<GameObject>();
    public string nodeTag = "PathNode";

    [Header("Node Settings")]
    public float waitTime = 0f;
    public Color nodeColor = Color.yellow;
    public float nodeSize = 0.5f;
    [Header("Debug Settings")]
    public bool showPath = false;
    public bool showNodes = false;


    public Transform GetNode(int id)
    {
        return allNodes[id].transform;
    }


    void OnDrawGizmos()
    {
        //Draw the node
        //Gizmos.color = nodeColor;
        //Gizmos.DrawSphere(transform.position, nodeSize);

        //// Draw connection to next node
        //if (nextNode != null)
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawLine(transform.position, nextNode.transform.position);

        //    // Draw arrow direction
        //    DrawArrow(transform.position, nextNode.transform.position);
        //}

        //// Draw complete path if we have all nodes
        //if (allNodes != null && allNodes.Count > 1)
        //{
        //    Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange with transparency
        //    for (int i = 0; i < allNodes.Count - 1; i++)
        //    {
        //        if (allNodes[i] != null && allNodes[i + 1] != null)
        //        {
        //            Gizmos.DrawLine(allNodes[i].transform.position, allNodes[i + 1].transform.position);
        //        }
        //    }
        //}
        if (showPath)
        {
            Gizmos.color = nodeColor;
            for (int i = 0; i < allNodes.Count - 1; i++)
            {
                if (allNodes[i] != null && allNodes[i + 1] != null)
                {
                    Gizmos.DrawLine(allNodes[i].transform.position, allNodes[i + 1].transform.position);
                }
            }
            Gizmos.DrawLine(allNodes[allNodes.Count - 1].transform.position, allNodes[0].transform.position);
        }
        if (showNodes)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < allNodes.Count - 1; i++)
                Gizmos.DrawSphere(allNodes[i].transform.position, nodeSize);
        }
    }

    void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, nodeSize + 0.1f);

        //// Highlight complete path when selected
        //if (allNodes != null && allNodes.Count > 1)
        //{
        Gizmos.color = nodeColor;
        for (int i = 0; i < allNodes.Count - 1; i++)
        {
            if (allNodes[i] != null && allNodes[i + 1] != null)
            {
                Gizmos.DrawLine(allNodes[i].transform.position, allNodes[i + 1].transform.position);
            }
        }
        Gizmos.DrawLine(allNodes[allNodes.Count - 1].transform.position, allNodes[0].transform.position);
    }
    //}

    //private void DrawArrow(Vector3 from, Vector3 to)
    //{
    //    Vector3 direction = (to - from).normalized;
    //    float arrowSize = 0.3f;
    //    Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 45, 0) * Vector3.forward;
    //    Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 45, 0) * Vector3.forward;

    //    Gizmos.DrawRay(to - direction * arrowSize, right * arrowSize);
    //    Gizmos.DrawRay(to - direction * arrowSize, left * arrowSize);
    //}

    public int GetNextNodeIndex(int currentTargetIndex)
    {
        if (currentTargetIndex + 1 > allNodes.Count - 1)
            return 0;
        return currentTargetIndex + 1;

    }

    internal int GetRandomNode(int currentTargetIndex)
    {
        return UnityEngine.Random.Range(0,allNodes.Count - 1);
    }
}