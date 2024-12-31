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
        public static bool shouldCrystalOnDash { get; private set; } = false;
        public static void reset() { shouldCrystalOnDash = false;  }
        private static void onPlayer(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player) {
            if(!(self is CrystalRefill)) {
                orig(self, player);
                return;
            }

            if (!MadelineCrystalEntity.isCrystal&&!shouldCrystalOnDash) {
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
                if (shouldCrystalOnDash || MadelineCrystalModule.Session.shouldAlwaysCrystalOnDash) {
                    MCrystalSwitcher.setCrystal(self, true);
                    reset();
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
