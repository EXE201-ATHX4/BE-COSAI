using System.Reflection;

namespace ToyShop.Core.Utils
{
    public static class StringHelpers
    {
        public static void TrimAllStrings<T>(this T obj)
        {
            if (obj == null) return;

            foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.PropertyType == typeof(string) && property.CanWrite)
                {
                    if (property.GetValue(obj) is string currentValue)
                    {
                        property.SetValue(obj, currentValue.Trim());
                    }
                }
            }
        }
    }
}
