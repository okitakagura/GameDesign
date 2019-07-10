using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public enum Target
    {
        Specific,
        Self,
        Actor,
        Tag,
        Find,
        Other,
        None
    }

    public enum FindTarget
    {
        Specific,
        Self,
        Actor,
        Tag
    }

    [Serializable]
    public struct TargetDesc
    {
        public Target Target;
        public FindTarget FindTarget;
        public GameObject Object;
        public string Tag;
        public string Name;

        public GameObject[] Get(GameObject self, GameObject other, GameObject[] actors)
        {
            switch (Target)
            {
                case Target.Self: 
                case Target.Actor: 
                case Target.Specific:
                case Target.Tag:
                    return get(Target, self, other, actors);

                case Target.Find:
                    var list = new List<GameObject>();

                    foreach (var target in get((Target)FindTarget, self, other, actors))
                        find(target.transform, list);

                    return list.ToArray();
            }

            return new GameObject[0];
        }

        private void find(Transform parent, List<GameObject> list)
        {
            var result = parent.Find(Name);

            if (result != null && result.gameObject != null)
                list.Add(result.gameObject);

            for (int i = 0; i < parent.childCount; i++)
                find(parent.GetChild(i), list);
        }

        private GameObject[] get(Target target, GameObject self, GameObject other, GameObject[] actors)
        {
            switch (target)
            {
                case Target.Self: return self == null ? new GameObject[0] : new GameObject[] { self };
                case Target.Actor: return actors == null ? new GameObject[0] : actors;
                case Target.Specific: return Object == null ? new GameObject[0] : new GameObject[] { Object };
                case Target.Tag: return (Tag == null || Tag.Length == 0) ? new GameObject[0] : GameObject.FindGameObjectsWithTag(Tag);
                case Target.Other: return other == null ? new GameObject[0] : new GameObject[] { other };
            }

            return new GameObject[0];
        }
    }
}
