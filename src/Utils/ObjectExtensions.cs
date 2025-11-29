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
    }
}