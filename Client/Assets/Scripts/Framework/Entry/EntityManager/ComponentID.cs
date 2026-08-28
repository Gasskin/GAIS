using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Framework
{
    public static class ComponentID
    {
        public const int GAIS = 0;
        public const int UNIT = 1;
        public const int SKILL = 2;
        public const int STATE = 2;
        public const int MAX = 4;

        private static Dictionary<Type, int> _type2IdDict = new();
        private static HashSet<int> _check = new();
        
        public static void BindReflection(Assembly assembly)
        {
            _type2IdDict.Clear();
            _check.Clear();
            
            var types = assembly
                .GetTypes()
                .Where(type =>
                    type != typeof(BaseComponent) &&
                    typeof(BaseComponent).IsAssignableFrom(type) &&
                    type.IsDefined(typeof(ComponentIDAttribute), inherit: true))
                .ToArray();

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<ComponentIDAttribute>();
                if (_check.Contains(attr.ComponentID))
                {
                    Debug.LogError("重复的ComponentID");
                    continue;
                }
                _type2IdDict.Add(type, attr.ComponentID);
            }
        }
        
        public static int GetComponentID(BaseComponent com)
        {
            return GetComponentID(com.GetType());
        }

        public static int GetComponentID(Type type)
        {
            if (_type2IdDict.TryGetValue(type, out var id))
            {
                return id;
            }
            Debug.LogError($"不存在ComponentID: {type}");
            return -1;
        }
    }
}
