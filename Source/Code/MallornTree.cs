using RimWorld;
using Verse;

namespace Elves
{
    public class MallornTree : Plant
    {
        private const int spawnEveryDays = 1;
        private readonly GraphicData mallornGreen;

        private bool firstTick;
        private bool isSummer;
        private int nextSpawnTimestamp = -1;

        private GraphicData MallornGreen => def.building.fullGraveGraphicData;
        private GraphicData ImmatureMallornGreen => def.building.trapUnarmedGraphicData;


        public override Graphic Graphic
        {
            get
            {
                if (!isSummer)
                {
                    return base.Graphic;
                }

                if (def.plant.immatureGraphic != null && !HarvestableNow)
                {
                    return ImmatureMallornGreen.Graphic;
                }

                return MallornGreen.Graphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            CheckSeason();
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Tick()
        {
            if (!firstTick)
            {
                firstTick = true;
                CheckSeason();
            }

            base.Tick();
        }

        public override void PostMapInit()
        {
            base.PostMapInit();
            CheckSeason();
        }

        public override void TickLong()
        {
            CheckSeason();
            base.TickLong();
        }

        private void CheckSeason()
        {
            if (MapHeld == null)
            {
                return;
            }

            var isSpring = GenLocalDate.Season(MapHeld) == Season.Spring;
            isSummer = GenLocalDate.Season(MapHeld) == Season.Summer;
            if (!isSpring)
            {
                return;
            }

            if (Find.TickManager.TicksGame < nextSpawnTimestamp)
            {
                return;
            }

            if (nextSpawnTimestamp != -1)
            {
                TrySpawnFilth();
            }

            nextSpawnTimestamp = Find.TickManager.TicksGame + (int) (spawnEveryDays * Rand.Range(10000f, 60000f));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextSpawnTimestamp, "nextSpawnTimestamp", -1);
        }

        public void TrySpawnFilth()
        {
            if (this.Map == null)
            {
                return;
            }
            IntVec3 c;
            if (!CellFinder.TryFindRandomReachableCellNear(this.Position, this.Map, 3, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false), (IntVec3 x) => x.Standable(this.Map), (Region x) => true, out c, 999999))
            {
                return;
            }
            FilthMaker.TryMakeFilth(c, this.Map, ThingDef.Named("LotRE_FilthMallornLeaves"), this.def.label, 1, FilthSourceFlags.Terrain);
        }
    }
}