using cfg.battle;
using Framework;

namespace Runtime
{
    public class GameAttrModifierCache : IPoolObject
    {
        public GameEffectSpec GameEffectSpec { get; set; }
        
        public GameAttrModifier Modifier { get; set; }
        
        public void OnRelease()
        {
            GameEffectSpec = null;
            Modifier = null;
        }
    }
}