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

        public T GetComponent<T>(int id) where T : BaseComponent
        {
            return _components[id] as T;
        }

        public void AddComponent(BaseComponent com)
        {
            if (com == null)
            {
                return;
            }
            if (com.ID < 0 || com.ID >= _components.Length)
            {
                return;
            }
            if (_components[com.ID] != null)
            {
                return;
            }
            _components[com.ID] = com;
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
                if (check.ID == com.ID)
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
            return x.ID - y.ID;
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