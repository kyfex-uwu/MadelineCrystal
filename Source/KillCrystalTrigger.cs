using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.MadelineCrystal {
    [CustomEntity("MadelineCrystal/KillIfCrystalTrigger")]
    public class KillIfCrystalTrigger : Trigger{
        public KillIfCrystalTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        }
        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (MadelineCrystalEntity.isCrystal) MadelineCrystalEntity.instance.Die();
        }
    }
}
