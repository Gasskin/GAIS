#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    public class ObjectPoolMonitor : MonoBehaviour
    {
    #region static
        [MenuItem("GAIS/ObjectPoolMonitor")]
        public static void ShowMonitor()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (_monitor != null)
            {
                return;
            }
            var o = new GameObject("[ObjectPoolMonitor]");
            _monitor = o.AddComponent<ObjectPoolMonitor>();
            DontDestroyOnLoad(o);
        }

        private static ObjectPoolMonitor _monitor;
    #endregion

        private class ObjectPoolState
        {
            public string Name;
            public int Get;
            public int Release;
        }

        [TextArea(1, 50), HideLabel]
        public string Monitor;

        private Dictionary<Type, ObjectPoolState> _states = new();


        [OnInspectorGUI]
        public void OnInspectorGUI()
        {
            _states.Clear();

            var get = ObjectPool.GetCount;
            var release = ObjectPool.ReleaseCount;

            foreach (var key in get.Keys)
            {
                if (!_states.ContainsKey(key))
                {
                    _states[key] = new ObjectPoolState() { Name = key.Name };
                }
            }

            foreach (var key in release.Keys)
            {
                if (!_states.ContainsKey(key))
                {
                    _states[key] = new ObjectPoolState() { Name = key.Name };
                }
            }

            foreach (var pair in get)
            {
                _states[pair.Key].Get = pair.Value;
            }
            foreach (var pair in release)
            {
                _states[pair.Key].Release = pair.Value;
            }

            Monitor = "";
            foreach (var state in _states.Values)
            {
                Monitor += $"{state.Get - state.Release} {state.Name}\n";
            }
        }
    }
}

#endif