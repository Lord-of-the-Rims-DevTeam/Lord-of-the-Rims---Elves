using Verse;

namespace Elves
{
    public class HediffGiver_GiveRacialTraits : HediffGiver
    {
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            HealthUtility.AdjustSeverity(pawn, this.hediff, 1f);
        }
    }
}
