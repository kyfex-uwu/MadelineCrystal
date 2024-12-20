using Celeste;
using Celeste.Mod.CelesteNet;
using Celeste.Mod.CelesteNet.Client;
using Celeste.Mod.CelesteNet.DataTypes;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;

namespace Celeste.Mod.MadelineCrystal {
    public class MadelineCrystalEntity : TheoCrystal {
        public static bool isCrystal {  get; private set; }//todo: make this a flag also
        public static readonly string isCrystalFlag = "MadelineCrystalHelper/isCrystal";
        public static void reset() {
            if (instance != null) {
                instance.containing.Visible = true;
                instance.containing.StateMachine.State = 0;
                instance.containing.Speed = instance.Speed;
                instance.containing.Collidable = true;
                instance.containing.dashRefillCooldownTimer = 0;
                instance.containing.ForceCameraUpdate = false;
                Audio.Play("event:/kyfexuwu/MadelineCrystal/from_crystal");
                instance.removeAnim = instance.sprite.PlayRoutine("shatter");
                instance.dead = true;
                instance.Level.Session.SetFlag(isCrystalFlag, false);
            }
            instance = null;
            isCrystal = false;

            SendCrystalUpdate(false);
        }
        public readonly Player containing;
        public static MadelineCrystalEntity instance { get; private set; }
        private static Color shatterColor = Color.Transparent;
        public MadelineCrystalEntity(Vector2 position, Player containing) : base(position) {
            this.Remove(this.sprite);
            this.Add(this.sprite = GFX.SpriteBank.Create("MadelineCrystal.crystal"));
            this.AddTag(Tags.Persistent);

            isCrystal = true;
            this.containing = containing;
            instance = this;

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
                reset();
                this.containing.Die(this.Speed);
            }
        }

        private static float AddMCrystalCheck(float f1, float f2, TheoCrystal self) {
            return (f1 < (f2+4) && !(self is MadelineCrystalEntity)) ? -1 : 1;//why +4? i think i am not doing what i think i am doing. skill issue
        }
        private static void allowUpTransition(ILContext il) {
            var cursor = new ILCursor(il);

            /*
            IL_03d9: ldarg.0
	        IL_03da: call instance float32 Monocle.Entity::get_Top()
	        IL_03df: ldarg.0
	        IL_03e0: ldfld class Celeste.Level Celeste.TheoCrystal::Level
	        IL_03e5: callvirt instance valuetype [FNA]Microsoft.Xna.Framework.Rectangle Celeste.Level::get_Bounds()
	        IL_03ea: stloc.s 4
	        IL_03ec: ldloca.s 4
	        IL_03ee: call instance int32 [FNA]Microsoft.Xna.Framework.Rectangle::get_Top()
	        IL_03f3: ldc.i4.4
	        IL_03f4: sub
	        IL_03f5: conv.r4
            <---
	        IL_03f6: bge.un.s IL_042a
             */

            Func<Instruction, bool>[] matches = new Func<Instruction, bool>[] {
                instr=>instr.MatchCall<Entity>("get_Top"),
                instr=>true,
                instr=>instr.MatchLdfld<TheoCrystal>("Level"),
                instr=>instr.MatchCallvirt<Level>("get_Bounds"),
                instr=>true,
                instr=>true,
                instr=>instr.MatchCall<Rectangle>("get_Top"),
                instr=>true,
                instr=>true,
                instr=>true
                // <-- if all these checks pass, the cursor is right after this last instruction (conv.r4)
            };
            cursor.GotoNext(MoveType.Before, instr => {//even though this is before, this bit of code will return true on the first instruction AFTER this section
                var currInstr = instr;
                foreach(var _ in matches) {
                    if (currInstr.Previous != null)
                        currInstr = currInstr.Previous;
                    else return false;
                }

                foreach(var match in matches) {
                    if (!match.Invoke(currInstr)) return false;
                    currInstr = currInstr.Next;
                }
                return true;
            });
            cursor.EmitLdarg0();
            cursor.EmitDelegate(AddMCrystalCheck);
            cursor.EmitLdcR4(0);
        }
        public static void enableHooks() {
            IL.Celeste.TheoCrystal.Update += allowUpTransition;
        }
        public static void disableHooks() {
            IL.Celeste.TheoCrystal.Update -= allowUpTransition;
        }

        //--

        private static void SendCrystalUpdate(bool isCrystal) {
            if (MadelineCrystalModule.hasCelesteNet) realSCR(isCrystal);
        }
        private static void realSCR(bool hehe) {
            CelesteNetClientModule.Instance.Client?.Send(new CrystalStateData {
                isCrystal = isCrystal
            });
        }
    }
}
