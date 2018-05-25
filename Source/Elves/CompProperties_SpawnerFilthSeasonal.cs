using RimWorld;
using Verse;

namespace Elves
{
    public class CompProperties_SpawnerFilthSeasonal : CompProperties
    {
        public CompProperties_SpawnerFilthSeasonal()
        {
            this.compClass = typeof(CompSpawnerFilthSeasonal);
        }

        public ThingDef filthDef;

        public int spawnCountOnSpawn = 5;

        public float spawnMtbHours = 12f;

        public float spawnRadius = 3f;

        public float spawnEveryDays = -1f;

        public RotStage? requiredRotStage;
        public Season? requiredSeason;
    }
}
