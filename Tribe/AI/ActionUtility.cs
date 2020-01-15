using System;

namespace Tribe
{

    /// <summary>
    /// Groups actions together with their need deltas. The values stored in UtilityDeltas will be used to reduce each Entities needs.
    /// </summary>
    public class ActionUtility
    {

        public readonly EntityAction Action;
        public readonly Tuple<Need, double>[] UtilityDeltas;

        public ActionUtility(EntityAction action, Tuple<Need, double>[] values)
        {
            Action = action;
            UtilityDeltas = values;

            action.SetUtilityDeltas(UtilityDeltas);
        }

        public static Tuple<Need, double> NewPair(Need need, double delta) => new Tuple<Need, double>(need, delta);

        public override string ToString() => Action.GetName();

    }
}
