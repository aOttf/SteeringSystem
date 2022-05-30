using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace SteeringSystem
{
    /// <summary>
    /// <para>Use Time to Collision Method to perform agents avoidance</para>
    /// References : <see href="http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter19_Guide_to_Anticipatory_Collision_Avoidance.pdf">Anticipitory Collision Avoidance </see>
    /// </summary>
    ///
    public class LEGACYGroupAvoidanceManager : LEGACYGroupBehaviourManager
    {
        private List<ISphereMoveable>[,] m_groupMembers;

        [Header("Update Mode")]
        public UpdateType updateType = UpdateType.TimeStep;
        public float tick = .5f;

        [Header("Spatial Partition config")]
        [SerializeField] private Vector3 m_pivot;   //Left Bottom Pivot of the partitioned environment
        [SerializeField] private int m_width;
        [SerializeField] private int m_height;
        [SerializeField] private int m_gridBucketCapacity = 5;

        [Header("Group Avoidance config")]
        public float time2Predict;  //Only Consider Future Collisions less than a given time
        public float deviationAngle;

        #region Caches

        private int m_rows;
        private int m_cols;
        private float sqrRadius;

        #endregion Caches

        #region Gizmos

        public bool drawGizmos;

        #endregion Gizmos

        protected IEnumerator GroupAvoidanceUpdate()
        {
            while (true)
            {
                ApplyGroupAvoidance();

                switch (updateType)
                {
                    case UpdateType.Update:
                        yield return null;
                        break;

                    case UpdateType.FixedUpdate:
                        yield return new WaitForFixedUpdate();
                        break;

                    case UpdateType.TimeStep:
                        yield return new WaitForSeconds(tick);
                        break;
                }
            }
        }

        private void OnDestroy()
        {
            StopCoroutine(nameof(GroupAvoidanceUpdate));
        }

        protected override void Awake()
        {
            base.Awake();
            m_groupMembers = new List<ISphereMoveable>[m_rows = Mathf.CeilToInt(m_height / radius), m_cols = Mathf.CeilToInt(m_width / radius)];
            for (int i = 0; i < m_rows; i++)
                for (int j = 0; j < m_cols; j++)
                    m_groupMembers[i, j] = new List<ISphereMoveable>(m_gridBucketCapacity);

            List<ISphereMoveable> members =
                GameObject.FindGameObjectsWithTag(groupTag).ToList<GameObject>()
                .ConvertAll<ISphereMoveable>(gameObj => gameObj.GetComponent<ISphereMoveable>());

            foreach (var member in members)
            {
                (int x, int y) = World2Grid(member.position);
                m_groupMembers[x, y].Add(member);
            }

            sqrRadius = radius * radius;
        }

        private void Start()
        {
            StartCoroutine(nameof(GroupAvoidanceUpdate));
        }

        /// <summary>
        /// Convert global position into grid index
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected (int x, int y) World2Grid(Vector3 pos)
        {
            return (Mathf.FloorToInt((pos.z - m_pivot.z) / radius), Mathf.FloorToInt((pos.x - m_pivot.x) / radius));
        }

        protected void UpdateInternalState()
        {
            for (int i = 0; i < m_rows; i++)
                for (int j = 0; j < m_cols; j++)
                {
                    List<ISphereMoveable> members = m_groupMembers[i, j];
                    ISphereMoveable curMember;
                    (int x, int y) gridIndex;
                    int cnt = 0;
                    while (cnt < members.Count)
                    {
                        curMember = members[cnt];
                        //Clear previours Steering
                        curMember.groupAvoidanceOutput = SteeringOutput.ZeroSteering;
                        if ((gridIndex = World2Grid(curMember.position)) != (i, j))
                        {
                            //Remove From the current bucket and add to the correct new bucket
                            members[cnt] = members[^1];
                            members.RemoveAt(members.Count - 1);
                            m_groupMembers[gridIndex.x, gridIndex.y].Add(curMember);
                        }
                        else
                            cnt++;
                    }
                }
        }

        protected void ApplyGroupAvoidance()
        {
            UpdateInternalState();

            for (int i = 0; i < m_rows; i++)
                for (int j = 0; j < m_cols; j++)
                {
                    List<ISphereMoveable> members = m_groupMembers[i, j];
                    for (int k = 0; k < members.Count; k++)
                    {
                        ISphereMoveable curMember = members[k];
                        //Query members in bottom neighbour grid
                        if (i < m_rows - 1)
                        {
                            //Perfrom Avoidance
                            foreach (var nei in m_groupMembers[i + 1, j])
                            {
                                if (Vector3.SqrMagnitude(nei.position - curMember.position) < sqrRadius)
                                {
                                    CalculateAvoidanceSteering(curMember, nei);
                                }
                            }

                            //Query members in bottom-right neighbour grid
                            if (j < m_cols - 1)
                            {
                                //Perfrom Avoidance
                                foreach (var nei in m_groupMembers[i + 1, j + 1])
                                {
                                    if (Vector3.SqrMagnitude(nei.position - curMember.position) < sqrRadius)
                                    {
                                        CalculateAvoidanceSteering(curMember, nei);
                                    }
                                }
                            }

                            //Query members in right beighbour grid
                            if (j < m_cols - 1)
                            {
                                //Perfrom Avoidance
                                foreach (var nei in m_groupMembers[i, j + 1])
                                {
                                    if (Vector3.SqrMagnitude(nei.position - curMember.position) < sqrRadius)
                                    {
                                        CalculateAvoidanceSteering(curMember, nei);
                                    }
                                }

                                //Query members in right-top neighbour grid
                                if (i > 0)
                                {
                                    // Perfrom Avoidance
                                    foreach (var nei in m_groupMembers[i - 1, j + 1])
                                    {
                                        if (Vector3.SqrMagnitude(nei.position - curMember.position) < sqrRadius)
                                        {
                                            CalculateAvoidanceSteering(curMember, nei);
                                        }
                                    }
                                }
                            }

                            //Query members in local grid
                            for (int m = k + 1; m < members.Count; m++)
                            {
                                //Perform Avoidance without checking distance
                                CalculateAvoidanceSteering(curMember, members[m]);
                            }
                        }
                    }
                }
        }

        protected void CalculateAvoidanceSteering(ISphereMoveable A, ISphereMoveable B)
        {
            float colTime = Time2Collision(A, B);
            if (colTime < time2Predict)
            {
                //Calculate acceleration direction
                Vector3 dir = (A.position - B.position + (A.linearVelocity - B.linearVelocity) * colTime).normalized;

                ////If the direction of avoidance acceleration is parallel w/ the velocity, apply a deviation angle
                //if (180f - Vector3.Angle(dir, A.linearVelocity) < float.Epsilon)
                //    dir = Quaternion.AngleAxis(deviationAngle, transform.up) * dir;

                Vector3 res = dir * A.MaxLinearAcceleration * (time2Predict - colTime) / (colTime + .1f);
                A.groupAvoidanceOutput += res;
                B.groupAvoidanceOutput -= res;
            }
        }

        /// <summary>
        /// <para>Derived from the Equation
        /// <code>dot(deltaV, deltaV) * t^2 + 2 * dot(deltaX, deltaV) * t + dot(deltaX, deltaX) - sumRadius^2 = 0</code>
        ///<code>t = (-b +- sqrt(b^2 - 4*a*c)) / (2*a) </code>
        /// After Simplification,<code> t = (-b +- sqrt(b^2 - a*c))/a</code>,
        /// where <code>b = dot(deltaX, deltaV), a = dot(deltaV, deltaV), c = (dektaX, deltaX) - sumRadius^2; </code></para>
        /// </summary>
        /// <param name="A">Collision Entity</param>
        /// <param name="B">Another Collision Entity</param>
        /// <returns></returns>
        protected float Time2Collision(ISphereMoveable A, ISphereMoveable B)
        {
            //Variables
            float sumRadius = A.radius + B.radius;
            Vector3 deltaV = B.linearVelocity - A.linearVelocity;
            Vector3 deltaX = B.position - A.position;

            float b = Vector3.Dot(deltaX, deltaV);
            float a = Vector3.Dot(deltaV, deltaV);
            float c = Vector3.Dot(deltaX, deltaX) - sumRadius * sumRadius;

            float delta = b * b - a * c;

            //Already in Collision
            if (c < 0)
                return 0f;

            //Delta < 0 or t < 0, the collision will never happen in the future
            float time;
            if (delta < 0 || (time = (-b - Mathf.Sqrt(delta)) / a) < 0)
                return float.PositiveInfinity;

            return time;
        }

        private void OnDrawGizmosSelected()
        {
            void DrawRect(Vector3 leftBtm)
            {
                Gizmos.DrawLine(leftBtm, leftBtm + radius * Vector3.right);
                Gizmos.DrawLine(leftBtm, leftBtm + radius * Vector3.forward);
                Gizmos.DrawLine(leftBtm + radius * Vector3.forward, leftBtm + radius * (Vector3.forward + Vector3.right));
                Gizmos.DrawLine(leftBtm + radius * Vector3.right, leftBtm + radius * (Vector3.right + Vector3.forward));
            }
            void DrawGridIndexes((int row, int col) index, List<ISphereMoveable> agents)
            {
                foreach (var agent in agents)
                {
                    Handles.Label(agent.position, index.ToString());
                }
            }

            if (drawGizmos && Application.isPlaying)
            {
                //Draw Pivot
                Gizmos.DrawSphere(m_pivot, 10f);
                //Draw Grids
                for (int i = 0; i < m_rows; i++)
                    for (int j = 0; j < m_cols; j++)
                    {
                        DrawRect(m_pivot + i * radius * Vector3.forward + j * radius * Vector3.right);
                        //Draw Grid Indexes
                        DrawGridIndexes((i, j), m_groupMembers[i, j]);
                    }
            }
        }

        #region Debugging

        private bool CheckAllSteeringsCleared()
        {
            for (int i = 0; i < m_rows; i++)
                for (int j = 0; j < m_cols; j++)
                    foreach (var mem in m_groupMembers[i, j])
                        if (!SteeringOutput.IsZero(mem.groupAvoidanceOutput))
                            return false;
            return true;
        }

        #endregion Debugging
    }
}