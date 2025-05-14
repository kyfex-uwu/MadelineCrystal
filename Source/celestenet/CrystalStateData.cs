using Celeste.Mod.CelesteNet.DataTypes;
using Celeste.Mod.CelesteNet;
using System;

namespace Celeste.Mod.MadelineCrystal {
    public class CrystalStateData : DataType<CrystalStateData> {
        static CrystalStateData() {
            DataID = "kyfexuwu_MadelineCrystalHelper_CrystalStateData";
        }

        public DataPlayerInfo Player;
        public bool isCrystal;
        public DateTime timestamp;

        public override MetaType[] GenerateMeta(DataContext ctx) => new MetaType[] {
                new MetaPlayerUpdate(Player)
            };

        public override void FixupMeta(DataContext ctx) {
            Player = Get<MetaPlayerUpdate>(ctx);
        }

        protected override void Read(CelesteNetBinaryReader reader) {
            isCrystal = reader.ReadBoolean();
            timestamp = reader.ReadDateTime();
        }

        protected override void Write(CelesteNetBinaryWriter writer) {
            writer.Write(isCrystal);
            writer.Write(timestamp);
        }

    }
}
