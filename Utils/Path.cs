using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public enum PathType
    { Linear, Circular }

    public class WayPoint
    {
        public Vector3 position;
        public WayPoint prev;
        public WayPoint next;

        public WayPoint(Vector3 value) => position = value;
    }

    public WayPoint First { get; set; }
    public WayPoint Last { get; set; }
    public WayPoint Current { get; private set; }

    public PathType type;

    private bool isBackward = false;    //Used by Linear Path

    public Path(params Vector3[] waypoints)
    {
        //Require at least two waypoints
        if (waypoints.Length < 2)
        {
            Debug.LogError("At Least two points");
            return;
        }

        //Init First
        First = new WayPoint(waypoints[0]);
        Current = First;

        WayPoint cur = First;
        for (int i = 1; i < waypoints.Length; i++)
        {
            cur.next = new WayPoint(waypoints[i]);
            cur.next.prev = cur;
            cur = cur.next;
        }

        //Init Last
        Last = cur;

        if (type == PathType.Linear)
        {
            Last.next = null;
            First.prev = null;
        }
        else
        {
            Last.next = First;
            First.prev = Last;
        }
    }

    public void Move2Next()
    {
        switch (type)
        {
            case PathType.Circular:
                Current = Current.next;
                break;

            case PathType.Linear:
                //at the first waypoint
                if (Current == First)
                {
                    Current = Current.next;
                    //Reset backward flag
                    isBackward = false;
                }
                //At the last waypoint
                else if (Current == Last)
                {
                    Current = Current.prev;
                    //Set backward flag
                    isBackward = true;
                }
                //In the middle of path
                else
                {
                    Current = (isBackward) ? Current.prev : Current.next;
                }
                break;
        }
    }

    public float RemainingDistance(Vector3 pos) => Vector3.Distance(Current.position, pos);
}