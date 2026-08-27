using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public class LevelManager : BaseManager
    {
        public int Level { get; private set; } = 2001;
        
        public override async UniTask Initialize()
        {
            await UniTask.Yield();
        }

        public override void Destroy()
        {
        }
        
        public void AddLevel()
        {
            Level++;
        }
    }
}