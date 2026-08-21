using System;
using System.Collections.Generic;
using cfg.battle;
using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public partial class GAISComponent : BaseComponent
    {
        public override int ID => ComponentID.GAIS;
        public override bool IsDefaultUpdate => true;
        public Dictionary<EGameAttr, float> InitValues { get; set; }


        public override async UniTask Initialize()
        {
            InitGameAttr(InitValues);
            InitValues = null;
            await UniTask.Yield();
        }

        public override void Update(float dt)
        {
        }

        public override void OnRelease()
        {
            ClearGameAttr();
        }

    

        public float GetTime()
        {
            return UnityEngine.Time.time;
        }


    }
}