using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MadelineCrystal;

[CustomEntity("MadelineCrystal/GiveCrystalDashesTrigger")]
public class GiveCrystalDashes : Trigger {
    private bool oneTime;
    private int amt;
    private SetCrystalTrigger.Mode mode;
    private Mode setMode;
    enum Mode {
        ADD,
        SET
    }
    public GiveCrystalDashes(EntityData data, Vector2 offset) : base(data, offset) {
        this.oneTime = data.Bool("oneTime", true);
        this.amt = data.Int("amt", 1);
        this.setMode = data.Enum("setMode", Mode.ADD);
        this.mode = data.Enum("mode", SetCrystalTrigger.Mode.ENTER);
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        if(this.mode == SetCrystalTrigger.Mode.ENTER) giveDashes(player);
        if(this.oneTime) this.RemoveSelf();
    }
    public override void OnLeave(Player player) {
        base.OnLeave(player);
        if(this.mode == SetCrystalTrigger.Mode.LEAVE) giveDashes(player);
        if(this.oneTime) this.RemoveSelf();
    }

    private void giveDashes(Player player) {
        switch (this.setMode) {
            case Mode.ADD:
                CrystalRefill.setCrystalOnDash(player, CrystalRefill.getCrystalDashes(player)+this.amt);
                break;
            case Mode.SET:
                CrystalRefill.setCrystalOnDash(player, this.amt);
                Logger.Error("MadelineCrystal",CrystalRefill.getCrystalDashes(player)+"");
                break;
        }
    }
}