using Celeste.Mod.CelesteNet.Client;

namespace Celeste.Mod.MadelineCrystal;

public class MiscStuff {
    
    public static void realSCR(bool hehe) {
        CelesteNetClientModule.Instance.Client?.Send(new CrystalStateData {
            isCrystal = hehe
        });
    }

    public static bool clientConnected() {
        return CelesteNetClientModule.Instance?.Client?.Con != null;
    }
}