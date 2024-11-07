using Celeste.Mod.Entities;
using IL.Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
