using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tribe
{

    public delegate void ItemEventHandler(GameObject gameObject, ItemEventArgs e);

    public class ItemEventArgs : EventArgs
    {
        public readonly Item Item;
        public ItemEventArgs(Item pickup) => Item = pickup;
    }
    
    public enum ItemType { Meat, Apple, Stick, Rock, Fiber, Leaf, Spear }

    public class Item : GameObject
    {

        public static Bitmap meatImage = new Bitmap(Utilities.GetResourceImage("steak.png"));
        public static Bitmap plantImage = new Bitmap(Utilities.GetResourceImage("apple.png"));
        public static Bitmap stickImage = new Bitmap(Utilities.GetResourceImage("stick.png"));
        public static Bitmap rockImage = new Bitmap(Utilities.GetResourceImage("rock.png"));
        public static Bitmap fiberImage = new Bitmap(Utilities.GetResourceImage("fiber.png"));
        public static Bitmap leafImage = new Bitmap(Utilities.GetResourceImage("leaf.png"));
        public static Bitmap spearImage = new Bitmap(Utilities.GetResourceImage("spear.png"));

        public readonly ItemType Type;
        public int Amount { get; private set; }
        public int ModValue { get; private set; } // +/- to stats for holding this item.
        public bool IsPickedUp { get; private set; }
        public readonly bool IsWeapon = false;
        private readonly ActionUtility pickupUtility;
        private readonly double pickupDelta = 0.4;

        public Item(int x, int y, ItemType type, int amount) : base(x, y, 10)
        {
            Type = type;
            Amount = amount;

            switch (type)
            {
                case ItemType.Stick:
                    Size = 15;
                    break;
                case ItemType.Fiber:
                    Size = 15;
                    break;
                case ItemType.Spear:
                    Size = 20;
                    ModValue = 15;
                    IsWeapon = true;
                    break;
            }
            
            pickupUtility = new ActionUtility(new PickupAction(this), new Tuple<Need, double>[]
            {
                ActionUtility.NewPair(Need.JobFullfilment, pickupDelta)
            });
        }

        public void AddAmount(int amount)
        {
            if (amount < 1)
                throw new ArgumentException("You must add a positive amount to the pickup");
            Amount += amount;
        }

        public void TakeAmount(int amount)
        {
            if (amount > Amount)
                throw new ArgumentException("Cannot take more value than a pickup has");
            Amount -= amount;

            if (Amount == 0)
                IsPickedUp = true;
        }

        public void SetPickedUp() { IsPickedUp = true; }

        public void SetPosition(OrderedPair<double> newPosition) { Position = newPosition; }

        public override Bitmap GetImage()
        {
            switch (Type)
            {
                case ItemType.Meat:
                    return meatImage;
                case ItemType.Apple:
                    return plantImage;
                case ItemType.Stick:
                    return stickImage;
                case ItemType.Rock:
                    return rockImage;
                case ItemType.Fiber:
                    return fiberImage;
                case ItemType.Leaf:
                    return leafImage;
                case ItemType.Spear:
                    return spearImage;
                default:
                    return new Bitmap(Utilities.GetResourceImage("default.png"));
            }
        }

        public override List<ActionUtility> GetAdvertisedActions(Entity entity, double mod)
        {
            List<ActionUtility> actionUtilities = new List<ActionUtility>();

            if (entity is Person person)
            {
                if (person.GetItemCount(Type) <= Person.carryCapacity)
                {
                    actionUtilities.Add(pickupUtility);
                }

                if (Type == ItemType.Apple || Type == ItemType.Meat)
                {
                    actionUtilities.Add(new ActionUtility(new EatAction(), new Tuple<Need, double>[]
                    {
                        ActionUtility.NewPair(Need.Hunger, 1.0 - entity.Hunger)
                    }));
                }

            }
            else if (Type == ItemType.Meat && entity is Animal animal && (animal.Type == AnimalType.Wolf || animal.Type == AnimalType.Bear))
            {
                actionUtilities.Add(new ActionUtility(new ConsumeAction(this), new Tuple<Need, double>[]
                {
                    ActionUtility.NewPair(Need.Hunger, DDeltaConfig.consumeFoodDelta)
                }));
            }

            return actionUtilities;
        }
    }
}
