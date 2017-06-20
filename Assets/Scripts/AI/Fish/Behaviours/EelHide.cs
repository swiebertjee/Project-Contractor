﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EelHide : FishState
{
    public EelHide(Fish pFish) : base(pFish) { }

    private Eel _eel;

    public override void Initialize()
    {
        _eel = (Eel)fish;
        _eel.Body.velocity = Vector3.zero;
        _eel.Body.angularVelocity = Vector3.zero;
        _eel.Direction = (_eel.HoleExit.position - _eel.Hole.position).normalized;
        _eel.Collider.enabled = true;
    }

    public override void Step()
    {
        if (_eel.DetectTarget())
            _eel.SetState<EelCharge>();

        //Move anchor and head back to start positions
        _eel.Anchor.position = Vector3.Lerp(_eel.Anchor.position, _eel.AnchorOrigPos, _eel.RotationSpeed);
        _eel.transform.position = Vector3.Lerp(_eel.transform.position, _eel.OriginPos, _eel.RotationSpeed);
        _eel.transform.rotation = Quaternion.Lerp(_eel.transform.rotation, _eel.OriginRot, _eel.RotationSpeed);
    }
}