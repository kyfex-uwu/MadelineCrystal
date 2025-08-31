using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.MadelineCrystal {
    [Tracked(true), CustomEntity("MadelineCrystal/MCrystalSwitcher")]
    public class MCrystalSwitcher : Entity {
        public readonly bool toCrystal;
        public readonly bool fromCrystal;
        private bool Usable(Player player) {
            return MadelineCrystalModule.isCrystal(player) ? this.fromCrystal : this.toCrystal;
        }
        private bool playSounds=false;
        private float cooldownTimer=0f;
        private Sprite sprite;
        private readonly bool legacyMode;

        public MCrystalSwitcher(EntityData data, Vector2 offset) : this(data.Position+offset, data.Bool("toCrystal"), data.Bool("fromCrystal"), data.Bool("legacyMode")) {}
        public MCrystalSwitcher(Vector2 position, bool toCrystal, bool fromCrystal, bool legacyMode) : base(position) {
            this.legacyMode = legacyMode;
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
            this.needToUpdate = true;
            UpdateSprite(false);
        }

        private bool needToUpdate = false;
        public void UpdateSprite(bool animate) {
            var player = this.Scene.Tracker.GetEntity<Player>();
            if (player == null) return;
            
            var isCrystal = MadelineCrystalModule.isCrystal(player);
            if (animate) {
                if (playSounds) {
                    Audio.Play(isCrystal ? "event:/kyfexuwu/MadelineCrystal/switch_to_crystal" : "event:/kyfexuwu/MadelineCrystal/switch_from_crystal", this.Position);
                }

                if (Usable(player)) {
                    sprite.Play(isCrystal ? "toCrystal" : "toNormal");
                } else {
                    if (playSounds) {
                        Audio.Play("event:/game/09_core/switch_dies", this.Position);
                    }

                    sprite.Play(isCrystal ? "toCrystalOff" : "toNormalOff");
                }
            } else if (Usable(player)) {
                sprite.Play(isCrystal ? "crystalLoop" : "normalLoop");
            } else {
                sprite.Play(isCrystal ? "crystalOffLoop" : "normalOffLoop");
            }

            playSounds = false;
            needToUpdate = false;
        }

        public void OnCollide(object collided) {
            var player = this.SceneAs<Level>().Tracker.GetEntity<Player>();
            if (collided is Player p && !this.legacyMode) player = p;

            if (player == null) return;//todo
            
            if (Usable(player) && cooldownTimer <= 0f) {
                this.playSounds = true;
                setCrystal(player, !MadelineCrystalModule.isCrystal(player));

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
            if (inCrystal == MadelineCrystalModule.isCrystal(player)) return;
            if (player.level == null) return;//this fixes a bug but it makes me uncomfy

            if (MadelineCrystalModule.isCrystal(player)) {
                MadelineCrystalEntity.reset(player);
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
            if(needToUpdate) UpdateSprite(true);
        }
    }
}
