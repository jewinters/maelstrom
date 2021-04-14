using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using Sleepey.FF8Mod.Main;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod;

namespace Sleepey.Maelstrom
{
    //This is a static class that is responsible for configuring and calling the AbilityRandomizer class and saving changes.
    //
    //All Ability-related randomizer settings are read here and are used to determine how to call the AbilityRandomizer class.
    public static class AbilityShuffle
    {
        public static List<AbilityMeta> Abilities = JsonSerializer.Deserialize<List<AbilityMeta>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Abilities.json"));
        public static List<GFMeta> GFNames = JsonSerializer.Deserialize<List<GFMeta>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.JunctionableGFs.json"));

        public static List<JunctionableGF> Randomise(FileSource mainSource, int seed, State settings)
        {
            var random = new Random(seed + 1);
            var kernel = new Kernel(mainSource.GetFile(Globals.KernelPath));
            var init = new Init(mainSource.GetFile(Globals.InitPath));

            AbilityRandomizer abilityRandomizer = new AbilityRandomizer(random, kernel, init);

            if (settings.GfAbilitiesSwapSets)
            {
                abilityRandomizer.SwapSets();
            }
            else
            {
                List<AbilityMeta> allFilteredAbilities = Abilities.Where(a => !settings.GfAbilitiesSpecific || settings.GfAbilities[a.AbilityID]).ToList();
                List<AbilityMeta> guaranteedAbilities = allFilteredAbilities.Where(a => a.BasicAbility && settings.GfAbilitiesBasics).ToList();
                List<AbilityMeta> uniqueAbilities = allFilteredAbilities.Where(a => a.MenuAbility && settings.GFAbilitiesNoMenuDuplicates).ToList();

                if ((allFilteredAbilities.Count - uniqueAbilities.Count) < 21)
                {
                    throw new ArgumentException("Not enough abilities selected. Skipping GF Ability randomization.");
                }

                abilityRandomizer.GenerateRandomSets(allFilteredAbilities, guaranteedAbilities, uniqueAbilities);
            }

            abilityRandomizer.ModifyAPCosts();

            if (settings.GfAbilitiesRandomAP)
            {
                abilityRandomizer.RandomizeAPCosts();
            }


            SaveChanges(mainSource, kernel, init);
            return kernel.JunctionableGFs.ToList();
        }

        private static void SaveChanges(FileSource mainSource, Kernel kernel, Init init)
        {
            mainSource.ReplaceFile(Globals.KernelPath, kernel.Encode());
            mainSource.ReplaceFile(Globals.InitPath, init.Encode());
        }
    }
}
