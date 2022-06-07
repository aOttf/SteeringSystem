using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Evade : Persue
    {
        protected override Vector3 GetSteering() => -base.GetSteering();
    }
}