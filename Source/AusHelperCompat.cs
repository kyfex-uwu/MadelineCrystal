using System.Collections.Generic;
using Celeste.Mod.auspicioushelper.iop;
using Monocle;

namespace Celeste.Mod.MadelineCrystal;

public class AusHelperCompat {
    public static void Load() {
        ChannelIop.registerIopFunc("MCrysHelper_getCrystal", getCrystal);
        ChannelIop.registerIopFunc("MCrysHelper_setCrystal", setCrystal);
    }

    public static void Unload() {
        ChannelIop.deregisterIopFunc("MCrysHelper_getCrystal", getCrystal);
        ChannelIop.deregisterIopFunc("MCrysHelper_setCrystal", setCrystal);
    }

    //returns number of crystals 
    private static int getCrystal(List<string> strs, List<int> ints) {
        if (Engine.Scene is Level) {
            return MadelineCrystalEntity.playerFromCrystal.Keys.Count;
        }
        
        return 0;
    }
    //1 on success
    private static int setCrystal(List<string> strs, List<int> ints) {
        if (Engine.Scene is Level level && strs.Count>=1) {
            var player = level.Tracker.GetEntity<Player>();
            var isCrystal = MadelineCrystalEntity.crystalFromPlayer.Keys.Contains(player);
            MadelineCrystalModule.SetCrystalState(strs[0]);
            if (strs[0] == "swap") return 1;
            return (isCrystal == (strs[0] == "none"))?1:0;
        }

        return 0;
    }
}