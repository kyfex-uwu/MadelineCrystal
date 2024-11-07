using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.MadelineCrystal {
    [Tracked(true), CustomEntity("MadelineCrystal/MCrystalSwitcher")]
    public class MCrystalSwitcher : Entity {
        public readonly bool toCrystal;
        public readonly bool fromCrystal;
        private bool Usable {
            get {
                return MadelineCrystalEntity.isCrystal ? this.fromCrystal : this.toCrystal;
            }
        }
        private bool playSounds=false;
        private float cooldownTimer=0f;
        private Sprite sprite;

        public MCrystalSwitcher(EntityData data, Vector2 offset) : this(data.Position+offset, data.Bool("toCrystal"), data.Bool("fromCrystal")) {}
        public MCrystalSwitcher(Vector2 position, bool toCrystal, bool fromCrystal) : base(position) {
            this.toCrystal = toCrystal;
            this.fromCrystal = fromCrystal;
            base.Collider = new Hitbox(16f, 24f, -8f, -12f);
            base.Depth = 2000;

            this.Add(new PlayerCollider(this.OnCollide));
            this.Add(new HoldableCollider((Action<Holdable>)this.OnCollide));
            this.Add(new CrystalSwapListener(this.OnSwap));
            this.Add(sprite = GFX.SpriteBank.Create("MadelineCrystal.MCrystalSwitcher"));
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            UpdateSprite(false);
        }

        public void UpdateSprite(bool animate) {
            if (animate) {
                if (playSounds) {
                    Audio.Play(MadelineCrystalEntity.isCrystal ? "event:/kyfexuwu/MadelineCrystal/switch_to_crystal" : "event:/kyfexuwu/MadelineCrystal/switch_from_crystal", this.Position);
                }

                if (Usable) {
                    sprite.Play(MadelineCrystalEntity.isCrystal ? "toCrystal" : "toNormal");
                } else {
                    if (playSounds) {
                        Audio.Play("event:/game/09_core/switch_dies", this.Position);
                    }

                    sprite.Play(MadelineCrystalEntity.isCrystal ? "toCrystalOff" : "toNormalOff");
                }
            } else if (Usable) {
                sprite.Play(MadelineCrystalEntity.isCrystal ? "crystalLoop" : "normalLoop");
            } else {
                sprite.Play(MadelineCrystalEntity.isCrystal ? "crystalOffLoop" : "normalOffLoop");
            }

            playSounds = false;
        }

        public void OnCollide(object doesntMatter) {
            if (Usable && cooldownTimer <= 0f) {
                this.playSounds = true;
                var player = this.SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player == null) return;//todo
                setCrystal(player, !MadelineCrystalEntity.isCrystal);

                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                player.level.Flash(Color.White * 0.15f, drawPlayerOver: true);
                Celeste.Freeze(0.05f);
                this.cooldownTimer = 1f;
            }
        }
        public void OnSwap() {
            UpdateSprite(true);
        }

        public static void setCrystal(Player player, bool inCrystal) {
            if (inCrystal == MadelineCrystalEntity.isCrystal) return;
            if (player.level == null) return;//this fixes a bug but it makes me uncomfy

            if (MadelineCrystalEntity.isCrystal) {
                MadelineCrystalEntity.reset();
            } else {
                player.level.Add(new MadelineCrystalEntity(player.Position + player.Collider.BottomCenter, player));
            }
            foreach (CrystalSwapListener listener in player.level.Tracker.GetComponents<CrystalSwapListener>()) {
                listener.OnSwap();
            }
        }

        public override void Update() {
            base.Update();
            if (cooldownTimer > 0f) {
                cooldownTimer -= Engine.DeltaTime;
            }
        }
    }
}
