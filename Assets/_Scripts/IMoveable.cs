using UnityEngine;
using UnityEngine.AI;

namespace SteeringSystem
{
    /// <summary>
    /// Represents A Moveable Entity
    /// </summary>
    public interface IMoveable
    {
        public Vector3 position { get; }
        public Vector3 forward { get; }
        public Vector3 up { get; }
        public Vector3 linearVelocity { get; }

        //public Vector3 linearAcceleration { get; }
        //public float angularAcceleration { get; }
        public float angularVelocity { get; }

        public float MaxLinearSpeed { get; }
        public float MaxAngularSpeed { get; }
        public float MaxLinearAcceleration { get; }
        public float MaxAngularAcceleration { get; }

        public SteeringOutput groupAvoidanceOutput { get; set; }
        public SteeringOutput groupFlockOutput { get; set; }

        public SteeringOutput this[GroupBehaviour behaviour]
        {
            get;
            set;
        }
    }

    public interface ISphereMoveable : IMoveable
    {
        public float radius { get; }
    }

    /// <summary>
    /// Represents A Moveable Entity with A Capsule Collider
    /// </summary>
    public interface ICapsuleMoveable : ISphereMoveable
    {
        public float height { get; }
    }
}