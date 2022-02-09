using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class BlendedSteering : SteeringBehaviour
    {
        public List<SteeringBehaviour> steeringBehaviours;

        public CombineMethod method = CombineMethod.WeightedTruncated;

        protected override void Start()
        {
            base.Start();
        }

        #region Add&Remove

        public void AddSteering(SteeringBehaviour pSteering) => steeringBehaviours.Add(pSteering);

        public void AddSteering(SteeringBehaviour pSteering, int priority) => steeringBehaviours.Insert(priority, pSteering);

        public bool RemoveSteering(SteeringBehaviour pSteering) => steeringBehaviours.Remove(pSteering);

        #endregion Add&Remove

        protected override SteeringOutput GetSteering()
        {
            var res = SteeringOutput.ZeroSteering;
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
                        if (!SteeringOutput.IsZero(res = steer.Steering))
                            break;
                    }

                    break;

                case CombineMethod.PriorityDithering:
                    foreach (var steer in steeringBehaviours)
                    {
                        //Probability Test and Steering Output is not zero
                        if (UnityEngine.Random.Range(0, 1) > steer.Probability || SteeringOutput.IsZero(res = steer.Steering))
                            continue;

                        //Otherwise, we pass the probability test and found a nontrivial steering
                    }
                    break;
            }

            return res;
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
        }

        public override string ToString() => base.ToString() + string.Join("+", steeringBehaviours.ConvertAll(steer => steer.ToString()));
    }
}