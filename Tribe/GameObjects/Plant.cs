using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tribe
{

    public enum GrowthStage { Seed, Juvenille, Adult }
    public enum PlantType { Berry, Potato, Shrub, Yucca, PineTree }

    /*
     *      Right now, all plants are of the same type. They use paramaters to customize their appearance and growth rate using
     *          static Plant factory methods.
     *      AppleTree needs to store Pickup data, so it needs to be a unique class, but other types for now will have the same behavior.
     *      Maybe an abstract class for base Plants, an AppleTree class, and a GenericPlant class to have the old style plants.
     */

    public abstract class Plant : GameObject, IGetData
    {

        private readonly int growthSeconds;
        public GrowthStage GrowthStage { get; private set; }
        public PlantAction Action { get; private set; }
        public Bitmap Image { get; protected set; }

        public event ItemEventHandler DropItem;
        public event DataChangedHandler UpdateElement;
        public event EventHandler CancelData;

        // Create default sizes which the static factory methods can modify.
        public int SeedSize { get; protected set; }
        public int JuvenilleSize { get; protected set; }
        public int AdultSize { get; protected set; }
        public bool IsHarvested { get; protected set; }

        public Plant(int seconds, GrowthStage stage, int x, int y) : base(x, y, 20)
        {

            SeedSize = 20;
            JuvenilleSize = 20;
            AdultSize = 20;

            Size = GetSize();
            CollisionDistance = Size / 2;

            Action = null;
            growthSeconds = seconds;
            GrowthStage = stage;
            if (GrowthStage != GrowthStage.Adult)
                base.OnScheduleEvent(this, new ScheduleEventArgs(growthSeconds, new GrowAction()));
        }

        protected void OnUpdateElement(ChangeType type, string elementName, object value)
        {
            DataChangedHandler handler = UpdateElement;
            handler?.Invoke(this, new DataChangedArgs(type, elementName, value));
        }

        public void OnCancelData()
        {
            EventHandler handler = CancelData;
            handler?.Invoke(this, new EventArgs());
        }

        public void CarryOutAction()
        {
            if (Action != null) // Entity controller can set actions to null or change them if it wants.
            {
                Action.Do(this);
                if (!Action.IsActive) // The action has reached an ending condition.
                {
                    Action = null;
                }
            }
        }


        public void TriggerGrowth()
        {
            base.OnScheduleEvent(this, new ScheduleEventArgs(growthSeconds, new GrowAction()));
        }

        public void Grow()
        {
            if (GrowthStage == GrowthStage.Seed)
            {
                GrowthStage = GrowthStage.Juvenille;
                Size = JuvenilleSize;
                base.OnScheduleEvent(this, new ScheduleEventArgs(growthSeconds, new GrowAction()));
            }
            else if (GrowthStage == GrowthStage.Juvenille)
            {
                GrowthStage = GrowthStage.Adult;
                Size = AdultSize;
            }

            OnUpdateElement(ChangeType.UpdateElement, "GrowthStage", GrowthStage);

            CollisionDistance = Size / 2;
        }

        public abstract void Harvest();

        private int GetSize()
        {
            int size = SeedSize;
            switch (GrowthStage)
            {
                case GrowthStage.Juvenille:
                    size = JuvenilleSize;
                    break;
                case GrowthStage.Adult:
                    size = AdultSize;
                    break;
            }
            return size;
        }

        protected void OnDropItem(ItemEventArgs e)
        {
            ItemEventHandler handler = DropItem;
            handler?.Invoke(this, e);
        }

        //public override RectangleF GetRectangleF()
        //{
        //    int size = GetSize();
        //    return new RectangleF((float)Position.X - size / 2, (float)Position.Y - size / 2, size, size);
        //}

        public void TrySetAction(PlantAction action)
        {
            Action = action;
        }

        public abstract ObjectData GetData();

        public int GetItemIndex()
        {
            return -1;
        }

        // These will not be used by Plant.
        public void IncrementItemIndex()
        {
            throw new NotImplementedException();
        }

        public void DecrementItemIndex()
        {
            throw new NotImplementedException();
        }
    }

    public class GeneralPlant : Plant
    {

        private static readonly Bitmap potatoSeed = new Bitmap(Utilities.GetResourceImage("potatoseed.png"));
        private static readonly Bitmap potatoJuvenille = new Bitmap(Utilities.GetResourceImage("potatojuvenille.png"));
        private static readonly Bitmap potatoAdult = new Bitmap(Utilities.GetResourceImage("potatoadult.png"));

        private static readonly Bitmap pineTreeJuvenille = new Bitmap(Utilities.GetResourceImage("babytree.png"));
        private static readonly Bitmap pineTreeAdult = new Bitmap(Utilities.GetResourceImage("pinetree.png"));

        private static readonly Bitmap yuccaJuvenille = new Bitmap(Utilities.GetResourceImage("yuccajuvenille.png"));
        private static readonly Bitmap yuccaAdult = new Bitmap(Utilities.GetResourceImage("yuccaadult.png"));

        private static readonly Bitmap shrubAdult = new Bitmap(Utilities.GetResourceImage("shrubadult.png"));

        //private static Bitmap berrySeed = new Bitmap(Utilities.GetResourceImage("berryseed.png"));
        //private static Bitmap berryJuvenille = new Bitmap(Utilities.GetResourceImage("berryjuvenille.png"));
        //private static Bitmap berryAdult = new Bitmap(Utilities.GetResourceImage("berryadult.png"));
        //private static Bitmap bushSeed = new Bitmap(Utilities.GetResourceImage("bushseed.png"));
        //private static Bitmap bushJuvenille = new Bitmap(Utilities.GetResourceImage("bushjuvenille.png"));
        //private static Bitmap bushAdult = new Bitmap(Utilities.GetResourceImage("bushadult.png"));


        private static Dictionary<Tuple<PlantType, GrowthStage>, Bitmap> imageMap;

        static GeneralPlant()
        {
            imageMap = new Dictionary<Tuple<PlantType, GrowthStage>, Bitmap>
            {
                [new Tuple<PlantType, GrowthStage>(PlantType.Potato, GrowthStage.Seed)] = potatoSeed,
                [new Tuple<PlantType, GrowthStage>(PlantType.Potato, GrowthStage.Juvenille)] = potatoJuvenille,
                [new Tuple<PlantType, GrowthStage>(PlantType.Potato, GrowthStage.Adult)] = potatoAdult,

                [new Tuple<PlantType, GrowthStage>(PlantType.PineTree, GrowthStage.Juvenille)] = pineTreeJuvenille,
                [new Tuple<PlantType, GrowthStage>(PlantType.PineTree, GrowthStage.Adult)] = pineTreeAdult,

                [new Tuple<PlantType, GrowthStage>(PlantType.Yucca, GrowthStage.Juvenille)] = yuccaJuvenille,
                [new Tuple<PlantType, GrowthStage>(PlantType.Yucca, GrowthStage.Adult)] = yuccaAdult,

                [new Tuple<PlantType, GrowthStage>(PlantType.Shrub, GrowthStage.Adult)] = shrubAdult
            };
        }

        public PlantType PlantType { get; private set; }

        public static Plant Berry(int x, int y)
        {
            return new GeneralPlant(5, GrowthStage.Seed, PlantType.Berry, x, y);
        }

        public static Plant Potato(int x, int y)
        {
            return new GeneralPlant(5, GrowthStage.Seed, PlantType.Potato, x, y);
        }

        public static Plant Shrub(int x, int y)
        {
            return new GeneralPlant(5, GrowthStage.Adult, PlantType.Shrub, x, y)
            {
                AdultSize = 20
            };
        }

        public static Plant PineTree(int x, int y)
        {
            return new GeneralPlant(10, GrowthStage.Juvenille, PlantType.PineTree, x, y)
            {
                AdultSize = 60
            };
            
        }

        public static Plant Yucca(int x, int y)
        {
            return new GeneralPlant(5, GrowthStage.Juvenille, PlantType.Yucca, x, y)
            {
                AdultSize = 20
            };
        }

        private GeneralPlant(int seconds, GrowthStage stage, PlantType type, int x, int y) 
            : base(seconds, stage, x, y)
        {
            PlantType = type;
        }

        // Checking if the type and growth state are valid will be done in GetAdvertisedActions().
        // Create a pickup that corresponds to the plant type and raise the pickup drop event to add it to the game.
        // The plant will optionally be destroyed in the future. All general plants right now are destroyed after harvesting.
        public override void Harvest()
        {
            if (IsHarvested || GrowthStage != GrowthStage.Adult)
                return;

            Item p = null;
            int x = (int)Position.X;
            int y = (int)Position.Y;

            switch (PlantType)
            {
                case PlantType.PineTree:
                    p = new Item(x, y, ItemType.Stick, 1);
                    break;
                case PlantType.Potato:
                    p = new Item(x, y, ItemType.Apple, 1);
                    break;
                case PlantType.Yucca:
                    p = new Item(x, y, ItemType.Fiber, 1);
                    break;
                case PlantType.Shrub:
                    p = new Item(x, y, ItemType.Leaf, 1);
                    break;
            }

            if (p != null)
                OnDropItem(new ItemEventArgs(p));
            IsHarvested = true;
        }

        public override Bitmap GetImage()
        {
            Tuple<PlantType, GrowthStage> t = new Tuple<PlantType, GrowthStage>(PlantType, GrowthStage);
            if (imageMap.ContainsKey(t))
            {
                return imageMap[t];
            }
            else
            {
                return new Bitmap(Utilities.ResourceDirectory + "default.png");
            }
        }

        public override ObjectData GetData()
        {
            // new Tuple<string, object>("", ),
            return new ObjectData(new List<Tuple<string, object>>()
            {
                new Tuple<string, object>("Type", PlantType),
                new Tuple<string, object>("GrowthStage", GrowthStage),

            }, 2);
        }

        public override List<ActionUtility> GetAdvertisedActions(Entity entity, double mod)
        {
            List<ActionUtility> actionUtilities = new List<ActionUtility>();

            if (entity is Person person)
            {
                if (GrowthStage == GrowthStage.Adult)
                {
                    actionUtilities.Add(new ActionUtility(new HarvestAction(this), new Tuple<Need, double>[]
                    {
                    ActionUtility.NewPair(Need.JobFullfilment, DDeltaConfig.harvestPlantDelta)
                    }));
                }
            }
            
            return actionUtilities;
        }
    }

    public class AppleTree : Plant
    {

        private static readonly Bitmap image = Utilities.GetResourceImage("appletree.png");

        private int appleCount = 0;
        private readonly int appleCapacity = 5;
        private readonly int appleSize = 10;
        private readonly Item[] apples;


        public AppleTree(int x, int y) : base(0, GrowthStage.Adult, x, y)
        {
            appleCount = 0;
            apples = new Item[appleCapacity];

            AdultSize = 60;
            Size = AdultSize;
            CollisionDistance = Size / 2;

            for (int i = 0; i < appleCapacity; i++)
            {
                AddApple();
            }
        }

        private Item CreatePickup(int position)
        {
            // Position set as relative to the tree at first. Rendering of AppleTree apples will calculate the absolute position from this offset.
            int x = 0;
            int y = 0;

            switch (position)
            {
                case 0:
                    x += -17; y += -15;
                    break;
                case 1:
                    x += -3; y += -23;
                    break;
                case 2:
                    x += -7; y += -9;
                    break;
                case 3:
                    x += 9; y += -17;
                    break;
                case 4:
                    x += 13; y += -7;
                    break;
            }

            return new Item(x, y, ItemType.Apple, 1);
        }


        public void AddApple()
        {
            if (appleCount < appleCapacity)
            {
                Item apple = CreatePickup(appleCount);
                apples[appleCount] = apple;
                appleCount++;

                RenderContext.AddAccessory(apple);
                OnUpdateElement(ChangeType.UpdateElement, "Count", appleCount);
            }
        }

        public void OnDropApple()
        {
            if (appleCount > 0)
            {
                Item p = apples[appleCount - 1];
                // Convert offset original position to absolute position with dropping to the bottom of the tree.
                p.SetPosition(new OrderedPair<double>(Position.X + p.Position.X, Position.Y + AdultSize - appleSize)); // Move apple to the bottom of the tree.
                RenderContext.RemoveAccessory(p);
                
                OnDropItem(new ItemEventArgs(p));
                apples[appleCount - 1] = null;
                appleCount--;

                OnUpdateElement(ChangeType.UpdateElement, "Count", appleCount);
            }
        }

        public override void Harvest()
        {
            OnDropApple();
        }

        public Item[] GetApples()
        {
            return apples;
        }

        public override Bitmap GetImage()
        {
            return image;
        }


        public override ObjectData GetData()
        {
            // new Tuple<string, object>("", ),
            return new ObjectData(new List<Tuple<string, object>>()
            {
                new Tuple<string, object>("Type", "Apple Tree"),
                new Tuple<string, object>("Count", appleCount)
            }, 2);
        }

        public override List<ActionUtility> GetAdvertisedActions(Entity entity, double mod)
        {
            List<ActionUtility> actionUtilities = new List<ActionUtility>();

            if (entity is Person person)
            {
                if (appleCount > 0)
                {
                    actionUtilities.Add(new ActionUtility(new HarvestAction(this), new Tuple<Need, double>[]
                    {
                    ActionUtility.NewPair(Need.JobFullfilment, DDeltaConfig.harvestPlantDelta)
                    }));
                }
            }

            return actionUtilities;
        }
    }

}
