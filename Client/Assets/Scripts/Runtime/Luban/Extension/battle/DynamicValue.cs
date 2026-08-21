using Runtime;

namespace cfg.battle
{
    partial class DynamicValue
    {
        public float Get(GAISComponent source)
        {
            if (source != null && DynamicAttr is >= EGameAttr.DynamicAttr01 and <= EGameAttr.DynamicAttr20)
            {
                var attr = source.GetAttr(DynamicAttr);
                if (attr != null)
                {
                    return attr.Current + BaseValue;
                }
            }
            return BaseValue;
        }
    }
}