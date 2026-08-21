using Framework;

namespace cfg.battle
{
    partial class FloatCalculator
    {
        public override float Calculate(GameEffectSpec gameEffectSpec)
        {
            return Value.Get(gameEffectSpec.Source);
        }
    }
}
