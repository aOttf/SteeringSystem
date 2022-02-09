using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Flock : GroupSteering
    {
        public float radius;

        public float seperationWeight;
        public float cohesionWeight;
        public float alignmentWeight;

        //Caches
        protected Vector3 m_acceleration;
        protected int m_neighbourCount;

        protected List<Vector3> m_neighbourStrengths;
        protected Vector3 m_centerofStrength;   //Seperation

        protected Vector3 m_centerOfMess;   //Cohesion
        protected Vector3 m_centerOfHead;   //Alignment

        protected override void Awake()
        {
            //Init
            tagName = "Flock";
            base.Awake();
        }

        protected override SteeringOutput GetSteering()
        {
            //Find all neightbours
            List<Transform> neighbours
                = groupMembers.FindAll(member => member != transform && Vector3.Distance(member.position, transform.position) < radius);
            m_neighbourCount = neighbours.Count;

            //Seperations
            //Inverse Squart Root
            m_neighbourStrengths = neighbours.ConvertAll(neighbour => (transform.position - neighbour.position) / (transform.position - neighbour.position).sqrMagnitude);
            m_centerofStrength = new Vector3(m_neighbourStrengths.Sum(strength => strength.x), m_neighbourStrengths.Sum(strength => strength.y), m_neighbourStrengths.Sum(strength => strength.z));
            m_acceleration = m_centerofStrength;

            //Cohesion
            m_centerOfMess = new Vector3(neighbours.Average(neighbour => neighbour.position.x), neighbours.Average(neighbour => neighbour.position.y), neighbours.Average(neighbour => neighbour.position.z));
            //Alignment
            m_centerOfHead = new Vector3(neighbours.Average(neighbour => neighbour.forward.x), neighbours.Average(neighbour => neighbour.forward.y), neighbours.Average(neighbour => neighbour.forward.z));

            return SteeringOutput.LinearSteering(m_acceleration);
        }
    }
}