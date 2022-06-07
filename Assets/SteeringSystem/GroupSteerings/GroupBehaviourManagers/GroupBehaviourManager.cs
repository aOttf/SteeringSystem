using System.Text;
using System.Linq;
using System.Collections;

using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public enum GroupBehaviour
    {
        CollisionAvoidance, Alignment, Cohesion, Seperation, Flock
    }

    public abstract class GroupBehaviourManager : MonoBehaviour
    {
        //Contains all set of groups seperated by group tag
        public static Dictionary<string, List<SteerAgent>> tagGroups = new Dictionary<string, List<SteerAgent>>();

        private Dictionary<(int x, int y), List<SteerAgent>> m_groupMembers = new Dictionary<(int x, int y), List<SteerAgent>>();

        [Header("Gizmos Options")]
        public bool drawGizmos;

        [Header("Update Mode Configs")]
        public UpdateType updateType = UpdateType.TimeStep;
        public float tick = .5f;

        [Header("Spatial Partition Configs")]
        [SerializeField] private float m_yOffset;
        [SerializeField] private int m_width;
        [SerializeField] private int m_height;
        [SerializeField] private int m_gridBucketCapacity = 5;

        [Header("Group Steering Configs")]
        [SerializeField] private float m_radius;
        [SerializeField] private string m_groupTag;

        #region Caches

        private float m_sqrRadius;

        private List<KeyValuePair<(int x, int y), SteerAgent>> m_backupMembers = new List<KeyValuePair<(int x, int y), SteerAgent>>();    //This is used to store members whose new grid indexes are not present in the dictionary

        protected GroupBehaviour m_groupBehaviourIndex; //Delegated Group Behaviour Index

        #endregion Caches

        public float QueryRadius
        {
            get => m_radius;
            set
            {
                m_radius = value;
                //Update Caches
            }
        }

        protected virtual void Awake()
        {
            //Find all group members
            if (!tagGroups.ContainsKey(m_groupTag))
                tagGroups.Add(m_groupTag,
                    GameObject.FindGameObjectsWithTag(m_groupTag).ToList<GameObject>().
                    ConvertAll(obj => obj.GetComponent<SteerAgent>()));

            m_groupMembers.EnsureCapacity(m_gridBucketCapacity);

            //Initialization
            List<SteerAgent> members =
                GameObject.FindGameObjectsWithTag(m_groupTag).ToList<GameObject>()
                .ConvertAll(gameObj => gameObj.GetComponent<SteerAgent>());

            foreach (var member in members)
            {
                //Add to dictionary
                WrapperDictionaryAdd(World2Grid(member.position), member);
            }

            m_sqrRadius = m_radius * m_radius;
        }

        protected void Start()
        {
            StartCoroutine(nameof(UpdateGroupBehaviour));
        }

        protected (int x, int y) World2Grid(Vector3 pos)
            => (Mathf.FloorToInt(pos.z / m_radius), Mathf.FloorToInt((pos.x / m_radius)));

        protected Vector3 Grid2World((int x, int y) idx) => new Vector3(idx.y * m_radius, m_yOffset, idx.x * m_radius);

        protected IEnumerator UpdateGroupBehaviour()
        {
            while (true)
            {
                ApplyGroupSteering();

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

        /// <summary>
        /// Update the entities grid index, clear group behaviour output and clear any empty grid in the dictionary
        /// </summary>
        private void UpdateInternalState()
        {
            /// <summary>
            /// Clear empty lists in the dictionary
            /// Empty list means there're currently no members inside the associated grid
            /// </summary>
            void ClearEmptyCaches()
            {
                IEnumerable<KeyValuePair<(int x, int y), List<SteerAgent>>> tmp
                    = m_groupMembers.Where(kv => kv.Value.Count == 0).ToArray();

                foreach (var pair in tmp)
                    m_groupMembers.Remove(pair.Key);
            }

            //Main Loop
            foreach (var pair in m_groupMembers)
            {
                (int x, int y) curIdx = pair.Key;
                (int x, int y) nextIdx;
                List<SteerAgent> curMembers = pair.Value;
                List<SteerAgent> nextGrid;
                SteerAgent curMember;

                int cnt = 0;
                while (cnt < curMembers.Count)
                {
                    curMember = curMembers[cnt];

                    //Clear group behaviour output
                    curMember[m_groupBehaviourIndex] = Vector3.zero;
                    //In the same grid
                    if ((nextIdx = World2Grid(curMember.position)) == curIdx)
                    {
                        cnt++;
                        continue;
                    }

                    WrapperListRemoveAt<SteerAgent>(curMembers, cnt);
                    //If the new grid exists in the dictionary, add it in
                    if (m_groupMembers.TryGetValue(nextIdx, out nextGrid))
                        nextGrid.Add(curMember);
                    else
                    {
                        //Add it to the temporary backup list
                        m_backupMembers.Add(new KeyValuePair<(int x, int y), SteerAgent>(nextIdx, curMember));
                    }
                }
            }

            //Remove Empty lists from dictionary
            ClearEmptyCaches();

            //Add members in backup list into the dictionary
            foreach (var kv in m_backupMembers)
                WrapperDictionaryAdd(kv.Key, kv.Value);

            //Clear backup list
            m_backupMembers.Clear();
        }

        /// <summary>
        /// Update all entities grid indexes, query each entity's neighbours and apply the steering behavior on them
        /// </summary>
        private void ApplyGroupSteering()
        {
            UpdateInternalState();

            //Main Loop
            foreach (var pair in m_groupMembers)
            {
                (int row, int col) curIdx = pair.Key;
                List<SteerAgent> curMembers = pair.Value;

                for (int i = 0; i < curMembers.Count; i++)
                {
                    SteerAgent curMember = curMembers[i];

                    //Query members in bottom neighbour grids
                    //Only if the grid is currently registered in the dictionary. I.E. there're members currently in the neighbouring grid
                    if (m_groupMembers.ContainsKey((curIdx.row + 1, curIdx.col)))
                        //Perfrom Steering
                        foreach (var nei in m_groupMembers[(curIdx.row + 1, curIdx.col)])
                        {
                            if (Vector3.SqrMagnitude(nei.position - curMember.position) < m_sqrRadius)
                            {
                                GroupSteering(curMember, nei);
                            }
                        }

                    //Query members in bottom-right neighbour grid
                    //Only if the grid is currently registered in the dictionary. I.E. there're members currently in the neighbouring grid
                    if (m_groupMembers.ContainsKey((curIdx.row + 1, curIdx.col + 1)))
                    {
                        //Perfrom Steering
                        foreach (var nei in m_groupMembers[(curIdx.row + 1, curIdx.col + 1)])
                        {
                            if (Vector3.SqrMagnitude(nei.position - curMember.position) < m_sqrRadius)
                            {
                                GroupSteering(curMember, nei);
                            }
                        }
                    }

                    //Query members in right neighbour grids
                    //Only if the grid is currently registered in the dictionary. I.E. there're members currently in the neighbouring grid
                    if (m_groupMembers.ContainsKey((curIdx.row, curIdx.col + 1)))
                        //Perfrom Steering
                        foreach (var nei in m_groupMembers[(curIdx.row, curIdx.col + 1)])
                        {
                            if (Vector3.SqrMagnitude(nei.position - curMember.position) < m_sqrRadius)
                            {
                                GroupSteering(curMember, nei);
                            }
                        }

                    //Only if the grid is currently registered in the dictionary. I.E. there're members currently in the neighbouring grid
                    if (m_groupMembers.ContainsKey((curIdx.row - 1, curIdx.col + 1)))
                        //Perfrom Steering
                        foreach (var nei in m_groupMembers[(curIdx.row - 1, curIdx.col + 1)])
                        {
                            if (Vector3.SqrMagnitude(nei.position - curMember.position) < m_sqrRadius)
                            {
                                GroupSteering(curMember, nei);
                            }
                        }

                    //Query members in local grid
                    for (int m = i + 1; m < curMembers.Count; m++)
                        //Perfrom Steering without checking distance
                        GroupSteering(curMember, curMembers[m]);
                }
            }
        }

        /// <summary>
        /// delegated group steering behaviour
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        protected abstract void GroupSteering(SteerAgent a, SteerAgent b);

        private void OnDrawGizmosSelected()
        {
            void DrawRect(Vector3 leftBtm)
            {
                Gizmos.DrawLine(leftBtm, leftBtm + m_radius * Vector3.right);
                Gizmos.DrawLine(leftBtm, leftBtm + m_radius * Vector3.forward);
                Gizmos.DrawLine(leftBtm + m_radius * Vector3.forward, leftBtm + m_radius * (Vector3.forward + Vector3.right));
                Gizmos.DrawLine(leftBtm + m_radius * Vector3.right, leftBtm + m_radius * (Vector3.right + Vector3.forward));
            }

            if (drawGizmos && Application.isPlaying)
            {
                //Draw Grids
                foreach (var v in m_groupMembers)
                {
                    DrawRect(Grid2World(v.Key));
                }
            }
        }

        #region List Operations

        ///<summary>Wrapper function for list's RemoveAt operation that doesn't perform shifting elements</summary>
        private void WrapperListRemoveAt<T>(List<T> ls, int idx)
        {
            ls[idx] = ls[ls.Count - 1];
            ls.RemoveAt(ls.Count - 1);
        }

        #endregion List Operations

        #region Dictionary Operations

        private void WrapperDictionaryAdd((int x, int y) key, SteerAgent value)
        {
            List<SteerAgent> members;
            if (m_groupMembers.TryGetValue(key, out members))
                members.Add(value);
            else
            {
                members = new List<SteerAgent>();
                members.Add(value);
                m_groupMembers.Add(key, members);
            }
        }

        #endregion Dictionary Operations
    }
}