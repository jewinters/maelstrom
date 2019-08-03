﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MiniMog
{
    class MonsterInfo
    {
        public string Name;
        public byte Hp1, Hp2, Hp3, Hp4;
        public byte Str1, Str2, Str3, Str4;
        public byte Mag1, Mag2, Mag3, Mag4;
        public byte Vit1, Vit2, Vit3, Vit4;
        public byte Spr1, Spr2, Spr3, Spr4;
        public byte Spd1, Spd2, Spd3, Spd4;
        public byte Eva1, Eva2, Eva3, Eva4;

        public MonsterInfo()
        {
            this.Name = "Unnamed";
        }

        public MonsterInfo(string name) : this()
        {
            this.Name = name;
        }

        public MonsterInfo(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                Name = FF8String.Decode(reader.ReadBytes(24));

                Hp1 = reader.ReadByte();
                Hp2 = reader.ReadByte();
                Hp3 = reader.ReadByte();
                Hp4 = reader.ReadByte();

                Str1 = reader.ReadByte();
                Str2 = reader.ReadByte();
                Str3 = reader.ReadByte();
                Str4 = reader.ReadByte();

                Vit1 = reader.ReadByte();
                Vit2 = reader.ReadByte();
                Vit3 = reader.ReadByte();
                Vit4 = reader.ReadByte();

                Mag1 = reader.ReadByte();
                Mag2 = reader.ReadByte();
                Mag3 = reader.ReadByte();
                Mag4 = reader.ReadByte();

                Spr1 = reader.ReadByte();
                Spr2 = reader.ReadByte();
                Spr3 = reader.ReadByte();
                Spr4 = reader.ReadByte();

                Spd1 = reader.ReadByte();
                Spd2 = reader.ReadByte();
                Spd3 = reader.ReadByte();
                Spd4 = reader.ReadByte();

                Eva1 = reader.ReadByte();
                Eva2 = reader.ReadByte();
                Eva3 = reader.ReadByte();
                Eva4 = reader.ReadByte();
            }
        }

        // hp2 & hp4 determine the lower boundary
        // mainly hp4 (thousands) with hp2 (tens) for fine tuning
        // hp1 & hp3 determine growth rate
        // hp1 sets the curve & hp3 adds a linear increase
        // there's no easy way to explain this in the ui is there...
        public int HpAtLevel(int level)
        {
            return (Hp1 * level * level / 20) + (Hp1 + Hp3 * 100) * level + Hp2 * 10 + Hp4 * 1000;
        }

        private int OffensiveStatAtLevel(int level, byte stat1, byte stat2, byte stat3, byte stat4)
        {
            return level * stat1 / 10 + level / stat2 - level * level / 2 / (stat4 + stat3) / 4;
        }

        private int NonOffensiveStatAtLevel(int level, byte stat1, byte stat2, byte stat3, byte stat4)
        {
            return level / stat2 - level / stat4 + level * stat1 + stat3;
        }

        public int StrAtLevel(int level)
        {
            return OffensiveStatAtLevel(level, Str1, Str2, Str3, Str4);
        }

        public int MagAtLevel(int level)
        {
            return OffensiveStatAtLevel(level, Mag1, Mag2, Mag3, Mag4);
        }

        public int VitAtLevel(int level)
        {
            return NonOffensiveStatAtLevel(level, Vit1, Vit2, Vit3, Vit4);
        }

        public int SprAtLevel(int level)
        {
            return NonOffensiveStatAtLevel(level, Spr1, Spr2, Spr3, Spr4);
        }

        public int SpdAtLevel(int level)
        {
            return NonOffensiveStatAtLevel(level, Spd1, Spd2, Spd3, Spd4);
        }

        public int EvaAtLevel(int level)
        {
            return NonOffensiveStatAtLevel(level, Eva1, Eva2, Eva3, Eva4);
        }
    }
}