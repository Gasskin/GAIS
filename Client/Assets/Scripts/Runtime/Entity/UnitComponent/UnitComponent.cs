using cfg.battle;
using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    [ComponentID(ComponentID.UNIT)]
    public class UnitComponent : BaseComponent
    {
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
            AssetsRef.gameObject.SetActive(true);
            await UniTask.Yield();
        }

        public override void Update(float dt)
        {
        }

        public override void OnRelease()
        {
            AssetsRef = null;
        }

        public void Skill(EGameSkillType type)
        {
            switch (type)
            {
                case EGameSkillType.Attack:
                    AssetsRef.spine.AnimationState.SetAnimation(0, ATTACK, false);
                    break;
                default:
                    AssetsRef.spine.AnimationState.SetAnimation(0, SKILL, false);
                    break;
            }
            AssetsRef.spine.AnimationState.AddAnimation(0, STAND, true, 0);
        }
    }
}