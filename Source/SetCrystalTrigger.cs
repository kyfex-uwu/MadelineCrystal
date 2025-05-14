using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MadelineCrystal {
    [CustomEntity("MadelineCrystal/SetCrystalTrigger")]
    public class SetCrystalTrigger : Trigger {
        private readonly bool inCrystal;
        private readonly int mode;
        public SetCrystalTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            this.inCrystal = data.Bool("crystal");
            this.mode = data.Int("mode");
            /**modes:
             * 0 - on enter
             * 1 - on leave
             * 
             */
        }
        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (this.mode!=0) return;

            MCrystalSwitcher.setCrystal(player, this.inCrystal);
        }
        public override void OnLeave(Player player) {
            base.OnLeave(player);
            if (this.mode != 1) return;

            MCrystalSwitcher.setCrystal(player, this.inCrystal);
        }
    }
}
