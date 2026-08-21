using Cysharp.Threading.Tasks;

namespace Framework
{
    public abstract class BaseManager
    {
        public abstract UniTask Initialize();
        public abstract void Destroy();
    }
}
