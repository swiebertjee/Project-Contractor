﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octopus : FishEnemy
{
    [Header("Octopus Variables")]
    [SerializeField]
    [Tooltip("Used to decrease idle intervals between bursts when chasing the player")]
    private float _idleIntervalChange = 2f;
    public float IdleIntervalChange { get { return _idleIntervalChange; } }

    [SerializeField]
    private float _latchOnRange = 10f;
    public float LatchOnRange { get { return _latchOnRange; } }

    [SerializeField]
    private float _latchOnOffset = 5f;
    public float LatchOnOffset { get { return _latchOnOffset; } }

    [SerializeField]
    [Tooltip("The time the Octopus stays on a rock before finding a new one")]
    private int _restTime = 500;
    public int RestTime { get { return _restTime; } }

    private Collider _collider;
    public Collider Collider { get { return _collider; } }

    //Resting pos
    private Vector3 _rockPos;
    public Vector3 RockPos { get { return _rockPos; } set { _rockPos = value; } }

    private Vector3 _rockNormal;
    public Vector3 RockNormal { get { return _rockNormal; } set { _rockNormal = value; } }

    private Vector3 _targetLatchPos;
    public Vector3 TargetLatchPos { get { return _targetLatchPos; } }

    private Vector3 _targetNormal;
    public Vector3 TargetNormal { get { return _targetNormal; } set { _targetNormal = value; } }

    private bool _isChasing = false;
    public bool IsChasing { get { return _isChasing; } }

    public override void Start()
    {
        base.Start();

        _collider = GetComponent<Collider>();
        _rockPos = origPos;

        stateCache[typeof(FishStateFindRock)] = new FishStateFindRock(this);
        stateCache[typeof(FishStateBurstIdle)] = new FishStateBurstIdle(this);
        stateCache[typeof(FishStateBurstMove)] = new FishStateBurstMove(this);
        stateCache[typeof(FishStateBurstChase)] = new FishStateBurstChase(this);
        stateCache[typeof(FishStateLatchOn)] = new FishStateLatchOn(this);
        stateCache[typeof(FishStateLatchOff)] = new FishStateLatchOff(this);

        SetState<FishStateFindRock>();
    }

    public override void Update()
    {
        base.Update();
    }

    public override bool DetectTarget()
    {
        bool detected = base.DetectTarget();

        if (detected)
            _isChasing = true;
        else
            _isChasing = false;

        return detected;
    }

    public override Quaternion GetLookRotation(Vector3 direction)
    {
        Quaternion lookRot = base.GetLookRotation(direction);
        lookRot.eulerAngles -= new Vector3(180f, 0f, 180f);
        return lookRot;
    }

    public Quaternion GetLatchOnRot(Vector3 direction)
    {
        Quaternion lookRot = Quaternion.LookRotation(direction);
        lookRot.eulerAngles -= new Vector3(90f, 0f, 180f);
        return lookRot;
    }

    public bool CheckLatchOnRange()
    {
        return (Vector3.Distance(transform.position, IsChasing ? Target.position : _rockPos) < _latchOnRange);
    }
}