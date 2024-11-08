using Celeste.Mod.Entities;
using IL.Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
