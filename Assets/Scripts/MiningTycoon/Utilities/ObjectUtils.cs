using UnityEngine;

namespace MiningTycoon.Utilities
{
    public static class ObjectUtils
    {
        public static void DestroyChildren(this Transform transform)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                while (transform.childCount > 0)
                {
                    Object.DestroyImmediate(transform.GetChild(0).gameObject);
                }
            }
#endif
            foreach (Transform child in transform)
            {
                Object.Destroy(child?.gameObject);
            }
        }
    }
}