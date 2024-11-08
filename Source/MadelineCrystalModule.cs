using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MadelineCrystal;

public class MadelineCrystalModule : EverestModule {
    public static MadelineCrystalModule Instance { get; private set; }

    public override Type SettingsType => typeof(MadelineCrystalModuleSettings);
    public static MadelineCrystalModuleSettings Settings => (MadelineCrystalModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(MadelineCrystalModuleSession);
    public static MadelineCrystalModuleSession Session => (MadelineCrystalModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(MadelineCrystalModuleSaveData);
    public static MadelineCrystalModuleSaveData SaveData => (MadelineCrystalModuleSaveData) Instance._SaveData;

    public MadelineCrystalModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(MadelineCrystalModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(MadelineCrystalModule), LogLevel.Info);
#endif
    }

    private static PlayerDeadBody resetCrystal(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
        if (!evenIfInvincible && MadelineCrystalEntity.instance != null && self == MadelineCrystalEntity.instance.containing) return null;

        MadelineCrystalEntity.reset();
        CrystalRefill.reset();
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }
    private static void resetCrystal2(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
        MadelineCrystalEntity.reset();
        CrystalRefill.reset();
        orig(self, scene);
    }
    private static bool disableIfCrystal(On.Celeste.PlayerCollider.orig_Check orig, PlayerCollider self, Player player) {
        if (MadelineCrystalEntity.instance != null) return false;
        return orig(self, player);
    }
    private static void mCrystalDie(On.Celeste.TheoCrystal.orig_Die orig, TheoCrystal self) {
        if(self is MadelineCrystalEntity mCrystal) {
            mCrystal.overrideDie();
        } else {
            orig(self);
        }
    }

    public override void Load() {
        On.Celeste.Player.Added += resetCrystal2;
        On.Celeste.Player.Die += resetCrystal;

        On.Celeste.PlayerCollider.Check += disableIfCrystal;

        On.Celeste.TheoCrystal.Die += mCrystalDie;

        CrystalRefill.enableHooks();
    }

    public override void Unload() {
        On.Celeste.Player.Added -= resetCrystal2;
        On.Celeste.Player.Die -= resetCrystal;

        On.Celeste.PlayerCollider.Check -= disableIfCrystal;

        On.Celeste.TheoCrystal.Die -= mCrystalDie;

        CrystalRefill.disableHooks();
    }
}