using System;
using System.Collections.Generic;
using System.Reflection;

namespace BOC.SynchronousService.Framework.Common
{
    public static class ObjectFactory
    {
        public static T CreateObject<T>(string typeName) where T : class
        {
            Type type = Type.GetType(typeName);

            if (type == null)
                throw new Exception(string.Format("无法确定类型: {0}.", typeName));

            return Activator.CreateInstance(type) as T;
        }

        public static T CreateObject<T>(string typeName, Dictionary<string, string> settings) where T : class
        {
            T obj = CreateObject<T>(typeName);

            if (settings != null && settings.Count != 0)
            {
                Type type = Type.GetType(typeName);
                foreach (KeyValuePair<string, string> setting in settings)
                {
                    string propertyName = setting.Key;
                    PropertyInfo property = type.GetProperty(propertyName);

                    string propertyValue = setting.Value;
                    property.SetValue(obj, propertyValue, null);
                }
            }

            return obj;
        }

        public static T CreateObject<T>(string typeName, Dictionary<string, string> settings, params object[] args) where T : class
        {
            Type type = Type.GetType(typeName);

            if (type == null)
                throw new Exception(string.Format("无法确定类型: {0}.", typeName));

            T obj = Activator.CreateInstance(type, args) as T;

            if (settings != null && settings.Count != 0)
            {
                foreach (KeyValuePair<string, string> setting in settings)
                {
                    string propertyName = setting.Key;
                    PropertyInfo property = type.GetProperty(propertyName);

                    string propertyValue = setting.Value;
                    property.SetValue(obj, propertyValue, null);
                }
            }

            return obj;
        }
    }
}