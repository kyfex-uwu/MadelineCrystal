using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.ComponentModel;

namespace Celeste.Mod.MadelineCrystal {
    public class MadelineCrystalEntity : TheoCrystal {
        public static bool isCrystal {  get; private set; }
        public static void reset() {
            if (instance != null) {
                instance.Level.Remove(instance);
                instance.containing.Visible = true;
                instance.containing.StateMachine.State = 0;
                instance.containing.Speed = instance.Speed;
                instance.containing.Collidable = true;
                instance.containing.dashRefillCooldownTimer = 0;
                instance.containing.ForceCameraUpdate = false;
            }
            instance = null;
            isCrystal = false;
        }
        public readonly Player containing;
        public static MadelineCrystalEntity instance { get; private set; }
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
        }

        public override void Update() {
            if (this.Level.Transitioning) {
                this.Position = this.containing.Position;
                return;
            }
            base.Update();
            this.containing.Position = this.Position;
            this.Level.EnforceBounds(this.containing);
            this.Position = this.containing.Position;
        }

        public void overrideDie() {
            if (!dead) {
                dead = true;
                this.containing.Die(-Vector2.UnitX * Math.Abs(this.Speed.X));
                this.containing.Visible = true;
                this.containing.StateMachine.State = 0;
                Audio.Play("event:/char/madeline/death", Position);
                sprite.Visible = false;
                base.Depth = -1000000;
                AllowPushing = false;
            }
        }
    }
}
