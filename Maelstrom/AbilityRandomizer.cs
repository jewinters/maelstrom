using Sleepey.FF8Mod.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.Maelstrom
{
    class AbilityRandomizer
    {
        private Random random;
        private Kernel kernel;
        private Init init;

        public AbilityRandomizer(Random random, Kernel kernel, Init init)
        {
            this.random = random;
            this.kernel = kernel;
            this.init = init;
        }

        public void SwapSets()
        {
            var unmatchedGFs = Enumerable.Range(0, 16).ToList();

            IList<JunctionableGF> cleanJunctionableGFs = kernel.JunctionableGFs.Select(junctionableGF => junctionableGF.Clone()).ToList();
            IList<InitGF> cleanInitGFs = init.GFs.Select(initGF => initGF.Clone()).ToList();

            for (int i = 0; i < 16; i++)
            {
                var matchedGF = unmatchedGFs[random.Next(unmatchedGFs.Count)];
                unmatchedGFs.Remove(matchedGF);

                init.GFs[i].Abilities = cleanInitGFs[matchedGF].Abilities;
                init.GFs[i].CurrentAbility = cleanInitGFs[matchedGF].CurrentAbility;
                kernel.JunctionableGFs[i].Abilities = cleanJunctionableGFs[matchedGF].Abilities;
            }
        }

        public void GenerateRandomSets(List<AbilityMeta> abilities, List<AbilityMeta> guaranteedAbilities, List<AbilityMeta> uniqueAbilities)
        {
            for (int gfId = 0; gfId < 16; gfId++)
            {
                // clear auto-unlocked abilities
                init.GFs[gfId].Abilities = new BitArray(init.GFs[gfId].Abilities.Length, false);

                List<int> unusedAbilities = abilities.Select(a => a.AbilityID).ToList();

                for (int learnSlotIndex = 0; learnSlotIndex < 21; learnSlotIndex++)
                {
                    if (learnSlotIndex < guaranteedAbilities.Count)
                    {
                        AddGuaranteedAbilities(gfId, unusedAbilities, guaranteedAbilities);

                        learnSlotIndex = guaranteedAbilities.Count - 1;
                    }
                    else
                    {
                        AddRandomAbility(gfId, learnSlotIndex, unusedAbilities, abilities, uniqueAbilities);
                    }
                }

                // sort abilities
                kernel.JunctionableGFs[gfId].Abilities = kernel.JunctionableGFs[gfId].Abilities.OrderBy(a => a.Ability == 0 ? byte.MaxValue : a.Ability).ToList();

                // clear ability being learned
                init.GFs[gfId].CurrentAbility = 0;
            }
        }

        private void AddGuaranteedAbilities(int gfId, List<int> unusedAbilities, List<AbilityMeta> guaranteedAbilities)
        {
            for (int index = 0; index < guaranteedAbilities.Count; index++)
            {
                AddAbility(gfId, index, guaranteedAbilities[index].AbilityID, true);

                unusedAbilities.Remove(guaranteedAbilities[index].AbilityID);
            }
        }

        private void AddRandomAbility(int gfId, int index, List<int> unusedAbilities, List<AbilityMeta> abilities, List<AbilityMeta> uniqueAbilities)
        {
            var ability = (byte)unusedAbilities[random.Next(unusedAbilities.Count)];

            AddAbility(gfId, index, ability);
            
            unusedAbilities.Remove(ability);

            if (uniqueAbilities.FindIndex(a => a.AbilityID == ability) >= 0)
            {
                abilities.RemoveAll(a => a.AbilityID == ability);
            }
        }

        private void AddAbility(int gfId, int abilityIndex, int abilityId, bool learned = false)
        {
            kernel.JunctionableGFs[gfId].Abilities[abilityIndex] = new GFAbility(1, 255, (byte) abilityId, 0);
            init.GFs[gfId].Abilities[abilityId] = learned || (abilityId >= 20 && abilityId <= 23);
        }

        public void ModifyAPCosts()
        {
            // reduce "empty" cost
            kernel.Abilities[24].APCost = 60;

            // increase "ribbon" cost
            kernel.Abilities[77].APCost = 250;
        }

        public void RandomizeAPCosts()
        {
            for (int ability = 0; ability < kernel.Abilities.Count; ability++)
            {
                int apCost = 10 * random.Next(1, 25);
                kernel.Abilities[ability].APCost = (byte)apCost;
            }
        }

    }
}
