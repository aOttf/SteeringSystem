using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SteeringSystem
{
    /// <summary>
    /// GroupSteeringBehaviour is the base class from which every concrete Group Steering Behaviour Script derives
    /// </summary>
    public abstract class GroupSteeringBehaviour : SteeringBehaviour
    {
        #region Debug Options

        public bool showNeighbours;
        public Color neighbourSphereColor;

        #endregion Debug Options

        public static Dictionary<string, List<ISphereMoveable>> tagGroups = new Dictionary<string, List<ISphereMoveable>>();

        [Space(50)]
        public float radius;
        public string tagName = "GroupSteer";

        //Cache
        public List<ISphereMoveable> groupMembers;
        protected List<ISphereMoveable> m_neighbours;

        protected override void Awake()
        {
            base.Awake();

            //Find all group members
            if (!tagGroups.TryGetValue(tagName, out groupMembers))
            {
                groupMembers = GameObject.FindGameObjectsWithTag(tagName).ToList().ConvertAll(obj => obj.GetComponent<ISphereMoveable>());
                tagGroups.Add(tagName, groupMembers);
            }
        }

        protected void FindNeighbours()
            =>
            m_neighbours = groupMembers.
                 FindAll(member => !ReferenceEquals(member, m_entity) && Vector3.Distance(member.position, m_entity.position) < radius);

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (Application.isPlaying)
            {
                //Show Neighbours
                if (showNeighbours && m_neighbours != null && m_neighbours.Count != 0)
                {
                    //Gizmos.DrawIcon(m_entity.position, "Fuck Unity Fuck Fuck Fuck Trash GUIs Trash APIs ");
                    //Draw Neighbour Radius
                    Gizmos.color = neighbourSphereColor;
                    Gizmos.DrawWireSphere(m_entity.position, radius);

                    //Draw Lines to Neighbours

                    foreach (var nei in m_neighbours)
                    {
                        Gizmos.DrawLine(m_entity.position, nei.position);
                    }
                }
            }
        }
    }
}