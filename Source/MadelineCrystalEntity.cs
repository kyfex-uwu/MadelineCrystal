using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.MadelineCrystal {
    public class MadelineCrystalEntity : TheoCrystal {
        public static readonly string isCrystalFlag = "MadelineCrystalHelper/isCrystal";
        public static readonly Dictionary<MadelineCrystalEntity, Player> playerFromCrystal = new();
        public static readonly Dictionary<Player, MadelineCrystalEntity> crystalFromPlayer = new();

        public static void reset(Player player) {
            if(MadelineCrystalModule.isCrystal(player)) reset(crystalFromPlayer[player]);
        }
        public static void reset(MadelineCrystalEntity toReset) {
            if (toReset != null) {
                toReset.containing.Visible = true;
                toReset.containing.StateMachine.State = 0;
                toReset.containing.Speed = toReset.Speed;
                toReset.containing.Collidable = true;
                toReset.containing.dashRefillCooldownTimer = 0;
                toReset.containing.ForceCameraUpdate = false;
                Audio.Play("event:/kyfexuwu/MadelineCrystal/from_crystal");
                toReset.removeAnim = toReset.sprite.PlayRoutine("shatter");
                toReset.dead = true;
                playerFromCrystal.Remove(toReset);
                crystalFromPlayer.Remove(toReset.containing);
                if(playerFromCrystal.Keys.Count == 0)
                    toReset.Level.Session.SetFlag(isCrystalFlag, false);
            }
        }
        
        public readonly Player containing;
        private static Color shatterColor = Color.Transparent;
        public MadelineCrystalEntity(Vector2 position, Player containing) : base(position) {
            this.Remove(this.sprite);
            this.Add(this.sprite = GFX.SpriteBank.Create("MadelineCrystal.crystal"));
            this.AddTag(Tags.Persistent);
            playerFromCrystal[this] = containing;
            crystalFromPlayer[containing] = this;

            this.containing = containing;
            this.Speed = containing.Speed;
        }
        public override void Added(Scene scene) {
            base.Added(scene);
            this.containing.Visible = false;
            this.containing.Drop();
            this.containing.RefillDash();
            this.containing.RefillStamina();
            this.containing.StateMachine.State = 17;
            this.containing.Speed = Vector2.Zero;
            this.containing.Collidable = false;
            this.containing.ForceCameraUpdate = true;

            this.Level.Session.SetFlag(isCrystalFlag, true);
            SendCrystalUpdate(true);

            Audio.Play("event:/kyfexuwu/MadelineCrystal/to_crystal");
            this.sprite.Play("form");

            if (shatterColor == Color.Transparent) { try {
                int colorInt;
                int.TryParse(this.sprite.Animations["burstColor"].Goto.choices[0].Value, System.Globalization.NumberStyles.HexNumber, null, out colorInt);
                shatterColor = new Color(colorInt >> 16, colorInt >> 8 & 0x00ff, colorInt & 0x0000ff);
            } catch (Exception) {} }
        }

        private IEnumerator removeAnim;
        private bool transitioned = false;
        public override void Update() {
            if (this.Level.Transitioning&&!this.dead) {
                this.transitioned = true;
                this.Position = this.containing.Position;
                return;
            }
            if (this.transitioned) {
                this.transitioned = false;
                this.containing.StateMachine.State = 17;
                this.containing.Speed = Vector2.Zero;
            }
            base.Update();
            if (this.Speed.X != 0)
                this.containing.Facing = this.Speed.X < 0 ? Facings.Left : Facings.Right;
            if (this.removeAnim != null && !this.removeAnim.MoveNext()) {
                CrystalDebris.Burst(Position, shatterColor, false, 16);
                this.RemoveSelf();
            }
            if (this.dead) {
                this.Hold.cannotHoldTimer = 621;
                return;
            }
            this.containing.Position = this.Position;
            this.Level.EnforceBounds(this.containing);
            this.Position = this.containing.Position;
        }

        public void overrideDie() {
            if (!dead) {
                dead = true;
                Audio.Play("event:/char/madeline/death", Position);
                AllowPushing = false;
                reset(this);
                this.containing.Die(this.Speed);
            }
        }

        private static bool isCrystal(TheoCrystal self) {
            return self is MadelineCrystalEntity;
        }
        private static void allowTransitions(ILContext il) {
            var cursor = new ILCursor(il);

            ILLabel elseLabel=null;
            cursor.GotoNext(MoveType.Before,
                instr => instr.MatchCall<Entity>("set_Left"));
            cursor.GotoPrev(MoveType.After,
                instr => instr.MatchBgeUn(out elseLabel));
            cursor.EmitLdarg0();
            cursor.EmitDelegate(isCrystal);
            cursor.EmitBrtrue(elseLabel);
            
            cursor.GotoNext(MoveType.Before,
                instr => instr.MatchCall<Entity>("set_Top"));
            cursor.GotoPrev(MoveType.After,
                instr => instr.MatchBgeUn(out elseLabel));
            cursor.EmitLdarg0();
            cursor.EmitDelegate(isCrystal);
            cursor.EmitBrtrue(elseLabel);

            //why does this exist?
            cursor.GotoNext(instr => instr.MatchLdcR4(32f));
            cursor.GotoPrev(MoveType.After,
                instr => instr.MatchBgeUn(out elseLabel));
            cursor.EmitLdarg0();
            cursor.EmitDelegate(isCrystal);
            cursor.EmitBrtrue(elseLabel);
        }
        public static void enableHooks() {
            IL.Celeste.TheoCrystal.Update += allowTransitions;
        }
        public static void disableHooks() {
            IL.Celeste.TheoCrystal.Update -= allowTransitions;
        }

        //--

        private static void SendCrystalUpdate(bool isCrystal) {
            if (MadelineCrystalModule.hasCelesteNet) MiscStuff.realSCR(isCrystal);
        }
    }
}
