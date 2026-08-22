using cfg.battle;
using Framework;

namespace Runtime
{
    public class TimelineEffect : IPoolObject
    {
        public float Delay;
        public GameEffectRow GameEffect;
        public GAISComponent Target;

        public void OnRelease()
        {
            Delay = 0;
            GameEffect = null;
            Target = null;
        }
    }
}