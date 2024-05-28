using System.Collections.Generic;
using UnityEngine;

namespace Editor.EMeshCombiner
{
    public static class TransformUtility
    {
        public static Transform FindCommonParent(GameObject[] objects)
        {
            if (objects.Length == 0) return null;
            Transform parent = objects[0].transform.parent;

            for (int i = 1; i < objects.Length; i++)
            {
                parent = GetCommonParent(parent, objects[i].transform.parent);
                if (parent == null) break;
            }

            return parent;
        }

        private static Transform GetCommonParent(Transform a, Transform b)
        {
            if (a == null || b == null) return null;

            List<Transform> aParents = new List<Transform>();
            while (a != null)
            {
                aParents.Add(a);
                a = a.parent;
            }

            while (b != null)
            {
                if (aParents.Contains(b))
                {
                    return b;
                }
                b = b.parent;
            }

            return null;
        }
    }
}