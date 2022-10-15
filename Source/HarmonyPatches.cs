using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Elves
{
    [StaticConstructorOnStartup]
    public static class HarmonyFactions
    {
        static HarmonyFactions()
        {
            var harmony = new Harmony("rimworld.lotr.elves");
            harmony.Patch(AccessTools.Method(typeof(TileFinder), "RandomSettlementTileFor"),
                new HarmonyMethod(typeof(HarmonyFactions).GetMethod("RandomSettlementTileFor_PreFix")));
        }

        public static bool RandomSettlementTileFor_PreFix(ref int __result, Faction faction)
        {
            if (faction?.def?.defName == "LotRE_ElfFactionSea" ||
                faction?.def?.defName == "LotRE_ElfFactionWood" ||
                faction?.def?.defName == "LotRE_ElfFactionHigh")
            {
                __result = RandomSettlementTileFor_Elves(faction);
                return false;
            }
            return true;
        }

        public static float GetElfColonyChance(Tile tile, int tileID, Faction faction)
        {
            //High Elves should appear near rivers
            if (faction?.def?.defName == "LotRE_ElfFactionHigh")
            {
                if (tile.Rivers is { Count: > 0 })
                {
                    return 1000f;
                }
            }

            //Sea Elves should appear near the ocean
            var neighbors = new List<int>();
            Find.WorldGrid.GetTileNeighbors(tileID, neighbors);
            if (neighbors.Count <= 0)
            {
                return tile.biome.settlementSelectionWeight;
            }
            foreach (var y in neighbors)
            {
                var tile2 = Find.WorldGrid[y];
                if (tile2.biome == BiomeDefOf.IceSheet || tile2.biome == BiomeDef.Named("SeaIce"))
                {
                    return 0f;
                }

                if (tile2.WaterCovered)
                {
                    return 1000f;
                }
            }

            //Wood elves should appear in the Mallorn Forest
            if (faction?.def?.defName == "LotRE_ElfFactionWood")
            {
                if (tile.biome == BiomeDef.Named("LotRE_MallornForest"))
                {
                    return 1000f;
                }
            }

            return 0f;
        }

        public static int RandomSettlementTileFor_Elves(
            Faction faction,
            bool mustBeAutoChoosable = false,
            Predicate<int> extraValidator = null
        )
        {

            for (var i = 0; i < 500; i++)
            {
                if (!(from _ in Enumerable.Range(0, 100)
                      select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate (int x)
                  {
                      var tile = Find.WorldGrid[x];
                      if (!tile.biome.canBuildBase || 
                          !tile.biome.implemented  || 
                           tile.hilliness == Hilliness.Impassable)
                      {
                          return 0f;
                      }
                      if (mustBeAutoChoosable && 
                         !tile.biome.canAutoChoose)
                      {
                          return 0f;
                      }
                      if (extraValidator != null &&
                         !extraValidator(x))
                      {
                          return 0f;
                      }
                      if (GetElfColonyChance(tile, x, faction) is float elfColonyChance && 
                          elfColonyChance > 0f)
                      {
                          return elfColonyChance;
                      }

                      float num2 = tile.biome.settlementSelectionWeight;
                      Faction faction2 = faction;
                      if (((faction2 != null) ? faction2.def.minSettlementTemperatureChanceCurve : null) != null)
                      {
                          num2 *= faction.def.minSettlementTemperatureChanceCurve.Evaluate(GenTemperature.MinTemperatureAtTile(x));
                      }
                      return num2;
                  }, out var num))
                {
                    continue;
                }

                if (TileFinder.IsValidTileForNewSettlement(num))
                {
                    return num;
                }
            }

            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }
    }
}