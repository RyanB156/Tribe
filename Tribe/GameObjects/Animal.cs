using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tribe
{
    // Animal strength given as a double between [0.0 and 1.0].
    // Overall damage can be modified using Entity.BaseDamage. Attack = strength * BaseDamage.

    public enum AnimalType { Wolf, Goat, Hog, Bear };

    public class Animal : Entity, IGetData
    {
        public ActionUtility AttackThisUtility { get; private set; }
        public ActionUtility FleeFromThisUtility { get; private set; }

        public AnimalType Type { get; private set; }
        public int DropCount { get; private set; } = 1;


        private Animal(int x, int y, AnimalType type) : base(1.5, 10, x, y, GetBaseColor(type), 5)
        {
            Type = type;
            // Set all needs in the middle. These won't be updated right now, but they will be used for Animal decision making.
            Hunger = 0.5;
            Social = 0.5;
            Lust = 0.5;
            Tiredness = 0.5;
            Boredom = 0.5;
            JobFullfilment = 0.5;

            AttackThisUtility = new ActionUtility(AttackThisAction, new Tuple<Need, double>[]
                            {
                                ActionUtility.NewPair(Need.JobFullfilment, 0.04)
                            });
            FleeFromThisUtility = new ActionUtility(new FleeAction(this), new Tuple<Need, double>[]
                            {
                            ActionUtility.NewPair(Need.JobFullfilment, 0.0025)
                            });           
        }

        public event DataChangedHandler UpdateElement;
        public event EventHandler CancelData;

        private void OnUpdateElement(ChangeType type, string elementName, object value)
        {
            DataChangedHandler handler = UpdateElement;
            handler?.Invoke(this, new DataChangedArgs(type, elementName, value));
        }

        public void OnCancelData()
        {
            EventHandler handler = CancelData;
            handler?.Invoke(this, new EventArgs());
        }

        public static Animal CreateGoat(int x, int y)
        {
            Animal a = new Animal(x, y, AnimalType.Goat)
            {
                FollowDistance = 20
            };
            return a;
        }

        public static Animal CreateHog(int x, int y)
        {
            Animal a = new Animal(x, y, AnimalType.Hog)
            {
                DropCount = 5,
                Strength = 0.5
            };
            return a;
        }

        public static Animal CreateWolf(int x, int y)
        {
            Animal a = new Animal(x, y, AnimalType.Wolf)
            {
                Strength = 0.75
            };
            return a;
        }

        public static Animal CreateBear(int x, int y)
        {
            Animal a = new Animal(x, y, AnimalType.Bear)
            {
                DropCount = 10,
                Strength = 0.9
            };
            return a;
        }

        public override bool TrySetAction(EntityAction newAction, bool locked = false)
        {
            OnUpdateElement(ChangeType.UpdateElement, "Action", Action != null ? Action.GetName() : "None");
            return base.TrySetAction(newAction, locked);
        }

        // Just reset the needs for now. Should some of these decrease over time? Maybe hunger for wolves and bears. Social for goats & wolves?...
        public void UpdateNeeds()
        {
            Hunger = 0.5;
            Social = 0.5;
            Lust = 0.5;
            Tiredness = 0.5;
            Boredom = 0.5;
            JobFullfilment = 0.5;
        }

        public static Color GetBaseColor(AnimalType type)
        {
            switch (type)
            {
                case AnimalType.Goat: return Color.FromArgb(255, 242, 153, 87);
                case AnimalType.Hog: return Color.FromArgb(255, 180, 10, 10);
                case AnimalType.Wolf: return Color.Gray;
                case AnimalType.Bear: return Color.FromArgb(255, 128, 43, 2);
                default: return Color.Chartreuse;
            }
        }


        public static string GetTypeString(AnimalType type)
        {
            switch (type)
            {
                case AnimalType.Goat: return "Goat";
                case AnimalType.Hog: return "Hog";
                case AnimalType.Wolf: return "Wolf";
                case AnimalType.Bear: return "Bear";
                default: return "";
            }
        }

        public override void SetBaseColor()
        {
            Color = GetBaseColor(Type);
        }


        public override void TakeDamage(int damage, Entity attacker)
        {
            base.TakeDamage(damage, attacker); // Assigns attacker so the animal will remember who attacked them and changes the color for damage.
            OnUpdateElement(ChangeType.UpdateElement, "Health", Health);
        }
        
        // Not used right now, but could be in the future. Required because GameObject has this method as abstract.
        public override Bitmap GetImage()
        {
            return new Bitmap(1, 1);
        }

        public ObjectData GetData()
        {
            // new Tuple<string, object>("", ),
            return new ObjectData(new List<Tuple<string, object>>()
            {
                new Tuple<string, object>("Animal", GetTypeString(Type)),
                new Tuple<string, object>("Health", GetHealth()),
                new Tuple<string, object>("Action", (Action == null) ? "None" : Action.GetName())
            }, 3);
        }

        
        public override List<ActionUtility> GetAdvertisedActions(Entity entity, double mod)
        {
            List<ActionUtility> actionUtilities = new List<ActionUtility>();

            if (entity is Person person)
            {
                // males attack or run away from all animal types. females run away or ignore based on animal types.
                // entity can defeat this animal in combat or the animal can't fight back.
                if (person.Sex == Sex.Male)
                {
                    if (this.Health / entity.Strength > person.GetHealth() / this.Strength || Type == AnimalType.Hog || Type == AnimalType.Goat)
                        actionUtilities.Add(AttackThisUtility);
                    else // Male runs away from scary animal.
                        actionUtilities.Add(FleeFromThisUtility);
                }
                else
                {
                    if (Type == AnimalType.Bear || Type == AnimalType.Wolf)
                        // Weak males and females should run away from the animal.
                        actionUtilities.Add(FleeFromThisUtility);
                }
            }
            else if (entity is Animal animal)
            {
                if (this.Type == AnimalType.Goat && animal.Type == AnimalType.Wolf) // Wolves will attack goats.
                {
                    actionUtilities.Add(AttackThisUtility);
                }
                else if (this.Type == AnimalType.Wolf && animal.Type == AnimalType.Goat) // Goats will run from wolves.
                {
                    actionUtilities.Add(FleeFromThisUtility);
                }
                else if (this.Type != AnimalType.Bear && this.Type == animal.Type) // All but bears will travel in packs.
                {
                    actionUtilities.Add(new ActionUtility(new FollowAction(this, FollowDistance), new Tuple<Need, double>[]
                    {
                        ActionUtility.NewPair(Need.JobFullfilment, 0.002)
                    }));
                }
            }
            
            return actionUtilities;
        }

        public override void ApplySleepDelta()
        {
            base.ApplySleepDelta();
            OnUpdateElement(ChangeType.UpdateElement, "Health", Health);
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        public int GetItemIndex()
        {
            return -1;
        }

        // These should not be used right now.
        public void IncrementItemIndex()
        {
            throw new NotImplementedException();
        }

        public void DecrementItemIndex()
        {
            throw new NotImplementedException();
        }
    }
}
