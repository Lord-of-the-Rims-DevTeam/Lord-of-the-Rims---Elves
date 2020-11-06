using RimWorld;
using Verse;

namespace Elves
{
    public class MallornTree : Plant
    {
        private readonly GraphicData mallornGreen;
        private bool isSummer;
        private int nextSpawnTimestamp = -1;
        private const int spawnEveryDays = 1;
        
        private GraphicData MallornGreen => def.building.fullGraveGraphicData;
        private GraphicData ImmatureMallornGreen => def.building.trapUnarmedGraphicData;


        public override Graphic Graphic
        {
            get
            {
                if (isSummer)
                {
                    if (def.plant.immatureGraphic != null && !HarvestableNow)
                    {
                        return ImmatureMallornGreen.Graphic;
                    }
                    return MallornGreen.Graphic;
                }
                else
                {
                    return base.Graphic;
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            CheckSeason();
            base.SpawnSetup(map, respawningAfterLoad);
        }

        private bool firstTick = false;
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
            if (MapHeld != null)
            {
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
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref nextSpawnTimestamp, "nextSpawnTimestamp", -1, false);
        }

        public void TrySpawnFilth()
        {
            if (Map == null)
            {
                return;
            }
            if (!CellFinder.TryFindRandomReachableCellNear(Position, Map, 3, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => x.Standable(Map), (Region x) => true, out IntVec3 c, 999999))
            {
                return;
            }
            FilthMaker.TryMakeFilth(c, Map, ThingDef.Named("LotRE_FilthMallornLeaves"), 1);
        }

    }
}