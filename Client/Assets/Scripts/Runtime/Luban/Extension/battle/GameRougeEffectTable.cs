using System;
using System.Collections.Generic;
using System.Linq;

namespace cfg.battle
{
    partial class GameRougeEffectTable
    {
        public List<GameRougeEffectRow> BaseGameRougeEffects = new();
        public List<GameRougeEffectRow> GameRougeEffects = new();
        
        public void Init()
        {
            BaseGameRougeEffects.Clear();
            GameRougeEffects.Clear();
            for (int i = 0; i < _dataList.Count; i++)
            {
                var d = _dataList[i];
                if (d.Quality == EGameRougeEffectQuality.Quality0)
                {
                    BaseGameRougeEffects.Add(d);
                }
                else
                {
                    GameRougeEffects.Add(d);
                }
            }
        }

        public void RandomBase(List<GameRougeEffectRow> result)
        {
            result.Clear();
            
            if (BaseGameRougeEffects.Count == 0)
            {
                return;
            }

            var count = Math.Min(3, BaseGameRougeEffects.Count);
            var pool = BaseGameRougeEffects.ToList();

            for (int i = 0; i < count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, pool.Count);
                (pool[i], pool[randomIndex]) = (pool[randomIndex], pool[i]);
            }
            
            result.AddRange(pool.GetRange(0, count));
        }
    }
}
