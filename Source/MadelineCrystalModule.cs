using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

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

        CrystalRefill.setCrystalOnDash(self,0);
        if(isCrystal(self)) MadelineCrystalEntity.reset(self);
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }

    private static void onPlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
        CrystalRefill.setCrystalOnDash(self,0);
        orig(self, scene);
    }

    private static void clearCrystalling(On.Celeste.Player.orig_IntroRespawnBegin orig, Player self) {
        orig(self);
        CrystalRefill.clearCrystalOnDash();
    }
    private static bool disableIfCrystal(On.Celeste.PlayerCollider.orig_Check orig, PlayerCollider self, Player player) {
        foreach(TheoCrystal e in player.Scene.Tracker.GetEntities<TheoCrystal>()) {
            if ((e is MadelineCrystalEntity m) && m.legacyBehavior) return false;
        }
        
        if (isCrystal(player) &&
            (!shouldReallyDisablePlayerCollision.TryGetValue(self.Entity.GetType(), out var func) ||
             func.Invoke(self))) return false;
        return orig(self, player);
    }
    private static void mCrystalDie(On.Celeste.TheoCrystal.orig_Die orig, TheoCrystal self) {
        if(self is MadelineCrystalEntity mCrystal) {
            mCrystal.overrideDie();
        } else {
            orig(self);
        }
    }

    public static readonly Dictionary<Holdable, Player> holdingHoldable = new();
    private static bool rememberHolder(On.Celeste.Holdable.orig_Pickup orig, Holdable self, Player player) {
        var toReturn = orig(self, player);
        if(toReturn) holdingHoldable[self] = player;
        return toReturn;
    }

    [Command("crystalstate", "Puts the player into a crystal, or takes the player out of it. Modes: swap, crystal, none")]
    public static void SetCrystalState(string mode = "swap") {
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

    public static Dictionary<Type, Func<PlayerCollider, bool>> shouldReallyDisablePlayerCollision = new();

    static MadelineCrystalModule() {
        shouldReallyDisablePlayerCollision[typeof(Strawberry)] = self => {
            if (!self.Entity.CollideCheck<BlockField>()) return true;
            return false;
        };
    }

    public override void Load() {
        On.Celeste.Player.Added += onPlayerAdded;
        On.Celeste.Player.IntroRespawnBegin += clearCrystalling;
        On.Celeste.Player.Die += resetCrystal;
        
        On.Celeste.PlayerCollider.Check += disableIfCrystal;

        On.Celeste.TheoCrystal.Die += mCrystalDie;

        On.Celeste.Holdable.Pickup += rememberHolder;

        CrystalRefill.enableHooks();
        MadelineCrystalEntity.enableHooks();

        // Everest.Events.AssetReload.OnAfterReload += MadelineCrystalEntity.onReload;
        
        if(hasAuspicious) AusHelperCompat.Load();
    }

    public override void Unload() {
        On.Celeste.Player.Added -= onPlayerAdded;
        On.Celeste.Player.Die -= resetCrystal;

        On.Celeste.PlayerCollider.Check -= disableIfCrystal;

        On.Celeste.TheoCrystal.Die -= mCrystalDie;
        
        On.Celeste.Holdable.Pickup -= rememberHolder;

        CrystalRefill.disableHooks();
        MadelineCrystalEntity.disableHooks();
        
        if(hasAuspicious) AusHelperCompat.Unload();
    }

    //hi brokemia helper
    private static EverestModuleMetadata celesteNetDependency = new EverestModuleMetadata { Name = "CelesteNet.Client", Version = new Version(2, 4, 1) };
    private static EverestModuleMetadata pandorasDependency = new EverestModuleMetadata { Name = "PandorasBox", Version = new Version(1, 0, 49) };
    private static EverestModuleMetadata auspiciousDependency = new EverestModuleMetadata { Name = "auspicioushelper", Version = new Version(0,2,11) };
    public static readonly bool hasCelesteNet = Everest.Loader.DependencyLoaded(celesteNetDependency);
    public static readonly bool hasPandoras = Everest.Loader.DependencyLoaded(pandorasDependency);
    public static readonly bool hasAuspicious = Everest.Loader.DependencyLoaded(auspiciousDependency);
    public static bool CelesteNetConnected() {
        return hasCelesteNet && MiscStuff.clientConnected();
    }
}