using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MadelineCrystal {
    [CustomEntity("MadelineCrystal/KillIfCrystalTrigger")]
    public class KillIfCrystalTrigger : Trigger{
        public KillIfCrystalTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        }
        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (MadelineCrystalModule.isCrystal(player)) 
                MadelineCrystalEntity.crystalFromPlayer[player].Die();
        }
    }
}
