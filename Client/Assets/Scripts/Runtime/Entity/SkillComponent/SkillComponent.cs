using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public class SkillComponent: BaseComponent
    {
        public override int ID => ComponentID.SKILL;
        public override bool IsDefaultUpdate => false;
        public override async UniTask Initialize()
        {
            await UniTask.Yield();
        }

        public override void Update(float dt)
        {
        }

        public override void OnRelease()
        {
        }
    }
}
