using Framework;

namespace Runtime
{
    public class GameEffectSpecRef : IPoolObject
    {
        public GameEffectSpec GameEffectSpec { get; set; }
        
        public void OnRelease()
        {
            GameEffectSpec = null;
        }
    }
}