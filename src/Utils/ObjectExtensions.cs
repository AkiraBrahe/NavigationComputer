using System.Linq;
using UnityEngine;

namespace NavigationComputer.Utils
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Applies an action to an object and returns the object.
        /// </summary>
        public static T Apply<T>(this T obj, System.Action<T> action)
        {
            action(obj);
            return obj;
        }

        /// <summary>
        /// Finds an inactive object by name in the children of the given object.
        /// </summary>
        public static GameObject FindInactive(this GameObject obj, string name)
        {
            return obj.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name)?.gameObject;
        }
    }
}