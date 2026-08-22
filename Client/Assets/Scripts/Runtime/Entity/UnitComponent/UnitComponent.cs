using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public class UnitComponent : BaseComponent
    {
        public override int ID => ComponentID.UNIT;
        public override bool IsDefaultUpdate => false;
        
        public UnitAssetsRef AssetsRef;
        
        public const string STAND = "stand";
        public const string ATTACK = "attack";
        public const string SKILL = "skill";
        public const string HURT = "hurt";
        public const string DEAD = "dead";
        
        
        public override async UniTask Initialize()
        {
            AssetsRef.spine.AnimationState.SetAnimation(0, STAND, true);
            AssetsRef.hpBar.Init(Entity);
            AssetsRef.actionBar.Init(Entity);
            await UniTask.Yield();
        }

        public override void Update(float dt)
        {
        }

        public override void OnRelease()
        {
            AssetsRef = null;
        }
    }
}