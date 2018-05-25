using RimWorld;
using Verse;

namespace Elves
{
    public class CompSpawnerFilthSeasonal : ThingComp
    {
        private CompProperties_SpawnerFilthSeasonal Props
        {
            get
            {
                return (CompProperties_SpawnerFilthSeasonal)this.props;
            }
        }

        private bool CanSpawnFilth
        {
            get
            {
                Hive hive = this.parent as Hive;
                if (hive != null && !hive.active)
                {
                    return false;
                }
                RotStage? requiredRotStage = this.Props.requiredRotStage;
                Season? requiredSeason = this.Props.requiredSeason;
                Map map = this.parent.MapHeld;
                if (map == null) return false;
                float latitude = Find.WorldGrid.LongLatOf(map.Tile).y;
                float longitude = Find.WorldGrid.LongLatOf(map.Tile).x;
                var meetsRotReqs = requiredRotStage == null || !(this.parent.GetRotStage() != this.Props.requiredRotStage);
                var meetsSeasonReqs = requiredSeason == null || GenDate.Season(Find.TickManager.TicksAbs, latitude, longitude) == requiredSeason;
                return meetsRotReqs && meetsSeasonReqs;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.nextSpawnTimestamp, "nextSpawnTimestamp", -1, false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                for (int i = 0; i < this.Props.spawnCountOnSpawn; i++)
                {
                    this.TrySpawnFilth();
                }
            }
        }

        public override void CompTickRare()
        {
            if (this.CanSpawnFilth)
            {
                if (this.Props.spawnMtbHours > 0f && Rand.MTBEventOccurs(this.Props.spawnMtbHours, 2500f, 250f))
                {
                    this.TrySpawnFilth();
                }
                if (this.Props.spawnEveryDays >= 0f && Find.TickManager.TicksGame >= this.nextSpawnTimestamp)
                {
                    if (this.nextSpawnTimestamp != -1)
                    {
                        this.TrySpawnFilth();
                    }
                    this.nextSpawnTimestamp = Find.TickManager.TicksGame + (int)(this.Props.spawnEveryDays * 60000f);
                }
            }
        }

        public void TrySpawnFilth()
        {
            if (this.parent.Map == null)
            {
                return;
            }
            IntVec3 c;
            if (!CellFinder.TryFindRandomReachableCellNear(this.parent.Position, this.parent.Map, this.Props.spawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => x.Standable(this.parent.Map), (Region x) => true, out c, 999999))
            {
                return;
            }
            FilthMaker.MakeFilth(c, this.parent.Map, this.Props.filthDef, 1);
        }

        private int nextSpawnTimestamp = -1;
    }
}
