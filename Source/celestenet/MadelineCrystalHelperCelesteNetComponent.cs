using Celeste.Mod.CelesteNet.Client;
using Celeste.Mod.CelesteNet;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MadelineCrystal {
    public class MadelineCrystalHelperCelesteNetComponent : CelesteNetGameComponent {

        public MadelineCrystalHelperCelesteNetComponent(CelesteNetClientContext context, Game game) : base(context, game) {
            Visible = false;
        }

        public void Handle(CelesteNetConnection con, CrystalStateData data) {
            Level level = Engine.Scene as Level;
            if (data.isCrystal) {
                //data.Player.
            } else {

            }
        }
    }
}
