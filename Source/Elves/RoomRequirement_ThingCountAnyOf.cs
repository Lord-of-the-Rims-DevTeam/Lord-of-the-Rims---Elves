using System.Collections.Generic;
using Verse;

namespace Elves
{
    public class RoomRequirement_ThingCountAnyOf : RimWorld.RoomRequirement_ThingAnyOf
    {
        public override bool Met(Room r, Pawn p = null)
        {
            return Count(r) >= count;
        }

        public int Count(Room r)
        {
            var thingCount = 0;
            foreach (ThingDef def in things)
            {
                thingCount += r.ThingCount(def);
            }
            return thingCount;
        }

        public override string Label(Room r = null)
        {
            var useLabelKey = !labelKey.NullOrEmpty();
            string text = (useLabelKey ? labelKey : "LotRE_RoomRequirementTotal").Translate(count);
            // after the introductory label, print all required things (indented so they are grouped visually):
            foreach (ThingDef def in things)
            {
                text = string.Concat(new object[]
                {
                        text,
                        "\n    ",
                        def.label.CapitalizeFirst()
                });
                // if a room is defined, also add the number of items it already does have:
                if (r != null)
                {

                    text = string.Concat(new object[]
                    {
                        text,
                        " (",
                        r.ThingCount(def),
                        ")"
                    });
                }
            }
            return text;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var text in base.ConfigErrors())
            {
                yield return text;
            }
            if (count <= 0)
            {
                yield return "count must be larger than 0";
            }
            yield break;
        }

        public int count;
    }
}
