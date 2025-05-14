using Monocle;
using System;

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
