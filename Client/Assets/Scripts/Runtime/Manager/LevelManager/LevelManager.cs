using System.Collections.Generic;
using cfg.battle;
using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public class LevelManager : BaseManager
    {
        public int Level { get; private set; } = 2001;
        
        private Dictionary<EGameAttr, float> _levelAttr;

        public Dictionary<EGameAttr, float> LevelAttr
        {
            get
            {
                if (_levelAttr == null)
                {
                    var row = GameEntry.Instance.LubanManager.Tables.GameAttrInitTable.DataMap.GetValueOrDefault(Level);
                    if (row != null)
                    {
                        _levelAttr = row.InitValues;
                    }
                }
                return _levelAttr;
            }
        }

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
            _levelAttr = null;
        }
    }
}