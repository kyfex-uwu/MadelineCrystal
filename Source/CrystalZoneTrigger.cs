using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MadelineCrystal {
    [CustomEntity("MadelineCrystal/CrystalZoneTrigger")]
    public class CrystalZoneTrigger : Trigger {
        public CrystalZoneTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        }
        public override void OnEnter(Player player) {
            base.OnEnter(player);
            MCrystalSwitcher.setCrystal(player, true);
        }
        public override void OnLeave(Player player) {
            base.OnLeave(player);
            MCrystalSwitcher.setCrystal(player, false);
        }
    }
}
