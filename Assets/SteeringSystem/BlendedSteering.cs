using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SteeringSystem
{
    public class BlendedSteering : SteeringBehaviour
    {
        [Header("Blended Params")]
        public List<SteeringBehaviour> steeringBehaviours;
        public CombineMethod method = CombineMethod.WeightedTruncated;

        #region Caches

        private SteeringBehaviour m_steerSelected;

        #endregion Caches

        protected override void Start()
        {
            base.Start();
        }

        #region Add&Remove

        public void AddSteering(SteeringBehaviour pSteering) => steeringBehaviours.Add(pSteering);

        public void AddSteering(SteeringBehaviour pSteering, int priority) => steeringBehaviours.Insert(priority, pSteering);

        public bool RemoveSteering(SteeringBehaviour pSteering) => steeringBehaviours.Remove(pSteering);

        #endregion Add&Remove

        protected override Vector3 GetSteering()
        {
            m_steerSelected = null;
            var res = Vector3.zero;

            switch (method)
            {
                case CombineMethod.WeightedTruncated:
                    foreach (var steer in steeringBehaviours)
                        res += steer.Steering;
                    break;

                case CombineMethod.PriorityWeightedTruncated:
                    foreach (var steer in steeringBehaviours)
                    {
                        //If we encounter a steering behavior that doesn't output zero, stop
                        if ((res = steer.Steering) != Vector3.zero)
                        {
                            m_steerSelected = steer;
                            break;
                        }
                    }

                    break;

                case CombineMethod.PriorityDithering:
                    foreach (var steer in steeringBehaviours)
                    {
                        //Probability Test and Steering Output is not zero
                        if (UnityEngine.Random.Range(0, 1) < steer.Probability && (res = steer.Steering) != Vector3.zero)
                        {
                            m_steerSelected = steer;
                            break;
                        }
                        //Otherwise, we pass the probability test and found a nontrivial steering
                    }
                    break;
            }

            return res;
        }

        protected override void OnDrawGizmosSelected()
        {
            if (method == CombineMethod.WeightedTruncated)
                base.OnDrawGizmosSelected();
            else if (showGizmos && Application.isPlaying)
            {
                if (showState)
                {
                    if (m_steerSelected)
                        Handles.Label(m_entity.position, m_steerSelected.steerTag);

                    if (showTargetVelocity)
                    {
                        Gizmos.color = targetVelocityColor;
                        Gizmos.DrawRay(m_entity.position, m_targetVelocity);
                    }
                }
            }
        }
    }
}