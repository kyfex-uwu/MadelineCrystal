using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MadelineCrystal {
    [Tracked(true), CustomEntity("MadelineCrystal/CrystalRefill")]
    public class CrystalRefill : Refill {
        public CrystalRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse")) {
        }
        public CrystalRefill(Vector2 position, bool oneUse) : base(position, false, oneUse) {

        }

        private static string setGraphics(Refill self, string defaultStr) {
            if (!(self is CrystalRefill)) return defaultStr;

            //TODO
            self.p_shatter = P_Shatter;
            self.p_regen = P_Regen;
            self.p_glow = P_Glow;
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
        public static bool shouldCrystalOnDash { get; private set; } = false;
        public static void reset() { shouldCrystalOnDash = false;  }
        private static void onPlayer(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player) {
            if(!(self is CrystalRefill)) {
                orig(self, player);
                return;
            }

            if (!MadelineCrystalEntity.isCrystal) {
                if (player.Dashes < player.MaxDashes) player.Dashes = player.MaxDashes;
                Audio.Play("event:/game/general/diamond_touch", self.Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                self.Collidable = false;
                self.Add(new Coroutine(self.RefillRoutine(player)));
                self.respawnTimer = 2.5f;
                shouldCrystalOnDash = true;
            }
        }

        private static void addCrystalDashListener(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);
            self.Add(new DashListener((Vector2) => {
                if (shouldCrystalOnDash) MCrystalSwitcher.setCrystal(self, true);
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
