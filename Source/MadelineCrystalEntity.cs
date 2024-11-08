using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
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

            Audio.Play("event:/kyfexuwu/MadelineCrystal/to_crystal");
            this.sprite.Play("form");

            if (shatterColor == Color.Transparent) { try {
                int colorInt;
                int.TryParse(this.sprite.Animations["burstColor"].Goto.choices[0].Value, System.Globalization.NumberStyles.HexNumber, null, out colorInt);
                shatterColor = new Color(colorInt >> 16, colorInt >> 8 & 0x00ff, colorInt & 0x0000ff);
            } catch (Exception) {} }
        }

        private IEnumerator removeAnim; 
        public override void Update() {
            if (this.Level.Transitioning&&!this.dead) {
                this.Position = this.containing.Position;
                return;
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
                this.containing.Die(-Vector2.UnitX * Math.Abs(this.Speed.X));
                Audio.Play("event:/char/madeline/death", Position);
                AllowPushing = false;
                reset();
            }
        }
    }
}
