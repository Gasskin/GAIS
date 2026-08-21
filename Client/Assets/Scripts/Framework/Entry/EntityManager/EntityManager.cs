using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Framework
{
    public class EntityManager : BaseManager, IUpdateManager
    {
        public static Transform EntityRoot { get; private set; }
        
        private List<Entity> _entities = new();
        
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
            entity.ManagerIndex = _entities.Count - 1;
            _entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            _entities.RemoveAt(entity.ManagerIndex);
            ObjectPool.Release(entity);
        }
    }
}