using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface IPoolObject
    {
        void OnRelease();
    }
    
    public static class ObjectPool
    {
        private static Dictionary<Type, List<IPoolObject>> _pools = new();
        private static int _defaultCapacity = 128;

#if UNITY_EDITOR
        public static Dictionary<Type, int> GetCount = new();
        public static Dictionary<Type, int> ReleaseCount = new();
#endif

        public static T Get<T>() where T : IPoolObject, new()
        {
            var type = typeof(T);
            if (!_pools.TryGetValue(type, out var pools))
            {
                pools = new List<IPoolObject>();
                _pools[type] = pools;
            }
#if UNITY_EDITOR
            GetCount.TryAdd(type, 0);
            GetCount[type]++;
#endif
            if (pools.Count > 0)
            {
                var t = pools[^1];
                pools.RemoveAt(pools.Count - 1);
                return (T)t;
            }
            return new T();
        }

        public static void Release<T>(T o) where T : IPoolObject
        {
            var type = o.GetType();
            if (!_pools.TryGetValue(type, out var pools))
            {
                Debug.LogError($"object pools not found: {type}");
                return;
            }
#if UNITY_EDITOR
            ReleaseCount.TryAdd(type, 0);
            ReleaseCount[type]++;
#endif
            o.OnRelease();
            if (pools.Count >= _defaultCapacity)
            {
                return;
            }
            pools.Add(o);
        }

        public static void Clear<T>() where T : IPoolObject
        {
            var type = typeof(T);
            Clear(type);
        }

        public static void Clear(Type type)
        {
            if (_pools.TryGetValue(type, out var pools))
            {
                pools.Clear();
            }
#if UNITY_EDITOR
            GetCount.Remove(type);
            ReleaseCount.Remove(type);
#endif
        }

        public static void ClearAll()
        {
            foreach (var pools in _pools.Values)
            {
                pools.Clear();
            }
            _pools.Clear();
#if UNITY_EDITOR
            GetCount.Clear();
            ReleaseCount.Clear();
#endif
        }
    }
}