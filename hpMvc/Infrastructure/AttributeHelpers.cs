using System;
using System.Linq;

namespace hpMvc.Infrastructure
{
    public static class AttributeHelpers
    {
        public static string GetDisplayName(Type objectType, string propertyName, Type attributeType, string attributePropertyName)
        {
            var propertyInfo= objectType.GetProperty(propertyName);
            if (propertyInfo != null)
            {
                if (Attribute.IsDefined(propertyInfo, attributeType))
                {
                    var attributeInstance = Attribute.GetCustomAttribute(propertyInfo, attributeType);
                    if (attributeInstance != null)
                    {
                        return (from info in attributeType.GetProperties()
                                where info.CanRead && String.Compare(info.Name, attributePropertyName, 
                                StringComparison.InvariantCultureIgnoreCase) == 0 select info.GetValue(attributeInstance, null)
                                .ToString()).FirstOrDefault();
                    }
                }
            }
            return null;
        }
    }
}