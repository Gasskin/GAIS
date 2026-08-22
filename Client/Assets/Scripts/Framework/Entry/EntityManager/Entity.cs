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

        private List<int> _needUpdateComponentIndex = new(16);
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
                        EnableComponentUpdate(com);
                    }
                    await com.Initialize();
                }
            }
            _needUpdateComponentIndex.Sort();
            await UniTask.Yield();
        }

        public void Update(float deltaTime)
        {
            if (_needUpdateComponentIndex.Count <= 0)
            {
                return;
            }
            for (int i = 0; i < _needUpdateComponentIndex.Count; i++)
            {
                var index = _needUpdateComponentIndex[i];
                var com = _components[index];
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
            if (com.UpdateIndex >= 0)
            {
                return;
            }
            com.UpdateIndex = _needUpdateComponentIndex.Count - 1;
            _needUpdateComponentIndex.Add(com.ID);
            _needUpdateComponentIndex.Sort();
        }

        public void DisableComponentUpdate(BaseComponent com)
        {
            if (com.UpdateIndex < 0 || com.UpdateIndex >= _needUpdateComponentIndex.Count)
            {
                return;
            }
            _needUpdateComponentIndex.RemoveAt(com.UpdateIndex);
            com.UpdateIndex = -1;
            _needUpdateComponentIndex.Sort();
        }

        public void OnRelease()
        {
            for (int i = _components.Length - 1; i >= 0; i--)
            {
                var com = _components[i];
                if (com != null)
                {
                    com.Entity = null;
                    com.UpdateIndex = -1;
                    ObjectPool.Release(com);
                }
            }
            Array.Fill(_components, null, 0, _components.Length);
            Uid = 0;
            _needUpdateComponentIndex.Clear();
        }
    }
}