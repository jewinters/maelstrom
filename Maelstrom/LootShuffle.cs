using System;
using System.Collections.Generic;
using System.Linq;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Battle;
using Sleepey.FF8Mod.Exe;

namespace Sleepey.Maelstrom
{
    public class LootShuffle
    {
        FileSource battleSource;
        State settings;

        Random dropRandom;
        Random stealRandom;
        Random drawRandom;

        List<Spell> drawPool;
        List<int> mugPool;
        List<int> dropPool;

        public LootShuffle(FileSource battleSource, int seed, State settings)
        {
            this.battleSource = battleSource;
            dropRandom = new Random(seed + 5);
            stealRandom = new Random(seed + 6);
            drawRandom = new Random(seed + 7);
            this.settings = settings;

            drawPool = DrawPointShuffle.Spells
                .Where(spell => spell.SpellID != 20 || settings.LootDrawsApoc)
                .Where(spell => !spell.SlotExclusive || settings.LootDrawsSlot)
                .Where(spell => !spell.CutContent || settings.LootDrawsCut)
                .ToList();

            mugPool = Item.Lookup.Values
                .Where(item => !item.KeyItem || settings.LootStealsKeyItems)
                .Where(item => !item.SummonItem || settings.LootStealsSummonItems)
                .Where(item => !item.Magazine || settings.LootStealsMagazines)
                .Where(item => !item.ChocoboWorld || settings.LootStealsChocoboWorld)
                .Select(item => item.ID).ToList();

            dropPool = Item.Lookup.Values
                .Where(item => !item.KeyItem || settings.LootDropsKeyItems)
                .Where(item => !item.SummonItem || settings.LootDropsSummonItems)
                .Where(item => !item.Magazine || settings.LootDropsMagazines)
                .Where(item => !item.ChocoboWorld || settings.LootDropsChocoboWorld)
                .Select(item => item.ID).ToList();
        }

        public List<MonsterInfo> Randomise()
        {

            var result = new List<MonsterInfo>();

            for (int i = 0; i < 144; i++)
            {
                Monster monster;
                try
                {
                    monster = Monster.ByID(battleSource, i);

                    // items to steal
                    if (settings.LootSteals)
                    {
                        monster.Info.MugLow = FourRandomItems(stealRandom, mugPool);
                        monster.Info.MugMed = FourRandomItems(stealRandom, mugPool);
                        monster.Info.MugHigh = FourRandomItems(stealRandom, mugPool);
                    }

                    // items dropped
                    if (settings.LootDrops)
                    {
                        monster.Info.DropLow = FourRandomItems(dropRandom, dropPool);
                        monster.Info.DropMed = FourRandomItems(dropRandom, dropPool);
                        monster.Info.DropHigh = FourRandomItems(dropRandom, dropPool);
                    }

                    // spells to draw
                    if (settings.LootDraws)
                    {
                        var gf = monster.Info.DrawLow.Where(d => d >= 64).FirstOrDefault();
                        int slots = Math.Max(1, Math.Min(4, settings.LootDrawsAmount));

                        var selectedSpells = RandomOrderedSpells(slots * 3);
                        var lowSpells = selectedSpells.GetRange(0, slots).Select(spell => (byte)spell.SpellID).ToList();
                        var medSpells = selectedSpells.GetRange(slots, slots).Select(spell => (byte)spell.SpellID).ToList();
                        var highSpells = selectedSpells.GetRange(2 * slots, slots).Select(spell => (byte)spell.SpellID).ToList();

                        //If there's a GF, it takes the last spot
                        if (gf > 0)
                        {
                            lowSpells.Add(gf);
                            medSpells.Add(gf);
                            highSpells.Add(gf);
                        }

                        monster.Info.DrawLow = lowSpells.ToArray();
                        monster.Info.DrawMed = medSpells.ToArray();
                        monster.Info.DrawHigh = highSpells.ToArray();
                    }

                    battleSource.ReplaceFile(Monster.GetPath(i), monster.Encode());
                    result.Add(monster.Info);
                }
                catch { }
            }

            return result;
        }

        private HeldItem RandomItem(Random random, List<int> itemPool)
        {
            // allow item slot to be empty
            if (!itemPool.Contains(0)) itemPool.Add(0);

            var id = itemPool[random.Next(itemPool.Count)];
            var quantity = id == 0 ? 0 : random.Next(1, 9);
            return new HeldItem(id, quantity);
        }

        private HeldItem[] FourRandomItems(Random random, List<int> itemPool)
        {
            return new HeldItem[]
            {
                RandomItem(random, itemPool),
                RandomItem(random, itemPool),
                RandomItem(random, itemPool),
                RandomItem(random, itemPool)
            };
        }

        private Spell RandomSpell()
        {
            return drawPool[drawRandom.Next(drawPool.Count)];
        }

        private List<Spell> RandomOrderedSpells(int count)
        {
            var result = new List<Spell>();

            for (int i = 0; i < count; i++) 
            {
                Spell spell = RandomSpell();

                //no dupes
                while (result.Contains(spell)) spell = RandomSpell();
                
                result.Add(spell);
            }

            if (settings.LootDrawsSort) { 
                result.Sort();
            }

            return result;
        }
    }
}
