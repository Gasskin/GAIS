using cfg.battle;
using Framework;

namespace Runtime
{
    public class TimelineEffect : IPoolObject
    {
        public float Delay;
        public GameEffectRow GameEffect;
        public int TargetUid;

        public void OnRelease()
        {
            Delay = 0;
            GameEffect = null;
            TargetUid = 0;
        }
    }
}