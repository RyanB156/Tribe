using System;
using System.Collections.Generic;
using System.Linq;

namespace Tribe
{
    public class AnimalBrain
    {
        private UtilityDecider<Need> utilityDecider;

        public AnimalBrain()
        {
            utilityDecider = new UtilityDecider<Need>();

            utilityDecider.AddResponseFunction(Need.Health, x => x);
            utilityDecider.AddResponseFunction(Need.Hunger, x => x);
            utilityDecider.AddResponseFunction(Need.Social, x => x);
            utilityDecider.AddResponseFunction(Need.Lust, x => x);
            utilityDecider.AddResponseFunction(Need.Tiredness, x => x);
            utilityDecider.AddResponseFunction(Need.Boredom, x => x);
            utilityDecider.AddResponseFunction(Need.JobFullfilment, x => x);
        }

        public void DecideAction(Animal animal, List<GameObject> nearbyObjects)
        {

            List<ActionUtility> actionAds = nearbyObjects.
                Where(o => o != animal)
                .SelectMany(o => o.GetAdvertisedActions(animal, 0.0)).ToList();

            actionAds.Add(new ActionUtility(animal.IdleAction, new Tuple<Need, double>[]
            {
                ActionUtility.NewPair(Need.JobFullfilment, 0.002)
            }));

            EntityAction attackerResponseAction;

            // Add actions to respond to the attacker being nearby. 
            // Bears will always attack nearby entities, but wolves might not, and goats and hogs will not.
            if (animal.Attacker != null && nearbyObjects.Contains(animal.Attacker))
            {
                if (animal.Type == AnimalType.Bear)
                {
                    attackerResponseAction = new AttackAction(animal.Attacker);
                }
                // Wolves will fight back if it can win. I.e. it will take less ticks for the wolf to kill the attacker than for the
                //  attacker to kill the wolf.
                else if (animal.Type == AnimalType.Wolf && animal.GetHealth() / animal.Attacker.Strength > animal.GetHealth() / animal.Strength)
                {
                    attackerResponseAction = new AttackAction(animal.Attacker);
                }
                else
                {
                    attackerResponseAction = new FleeAction(animal.Attacker);
                }
                actionAds.Add(new ActionUtility(attackerResponseAction, new Tuple<Need, double>[]
                {
                    ActionUtility.NewPair(Need.JobFullfilment, DDeltaConfig.attackerResponseDelta)
                }));
            }

            EntityAction bestAction = utilityDecider.GetBestAction(animal, actionAds);

            if (!Utilities.AreSameBaseType(animal.Action, bestAction))
            {
                animal.TrySetAction(bestAction);
            }
        }
    }
}
