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
        #region GroupSteering Gizmos

        [Header("GroupSteering Gizmos")]
        public bool showNeighbours;
        public Color neighbourSphereColor;

        #endregion GroupSteering Gizmos

        protected override void Awake()
        {
            base.Awake();

            /** LEGACY
             //Find all group members
             if (!tagGroups.TryGetValue(tagName, out groupMembers))
             {
                 groupMembers = GameObject.FindGameObjectsWithTag(tagName).ToList().ConvertAll(obj => obj.GetComponent<ISphereMoveable>());
                 tagGroups.Add(tagName, groupMembers);
             }
            */
        }

        /** LEGACY
        protected void FindNeighbours()
            =>
            m_neighbours = groupMembers.
                 FindAll(member => !ReferenceEquals(member, m_entity) && Vector3.Distance(member.position, m_entity.position) < radius);
        **/

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (Application.isPlaying)
            {
                //Show Neighbours
                if (showNeighbours)
                {
                    //Gizmos.DrawIcon(m_entity.position, "Fuck Unity Fuck Fuck Fuck Trash GUIs Trash APIs ");
                    //Draw Neighbour Radius
                    Gizmos.color = neighbourSphereColor;
                    //Gizmos.DrawWireSphere(m_entity.position, m_manager.QueryRadius);

                    ////Draw Lines to Neighbours
                    //foreach (var nei in m_neighbours)
                    //{
                    // Gizmos.DrawLine(m_entity.position, nei.position);
                    //}
                }
            }
        }
    }
}