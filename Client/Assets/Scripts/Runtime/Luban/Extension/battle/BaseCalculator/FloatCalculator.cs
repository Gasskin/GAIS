using Runtime;

namespace cfg.battle
{
    partial class FloatCalculator
    {
        public override float Calculate(GameEffectSpec gameEffectSpec)
        {
            return MultStackCount ? Value * gameEffectSpec.StackCount : Value;
        }
    }
}