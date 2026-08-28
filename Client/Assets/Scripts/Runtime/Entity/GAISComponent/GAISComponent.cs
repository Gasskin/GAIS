using System;
using System.Collections.Generic;
using cfg.battle;
using Cysharp.Threading.Tasks;
using Framework;
using UnityEngine;

namespace Runtime
{
    [ComponentID(ComponentID.GAIS)]
    public partial class GAISComponent : BaseComponent
    {
        public override bool IsDefaultUpdate => true;
        public Dictionary<EGameAttr, float> InitValues { get; set; }
        public List<EGameTag> InitTags { get; set; }

        public float BattleTime { get; private set; }

        public override async UniTask Initialize()
        {
            InitGameAttr(InitValues);
            InitValues = null;
            InitTag(InitTags);
            InitTags = null;
            await UniTask.Yield();
        }

        public override void Update(float dt)
        {
            BattleTime += dt;
            TickGameEffect(dt);
        }

        public override void OnRelease()
        {
            ClearGameEffectSpecs();
            ClearGameAttr();
            ClearGameTag();
        }
    }
}