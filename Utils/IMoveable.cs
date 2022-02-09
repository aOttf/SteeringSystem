using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable
{
    public Vector3 position { get; }
    public Vector3 linearVelocity { get; }
    public Vector3 linearAcceleration { get; }
    public float angularVelocity { get; }
    public float angularAcceleration { get; }
}