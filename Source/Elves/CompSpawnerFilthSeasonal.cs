using RimWorld;
using Verse;

namespace Elves
{
    public class CompSpawnerFilthSeasonal : ThingComp
    {
        private CompProperties_SpawnerFilthSeasonal Props => (CompProperties_SpawnerFilthSeasonal)props;

        private bool CanSpawnFilth
        {
            get
            {
                if (parent is Hive hive && !hive.CompDormant.Awake)
                {
                    return false;
                }
                RotStage? requiredRotStage = Props.requiredRotStage;
                Season? requiredSeason = Props.requiredSeason;
                Map map = parent.MapHeld;
                if (map == null)
                {
                    return false;
                }

                var latitude = Find.WorldGrid.LongLatOf(map.Tile).y;
                var longitude = Find.WorldGrid.LongLatOf(map.Tile).x;
                var meetsRotReqs = requiredRotStage == null || !(parent.GetRotStage() != Props.requiredRotStage);
                var meetsSeasonReqs = requiredSeason == null || GenDate.Season(Find.TickManager.TicksAbs, latitude, longitude) == requiredSeason;
                return meetsRotReqs && meetsSeasonReqs;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref nextSpawnTimestamp, "nextSpawnTimestamp", -1, false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                for (var i = 0; i < Props.spawnCountOnSpawn; i++)
                {
                    TrySpawnFilth();
                }
            }
        }

        public override void CompTickRare()
        {
            if (CanSpawnFilth)
            {
                if (Props.spawnMtbHours > 0f && Rand.MTBEventOccurs(Props.spawnMtbHours, 2500f, 250f))
                {
                    TrySpawnFilth();
                }
                if (Props.spawnEveryDays >= 0f && Find.TickManager.TicksGame >= nextSpawnTimestamp)
                {
                    if (nextSpawnTimestamp != -1)
                    {
                        TrySpawnFilth();
                    }
                    nextSpawnTimestamp = Find.TickManager.TicksGame + (int)(Props.spawnEveryDays * 60000f);
                }
            }
        }

        public void TrySpawnFilth()
        {
            if (parent.Map == null)
            {
                return;
            }
            if (!CellFinder.TryFindRandomReachableCellNear(parent.Position, parent.Map, Props.spawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => x.Standable(parent.Map), (Region x) => true, out IntVec3 c, 999999))
            {
                return;
            }
            FilthMaker.TryMakeFilth(c, parent.Map, Props.filthDef, 1);
        }

        private int nextSpawnTimestamp = -1;
    }
}
