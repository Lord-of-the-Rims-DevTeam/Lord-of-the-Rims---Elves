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
            switch (faction?.def?.defName)
            {
                case "LotRE_ElfFactionSea":
                case "LotRE_ElfFactionWood":
                case "LotRE_ElfFactionHigh":
                {
                    //Try once
                    var tempPrimaryResult = RandomSettlementTileFor_Elves(faction, false, null, true);
                    if (tempPrimaryResult != 0) //If finding a settlement with primary criteria doesn't fail...
                    {
                        __result = tempPrimaryResult;
                        return false;
                    }

                    //Try twice
                    var tempSecondaryResult = RandomSettlementTileFor_Elves(faction, false, null, true);
                    if (tempSecondaryResult != 0) //If finding a settlement with secondary criteria doesn't fail...
                    {
                        __result = tempSecondaryResult;
                        return false;
                    }
                    //Otherwise, give up and choose any location
                    break;
                }
            }
            return true;
        }

        public static float GetElfColonyChance(Tile tile, int tileID, Faction faction, bool primary)
        {
            if (faction?.def?.defName == "LotRE_ElfFactionHigh")
            {
                //High Elves should appear near rivers
                if (primary)
                {
                    if (tile.Rivers is { Count: > 0 })
                    {
                        return 1000f;
                    }
                }
                //If no rivers, then try a temperate or boreal forest
                else
                {
                    if (tile.biome == BiomeDefOf.TemperateForest || 
                        tile.biome == BiomeDefOf.BorealForest)
                    {
                        return 1000f;
                    }
                }
            }

            if (faction?.def?.defName == "LotRE_ElfFactionSea")
            {
                if (primary)
                {
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
                            continue;
                        }

                        if (tile2.WaterCovered)
                        {
                            return 1000f;
                        }
                    }
                }
                else
                {
                    //The sea should never fail; however, if it does, try the rivers
                    if (tile.Rivers is { Count: > 0 })
                    {
                        return 1000f;
                    }
                }
            }

            if (faction?.def?.defName == "LotRE_ElfFactionWood")
            {
                //Wood elves should appear in the Mallorn Forest
                if (primary)
                {
                    if (tile.biome == BiomeDef.Named("LotRE_MallornForest"))
                    {
                        return 1000f;
                    }
                }
                //Otherwise, any forest would do
                else
                {
                    if (tile.biome == BiomeDefOf.TemperateForest || 
                        tile.biome == BiomeDefOf.TropicalRainforest ||
                        tile.biome == BiomeDefOf.BorealForest)
                    {
                        return 1000f;
                    }
                }
            }

            return 0f;
        }

        
        
        public static int RandomSettlementTileFor_Elves(
            Faction faction,
            bool mustBeAutoChoosable = false,
            Predicate<int> extraValidator = null,
            bool primary = true
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
                      
                      float elfColonyChance = GetElfColonyChance(tile, x, faction, primary);
                      return elfColonyChance;
                  }, out var num))
                {
                    continue;
                }

                if (TileFinder.IsValidTileForNewSettlement(num))
                {
                    return num;
                }
            }

            //Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }
    }
}