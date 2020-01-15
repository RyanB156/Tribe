using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Tribe
{

    public delegate void PropertyChangedHandler(object o, PropertyChangedEventArgs e);
    public class PropertyChangedEventArgs : EventArgs
    {
        public readonly object Value;
        public readonly string PropertyName;
        public PropertyChangedEventArgs(string name, object value) { this.PropertyName = name;  this.Value = value; }
    }

    /// <summary>
    /// Contains data that WorldController advertises to the form. Created when the player clicks on the game panel on a GameObject.
    /// </summary>
    public class WorldClickData
    {
        public readonly IGetData DataObject;
        public readonly ObjectData Data; // Data returned by an IGetData object.
        public readonly bool CanSwap; // Flag for if the player can swap to the current object or not.
        public readonly bool ItemsAccessible;
        public readonly Point MousePoint;

        public WorldClickData(IGetData dataObject, bool canSwap, bool itemsAccessible, Point mousePoint)
        {
            DataObject = dataObject;
            Data = dataObject.GetData();
            CanSwap = canSwap;
            ItemsAccessible = itemsAccessible;
            MousePoint = mousePoint;
        }
    }


    public class WorldController
    {

        public CraftingMenu CraftingMenu { get; private set; }

        // List of time and action pairs so that actions occur at a certain time. Sorts on every insert.
        private PriorityQueue<ActionGroup> ScheduleQueue;

        private readonly int gridSize = 50; // Set grid size for the object mesh. Each array will span <gridSize> pixels.
        private int numRows, numCols; // Store the height and width of the object mesh.
        private readonly int animationSeconds = 5;

        public event EventHandler PlayerDied;
        public bool PlayerIsDead { get; private set; }

        public GameTime GameTime { get; private set; }
        public GameTime ClearEffectTime { get; private set; }
        public double Scale { get; private set; } = 1.0;

        private Point mousePoint;
        private bool canSwap = false;

        public IGetData CurrentDataObject { get; private set; }
        public bool DataObjectSelected { get; private set; }

        public DefaultPictureRenderer PictureRenderer { get; private set; }

        public EntityController EntityController { get; }
        private PlantController PlantController;
        private int pickupSpawnCounter = 0;
        private List<Item> Items;
        private List<Effect> Effects;
        private List<GameObject> Miscellaneous;
        private ObjectMesh ObjectMesh;


        public WorldController(GameTime time, int width, int height)
        {

            Items = new List<Item>();
            Effects = new List<Effect>();
            Miscellaneous = new List<GameObject>();

            GameTime = time;
            ClearEffectTime = GameTime;
            ClearEffectTime.AddSeconds(10);
            SetWorldSize(width, height);

            ScheduleQueue = new PriorityQueue<ActionGroup>();

            CraftingMenu = new CraftingMenu();

            Items.Add(new Item(width / 2 - 40, height / 2 - 200, ItemType.Spear, 1));

            Miscellaneous.Add(new Campfire(width / 2 - 200, height / 2 - 300));

            PictureRenderer = new DefaultPictureRenderer();

            EntityController = new EntityController(time.Copy(), width, height);
            // EntityController adds an entity to its list. Add world level event listeners to the new entity.
            EntityController.PlayerDied += EntityController_PlayerDied;
            EntityController.AddGameObject += Controller_AddGameObject;
            EntityController.RemoveGameObject -= Controller_RemoveGameObject;
            EntityController.CreateEffect += EntityController_CreateEffect;
            // Make EntityController spawn new entities to add to the world.
            EntityController.CreateObjects(new OrderedPair<int>(width / 2, height / 2));

            PlantController = new PlantController();
            // Plant Controller adds a plant to its list. Add world level event listeners to the new plant.
            PlantController.AddGameObject += Controller_AddGameObject;
            PlantController.RemoveGameObject += Controller_AddGameObject;

            PlantController.CreateObjects(new OrderedPair<int>(width / 2, height / 2));

        }


        private void EntityController_PlayerDied(object sender, EventArgs e)
        {
            if (PlayerIsDead == false)
            {
                PlayerIsDead = true;
                EventHandler handler = PlayerDied;
                handler?.Invoke(this, new EventArgs());
            }

        }

        // PlayerIsDead == true and the player has selected a person using the display panel. Have EntityController update the player's character.
        public void SetPlayerToObject(IGetData dataObject)
        {
            PlayerIsDead = false;
            EntityController.SetPlayer((Person)dataObject);
        }

        // Animals drop items when they die.
        private void EntityController_AddItem(object sender, ItemEventArgs e)
        {
            Items.Add(e.Item);
        }

        #region WorldControllerEffects

        // Add effect to list and listen for the scheduled event.
        private void AddEffect(Effect effect)
        {
            //Console.WriteLine("Adding effect {0}", effect.ObjectID);
            Effects.Add(effect);

            effect.ScheduleEvent += OnScheduleEvent;
            effect.Disposed += Effect_Disposed;

            // Schedule the deletion action for the event. This will raise another event to remove references and delete the effect from the list "Effects".
            // Effects that are not finite need to do effect.Dispose() elsewhere.
            if (effect.IsFinite)
                effect.OnScheduleEvent(effect, new ScheduleEventArgs(animationSeconds, new DisposeAction()));
        }

        // Effect has run its course. Remove it and delete all event hooks.
        private void Effect_Disposed(object sender, EventArgs e)
        {
            Effect effect = (Effect)sender;
            effect.ScheduleEvent -= OnScheduleEvent;
            effect.Disposed -= Effect_Disposed;
            Effects.Remove(effect);
        }

        private void EntityController_CreateEffect(object o, EffectEventArgs e)
        {
            AddEffect(e.Effect);
        }
        #endregion


        // Entity or Plant controller has added an object. Make sure this entity can have world scheduled events.
        private void Controller_AddGameObject(object sender, GameObjectEventArgs e)
        {
            e.gameObject.ScheduleEvent += OnScheduleEvent;

            if (e.gameObject is Item pickup)
            {
                Items.Add(pickup);
            }
        }

        // Entity or Plant controller is removing an object. Make sure event hooks are removed.
        private void Controller_RemoveGameObject(object sender, GameObjectEventArgs e)
        {
            e.gameObject.ScheduleEvent -= OnScheduleEvent;
        }

        public void SetWorldSize(int width, int height)
        {
            Utilities.WorldWidth = width;
            Utilities.WorldHeight = height;
            Entity.SetBoundingBox(width, height); // Update size in Entity for entities to move around.
            numRows = (int)Math.Ceiling(Utilities.WorldHeight / (double)gridSize);
            numCols = (int)Math.Ceiling(Utilities.WorldWidth / (double)gridSize);
        }

        private void OnScheduleEvent(GameObject gameObject, ScheduleEventArgs e)
        {
            GameTime scheduleTime = GameTime.Copy();
            scheduleTime.AddSeconds(e.SecondsUntilAction);
            //Schedule.Add(new ActionGroup(scheduleTime, e.Action, gameObject));

            ScheduleQueue.Insert(new ActionGroup(scheduleTime, e.Action, gameObject));

            // Pattern match for events that need extra listeners.
            if (e.Action is BirthAction && gameObject is Person p)
            {
                p.GiveBirth += EntityController.OnGiveBirth;
            }
        }

        public void SpawnItem(ItemType type)
        {
            OrderedPair<int> location = Utilities.GetRandomPoint();
            Items.Add(new Item(location.X, location.Y, type, 1));
        }


        // Update Cycle.
        // Apply logic updates to everything in the world.
        public void Update(GameTime time, int mouseX, int mouseY)
        {
            GameTime = time;

            List<GameObject> allObjects = EntityController.Entities
                .Concat(new GameObject[] { EntityController.House })
                .Concat(PlantController.Plants.Select(x => (GameObject)x))
                .Concat(Items.Select(x => (GameObject)x))
                .Concat(Miscellaneous)
                .ToList();

            if (EntityController.Player.IsAlive)
                allObjects.Add(EntityController.Player.BasePerson);

            ObjectMesh = new ObjectMesh(Utilities.WorldWidth, Utilities.WorldHeight, gridSize, allObjects);

            // Send game time and object grid to entity controller to apply decision making and updates.
            Parallel.Invoke(new Action[] {() =>
            {
                EntityController.EntityUpdate(GameTime, ObjectMesh, mouseX, mouseY);
                PlantController.PlantUpdate(GameTime);
            }
            });

            // --- PickupArea ---

            // Spawn in pickups in random locations.
            if (++pickupSpawnCounter > 1000)
            {
                SpawnItem(ItemType.Apple);
                SpawnItem(ItemType.Rock);
                pickupSpawnCounter = 0;
            }

            // Check collisions on Items. Only people can pick up pickups.
            Stack<Item> clearedItems = new Stack<Item>();
            foreach (Item pi in Items)
            {
                // O(n) action now. People pickup items, which removes some or all of the value of the pickup.
                //  This part checks all pickups and removes those that have no value left.
                if (pi.Amount <= 0)
                {
                    pi.SetPickedUp();
                    clearedItems.Push(pi); // Stack of pickups to remove.
                }
            }
            // Remove pickups from the list.
            while (clearedItems.Count > 0)
            {
                Item p = clearedItems.Pop();
                Items.Remove(p);
            }

            // --- ScheduleArea ---

            // Check the schedule to apply actions. These are global events that are tied to the gametime.

            while (ScheduleQueue.Length > 0)
            {
                ActionGroup group = ScheduleQueue.PopMin();

                if (group.time <= time)
                {
                    // Apply scheduled action to entities.
                    if (group.gameObject is Entity en && group.action is EntityAction ea)
                    {
                        // Entity actions done just once.
                        en.TrySetAction(ea, true);
                    }

                    if (group.gameObject is Plant pl && group.action is PlantAction pa)
                    {
                        //Console.WriteLine("Assigning scheduled action to plant using pattern matching");
                        pl.TrySetAction(pa);
                    }

                    if (group.gameObject is Effect ef && group.action is EffectAction efa)
                    {
                        //Console.WriteLine("Assigning scheduled action to effect using pattern matching");
                        ef.TrySetAction(efa);
                    }

                }
                else
                {
                    ScheduleQueue.Insert(group); // Put action group back into the schedule.
                    break;
                }
            }

            // Force clear effects that have expired.
            if (GameTime > ClearEffectTime)
            {
                Effects.Clear();
                ClearEffectTime.AddSeconds(10);
            }
        }

        // TODO: Use the ObjectMesh to optimize rendering. If the world is big, only need to draw what is inside of the view area.
        public void Render(Graphics graphics)
        {
            // Position of the center of the screen.
            OrderedPair<double> screenCenter = new OrderedPair<double>(Utilities.ViewWidth / 2, Utilities.ViewHeight / 2);
            // Position of the center of the world.
            OrderedPair<double> worldCenter = new OrderedPair<double>(Utilities.WorldWidth / 2, Utilities.WorldHeight / 2);
            double xOffset, yOffset;

            PlantController.RenderPlants(graphics, Scale, EntityController.Player.Position, screenCenter);

            // Render Items and Effects.
            foreach (GameObject gameObject in Items.Concat(Miscellaneous).Concat(Effects))
            {
                PictureRenderer.DrawRender(gameObject.RenderContext, Scale, EntityController.Player.Position, screenCenter, graphics);
            }

            EntityController.RenderEntities(graphics, Scale, screenCenter);

            // Draw a rectangle to represent the edge of the map.
            xOffset = worldCenter.X - EntityController.Player.Position.X;
            yOffset = worldCenter.Y - EntityController.Player.Position.Y;

            Rectangle wallRectangle = new Rectangle((int)(screenCenter.X + (xOffset - Utilities.WorldWidth / 2) * Scale),
                (int)(screenCenter.Y + (yOffset - Utilities.WorldHeight / 2) * Scale),
                (int)(Scale * Utilities.WorldWidth), (int)(Scale * Utilities.WorldHeight));

            graphics.DrawRectangle(new Pen(Color.Red), wallRectangle);

            // Display a circle around the currently selected data object.
            if (CurrentDataObject != null && DataObjectSelected)
            {
                GameObject currentObject;

                if (CurrentDataObject is EntityController) // House does not need to be highlighted.
                    return;

                if (CurrentDataObject is Player player)
                    currentObject = player.BasePerson;
                else
                    currentObject = (GameObject)CurrentDataObject;

                xOffset = currentObject.Position.X - EntityController.Player.Position.X;
                yOffset = currentObject.Position.Y - EntityController.Player.Position.Y;

                graphics.DrawEllipse(new Pen(Color.Red), (int)(screenCenter.X + (xOffset - 7) * Scale),
                    (int)(screenCenter.Y + (yOffset - 7) * Scale), (int)(14 * Scale), (int)(14 * Scale));

            }
        }

        // Retrieve GameObject at the specified point and satisfies the given predicate.
        public GameObject GetObjectAtPoint(Point point, Predicate<GameObject> predicate)
        {
            RectangleF pointRectangle = new RectangleF(point, new SizeF(0.1f, 0.1f));

            List<GameObject> gameObjects = ObjectMesh.GetObjectsInRange(point.X, point.Y, 1)
                .Where(g => predicate(g) && g.GetRectangleF().IntersectsWith(pointRectangle)).ToList();

            if (gameObjects.Count > 0)
                return gameObjects[0];
            else
                return null;
        }


        // Get Data from the object at the specified point.
        public WorldClickData GetObjectData(Point point, bool altData)
        {
            mousePoint = point;
            bool itemsAccessible = false;

            OrderedPair<int> screenCenter = new OrderedPair<int>(Utilities.ViewWidth / 2, Utilities.ViewHeight / 2);
            point = new Point((int)((point.X - screenCenter.X) / Scale + EntityController.Player.Position.X),
                (int)((point.Y - screenCenter.Y) / Scale + EntityController.Player.Position.Y));

            // Check if the mouse was clicked on the house first.
            if (EntityController.ClickOnHouse(point.X, point.Y, out ObjectData houseData))
            {
                canSwap = false;
                DataObjectSelected = true;
                CurrentDataObject = EntityController;
                itemsAccessible = true;
            }
            else
            {
                // Check if the mouse was clicked over a valid GameObject. See "GetObjectAtPoint" for options.
                GameObject targetObject = GetObjectAtPoint(point, g => g is IGetData);

                if (targetObject == null)
                {
                    DataObjectSelected = false;
                    return null;
                }

                canSwap = PlayerIsDead && targetObject is Person;

                DataObjectSelected = true;

                // Player has player specific data and person specific data. Use mod key to access player data.
                if (altData && targetObject is Person person && person == EntityController.Player.BasePerson)
                {
                    CurrentDataObject = EntityController.Player;
                    itemsAccessible = true;
                }
                else
                {
                    CurrentDataObject = (IGetData)targetObject;
                }
            }

            return new WorldClickData(CurrentDataObject, canSwap, itemsAccessible, point);
        }

        public void ClearCurrentObject()
        {
            CurrentDataObject = null;
            canSwap = false;
        }

        public void ApplyTaskChange(bool[] taskSelections, string[] taskData)
        {
            EntityController.ApplyTaskChange(taskSelections, taskData);
        }

        public void ChangePlayerVelocity(Direction direction, bool setZero = false)
        {
            if (setZero)
                EntityController.Player.RemoveVelocity(direction);
            else
                EntityController.Player.SetVelocity(direction);
        }

        public WorldClickData GetPlayerInventory(Point point)
        {
            if (!EntityController.Player.IsAlive)
                return null;

            mousePoint = point;
            CurrentDataObject = EntityController.Player;
            return new WorldClickData(CurrentDataObject, false, true, point);
        }

        public void IncreaseScale()
        {
            Scale = Math.Min(5.0, Scale + 0.25);
            EntityController.UpdateScale(Scale);
        }

        public void DecreaseScale()
        {
            Scale = Math.Max(0.25, Scale - 0.25);
            EntityController.UpdateScale(Scale);
        }

        public bool TryCraftItem()
        {
            bool canCraft = CraftingMenu.Recipes[CraftingMenu.Index].Item1
                .AllSatisfy(comp => EntityController.Player.InventoryContainsWithCount(comp.Type, comp.Amount));
            
            if (canCraft)
            {
                EntityController.Player.RemoveCraftingItems(CraftingMenu.Recipes[CraftingMenu.Index].Item1);
            }

            return canCraft;
        }

        public void PlayerAttack()
        {
            EntityController.Player.SetPlayerAction(new PlayerAttackAction());
        }

        public void PlayerMate()
        {
            // For debugging reproduction things.

            //if (EntityController.Entities[0] is Person mate)
            //{
            //    EntityController.Player.BasePerson.MateWith(mate);
            //}
            EntityController.Player.SetPlayerAction(new PlayerMateAction());
        }

        public void PlayerDropItems()
        {
            EntityController.PlayerDropItems(false);
        }

        public void PlayerDropAllItems()
        {
            EntityController.PlayerDropItems(true);
        }

        public void PlayerPickup(bool checkHouse)
        {
            if (checkHouse)
            {
                EntityController.Player.SetPlayerAction(new PlayerPickupActionWithHouseCheck(EntityController.GetSelectedItem(), EntityController.House));
            }
            else
            {
                EntityController.Player.SetPlayerAction(new PlayerPickupAction());
            }
            
        }

        public void PlayerToggleSex()
        {
            EntityController.Player.BasePerson.ToggleSex();
        }

        public void SpawnRandomAnimal()
        {
            EntityController.SpawnRandomAnimal();
        }

        public void SpawnChosenAnimal(int choice, int mouseX, int mouseY)
        {
            OrderedPair<int> screenCenter = new OrderedPair<int>(Utilities.ViewWidth / 2, Utilities.ViewHeight / 2);
            int x = (int)(EntityController.Player.Position.X + (mouseX - screenCenter.X) * Scale);
            int y = (int)(EntityController.Player.Position.Y + (mouseY - screenCenter.Y) * Scale);
            EntityController.SpawnChosenAnimal(choice, x, y);
        }

        public void PlayerHarvest()
        {
            EntityController.Player.SetPlayerAction(new PlayerHarvestAction());
        }

        public void PlayerSleep()
        {
            EntityController.Player.SetPlayerAction(new SleepAction(Person.sleepSeconds, true));
            EntityController.Player.BasePerson.Action.SetUtilityDeltas(
                new Tuple<Need, double>[] { ActionUtility.NewPair(Need.Tiredness, DDeltaConfig.sleepActionAdvertisement) });
        }
    }
}
