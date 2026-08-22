using Framework;

namespace Runtime
{
    public class GameEffectSpecRef: IPoolObject
    {
        public GameEffectSpec GameEffectSpec;
        
        public void OnRelease()
        {
            GameEffectSpec = null;
        }
    }
}
