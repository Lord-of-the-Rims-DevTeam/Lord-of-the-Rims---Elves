using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Elves
{
    [StaticConstructorOnStartup]
    public static class HarmonyFactions
    {
        static HarmonyFactions()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.lotr.elves");

            harmony.Patch(AccessTools.Method(typeof(TileFinder), "RandomSettlementTileFor"), new HarmonyMethod(typeof(HarmonyFactions).GetMethod("RandomSettlementTileFor_PreFix")), null);
            
        }

        public static bool RandomSettlementTileFor_PreFix(ref int __result, Faction faction)
        {
            //if (faction.def.defName == "TheAgency")
            //{
            //    __result = RandomSettlementTileFor_TheAgency(faction);
            //    return false;
            //}
            if (faction?.def?.defName == "LotRE_ElfFactionSea")
            {
                __result = RandomSettlementTileFor_SeaElves(faction);
                return false;
            }
            if (faction?.def?.defName == "LotRE_ElfFactionWood")
            {
                __result = RandomSettlementTileFor_WoodElves(faction);
                return false;
            }
            if (faction?.def?.defName == "LotRE_ElfFactionHigh")
            {
                __result = RandomSettlementTileFor_HighElves(faction);
                return false;
            }
            return true;
        }
        

        public static int RandomSettlementTileFor_HighElves(Faction faction, bool mustBeAutoChoosable = false)
        {
            for (int i = 0; i < 500; i++)
            {
                int num;
                if ((from _ in Enumerable.Range(0, 100)
                    select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate (int x)
                {
                    Tile tile = Find.WorldGrid[x];
                    if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                    {
                        return 0f;
                    }
                    if (tile.rivers != null && tile.rivers.Count > 0)
                        return 1000f;
                    return 0f; //tile.biome.factionBaseSelectionWeight;
                }, out num))
                {
                    if (TileFinder.IsValidTileForNewSettlement(num, null))
                    {
                        return num;
                    }
                }
            }
            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }
        

        public static int RandomSettlementTileFor_WoodElves(Faction faction, bool mustBeAutoChoosable = false)
        {
            for (int i = 0; i < 500; i++)
            {
                int num;
                if ((from _ in Enumerable.Range(0, 100)
                     select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate (int x)
                     {
                         Tile tile = Find.WorldGrid[x];
                         if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                         {
                             return 0f;
                         }
                         if (tile.biome == BiomeDef.Named("LotRE_MallornForest"))
                             return 1000f;
                         return 0f; //tile.biome.factionBaseSelectionWeight;
                     }, out num))
                    {
                    if (TileFinder.IsValidTileForNewSettlement(num, null))
                    {
                        return num;
                    }
                }
            }
            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }
        
        public static int RandomSettlementTileFor_SeaElves(Faction faction, bool mustBeAutoChoosable = false)
        {
            for (int i = 0; i < 500; i++)
            {
                int num;
                if ((from _ in Enumerable.Range(0, 100)
                     select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate (int x)
                     {
                         Tile tile = Find.WorldGrid[x];
                         if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                         {
                             return 0f;
                         }
                         List<int> neighbors = new List<int>();
                         Find.WorldGrid.GetTileNeighbors(x, neighbors);
                         //Log.Message("Neighbors " + neighbors.Count.ToString());
                         if (neighbors != null && neighbors.Count > 0)
                         {
                             foreach (int y in neighbors)
                             {
                                 Tile tile2 = Find.WorldGrid[y];
                                 if (tile2.biome == BiomeDefOf.IceSheet || tile2.biome == BiomeDef.Named("SeaIce"))
                                     return 0f;
                                 if (tile2.WaterCovered)
                                     return 1000f;
                             }
                         }

                         return tile.biome.factionBaseSelectionWeight;
                     }, out num))
                {
                    if (TileFinder.IsValidTileForNewSettlement(num, null))
                    {
                        return num;
                    }
                }
            }
            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }
        
    }
}
