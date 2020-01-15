using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Tribe
{
    // Store action with its corresponding mod value. This will be multiplied to the need values from the ActionUtility class.
    public class Task
    {

        public enum TaskType { Pickup, Hunt, Mate, Sleep, Guard }

        public EntityAction Action { get; private set; }
        public double ActionMod { get; private set; }

        public Task(EntityAction action, double mod)
        {
            Action = action;
            ActionMod = mod;
        }
    }

    public class PersonBrain
    {
        private readonly int personLimit;
        private readonly int desiredFoodCount;
        private readonly OrderedPair<int> housePosition;
        private RectangleF houseRectangle;
        private UtilityDecider<Need> utilityDecider;
        public List<Task> Tasks { get; private set; } // Hold the list of active tasks, these will be sent to PersonBrain for filtering.

        private readonly ActionUtility dropUtility;

        public PersonBrain(int personLimit, int desiredFoodCount, OrderedPair<int> housePosition, RectangleF houseBox)
        {
            this.personLimit = personLimit;
            this.desiredFoodCount = desiredFoodCount;
            this.housePosition = housePosition;
            houseRectangle = houseBox;
            Tasks = new List<Task>();

            utilityDecider = new UtilityDecider<Need>();

            utilityDecider.AddResponseFunction(Need.Hunger, x => x);
            utilityDecider.AddResponseFunction(Need.Social, x => x);
            utilityDecider.AddResponseFunction(Need.Lust, x => x);
            utilityDecider.AddResponseFunction(Need.Tiredness, x => x);
            utilityDecider.AddResponseFunction(Need.Boredom, x => x);
            utilityDecider.AddResponseFunction(Need.JobFullfilment, x => x);

            dropUtility = new ActionUtility(new DropAction(houseRectangle), new Tuple<Need, double>[]
                {
                    ActionUtility.NewPair(Need.JobFullfilment, DDeltaConfig.dropFoodDelta)
                });
        }

        public void AddTask(EntityAction taskAction, double mod)
        {
            Tasks.Add(new Task(taskAction, mod));
        }

        public void ClearTasks()
        {
            Tasks.Clear();
        }

        // Uses the person's needs and the list of GameObjects that are nearby
        public void DecideAction(Person person, List<GameObject> nearbyObjects, int personCount, int food, int mouseX, int mouseY)
        {

            //person.SetAction(new FollowMouseAction(mouseX, mouseY, housePosition, houseRectangle));

            double personCountMod = (personCount == 0 ? 1.0 : food / (2 * personCount));

            List<ActionUtility> actionAds = nearbyObjects
                .Where(o => o != person)
                // Kinda clunky but this will allow extra data to control action advertisements.
                .SelectMany(o => o is Person ? o.GetAdvertisedActions(person, personCountMod) : o.GetAdvertisedActions(person, 0.0))
                .ToList();

            //if (personCount >= personLimit)
            //    actionAds = actionAds.Where(ad => !(ad.Action is MateAction)).ToList();


            // Limit the number of people that can be present at one time. This would be more efficient as a check inside Person to prevent
            //  people from advertising a mate action if their are too many people.
            if (personCount >= personLimit)
            {
                actionAds = actionAds.Where(au => !(au.Action is MateAction)).ToList();
            }

            double deliverFoodDelta = person.GetItemCount(ItemType.Apple) / 2;
            // Return food to house.
            actionAds.Add(new ActionUtility(new DeliverFoodAction(houseRectangle), new Tuple<Need, double>[]
            {
                ActionUtility.NewPair(Need.JobFullfilment, deliverFoodDelta)
            }));
            // Wander around.
            actionAds.Add(new ActionUtility(person.IdleAction, new Tuple<Need, double>[]
            {
                ActionUtility.NewPair(Need.JobFullfilment, DDeltaConfig.idleActionAdvertisement)
            }));

            // Hunger need.
            if ((person.GetItemCount(ItemType.Apple) > 0 || food > 0) && person.Hunger <= 0.5)
            {
                actionAds.Add(new ActionUtility(new EatAction(), new Tuple<Need, double>[]
                {
                    ActionUtility.NewPair(Need.Hunger, 1.0 - person.Hunger)
                }));
            }

            // Tiredness need.
            if ((person.Tiredness < 0.6 || person.Health < 50) && !person.AttackerInRange()) // Sleep required. ** Add check to see if attacker is nearby **.
            {
                actionAds.Add(new ActionUtility(new SleepAction(Person.sleepSeconds, true), new Tuple<Need, double>[]
                {
                    ActionUtility.NewPair(Need.Tiredness, DDeltaConfig.sleepActionAdvertisement),
                    ActionUtility.NewPair(Need.Health, DDeltaConfig.sleepHealthAdvertisement)
                }));
            }
            else // Napping optional. This should be picked more often unless there are a lot of other actions to pick.
            {
                actionAds.Add(new ActionUtility(new SleepAction(Person.napSeconds, false), new Tuple<Need, double>[]
                {
                    ActionUtility.NewPair(Need.Tiredness, DDeltaConfig.napActionDelta)
                }));
            }

            if (person.GetRectangleF().IntersectsWith(houseRectangle))
            {
                actionAds.Add(dropUtility);
            }

            EntityAction attackerResponseAction;

            if (person.Attacker != null && nearbyObjects.Contains(person.Attacker))
            {
                if (person.Sex == Sex.Male && person.GetHealth() / person.Attacker.Strength > person.GetHealth() / person.Strength)
                {
                    attackerResponseAction = new AttackAction(person.Attacker);
                }
                else
                {
                    attackerResponseAction = new FleeAction(person.Attacker);
                }

                actionAds.Add(new ActionUtility(attackerResponseAction, new Tuple<Need, double>[]
                {
                    ActionUtility.NewPair(Need.JobFullfilment, DDeltaConfig.attackerResponseDelta)
                }));
            }

            // *** Apply action ad filters here from EntitController's tasks ***
            ApplyTaskFilters(actionAds);

            // NOTE: Doesn't filter actions that give 0.0 delta or a negative!
            EntityAction bestAction = utilityDecider.GetBestAction(person, actionAds);

            if (!Utilities.AreSameBaseType(person.Action, bestAction))
            {
                if (!(bestAction is WanderAction)) // Remove leftover velocity from a WanderAction.
                    person.Stop();
                person.TrySetAction(bestAction);
            }

        }

        // Will need to check data embedded in different tasks. E.g. hunting task will specify the animal to hunt.
        // All of these will have to be written for each task option.

        // Appears to work so far, needs to be balanced though...
        public void ApplyTaskFilters(List<ActionUtility> actionAds)
        {
            foreach (ActionUtility au in actionAds)
            {
                var auType = au.Action.GetType();
                // Find the action in the list of tasks that has the same type as the action embedded in the action ad.
                Task task = Tasks.Find(a => a.Action.SameBaseType(au.Action));

                if (task != null)
                {
                    Console.WriteLine($"Applying mod to {au.Action.GetName()}");

                    for (int i = 0; i < au.UtilityDeltas.Length; i++) // Apply updates to each need delta using the task.ActionMod.
                    {
                        var pair = au.UtilityDeltas[i]; // Grab each pair from the list.
                        au.UtilityDeltas[i] = new Tuple<Need, double>(pair.Item1, pair.Item2 * task.ActionMod); // Update the pair with the new deltas.
                    }
                }
            }
        }

        public void ForceAction(Entity entity, EntityAction action)
        {
            action.Do(entity);
        }

    }
}
