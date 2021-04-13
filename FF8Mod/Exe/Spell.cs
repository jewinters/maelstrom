using System;

namespace Sleepey.FF8Mod.Exe
{
    public class Spell : IComparable<Spell>
    {
        public int SpellID { get; set; }
        public string SpellName { get; set; }
        public bool SlotExclusive { get; set; } = false;
        public bool CutContent { get; set; } = false;
        public int SpellLevel { get; set; }

        public int CompareTo(Spell otherSpell) => otherSpell == null ? 1 : SpellLevel.CompareTo(otherSpell.SpellLevel);
    }
}
