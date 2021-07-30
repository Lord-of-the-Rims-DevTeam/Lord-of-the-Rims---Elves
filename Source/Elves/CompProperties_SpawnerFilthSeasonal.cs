using RimWorld;
using Verse;

namespace Elves
{
    public class CompProperties_SpawnerFilthSeasonal : CompProperties
    {
        public ThingDef filthDef;

        public RotStage? requiredRotStage;
        public Season? requiredSeason;

        public int spawnCountOnSpawn = 5;

        public float spawnEveryDays = -1f;

        public float spawnMtbHours = 12f;

        public float spawnRadius = 3f;

        public CompProperties_SpawnerFilthSeasonal()
        {
            compClass = typeof(CompSpawnerFilthSeasonal);
        }
    }
}