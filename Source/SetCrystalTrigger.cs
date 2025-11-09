using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MadelineCrystal {
    [CustomEntity("MadelineCrystal/SetCrystalTrigger")]
    public class SetCrystalTrigger : Trigger {
        private readonly bool inCrystal;
        private readonly Mode mode;

        public enum Mode {
            ENTER,
            LEAVE
        }
        public SetCrystalTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            this.inCrystal = data.Bool("crystal");
            this.mode = data.Enum("mode", Mode.ENTER);
        }
        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (this.mode!=0) return;

            MCrystalSwitcher.setCrystal(player, this.inCrystal);
        }
        public override void OnLeave(Player player) {
            base.OnLeave(player);
            if (this.mode != Mode.LEAVE) return;

            MCrystalSwitcher.setCrystal(player, this.inCrystal);
        }
    }
}
