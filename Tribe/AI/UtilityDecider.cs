using System;
using System.Collections.Generic;
using System.Linq;

namespace Tribe
{

    // Map need weights through functions in the dictionary. Not really necessary now because of attenuator function.
    // Keep this for the future...
    // TODO: Polish the necessary methods in here, but keep the function ones because they may be useful in the future.
    // TODO: Rework this to work with generics. Especially WeightedRandomChoice & WeightChoices.
    // WeightedRandomChoice needs to be usable on any List<Tuple<T, double>>, where T may or may not be the same as UtilityDecider's T.

    //List<Tuple<T, double>> orderedNeeds = GetNeedPairs(needs).OrderByDescending(keyResultPair => keyResultPair.Item2).ToList();

    public class UtilityDecider<T>
    {

        public Dictionary<T, Func<double, double>> Functions { get; private set; }

        public UtilityDecider()
        {
            Functions = new Dictionary<T, Func<double, double>>();
        }

        public UtilityDecider(List<T> keys, List<Func<double, double>> values)
        {
            Functions = new Dictionary<T, Func<double, double>>();

            if (keys.Count != values.Count)
                throw new ArgumentException("Lists have to be the same length");

            for (int i = 0; i < keys.Count; i++)
            {
                Functions.Add(keys[i], values[i]);
            }
        }

        public void AddResponseFunction(T key, Func<double, double> function)
        {
            if (!Functions.ContainsKey(key))
                Functions.Add(key, function);
        }

        private List<Tuple<T, double>> GetNeedPairs(List<Tuple<T, double>> needs)
        {
            return needs.Select(need => new Tuple<T, double>(need.Item1, Functions[need.Item1](need.Item2))).ToList();
        }

        public List<T> GetDescendingOrderedNeeds(List<Tuple<T, double>> needs)
        {
            // Will throw a key exception if the needs list contains an enum value that is not in Functions.
            // This might be for the best though. It is an exceptional occurrence.
            return GetNeedPairs(needs)
                .OrderByDescending(keyResultPair => keyResultPair.Item2)
                .Select(keyResultPair => keyResultPair.Item1)
                .ToList();
        }

        // Return a list containing the top three need tuples: (need type, weight / the total of the top three weights).
        private static List<Tuple<T, double>> WeightChoices<T>(List<Tuple<T, double>> needs)
        {
            // Determine the total x^2 value of all needs.
            double total = 0.0;
            for (int i = 0; i < needs.Count; i++)
            {
                total += needs[i].Item2 * needs[i].Item2; // Apply x^2 nonlinear function to give more weight to highest need.
            }

            // Flatten x^2 back so that they sum to 1. Keeping this as extensible as possible so this can work with n objects to select.
            return needs
                .Select(needPair => new Tuple<T, double>(needPair.Item1, needPair.Item2 * needPair.Item2 / total))
                .ToList();
        }

        /// <summary>
        /// Make a choice 
        /// </summary>
        /// <param name="weightedOptions"></param>
        /// <returns></returns>
        public static T WeightedRandomChoice<T>(List<Tuple<T, double>> weightedOptions)
        {

            weightedOptions = WeightChoices(weightedOptions);
            double randomDouble = Utilities.Rng.NextDouble();

            double[] weights = new double[weightedOptions.Count];
            weights[0] = weightedOptions[0].Item2;

            // [0.1, 0.2, 0.3, 0.4] 
            for (int i = 1; i < weights.Length; i++) // Set the weights to contain their adjusted thresholds.
            {
                weights[i] = weightedOptions[i].Item2 + weights[i - 1];
            }

            weights[weights.Length - 1] = 1.0;

            for (int i = 0; i < weights.Length; i++)
            {
                if (randomDouble <= weights[i])
                {
                    return weightedOptions[i].Item1;
                }
            }

            return weightedOptions.Last().Item1;
        }


        // Need to select action that gives the highest change to the lowest need values. 0.0 is max, 1.0 is min need.
        // Returns best action for now...
        public EntityAction GetBestAction(Entity entity, List<ActionUtility> actionAds)
        {

            double attenuator(double input)
            {
                return 5.0 / (input == 0.0 ? 0.000001 : input);
            }

            double attenuatorDelta(Tuple<Need, double> deltaPair)
            {
                double currentValue = entity.GetNeedValue(deltaPair.Item1);
                double futureValue = currentValue + deltaPair.Item2;
                return attenuator(currentValue) - attenuator(futureValue);
            }

            Tuple<ActionUtility, double> createWeightPairs(ActionUtility actionUtility)
            {
                double weight = actionUtility.UtilityDeltas.Sum(deltaPair => attenuatorDelta(deltaPair));
                Tuple<ActionUtility, double> weightedActionUtility = new Tuple<ActionUtility, double>(actionUtility, weight);
                return weightedActionUtility;
            }

            // Pair each ActionUtility from actionAds with its score from sumFunction. This applies the attenuation function f(x) = 10.0 / x
            //  to prioritize needs that are more important and favor actions that will reduce that need the most.
            List<Tuple<ActionUtility, double>> weightedActionAds = actionAds
                .Select(actionAd => createWeightPairs(actionAd))
                .OrderByDescending(weightPair => weightPair.Item2) // Sort action advertisements by their weight value. Highest first.
                .Take(3) // Take the top 3 from the list.
                .ToList();

            EntityAction action = WeightedRandomChoice(weightedActionAds).Action;
            return action;

            // Assign a value to each action based on the increase in utility. Weight values nonlinearly based on the urgency of the need
            // List.Sum(attenuator(currentNeed) - attenuator(futureNeed))
        }
    }
}
