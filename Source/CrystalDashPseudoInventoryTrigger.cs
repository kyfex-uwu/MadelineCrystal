using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MadelineCrystal {
    [CustomEntity("MadelineCrystal/CrystalInventoryTrigger")]
    public class CrystalDashPseudoInventoryTrigger : Trigger {
        private readonly bool shouldCrystal;
        public CrystalDashPseudoInventoryTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            this.shouldCrystal = data.Bool("shouldCrystal");
        }
        public override void OnEnter(Player player) {
            base.OnEnter(player);
            MadelineCrystalModule.Session.shouldAlwaysCrystalOnDash = this.shouldCrystal;
        }
    }
}
