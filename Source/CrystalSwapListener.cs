using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.MadelineCrystal {

    [Tracked(false)]
    internal class CrystalSwapListener : Component {
        public Action OnSwap;

        public CrystalSwapListener(Action onSwap)
            : base(active: false, visible: false) {
            OnSwap = onSwap;
        }
    }
}
