using System.Reflection;

namespace GrpcService.Client.Utils.Extensions
{
    public static class ObjectExtension
    {
        /// <summary>
        /// Converts object properties to dictionary with ignoring of the system properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Dictionary<string, object> ConvertPropertiesToDictionary(this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "Object is null");
            }

            Type type = obj.GetType();

            PropertyInfo[] properties = type.GetProperties();
            var propertyDictionary = new Dictionary<string, object>();

            foreach (PropertyInfo prop in properties )
            {
                if (prop.Name == "Parser" || prop.Name == "Descriptor")
                {
                    continue;
                }

                var value = prop.GetValue(obj, null);
                propertyDictionary[prop.Name] = value!;
            }

            return propertyDictionary;
        }
    }
}
