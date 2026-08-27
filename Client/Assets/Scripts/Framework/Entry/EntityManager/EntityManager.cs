using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using UnityEngine;

namespace Framework
{
    public class EntityManager : BaseManager, IUpdateManager
    {
        public static Transform EntityRoot { get; private set; }

        private List<Entity> _entities = new();
        private Dictionary<int, Entity> _entitiesDict = new();
        private Stack<int> _emptyEntityIndex = new();
        private HashSet<int> _removeQueue = new();
        private HashSet<int> _removeHelper = new();

        public override async UniTask Initialize()
        {
            if (EntityRoot == null)
            {
                EntityRoot = new GameObject("[EntityManager]").transform;
                EntityRoot.SetParent(GameEntry.Instance.transform, false);
            }

            await UniTask.Yield();
        }

        public override void Destroy()
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i] == null)
                {
                    continue;
                }
                ObjectPool.Release(_entities[i]);
            }
            _entities.Clear();
            _entitiesDict.Clear();
            _emptyEntityIndex.Clear();
            _removeQueue.Clear();
            Object.Destroy(EntityRoot.gameObject);
        }

        public void Update(float dt)
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                _entities[i]?.Update(dt);
            }

            RemoveEntityInterval();
        }

        public void AddEntity(Entity entity)
        {
            if (!_entitiesDict.TryAdd(entity.Uid, entity))
            {
                return;
            }
            if (_emptyEntityIndex.Count > 0)
            {
                entity.ManagerIndex = _emptyEntityIndex.Pop();
                _entities[entity.ManagerIndex] = entity;
            }
            else
            {
                entity.ManagerIndex = _entities.Count;
                _entities.Add(entity);
            }
        }

        public void RemoveEntity(int uid)
        {
            if (uid <= 0 || !_entitiesDict.ContainsKey(uid))
            {
                return;
            }
            _removeQueue.Add(uid);
        }

        private void RemoveEntityInterval()
        {
            if (_removeQueue.Count > 0)
            {
                _removeHelper.AddRange(_removeQueue);
                _removeQueue.Clear();
                foreach (var id in _removeHelper)
                {
                    if (!_entitiesDict.Remove(id, out var entity))
                    {
                        continue;
                    }
                    _emptyEntityIndex.Push(entity.ManagerIndex);
                    _entities[entity.ManagerIndex] = null;
                    ObjectPool.Release(entity);
                }
                _removeHelper.Clear();
            }
        }

        public Entity GetEntity(int id)
        {
            if (_removeQueue.Contains(id) || _removeHelper.Contains(id))
            {
                return null;
            }
            return _entitiesDict.GetValueOrDefault(id);
        }
    }
}