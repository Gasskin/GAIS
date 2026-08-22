using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Framework
{
    public class EntityManager : BaseManager, IUpdateManager
    {
        public static Transform EntityRoot { get; private set; }

        private List<Entity> _entities = new();
        private Dictionary<int, Entity> _entitiesDict = new();

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
                ObjectPool.Release(_entities[i]);
            }
            _entities.Clear();
            _entitiesDict.Clear();
            Object.Destroy(EntityRoot.gameObject);
        }

        public void Update(float dt)
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                _entities[i].Update(dt);
            }
        }

        public void AddEntity(Entity entity)
        {
            if (!_entitiesDict.TryAdd(entity.Uid, entity))
            {
                return;
            }
            entity.ManagerIndex = _entities.Count;
            _entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (entity == null)
            {
                return;
            }
            for (int i = entity.ManagerIndex; i < _entities.Count - 1; i++)
            {
                _entities[i] = _entities[i + 1];
            }
            _entities.RemoveAt(_entities.Count - 1);
            ObjectPool.Release(entity);
        }

        public bool HasEntity(int sourceId)
        {
            return _entitiesDict.ContainsKey(sourceId);
        }

        public Entity GetEntity(int sourceId)
        {
            return _entitiesDict.GetValueOrDefault(sourceId);
        }
    }
}