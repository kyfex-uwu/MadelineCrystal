using Microsoft.Xna.Framework;
using Monocle;
using System;
using MonoMod.Utils;

namespace Celeste.Mod.MadelineCrystal;

public class MadelineCrystalModule : EverestModule {
    public static MadelineCrystalModule Instance { get; private set; }

    // public override Type SettingsType => typeof(MadelineCrystalModuleSettings);
    // public static MadelineCrystalModuleSettings Settings => (MadelineCrystalModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(MadelineCrystalModuleSession);
    public static MadelineCrystalModuleSession Session => (MadelineCrystalModuleSession) Instance._Session;
    //
    // public override Type SaveDataType => typeof(MadelineCrystalModuleSaveData);
    // public static MadelineCrystalModuleSaveData SaveData => (MadelineCrystalModuleSaveData) Instance._SaveData;

    public MadelineCrystalModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel("MadelineCrystal", LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel("MadelineCrystal", LogLevel.Info);
#endif
    }

    public static bool isCrystal(Player player) {
        return MadelineCrystalEntity.crystalFromPlayer.TryGetValue(player, out var _);
    }

    private static PlayerDeadBody resetCrystal(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
        if (!evenIfInvincible && isCrystal(self)) return null;

        CrystalRefill.shouldCrystalOnDash.Remove(self);
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }

    private static void onPlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
        CrystalRefill.shouldCrystalOnDash.Clear();
        orig(self, scene);
    }
    private static bool disableIfCrystal(On.Celeste.PlayerCollider.orig_Check orig, PlayerCollider self, Player player) {
        if (isCrystal(player)) return false;
        return orig(self, player);
    }
    private static void mCrystalDie(On.Celeste.TheoCrystal.orig_Die orig, TheoCrystal self) {
        if(self is MadelineCrystalEntity mCrystal) {
            mCrystal.overrideDie();
        } else {
            orig(self);
        }
    }

    [Command("crystalstate", "Puts the player into a crystal, or takes the player out of it. Modes: swap, crystal, none")]
    private static void SetCrystalState(string mode = "swap") {
        var scene = Celeste.Instance.scene;
        if (scene is not Level level) return;
        var player = level.Tracker.GetEntity<Player>();
        if (player != null){
            switch (mode) {
                case "swap":
                    Engine.Commands.Log($"Swapping {(isCrystal(player) ? "from" : "to")} crystal");
                    MCrystalSwitcher.setCrystal(player, !isCrystal(player));
                    return;
                case "crystal":
                    Engine.Commands.Log("Switching to crystal");
                    MCrystalSwitcher.setCrystal(player, true);
                    return;
                case "none":
                    Engine.Commands.Log("Switching from crystal");
                    MCrystalSwitcher.setCrystal(player, false);
                    return;
                default:
                    Engine.Commands.Log("Modes: swap, crystal, none");
                    return;
            }
        }
        Engine.Commands.Log("No Player found");
    }


    public override void Load() {
        On.Celeste.Player.Added += onPlayerAdded;
        On.Celeste.Player.Die += resetCrystal;

        On.Celeste.PlayerCollider.Check += disableIfCrystal;

        On.Celeste.TheoCrystal.Die += mCrystalDie;

        CrystalRefill.enableHooks();
        MadelineCrystalEntity.enableHooks();
    }

    public override void Unload() {
        On.Celeste.Player.Added -= onPlayerAdded;
        On.Celeste.Player.Die -= resetCrystal;

        On.Celeste.PlayerCollider.Check -= disableIfCrystal;

        On.Celeste.TheoCrystal.Die -= mCrystalDie;

        CrystalRefill.disableHooks();
        MadelineCrystalEntity.disableHooks();
    }

    //hi brokemia helper
    private static EverestModuleMetadata celesteNetDependency = new EverestModuleMetadata { Name = "CelesteNet.Client", Version = new Version(2, 4, 1) };
    private static EverestModuleMetadata pandorasDependency = new EverestModuleMetadata { Name = "PandorasBox", Version = new Version(1, 0, 49) };
    public static readonly bool hasCelesteNet = Everest.Loader.DependencyLoaded(celesteNetDependency);
    public static readonly bool hasPandoras = Everest.Loader.DependencyLoaded(pandorasDependency);
    public static bool CelesteNetConnected() {
        return hasCelesteNet && MiscStuff.clientConnected();
    }
}