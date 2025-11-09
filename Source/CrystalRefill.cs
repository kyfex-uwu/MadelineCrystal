using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System.Collections.Generic;

namespace Celeste.Mod.MadelineCrystal {
    [Tracked(true), CustomEntity("MadelineCrystal/CrystalRefill")]
    public class CrystalRefill : Refill {
        private readonly bool legacyMode = false;
        public CrystalRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse"), data.Bool("legacyMode")) {
        }

        public CrystalRefill(Vector2 position, bool oneUse, bool legacyMode) : base(position, false, oneUse) {
            this.legacyMode = legacyMode;
        }

        private static ParticleType particle(ParticleType blueprint, Color c1, Color c2) {
            return new ParticleType(blueprint) {
                Color = c1,
                Color2 = c2
            };
        }
        private static readonly ParticleType shatter = particle(P_Shatter, new Color(203, 219, 252), new Color(99, 155, 255));
        private static readonly ParticleType regen = particle(P_Regen, new Color(99, 155, 255), new Color(91, 110, 225));
        private static readonly ParticleType glow = particle(P_Glow, new Color(99,155,255), new Color(91,110,225));
        private static string setGraphics(Refill self, string defaultStr) {
            if (!(self is CrystalRefill)) return defaultStr;

            self.p_shatter = shatter;
            self.p_regen = regen;
            self.p_glow = glow;
            return "objects/MadelineCrystal/refill/";
        }
        private static void ctorCrystal(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, instr => instr.Previous != null && instr.Previous.MatchLdsfld<Refill>("P_Glow"));
            /*
    IL_0096: ldsfld class Monocle.ParticleType Celeste.Refill::P_Glow <---
    IL_009b: stfld class Monocle.ParticleType Celeste.Refill::p_glow
             */
            cursor.EmitLdarg0();
            cursor.EmitLdloc0();
            cursor.EmitDelegate(setGraphics);
            cursor.EmitStloc0();
        }

        private static readonly Dictionary<Player, int> shouldCrystalOnDash = new();
        private static readonly HashSet<Player> shouldCrystalOnDashLegacy = new();

        public static void setCrystalOnDash(Player player, int amt, bool legacy=false) {
            if (legacy) {
                shouldCrystalOnDash.Remove(player);
                if(amt <= 0) shouldCrystalOnDashLegacy.Remove(player);
                else shouldCrystalOnDashLegacy.Add(player);
            }else{
                shouldCrystalOnDashLegacy.Remove(player);
                if (amt <= 0) shouldCrystalOnDash.Remove(player);
                else shouldCrystalOnDash[player] = amt;
            }
        }

        public static int getCrystalDashes(Player player) {
            if (shouldCrystalOnDash.ContainsKey(player)) return shouldCrystalOnDash[player];
            return 0;
        }

        public static void clearCrystalOnDash() {
            shouldCrystalOnDash.Clear();
            shouldCrystalOnDashLegacy.Clear();
        }

        public enum ShouldCrystalOnDashVal {
            TRUE,
            LEGACY,
            FALSE
        }

        public static ShouldCrystalOnDashVal ShouldCrystalOnDash(Player player) {
            if (shouldCrystalOnDash.ContainsKey(player) && shouldCrystalOnDash[player]>0) return ShouldCrystalOnDashVal.TRUE;
            if (shouldCrystalOnDashLegacy.Contains(player)) return ShouldCrystalOnDashVal.LEGACY;
            return ShouldCrystalOnDashVal.FALSE;
        }
        private static void onPlayer(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player) {
            if(!(self is CrystalRefill cRefill)) {
                orig(self, player);
                return;
            }

            var targetedPlayer = cRefill.legacyMode ? self.Scene.Tracker.GetEntity<Player>() : player;
            if (ShouldCrystalOnDash(targetedPlayer) == ShouldCrystalOnDashVal.FALSE) {
                if (targetedPlayer.Dashes < targetedPlayer.MaxDashes) targetedPlayer.Dashes = targetedPlayer.MaxDashes;
                Audio.Play("event:/game/general/diamond_touch", self.Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                self.Collidable = false;
                self.Add(new Coroutine(self.RefillRoutine(targetedPlayer)));
                self.respawnTimer = 2.5f;

                setCrystalOnDash(targetedPlayer, 1, cRefill.legacyMode);
            }
        }

        private static void addCrystalDashListener(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);
            self.Add(new DashListener((vector) => {
                var should = ShouldCrystalOnDash(self);
                if (should!=ShouldCrystalOnDashVal.FALSE || MadelineCrystalModule.Session.shouldAlwaysCrystalOnDash) {
                    MCrystalSwitcher.setCrystal(self, true, should==ShouldCrystalOnDashVal.LEGACY);
                    setCrystalOnDash(self,getCrystalDashes(self)-1);
                }
            }));
        }

        public static void enableHooks() {
            IL.Celeste.Refill.ctor_Vector2_bool_bool += ctorCrystal;
            On.Celeste.Refill.OnPlayer += onPlayer;

            On.Celeste.Player.ctor += addCrystalDashListener;
        }

        public static void disableHooks() {
            IL.Celeste.Refill.ctor_Vector2_bool_bool -= ctorCrystal;
            On.Celeste.Refill.OnPlayer -= onPlayer;

            On.Celeste.Player.ctor -= addCrystalDashListener;
        }
    }
}
