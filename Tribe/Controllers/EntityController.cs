using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Tribe
{

    public class EntityController : GameObjectController, IGetData
    {
        // This copy here is used for creating new ActionGroups and adding them to Schedule. 
        public GameTime GameTime { get; private set; } 
        public Player Player { get; private set; }
        public List<Entity> Entities { get; private set; }
        public int PersonCount { get; private set; }

        public int SelectedItem { get; private set; } = 0; // Default to the first item type.

        public Renderer entityRenderer;

        private readonly double taskMod = 5.0;
        private readonly int personBrainPeriod = 8;
        private readonly int animalBrainPeriod = 8;
        private int personBrainCounter = 0;
        private int animalBrainCounter = 4; // Offset this one to spread out the decision checks.
        private PersonBrain personBrain;
        private AnimalBrain animalBrain;

        private readonly int maxEntityCount = 100; // Limit used for animalSpawns.
        private int entityCount;
        private readonly double animalSpawnChance = 0.0069; // Should average 1/6 seconds. 1 / (6 seconds * 24 ticks per second).
        private readonly List<Tuple<AnimalType, double>> animalSpawnWeights = new List<Tuple<AnimalType, double>>()
        {
            new Tuple<AnimalType, double>(AnimalType.Bear, 0.10),
            new Tuple<AnimalType, double>(AnimalType.Goat, 0.50),
            new Tuple<AnimalType, double>(AnimalType.Wolf, 0.20),
            new Tuple<AnimalType, double>(AnimalType.Hog, 0.20)
        };

        public House House { get; private set; }
        private OrderedPair<int> HousePosition;
        private readonly int houseSize = 30;
        private RectangleF HouseBox;
        private readonly Bitmap HouseImage;

        private ObjectData data;
        public List<Item> Inventory { get; private set; }

        public delegate void EffectEventHandler(object sender, EffectEventArgs e);
        public event EffectEventHandler CreateEffect;

        public event EventHandler PlayerDied;
        public event DataChangedHandler UpdateElement;
        public event EventHandler CancelData; // Required for IGetData interface. Not used in EntityController.


        public EntityController(GameTime time, int width, int height)
        {
            GameTime = time;

            Inventory = new List<Item>();
            Entities = new List<Entity>();

            House = new House(width / 2, height / 2, houseSize);
            HousePosition = new OrderedPair<int>(width / 2, height / 2);
            Entity.SetHomePosition(width / 2, height / 2);
            HouseBox = new RectangleF(HousePosition.X - houseSize / 2, HousePosition.Y - houseSize / 2, houseSize, houseSize);
            HouseImage = new Bitmap(Utilities.ResourceDirectory + "house.png");

            personBrain = new PersonBrain(20, 25, HousePosition, HouseBox);
            animalBrain = new AnimalBrain();

            entityRenderer = new DefaultEntityRenderer();
        }

        private void OnUpdateElement(ChangeType type, string elementName, object value)
        {
            DataChangedHandler handler = UpdateElement;
            handler?.Invoke(this, new DataChangedArgs(type, elementName, value));
        }

        // Add default EntityController controlled objects to the world after WorldController is listening for their creation.
        public void CreateObjects(OrderedPair<int> center)
        {
            Person playerPerson = new Person("Ryan", "Bressette", Sex.Male, center.X - 80, center.Y - 60);
            Player = new Player(playerPerson);

            Player.BasePerson.AddItem(new Item(0, 0, ItemType.Rock, 1));
            //player.BasePerson.RenderContext.AddAccessory(new Halo(-100, -100));

            Person hazel = new Person("Hazel", "Brunelle", Sex.Female, center.X - 100, center.Y - 20, true);

            OrderedPair<int> position = Utilities.GetRandomPoint();
            Animal g = Animal.CreateGoat(position.X, position.Y);

            position = Utilities.GetRandomPoint();
            Animal h = Animal.CreateHog(position.X, position.Y);

            position = Utilities.GetRandomPoint();
            Animal w = Animal.CreateWolf(position.X, position.Y);

            List<Entity> entities = new List<Entity>() { hazel, g, h, w };

            // Spawn random people.
            for (int i = 0; i < 4; i++)
            {
                position = Utilities.GetRandomPointCloseToPoint(HousePosition.X, HousePosition.Y, 100);
                Sex sex = Utilities.Rng.Next(0, 2) == 0 ? Sex.Male : Sex.Female;
                Person p = new Person(sex, position.X, position.Y, true);

                entities.Add(p);
            }

            // Add people to the world.
            EntitiesAdd(Player.BasePerson, false);
            Player.BasePerson.Age();
            foreach (Entity e in entities)
            {
                e.Age();
                EntitiesAdd(e);
            }
        }

        // Listen for when a person drops their items. If the person was over the house when it dropped its items, add the food to this.food
        //  and let WorldController know that the food count has changed. Otherwise, create a new pickup at the person's position
        //  and let WorldController know that a new Item has been created.

        private void P_DroppedItem(Person person, ItemDropEventArgs e)
        {
            // Search the inventory list to see if the item already exists in storage.
            Item storedItem = Inventory.Find(p => p.Type == e.Type);
            
            if (storedItem == null) // If not, create a new item in storage.
            {
                Inventory.Add(new Item(0, 0, e.Type, e.Count));
                OnUpdateElement(ChangeType.NewElement, e.Type.ToString(), e.Count);
            }
            else // otherwise add it to the item that already exists.
            {
                storedItem.AddAmount(e.Count);
                OnUpdateElement(ChangeType.UpdateElement, e.Type.ToString(), e.Count);
            }
                

            if (!e.InHouse)
            {
                Item pickup = new Item((int)person.Position.X, (int)person.Position.Y, e.Type, e.Count);
                OnAddGameObject(pickup);
            }
        }

        // An entity has done something that triggered an animated image. Pass the effect to WorldController.
        private void Entity_CreateEffect(object o, EffectEventArgs e)
        {
            OnCreateEffect(o, e);
        }
        

        // Receive item request from a person. If there is food available, give it to the person.
        private void P_RequestItem(GameObject gameObject, ItemEventArgs e)
        {

            if (!(gameObject is Person person))
                return;

            Item storedItem = Inventory.Find(p => p.Type == e.Item.Type);

            if (storedItem == null)
            {
                Console.WriteLine($"Couldn't find pickup {e.Item.Type}");
                return;
            }

            //Console.WriteLine($"Giving item {storedItem.Type} to {person.FirstName}");

            storedItem.TakeAmount(1);
            if (storedItem.Amount == 0)
            {
                Inventory.Remove(storedItem);
                OnUpdateElement(ChangeType.RemoveElement, storedItem.Type.ToString(), 0);
            }
            else
            {
                OnUpdateElement(ChangeType.UpdateElement, storedItem.Type.ToString(), storedItem.Amount);
            }
                
            person.AddItem(new Item(0, 0, storedItem.Type, 1));

        }

        // Update Cycle.
        public void EntityUpdate(GameTime gameTime, ObjectMesh objectMesh, int mouseX, int mouseY)
        {
            entityCount = Entities.Count;
            UpdateData();

            if (Utilities.Rng.NextDouble() < animalSpawnChance && entityCount < maxEntityCount)
            {
                SpawnRandomAnimal();
            }

            List<Entity> deadEntities = new List<Entity>();

            // Player updates.
            if (Player.BasePerson != null)
            {
                if (Player.BasePerson.Hunger <= 0.0)
                {
                    //Player.BasePerson.TakeDamage(1, null);
                }

                if (Player.IsAlive && Player.BasePerson.Health <= 0)
                {
                    Player.OnCancelData(); // Player is dead and does not have data to display.
                    Player.Kill();
                    deadEntities.Add(Player.BasePerson);
                    Player.Move();
                }
                else
                {
                    Player.CarryOutAction(objectMesh, GameTime);
                    Player.Move();
                    Player.UpdateData();
                }
            }
            
            // Updates for the entities in the list.
            for (int i = 0; i < Entities.Count; i++)
            {
                Entity e = Entities[i];

                List<GameObject> nearbyObjects = objectMesh
                    .GetObjectsInRange(e.Position.X, e.Position.Y, e.VisionRange)
                    .Where(o => o != e)
                    .ToList();

                if (e.GetHealth() <= 0)
                {
                    deadEntities.Add(e);
                    if (e is Person person)
                    {
                        person.DropAllItems(false);
                        person.OnCancelData();
                    }
                    else if (e is Animal animal)
                    {
                        animal.OnCancelData();
                    } 
                }
                else
                {
                    // -- Person Updates --
                    if (e is Person p && personBrainCounter == 0) // Only decide action after a certain period of time. 1 / personBrainPeriod times.
                    {

                        if (e.Hunger <= 0.0)
                        {
                            e.TakeDamage(1, null);
                        }

                        p.UpdateNeeds();
                        p.UpdateData();

                        // People that are close together will decrease their social need.
                        if (nearbyObjects.OneSatisfies(g => g.ObjectID != p.ObjectID && g is Person))
                            p.DecreaseSocialNeed();

                        // People close to a campfire will be warmed up.
                        if (nearbyObjects.OneSatisfies(g => g is Campfire))
                            p.DecreaseWarmthNeed();

                        if (!e.ActionLocked) // Locked actions will not be overwritten. Used to make entity wait.
                        {
                            personBrain.DecideAction(p, nearbyObjects, PersonCount, GetItemCount(ItemType.Apple), mouseX, mouseY);
                        }

                    }

                    // -- Animal Updates --
                    if (e is Animal a && animalBrainCounter == 0)
                    {
                        if (!a.ActionLocked)
                        {
                            animalBrain.DecideAction(a, nearbyObjects);
                        }
                        a.UpdateNeeds();
                    }
                    e.CarryOutAction(objectMesh, GameTime);
                }
            }

            // Update brain counters after all entities have been updated.
            personBrainCounter = (personBrainCounter + 1) % personBrainPeriod;
            animalBrainCounter = (animalBrainCounter + 1) % animalBrainPeriod;

            foreach (Entity e in deadEntities)
                EntitiesRemove(e);

            GameTime = gameTime;
        }

        public void ApplyTaskChange(bool[] taskSelections, string[] taskData)
        {
            personBrain.ClearTasks(); // Remove all active tasks from PersonBrain.
            List<Task> taskList = new List<Task>();

            if (taskSelections[0]) // Pickup.
            {
                ItemType itemType;
                switch (taskData[0])
                {
                    case "Food": itemType = ItemType.Apple; break;
                    case "Stick": itemType = ItemType.Stick; break;
                    case "Rock": itemType = ItemType.Rock; break;
                    case "Fiber": itemType = ItemType.Fiber; break;
                    case "Leaf": itemType = ItemType.Leaf; break;
                    default: throw new Exception("Invalid item type selected in Task Menu");
                }

                personBrain.AddTask(new PickupAction(new Item(0, 0, itemType, 1)), taskMod);
            }
            if (taskSelections[1]) // Hunt.
            {
                Animal animal;
                switch (taskData[0])
                {
                    case "Goat": animal = Animal.CreateGoat(0, 0); break;
                    case "Hog": animal = Animal.CreateHog(0, 0); break;
                    case "Wolf": animal = Animal.CreateWolf(0, 0); break;
                    case "Bear": animal = Animal.CreateBear(0, 0); break;
                    default: throw new Exception("Invalid animal type selected in Task Menu");
                }

                personBrain.AddTask(new AttackAction(animal), taskMod);
            }
            if (taskSelections[2]) // Mate.
            {
                personBrain.AddTask(new MateAction(null, 5), taskMod);
            }
            if (taskSelections[3]) // Sleep.
            {
                personBrain.AddTask(new SleepAction(5, false), taskMod);
            }
            if (taskSelections[4]) // Guard.
            {
                personBrain.AddTask(new GuardBaseAction(House.Position, Convert.ToInt32(taskData[4])), taskMod);
            }
        }

        private int GetItemCount(ItemType type)
        {
            Item storedItem = Inventory.Find(p => p.Type == type);
            return (storedItem == null) ? 0 : storedItem.Amount;
        }

        public Item GetSelectedItem()
        {
            if (SelectedItem >= 0 && SelectedItem < Inventory.Count)
                return Inventory[SelectedItem];
            else
                return null;
        }

        public void IncrementItemIndex()
        {
            if (Inventory.Count > 0)
                SelectedItem = ++SelectedItem % Inventory.Count;
        }

        public void DecrementItemIndex()
        {
            SelectedItem = (SelectedItem == 0) ? (Inventory.Count - 1) : (SelectedItem - 1);   
        }

        private void OnAddItem(Item p)
        {
            OnAddGameObject(p);
        }

        private void EntitiesAdd(Entity entity, bool addToEntityList = true)
        {
            if (addToEntityList)
                Entities.Add(entity);

            OnAddGameObject(entity);

            if (entity is Person person)
            {
                PersonCount++;
                OnUpdateElement(ChangeType.UpdateElement, "PersonCount", PersonCount);
                person.DroppedItem += P_DroppedItem;
                person.RequestItem += P_RequestItem;
            }
            entity.CreateEffect += Entity_CreateEffect;
        }

        // Remove EntityController listeners from the entity and remove it from the list for the garbage collector.
        private void EntitiesRemove(Entity entity)
        {
            bool isPlayerCharacter = false;

            OnRemoveGameObject(entity);
            if (entity is Person person)
            {
                PersonCount--;
                OnUpdateElement(ChangeType.UpdateElement, "PersonCount", PersonCount);
                person.DroppedItem -= P_DroppedItem;
                person.RequestItem -= P_RequestItem;

                if (person.IsPregnant)
                    person.GiveBirth -= OnGiveBirth;

                // If the player died.
                if (person == Player.BasePerson)
                {
                    isPlayerCharacter = true;
                    OnPlayerDeath();
                }
            }

            else if (entity is Animal animal) // Animal has died. Put meat on the ground.
            {
                OnAddItem(new Item((int)animal.Position.X, (int)animal.Position.Y, ItemType.Meat, animal.DropCount));
            }

            entity.CreateEffect -= Entity_CreateEffect;

            if (!isPlayerCharacter) // Player's BasePerson is not in Entities.
                Entities.Remove(entity);
        }

        private void OnPlayerDeath()
        {
            EventHandler handler = PlayerDied;
            handler?.Invoke(this, new EventArgs());
        }

        public void SpawnChosenAnimal(int choice, int x, int y)
        {
            Animal a;
            

            switch (choice)
            {
                case 0: a = Animal.CreateBear(x, y); break;
                case 1: a = Animal.CreateGoat(x, y); break;
                case 2: a = Animal.CreateHog(x, y); break;
                default: a = Animal.CreateWolf(x, y); break;
            }

            // Console.WriteLine("Spawning a {0}", a.Type);
            EntitiesAdd(a);
        }

        public void SpawnRandomAnimal()
        {
            Animal a;
            OrderedPair<int> spawnLocation = Utilities.GetRandomPoint();
            int x = spawnLocation.X;
            int y = spawnLocation.Y;

            AnimalType animalType = UtilityDecider<AnimalType>.WeightedRandomChoice(animalSpawnWeights);
            switch (animalType)
            {
                case AnimalType.Bear:
                    a = Animal.CreateBear(x, y);
                    break;
                case AnimalType.Wolf:
                    a = Animal.CreateWolf(x, y);
                    break;
                case AnimalType.Hog:
                    a = Animal.CreateHog(x, y);
                    break;
                default:
                    a = Animal.CreateGoat(x, y);
                    break;
            }

            Console.WriteLine("Spawning a {0}", animalType);
            EntitiesAdd(a);

        }

        public List<Entity> GetAll()
        {
            return Entities.Concat(new Entity[] { Player.BasePerson }).ToList();
        }

        // Event for the person to signal that they have become pregnant.
        // Schedule the birth event.
        public void OnGiveBirth(Person person, BirthEventArgs e)
        {
            //Console.WriteLine("Adding {0} to game", e.Child.FirstName);
            EntitiesAdd(e.Child);
            
            e.Child.OnScheduleEvent(e.Child, new ScheduleEventArgs(Person.ageSeconds, new AgeAction()));
            person.Abort();
            person.GiveBirth -= OnGiveBirth; // Remove the listener.
        }

        // Send effect updates to worldcontroller for display.
        private void OnCreateEffect(object o, EffectEventArgs e)
        {
            EffectEventHandler handler = CreateEffect;
            handler?.Invoke(o, e);
        }

        public void SetPlayer(Person person)
        {
            Player.SwapPerson(person); // Set Player's character to the person.
            Entities.Remove(person); // Remove person from the list of Entities.
        }

        public void PlayerDropItems(bool allItems)
        {
            if (Player.BasePerson == null)
                return;

            int count = allItems ? Player.GetSelectedItemCount() : 1;
            Player.BasePerson.TrySetAction(new PlayerDropAction(Player, HouseBox, Player.SelectedItem, count));
        }

        public void RenderEntities(Graphics graphics, double scale, OrderedPair<double> center)
        {
            double xOffset = House.Position.X - Player.Position.X;
            double yOffset = House.Position.Y - Player.Position.Y;
            graphics.DrawImage(House.GetImage(), House.GetRectangleFWithOffset(center, xOffset, yOffset, scale));

            if (Player.BasePerson != null && Player.BasePerson.Health >= 0)
            {
                entityRenderer.DrawRender(Player.BasePerson.RenderContext, scale, Player.Position, center, graphics);

                Rectangle playerDisplayRectangle = new Rectangle(Utilities.ViewWidth / 2 - Player.BasePerson.CollisionDistance, 
                    Utilities.ViewHeight / 2 - Player.BasePerson.CollisionDistance,
                    (int)(Player.BasePerson.Size * scale), (int)(Player.BasePerson.Size * scale));

                graphics.DrawRectangle(new Pen(new SolidBrush(Color.Gold), (float)(1.5 * scale)), playerDisplayRectangle);
            }

            foreach (Entity en in Entities)
            {
                if (en != null)
                {
                    entityRenderer.DrawRender(en.RenderContext, scale, Player.Position, center, graphics);
                    // Draw a circle representing the entities vision range. Not 100% accurate because object mesh squashes the game space into 0 width cells.
                    //graphics.DrawEllipse(new Pen(new SolidBrush(Color.Black)), en.GetSearchRadius(en.VisionRange));
                }
            }
        }

        private void UpdateData()
        {
            data = new ObjectData(new List<Tuple<string, object>>()
            {
                new Tuple<string, object>("PersonCount", PersonCount)
            }, 1);

            for (int i = 0; i < Inventory.Count; i++)
            {
                data.DataList.Add(new Tuple<string, object>(Inventory[i].Type.ToString(), Inventory[i].Amount));
            }
        }

        public ObjectData GetData()
        {
            return data;
        }

        public bool ClickOnHouse(int x, int y, out ObjectData data)
        {
            // Mouse position is inside the house. Show display panel with the items in storage.
            if (HouseBox.IntersectsWith(new RectangleF(x, y, 1, 1)))
            {
                data = GetData();
                return true;
            }
            else
            {
                data = null;
                return false;
            }
        }

        public void UpdateScale(double scale)
        {

            Player.BasePerson.ScaleSpeed(scale);

            foreach (Entity e in Entities)
                e.ScaleSpeed(scale);
        }

        public int GetItemIndex()
        {
            return SelectedItem;
        }

    }
}
