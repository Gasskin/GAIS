using Cysharp.Threading.Tasks;

namespace Framework
{
    public abstract class BaseComponent : IPoolObject
    {
        public abstract int ID { get; }
        
        public abstract bool IsDefaultUpdate { get; }
        
        public Entity Entity { get; set; } = null;

        public int UpdateIndex { get; set; } = -1;
        
        public abstract UniTask Initialize();

        public abstract void Update(float dt);

        public abstract void OnRelease();
    }
}