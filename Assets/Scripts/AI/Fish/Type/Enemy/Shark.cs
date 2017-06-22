﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shark : FishEnemy
{
    //[SerializeField]
    private int _timeForRandomDir = 200;
    public int TimeForRandomDir { get { return _timeForRandomDir; } }

    [Header("Shark Variables")]
    [SerializeField]
    private int _knockBackStrength = 60;
    public int KnockBackStrength { get { return _knockBackStrength; } }

    [SerializeField]
    private Transform _path = null;

    [SerializeField]
    private int _detectionAngle = 45;
    public int DetectionAngle { get { return _detectionAngle; } }

    [SerializeField]
    [Tooltip("The range of how far the shark can see")]
    private int _viewRange = 30;
    public int ViewRange { get { return _viewRange; } }

    [SerializeField]
    private int _pointRange = 4;
    public int PointRange { get { return _pointRange; } }

    private Transform[] _waypoints;
    public Transform[] WayPoints { get { return _waypoints; } }

    private int _wayId = 0;
    public int WayId { get { return _wayId; } set { _wayId = value; } }

    public Shark SyncTarget = null;

    private SharkTail _tail;

    [HideInInspector]
    public int SyncStep = 0;

    private bool _isChasing = false;
    public bool IsChasing { get { return _isChasing; } }

    public override void Start()
    {
        base.Start();

        _tail = GetComponent<SharkTail>();

        //Remove parent from array
        List<Transform> temp = _path.GetComponentsInChildren<Transform>().ToList();
        temp.RemoveAt(0);
        _waypoints = temp.ToArray();

        _wayId = GetWayPointId(GetNearestWayPointTo(transform));

        if (SyncTarget!= null)
            SyncStep = Mathf.Abs(GetWayPointId(GetNearestWayPointTo(transform)) - GetWayPointId(GetNearestWayPointTo(SyncTarget.transform)));

        stateCache[typeof(SharkIdle)] = new SharkIdle(this);
        stateCache[typeof(SharkWayPoint)] = new SharkWayPoint(this);
        stateCache[typeof(SharkChase)] = new SharkChase(this);
        stateCache[typeof(SharkReturn)] = new SharkReturn(this);

        SetState<SharkWayPoint>();
    }

    public void OnCollisionEnter(Collision c)
    {
        if (c.transform != Target)
            return;

        //Add force to the player's body in which direction the shark is going
        c.rigidbody.AddForce(Direction * KnockBackStrength, ForceMode.Impulse);
        SubMovement sub = Target.GetComponent<SubMovement>();
        //Set stun time
        sub.StunSlowCooldown = 60;
        //Stun the player
        sub.StunPlayer();
        //Drain oxygen from the player
        OxygenVals.Remove(OxygenDrain);

        SetState<SharkIdle>();
        _tail.SetState<TailIdle>();
    }

    public Transform GetNearestWayPointTo(Transform target)
    {
        int id = -1;
        float lowestRange = 9999f;

        //Go through each waypoint
        for (int i = 0; i < _waypoints.Length; i++)
        {
            //Get the distance between the shark and the waypoint
            float testRange = Vector3.Distance(target.position, _waypoints[i].position);
            //Check if it's more near
            if (testRange < lowestRange)
            {
                //Assign new nearest waypoint
                id = i;
                lowestRange = testRange;
            }
        }

        //Return nearest waypoint
        return _waypoints[id];
    }

    public int GetWayPointId(Transform waypoint)
    {
        for (int i = 0; i < _waypoints.Length; i++)
        {
            if (_waypoints[i] == waypoint)
                //Waypoint found
                return i;
        }

        //Waypoint not found
        return -1;
    }

    public override bool DetectTarget()
    {
        Transform nearTarget = GetNearestWayPointTo(Target);
        float targetDis = Vector3.Distance(transform.position, Target.position);
        float targetPointDis = Vector3.Distance(nearTarget.position, Target.position);

        /* Check if:
         * Target is in angle in front of the shark
         * Target is not obstructed by walls
         * Target is near a waypoint
         * Shark is in range of the Target */
        _isChasing = (Vector3.Angle(transform.forward, Target.position - transform.position) < _detectionAngle &&
            !Physics.Linecast(transform.position, Target.position, ~IgnoreDetection) &&
            targetPointDis < Range && targetDis < ViewRange);
        return _isChasing;
    }

    public override Quaternion GetLookRotation(Vector3 direction)
    {
        //Proper rotation for the model for rotating towards points
        Quaternion lookRot = base.GetLookRotation(direction);
        lookRot.eulerAngles -= new Vector3(0, 0, 0);
        return lookRot;
    }

    public Transform GetCurrentWayPoint()
    {
        return _waypoints[_wayId];
    }
}