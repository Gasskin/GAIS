using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Framework
{
    public class Entity : IPoolObject
    {
        public int ManagerIndex { get; set; }
        public int Uid { get; private set; }
        private static int _uidFactory = 1;

        private List<BaseComponent> _needUpdateComponents = new(16);
        private BaseComponent[] _components = new BaseComponent[ComponentID.MAX];


        public async UniTask Initialize()
        {
            if (_uidFactory == int.MaxValue)
            {
                _uidFactory = 1;
            }
            Uid = _uidFactory++;
            for (int i = 0; i < _components.Length; i++)
            {
                var com = _components[i];
                if (com != null)
                {
                    if (com.IsDefaultUpdate)
                    {
                        _needUpdateComponents.Add(com);
                        com.IsUpdate = true;
                    }
                    await com.Initialize();
                }
            }
            _needUpdateComponents.Sort(SortUpdateComponent);
            await UniTask.Yield();
        }


        public void Update(float deltaTime)
        {
            if (_needUpdateComponents.Count <= 0)
            {
                return;
            }
            for (int i = 0; i < _needUpdateComponents.Count; i++)
            {
                var com = _needUpdateComponents[i];
                if (com != null)
                {
                    com.Update(deltaTime);
                }
            }
        }

    #region GetComponent
        public T GetComponent<T>() where T : BaseComponent
        {
            var id =  ComponentID.GetComponentID(typeof(T));
            return _components[id] as T;
        }

        public bool GetAllComponents<T1, T2>(
            out T1 t1, out T2 t2)
            where T1 : BaseComponent
            where T2 : BaseComponent
        {
            var id1 =  ComponentID.GetComponentID(typeof(T1));
            var id2 =  ComponentID.GetComponentID(typeof(T2));
            t1 = _components[id1] as T1;
            t2 = _components[id2] as T2;
            return t1 != null && t2 != null;
        }

        public bool GetAllComponents<T1, T2, T3>(
            out T1 t1, out T2 t2, out T3 t3)
            where T1 : BaseComponent
            where T2 : BaseComponent
            where T3 : BaseComponent
        {
            var id1 =  ComponentID.GetComponentID(typeof(T1));
            var id2 =  ComponentID.GetComponentID(typeof(T2));
            var id3 =  ComponentID.GetComponentID(typeof(T3));
            t1 = _components[id1] as T1;
            t2 = _components[id2] as T2;
            t3 = _components[id3] as T3;
            return t1 != null && t2 != null && t3 != null;
        }

        public bool GetAllComponents<T1, T2, T3, T4>(
            out T1 t1, out T2 t2, out T3 t3, out T4 t4)
            where T1 : BaseComponent
            where T2 : BaseComponent
            where T3 : BaseComponent
            where T4 : BaseComponent
        {
            var id1 =  ComponentID.GetComponentID(typeof(T1));
            var id2 =  ComponentID.GetComponentID(typeof(T2));
            var id3 =  ComponentID.GetComponentID(typeof(T3));
            var id4 =  ComponentID.GetComponentID(typeof(T4));
            t1 = _components[id1] as T1;
            t2 = _components[id2] as T2;
            t3 = _components[id3] as T3;
            t4 = _components[id4] as T4;
            return t1 != null && t2 != null && t3 != null && t4 != null;
        }

        public bool GetAllComponents<T1, T2, T3, T4, T5>(
            out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5)
            where T1 : BaseComponent
            where T2 : BaseComponent
            where T3 : BaseComponent
            where T4 : BaseComponent
            where T5 : BaseComponent
        {
            var id1 =  ComponentID.GetComponentID(typeof(T1));
            var id2 =  ComponentID.GetComponentID(typeof(T2));
            var id3 =  ComponentID.GetComponentID(typeof(T3));
            var id4 =  ComponentID.GetComponentID(typeof(T4));
            var id5 =  ComponentID.GetComponentID(typeof(T5));
            t1 = _components[id1] as T1;
            t2 = _components[id2] as T2;
            t3 = _components[id3] as T3;
            t4 = _components[id4] as T4;
            t5 = _components[id5] as T5;
            return t1 != null && t2 != null && t3 != null && t4 != null && t5 != null;
        }
    #endregion

        public void AddComponent(BaseComponent com)
        {
            if (com == null)
            {
                return;
            }
            var id = ComponentID.GetComponentID(com);
            if (id < 0 || id >= _components.Length)
            {
                return;
            }
            if (_components[id] != null)
            {
                return;
            }
            _components[id] = com;
            com.Entity = this;
        }

        public void EnableComponentUpdate(BaseComponent com)
        {
            if (com.IsUpdate)
            {
                return;
            }
            com.IsUpdate = true;
            _needUpdateComponents.Add(com);
            _needUpdateComponents.Sort(SortUpdateComponent);
        }

        public void DisableComponentUpdate(BaseComponent com)
        {
            if (!com.IsUpdate)
            {
                return;
            }
            com.IsUpdate = false;
            for (int i = 0; i < _needUpdateComponents.Count; i++)
            {
                var check = _needUpdateComponents[i];
                if (check == com)
                {
                    for (int j = i; j < _needUpdateComponents.Count - 1; j++)
                    {
                        _needUpdateComponents[j] = _needUpdateComponents[j + 1];
                    }
                    _needUpdateComponents.RemoveAt(_needUpdateComponents.Count - 1);
                    break;
                }
            }
        }

        private int SortUpdateComponent(BaseComponent x, BaseComponent y)
        {
            var idx = ComponentID.GetComponentID(x);
            var idy = ComponentID.GetComponentID(y);
            return idx - idy;
        }

        public void OnRelease()
        {
            for (int i = _components.Length - 1; i >= 0; i--)
            {
                var com = _components[i];
                if (com != null)
                {
                    com.Entity = null;
                    com.IsUpdate = false;
                    ObjectPool.Release(com);
                }
            }
            Array.Fill(_components, null, 0, _components.Length);
            Uid = 0;
            _needUpdateComponents.Clear();
        }
    }
}