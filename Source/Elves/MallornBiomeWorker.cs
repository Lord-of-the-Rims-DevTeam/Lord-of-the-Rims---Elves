using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Elves
{
    public class MallornBiomeWorker : BiomeWorker
    {
        public override float GetScore(Tile tile, int tileID)
        {
            var minRainfall = 610.0;
            var minTemperature = -10.0;
            var chance = .007;
            if (tile.WaterCovered)
            {
                return -100f;
            }
            if (tile.temperature < minTemperature)
            {
                return 0f;
            }
            if (tile.rainfall < minRainfall)
            {
                return 0f;
            }
            if (Rand.Value > chance)
            {
                return 0f;
            }
            return (float)(16.0 + (tile.temperature - 7.0) + (tile.rainfall - minRainfall) / 180.0);
        }
    }
}