using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SteeringSystem
{
    public abstract class LEGACYGroupBehaviourManager : MonoBehaviour
    {
        //Contains all set of groups seperated by group tag
        public static Dictionary<string, List<ISphereMoveable>> tagGroups = new Dictionary<string, List<ISphereMoveable>>();

        public float radius;
        public string groupTag;

        protected virtual void Awake()
        {
            //Find all group members
            if (!tagGroups.ContainsKey(groupTag))
                tagGroups.Add(groupTag,
                    GameObject.FindGameObjectsWithTag(groupTag).ToList<GameObject>().
                    ConvertAll<ISphereMoveable>(obj => obj.GetComponent<ISphereMoveable>()));
        }

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}