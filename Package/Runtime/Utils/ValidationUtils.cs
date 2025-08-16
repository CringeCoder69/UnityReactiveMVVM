using UnityEngine;

namespace UnityReactiveMVVM.Utils
{
    public class ValidationUtils
    {
        public static void ValidateComponent<T>(ref GameObject obj)
        {
            if (obj == null)
                return;

            var interfaceName = typeof(T).Name;
            if (!obj.TryGetComponent<T>(out _))
            {
                Debug.LogError($"Missing {interfaceName} component.", obj);
                obj = null;
            }
        }
    }
}
