using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tribe
{

    public interface ICheckNearbyObjects
    {
        void DoWithSearch(Entity entity, ObjectMesh objectMesh);
    }

    public interface IDoWithTime
    {
        void DoWithTime(Entity entity, GameTime time);
    }

    public interface ICheckNearbyObjectsWithTime
    {
        void DoWithSearchAndTime(Entity entity, ObjectMesh objectMesh, GameTime time);
    }

    public enum ActionState { Running, Success, Failure };

    public abstract class GameObjectAction
    {
        public bool IsActive { get; protected set; } = true;
        public ActionState State { get; protected set; } = ActionState.Running;
        
    }


    #region EntityActions
    // Functions as a wrapper for methods inside of an Entity.
    public abstract class EntityAction : GameObjectAction
    {
        public static readonly int attackDelay = 20;
        public static readonly int entitySize = 10;

        public Tuple<Need, double>[] UtilityDeltas { get; private set; }
        public bool IsInterruptible { get; protected set; }
        public virtual void Do(Entity entity)
        {
            Console.WriteLine("Used default implementation of EntityAction.Do(Entity entity)");
        }

        /// <summary>
        /// Ends the current action and marks it for reassignment by PersonBrain. Some actions can override this to clean up before being deleted.
        /// </summary>
        public virtual void End()
        {
            IsActive = false;
        }

        public void SetActiveState(bool state) => IsActive = state;

        public abstract string GetName();

        public EntityAction(bool isInterruptible)
        {
            IsInterruptible = isInterruptible;
        }

        public void SetUtilityDeltas(Tuple<Need, double>[] deltas)
        {
            UtilityDeltas = deltas;
        }

    }

    // Additional class in the hierarchy to avoid code repetition from starting and checking the timer.
    public abstract class EntityTimeAction : EntityAction
    {
        protected GameTime stopTime;
        protected bool timerEnabled = false;
        protected int waitSeconds;

        public EntityTimeAction(int waitSeconds, bool isInterruptible) : base(isInterruptible)
        {
            this.waitSeconds = waitSeconds;
        }

        /// <summary>
        /// Allow the entity to move again and mark this action for deletion.
        /// </summary>
        /// <param name="entity"></param>
        protected void Release(Entity entity)
        {
            entity.SetCanMove(true);
            End();
        }

        ///<summary>
        /// Prevent the entity from moving and from being assigned a new action.
        /// </summary>
        /// <param name="entity"></param>
        protected void Hold(Entity entity)
        {
            entity.SetActionLockState(true);
            entity.SetCanMove(false);
        }

        public abstract void DoWithTime(Entity entity, GameTime time);

        protected void StartWaitTimer(Entity entity, GameTime time)
        {
            stopTime = time.Copy();
            stopTime.AddSeconds(waitSeconds);
            Hold(entity);
            timerEnabled = true;
        }
    }

    public class GotoAction : EntityAction
    {
        private readonly double x;
        private readonly double y;
        private RectangleF rect;
        private bool isDirectionSet = false;

        public GotoAction(double xC, double yC, int size) : base(false)
        {
            x = xC;
            y = yC;
            int halfSize = size / 2;
            rect = new RectangleF((float)xC - halfSize, (float)yC - halfSize, size, size);
        }

        public void Reset()
        {
            IsActive = true;
            State = ActionState.Running;
            isDirectionSet = false;
        }

        public override void Do(Entity entity)
        {
            RectangleF entityRectangle = entity.GetRectangleF();
            if (entityRectangle.IntersectsWith(rect)) // Check end condition. Goto is completed when the entity has reached the destination.
            {
                End();
                return;
            }
            else
            {
                if (IsActive)
                {
                    bool moveResult;
                    // First call or if the entity has become motionless for some reason -> set direction.
                    if (!isDirectionSet || (entity.Velocity.X == 0.0 && entity.Velocity.Y == 0.0))
                    {
                        moveResult = entity.Goto(x, y);
                        isDirectionSet = true;   
                    }
                    else // Subsequent calls apply direction.
                    {
                        moveResult = entity.Move();
                    }

                    if (!moveResult)
                    {
                        End();
                        State = ActionState.Failure;
                    }
                }
            }
        }

        public override string GetName()
        {
            return "GotoAction";
        }
    }

    // TODO: Make sure this works!!!
    // Action that will make the entity loiter a certain distance away from a target point. If the entity is too far away, it will move closer, then stop.
    public class GuardBaseAction : EntityAction
    {
        private OrderedPair<double> basePosition;
        private readonly int distance;

        public GuardBaseAction(OrderedPair<double> point, int distance) : base(true)
        {
            basePosition = point;
            this.distance = distance;
        }

        public override void Do(Entity entity)
        {
            if (Utilities.SquaredDistance(entity.Position, basePosition) > distance * distance) // If too far away, move closer.
            {
                entity.Goto(basePosition.X, basePosition.Y);
            }
            else // Otherwise stop.
            {

                Console.WriteLine($"GameObject {entity.ObjectID} guarding house");

                entity.Stop();
                entity.ApplyNeedDeltas(UtilityDeltas);
            }
        }

        public override string GetName()
        {
            return "GuardBaseAction";
        }
    }

    public class DeliverFoodAction : EntityAction
    {
        private GotoAction moveAction;
        private RectangleF home;

        public DeliverFoodAction(RectangleF home) : base(true)
        {
            this.home = home;
            moveAction = new GotoAction(home.X + home.Width / 2, home.Y + home.Height / 2, entitySize);
        }

        public override void Do(Entity entity)
        {

            if (entity is Person person)
            {
                //if (person.ActionLocked)
                //    person.SetActionLockState(true);

                moveAction.Do(entity);

                RectangleF entityRectangle = entity.GetRectangleF();
                if (entityRectangle.IntersectsWith(home)) // person has reached the house.
                {
                    person.ApplyNeedDeltas(UtilityDeltas);
                    person.DropAllItems(true);
                    End();
                }
            }
            else
                End();

        }

        public override string GetName()
        {
            return "DeliverFoodAction";
        }
    }


    // *** Issues with AI getting stuck at all points, especially (0, 0) and all sitting still sometimes...
    // AI stuck on all edges of the screen when they reach 5 food...
    public class WanderAction : EntityAction
    {
        private int moveTime = 0;
        private const int changeCount = 25;
        private const double changeProbability = 0.25;
        private OrderedPair<int> destination;

        private GotoAction moveAction;

        public WanderAction(int x, int y) : base(true)
        {
            destination = new OrderedPair<int>(x, y);
            moveAction = new GotoAction(destination.X, destination.Y, entitySize);
        }

        public WanderAction() : base(true)
        {
            destination = Utilities.GetRandomPoint();
            moveAction = new GotoAction(destination.X, destination.Y, entitySize);
        }

        public override void Do(Entity entity)
        {
            moveTime++;
            moveAction.Do(entity); // GotoAction checks collision with destination and hitting walls.
            bool result = moveAction.IsActive;

            if (moveTime == changeCount) // Change directions after a set time.
            {
                moveTime = 0;
                double d = Utilities.Rng.NextDouble();
                if (d < changeProbability)
                {
                    // Applies goto to set direction to (x, y). Goto applies Move(). False means the entity hit a wall and needs to head to another square.
                    destination = Utilities.GetRandomPoint();
                    moveAction = new GotoAction(destination.X, destination.Y, entitySize);
                }
                else if (d > 0.75)
                {
                    entity.Stop();
                }
            }
            // The entity has hit a wall or reached the destination.
            else if (!result && entity.CollisionDirection != Direction.None)
            {
                destination = Utilities.GetDirectionalPoint(entity.CollisionDirection, entity.Position);
                moveAction = new GotoAction(destination.X, destination.Y, entitySize);
            }

            //entity.ApplyNeedDeltas(UtilityDeltas);
            // Advertise needs deltas to entities but don't actually give them the reward. This is for a default action.
        }

        public override string GetName()
        {
            return "WanderAction";
        }
    }



    // Player calls MateWith person that forces them to mate. Person.MateWith will set their actions to this
    //  which will update needs over its runtime and can be interrupted. If it is interrupted, dispose of the child passed in from BreedWith
    // All People, AI and Player, will have this action set for the wait time and possibly getting pregnant.
    // But PlayerMateAction has Player mate with all that are in range. Maybe set this inside MateWith?

    // Need a way to put need deltas in here too, based on what was advertised when this action was selected.

    // female advertises MateAction to males. Males pick MateAction, come here, and do male.MateWith(female) in the timer start stage.
    //  female will have her action set to WaitActionWithTimer and the male will be apply need delta to both, and do male.Reproduce(female) when
    //  this action ends.
    // This should work for females initiating the action too.
    public class MateAction : EntityTimeAction
    {
        bool started = false;
        Person mate;

        public MateAction(Person mate, int waitSeconds) : base(waitSeconds, true)
        {
            this.mate = mate;
        }

        public override void DoWithTime(Entity entity, GameTime time)
        {
            if (!started)
            {
                // Make entity get close to mate.
                if (Math.Sqrt(Utilities.SquaredDistance(entity.Position, mate.Position)) > Person.mateRange)
                {
                    entity.Goto(mate.Position.X, mate.Position.Y);
                    return;
                }
                else
                {
                    ((Person)entity).MateWith(mate);
                    started = true;
                }
                
            }

            if (!timerEnabled)
            {
                StartWaitTimer(entity, time);
            }
            if (time >= stopTime)
            {
                ((Person)entity).ReproduceWith(mate);

                Release(entity);
            }

            entity.ApplyNeedDeltas(UtilityDeltas);
            mate.ApplyNeedDeltas(UtilityDeltas);

        }

        public override string GetName()
        {
            return "MateAction";
        }
    }

    public class WaitAction : EntityTimeAction
    {

        private EntityAction finalAction;

        // Cause the entity to wait and schedule when it can move again.
        public WaitAction(int seconds, bool interruptible = true) : base(seconds, interruptible)
        {
            IsInterruptible = interruptible;
            finalAction = null;
        }
        // Cause the entity to wait and schedule when it can move again with an additional action.
        public WaitAction(int seconds, EntityAction action, bool interruptible = true) : base(seconds, interruptible)
        {
            IsInterruptible = interruptible;
            finalAction = action;
        }

        public override void DoWithTime(Entity entity, GameTime time)
        {
            if (!timerEnabled)
            {
                StartWaitTimer(entity, time);
            }
            else
            {
                if (time >= stopTime)
                {
                    if (finalAction != null)
                        entity.DoActionOnce(finalAction);
                    Release(entity);

                }
            }
        }

        public override string GetName()
        {
            return "WaitActionWithTimer" + ((finalAction != null) ? ": " + finalAction.GetName() : "");
        }
    }


    public class DropAction : EntityAction
    {

        private RectangleF home;

        public DropAction(RectangleF home) : base(true)
        {
            this.home = home;
        }

        public override void Do(Entity entity)
        {
            if (entity is Person person)
            {
                if (person.GetRectangleF().IntersectsWith(home))
                {
                    person.DropAllItems(true);
                }
                else
                {
                    person.DropAllItems(false);
                }
            }

            End();
        }

        public override string GetName()
        {
            return "DropAction";
        }
    }

    // Both naps and sleeping use the same action. Naps are shorter and apply small utility over time while sleep is long and sets tiredness to 1.0.
    public class SleepAction : EntityTimeAction
    {

        private Effect sleepEffect;
        private readonly bool isFullSleep; // true: set tiredness to 1.0 when finished (if it is not interrupted); false: apply delta over time.

        public SleepAction(int seconds, bool isFullSleep) : base(seconds, true)
        {
            this.isFullSleep = isFullSleep;
        }

        public override void DoWithTime(Entity entity, GameTime time)
        {
            if (sleepEffect == null)
            {
                sleepEffect = new Effect((int)entity.Position.X, (int)entity.Position.Y, EffectType.Sleep, entity.CollisionDistance, false);
                entity.OnCreateEffect(sleepEffect);
            }
                
            if (!timerEnabled)
            {
                StartWaitTimer(entity, time);
            }
            else
            {
                if (time >= stopTime)
                {
                    Release(entity);
                    End();
                    if (isFullSleep)
                        entity.ApplySleepDelta(); // Only apply the values if the entity has not completed the action.
                }
                else if (!isFullSleep)
                {
                    entity.ApplyNeedDeltas(UtilityDeltas);
                }
            }
        }

        public override void End()
        {
            // Cause the sleep effect to dispose itself. The effect raises an event that travels through
            //  WorldController to Form1 that causes this effect to be removed from a list and stop animating. Then it is garbage collected.
            sleepEffect.Dispose();
            base.End();
        }

        public override string GetName()
        {
            return "SleepAction: " + ((isFullSleep) ? "Full Rest" : "Nap");
        }
    }

    // Create a general request action that will be paramaterized and have the person call the right request method.
    // Will need a "PlayerRequestItemAction" that doesn't make the player move to the home. It just checks the player's position.
    
    // TODO: Finish this so people will not starve to death.
    public class RequestItemAction : EntityAction
    {
        private readonly string itemName;

        public RequestItemAction(string itemName) : base(true)
        {
            this.itemName = itemName;
        }

        public override void Do(Entity entity)
        {
            if (entity is Person person)
            {
                switch (itemName)
                {
                    case "Food":

                        break;
                    case "Sticks":

                        break;
                    case "Rocks":

                        break;
                    case "Fiber":

                        break;
                }
            }
        }

        public override string GetName()
        {
            return "RequestItemAction";
        }
    }

    public class EatAction : EntityAction
    {
        private bool appleRequested = false;
        private bool meatRequested = false;
        private GotoAction gotoAction;

        public EatAction() : base(true)
        {
            gotoAction = new GotoAction(Entity.HomePosition.X, Entity.HomePosition.Y, entitySize);
        }
        // May or may not work. Requires some workarounds for getting food count from EntityController using events.
        // Entity requests food but may not get it. This will create behavior where the entity starves to death on the house waiting for food.
        public override void Do(Entity entity)
        {
            if (entity is Person person)
            {
                if (person.GetItemCount(ItemType.Apple) > 0)
                {
                    person.Eat(); // Applies hunger adjustment manually.
                }
                else if (appleRequested) // Person tried requesting food but failed.
                {
                    End();
                }
                else
                {
                    gotoAction.Do(entity);
                    if (!gotoAction.IsActive) // The person has reached the house.
                    {
                        if (!appleRequested)
                        {
                            person.RequestItemByType(ItemType.Apple);
                            appleRequested = true;
                        }
                        else if (!meatRequested)
                        {
                            person.RequestItemByType(ItemType.Meat);
                            meatRequested = true;
                        }

                    }
                }
            }
            else
            {
                End();
            }
        }

        public override string GetName()
        {
            return "EatAction";
        }
    }

    public class FleeAction : EntityAction
    {
        GameObject fleeObject;
        EntityAction activeAction;
        readonly EntityAction idleAction;

        public FleeAction(GameObject entity) : base(true)
        {
            fleeObject = entity;
            idleAction = new WanderAction();
        }

        public override void Do(Entity entity)
        {
            double squaredDistance = Utilities.SquaredDistance(entity.Position, fleeObject.Position);

            if (squaredDistance < entity.VisionRange * entity.VisionRange)
            {
                double x = entity.Position.X + entity.Position.X - fleeObject.Position.X;
                double y = entity.Position.Y + entity.Position.Y - fleeObject.Position.Y;

                activeAction = new GotoAction(x, y, entity.Size);
                activeAction.Do(entity);
            }
            else
            {
                entity.ApplyNeedDeltas(UtilityDeltas);
                End();
            }
            

            if (activeAction != null && activeAction.State == ActionState.Failure) // If the entity has hit a wall while running away, head to a random point.
            {
                OrderedPair<int> randomPoint = Utilities.GetDirectionalPoint(entity.CollisionDirection, entity.Position);
                activeAction = new GotoAction(randomPoint.X, randomPoint.Y, entity.Size);
            }
        }

        public override string GetName()
        {
            return "FleeAction";
        }
    }


    public class PickupAction : EntityAction
    {
        Item target;
        GotoAction moveAction;
        bool moveActionSet = false;

        public PickupAction(Item pickup) : base(true)
        {
            target = pickup;
        }

        public override void Do(Entity entity)
        {

            if (!moveActionSet)
            {
                // Cannot set the move action in the constructor because the pickup's position might change after creating the pickup.
                // The PickupAction is created when the Item is constructed to avoid creating a new object every time a pickup advertises an action.
                moveAction = new GotoAction(target.Position.X, target.Position.Y, entitySize);
                moveActionSet = true;
            }
            else
            {
                if (entity is Person person)
                {
                    // Once an entity decides to pickup an item, it must pick it up if it is still available.
                    // This is forced to make the food gathering behavior more aggressive. I should be able to fix this with the action utilities though.
                    if (!person.ActionLocked)
                        person.SetActionLockState(true);

                    if (target.IsPickedUp)
                    {
                        End();
                        return;
                    }

                    moveAction.Do(entity);

                    if (!moveAction.IsActive) // If the entity has reached the item and picked it up.
                    {
                        person.Pickup(target);
                        entity.ApplyNeedDeltas(UtilityDeltas);
                        End();
                    }
                }
                else
                    End();
            }
        }

        public override string GetName()
        {
            return "PickupAction";
        }
    }

    public class ConsumeAction : EntityAction
    {
        private readonly Item pickup;
        private readonly GotoAction moveAction;

        public ConsumeAction(Item pickup) : base(true)
        {
            this.pickup = pickup;
            moveAction = new GotoAction(pickup.Position.X, pickup.Position.Y, pickup.Size);
        }

        public override void Do(Entity entity)
        {
            moveAction.Do(entity);

            if (!moveAction.IsActive) // If the entity has reached the item and picked it up.
            {
                pickup.TakeAmount(pickup.Amount);
                End();
            }
        }

        public override string GetName()
        {
            return "ConsumeAction";
        }
    }

    public class HarvestAction : EntityAction
    {
        Plant plant;

        public HarvestAction(Plant plant) : base(true)
        {
            this.plant = plant;
        }

        public override void Do(Entity entity)
        {
            if (!entity.GetRectangle().IntersectsWith(plant.GetRectangle())) // Entity needs to be touching the tree.
                entity.Goto(plant.Position.X, plant.Position.Y);

            else // Entity is touching the plant. Make the plant drop an apple.
            {
                // Console.WriteLine($"{entity.ObjectID} shaking a plant");
                plant.Harvest();
                IsActive = false;
            }
        }

        public override string GetName()
        {
            return "HarvestAction";
        }
    }    

    public class AttackAction : EntityAction
    {

        GameObject target;

        public AttackAction(GameObject gameObject) : base(true)
        {
            if (!(gameObject is IMortal))
                throw new ArgumentException("Attack Action target was not valid");

            target = gameObject;
        }

        public override void Do(Entity entity)
        {
            if (Math.Sqrt(Utilities.SquaredDistance(entity.Position, target.Position)) > entity.AttackRange)
            {
                entity.Goto(target.Position.X, target.Position.Y);
            }
            else
            {
                entity.Attack((IMortal)target);
                End();
                entity.ApplyNeedDeltas(UtilityDeltas);
            }
        }

        public override string GetName()
        {
            return "AttackAction";
        }
    }


    public class ResetColorAction : EntityAction
    {

        public ResetColorAction() : base(true) { }

        public override void Do(Entity entity)
        {
            if (entity is Person person)
                person.SetColor(Person.GetBaseColor(person.Sex, person.IsPregnant));
            else if (entity is Animal animal)
                animal.SetColor(Animal.GetBaseColor(animal.Type));

            End();
        }

        public override string GetName()
        {
            return "ResetColorAction";
        }
    }

    public class BirthAction : EntityTimeAction
    {

        public BirthAction(int laborSeconds) : base(laborSeconds, false) // Birth is not interruptible.
        {
            SetUtilityDeltas(new Tuple<Need, double>[] { new Tuple<Need, double>(Need.JobFullfilment, 0.05) });
        }

        public override void DoWithTime(Entity entity, GameTime time)
        {
            if (!timerEnabled)
                StartWaitTimer(entity, time);
            else
            {
                if (stopTime >= time)
                {
                    if (entity is Person person)
                    {
                        person.Birth();
                        person.ApplyNeedDeltas(UtilityDeltas);
                        Release(entity);
                    }
                    else
                        throw new ArgumentException("entity is not a person and cannot give birth");
                }
            }
        }

        public override string GetName()
        {
            return "BirthAction";
        }
    }

    public class AgeAction : EntityAction
    {

        public AgeAction() : base(false) { }

        public override void Do(Entity entity)
        {
            entity.Age();
            End();
        }

        public override string GetName()
        {
            return "AgeAction";
        }
    }

    public class SearchZoneColorChange : EntityAction, ICheckNearbyObjects
    {

        public SearchZoneColorChange() : base(true) { }

        public void DoWithSearch(Entity entity, ObjectMesh objectMesh)
        {
            List<GameObject> nearbyObjects = objectMesh.GetObjectsInRange((int)entity.Position.X, (int)entity.Position.Y, 100);

            foreach (GameObject o in nearbyObjects)
            {
                if (o is Person p && p.Sex == Sex.Female)
                {
                    entity.SetColor(Color.Red);
                    return;
                }
            }

            entity.SetBaseColor();
        }

        public override string GetName()
        {
            return "SearchZoneColorChange";
        }
    }

    public class FollowAction : EntityAction
    {
        Entity target;
        readonly int distance;
        private readonly int dx;
        private readonly int dy;

        public FollowAction(Entity entity, int distance) : base(true)
        {
            // Target position can deviate by +- 5 to make following look more natural.
            dx = Utilities.Rng.Next(0, 11) - 5;
            dy = Utilities.Rng.Next(0, 11) - 5;
            this.distance = distance;
            target = entity;
        }

        public override void Do(Entity entity)
        {
            double distance = Math.Sqrt(Utilities.SquaredDistance(entity.Position, target.Position));
            if (distance > 30)
            {
                entity.GotoWithDistance(target.Position.X + dx, target.Position.Y + dy, 30);
            }
            else if (distance > entity.VisionRange)
            {
                End();
            }
            else
            {
                entity.ApplyNeedDeltas(UtilityDeltas);
            }
        }

        public override string GetName()
        {
            return "FollowAction";
        }
    }



    #region PlayerActions

    public class PlayerAttackAction : EntityAction, ICheckNearbyObjects
    {

        public PlayerAttackAction() : base(true) { }

        public void DoWithSearch(Entity entity, ObjectMesh objectMesh)
        {

                List<IMortal> nearbyObjects = objectMesh.GetMortalObjectsInRange(entity, entity.AttackRange);

                foreach (IMortal m in nearbyObjects)
                {
                    // People can only attack animals and animals can only attack people.
                    if (m != entity && (entity is Person && m is Animal || entity is Animal && m is Person))
                    {
                        entity.Attack(m);
                        //Console.WriteLine($"Health = {m.GetHealth()}");
                    }
                }

            End();
        }

        public override string GetName()
        {
            return "PlayerAttackAction";
        }
    }

    public class PlayerDropAction : EntityAction
    {
        private Player player;
        private RectangleF houseRectangle;
        private readonly int itemChoice;
        private readonly int itemCount;

        public PlayerDropAction(Player player, RectangleF houseRectangle, int itemChoice, int itemCount) : base(true)
        {
            this.player = player;
            this.houseRectangle = houseRectangle;
            this.itemChoice = itemChoice;
            this.itemCount = itemCount;
        }

        public override void Do(Entity entity)
        {
            if (itemChoice == -1)
            {
                IsActive = false;
                return;
            }

            if (entity is Person person)
            {
                if (player.GetItemChoice(itemChoice, out ItemType pickupType))
                {
                    bool inHouse = person.GetRectangleF().IntersectsWith(houseRectangle);

                    person.RemoveItem(new Item(0, 0, pickupType, 1), inHouse);
                    IsActive = false;
                }
            }
        }

        public override string GetName()
        {
            return "PlayerDropAction";
        }
    }

    // Player's mate action. Needs to search for gameobjects that are nearby and apply a delay.
    public class PlayerMateAction : EntityAction, ICheckNearbyObjectsWithTime
    {
        private bool timerEnabled = false;
        private GameTime stopTime = null;

        public PlayerMateAction() : base(false) { }


        public void DoWithSearchAndTime(Entity entity, ObjectMesh objectMesh, GameTime time)
        {
            if (timerEnabled && stopTime != null)
            {
                if (time >= stopTime)
                {
                    entity.SetCanMove(true);
                    End();
                }
                return;
            }

            if (entity is Person person)
            {
                int range = Person.mateRange;
                int mateRangeSquared = Person.mateRange * Person.mateRange;
                // Search all nearbyObjects for people to mate with. Then mate with all of them that are in range.
                List<GameObject> nearbyObjects = objectMesh.GetObjectsInRange(person.Position.X, person.Position.Y, range);
                foreach (GameObject gameObject in nearbyObjects)
                {
                    if (gameObject is Person mate && mate.Sex != person.Sex && !person.IsPregnant && !mate.IsPregnant
                        && Utilities.SquaredDistance(person.Position, mate.Position) < mateRangeSquared)
                    {
                        person.ReproduceWith(mate);
                        // Start timer. Initialize stopTime to the current time plus x number of seconds. Start checking time to allow player to move.
                        if (!timerEnabled)
                        {
                            timerEnabled = true;
                            person.SetCanMove(false);
                            stopTime = time.Copy();
                            stopTime.AddSeconds(Person.mateTime);
                        }
                    }
                }
            }

                if (!timerEnabled)
                    End();
        }

        public override string GetName()
        {
            return "PlayerMateAction";
        }
    }

    // Pickup all items that the player is on top of.
    public class PlayerPickupAction : EntityAction, ICheckNearbyObjects
    {

        public PlayerPickupAction() : base(true) { }

        public void DoWithSearch(Entity entity, ObjectMesh objectMesh)
        {
            if (entity is Person person)
            {
                List<Item> pickups = objectMesh.GetTypedObjectsInRange<Item>(entity, entitySize);

                foreach (Item p in pickups)
                    if (person.GetRectangleF().IntersectsWith(p.GetRectangleF()))
                    {
                        person.Pickup(p);
                    }
            }

            End();
        }

        public override string GetName()
        {
            return "PlayerPickupAction";
        }
    }

    // Pickup all items that the player is on top of.
    // Also checks what item is selected in the house if that is available.
    public class PlayerPickupActionWithHouseCheck : EntityAction, ICheckNearbyObjects
    {

        private Item housePickup;
        private House house;

        public PlayerPickupActionWithHouseCheck(Item selectedHousePickup, House house) : base(true)
        {
            housePickup = selectedHousePickup;
            this.house = house;
        }

        public void DoWithSearch(Entity entity, ObjectMesh objectMesh)
        {
            if (entity is Person person)
            {
                List<Item> pickups = objectMesh.GetTypedObjectsInRange<Item>(entity, entitySize);

                foreach (Item p in pickups)
                    if (person.GetRectangleF().IntersectsWith(p.GetRectangleF()))
                    {
                        person.Pickup(p);
                    }

                // If the person is inside the house, request the house's selected item.
                if (person.GetRectangleF().IntersectsWith(house.GetRectangleF()) && housePickup != null)
                {
                    person.RequestItemByType(housePickup.Type);
                }
            }

            End();
        }

        public override string GetName()
        {
            return "PlayerPickupActionWithHouseCheck";
        }
    }

    public class PlayerHarvestAction : EntityAction, ICheckNearbyObjects
    {

        public PlayerHarvestAction() : base(true)
        {

        }

        public void DoWithSearch(Entity entity, ObjectMesh objectMesh)
        {

            List<Plant> plants = objectMesh.GetTypedObjectsInRange<Plant>(entity, entity.Size);

            foreach (Plant plant in plants)
            {
                if (entity.GetRectangle().IntersectsWith(plant.GetRectangle())) // Entity needs to be touching the tree.
                {
                    plant.Harvest();
                }
            }
            

            IsActive = false;
        }

        public override string GetName()
        {
            return "PlayerHarvestAction";
        }
    }

    #endregion
    #endregion

    #region PlantActions
    public abstract class PlantAction : GameObjectAction
    {
        public abstract void Do(Plant plant);

    }

    public class GrowAction : PlantAction
    {
        public override void Do(Plant plant)
        {
            plant.Grow();
            IsActive = false;
        }

    }

    #endregion

    #region EffectActions
    public abstract class EffectAction : GameObjectAction
    {
        public abstract void Do(Effect effect);
    }

    public class DisposeAction : EffectAction
    {
        public override void Do(Effect effect)
        {
            effect.Dispose();
        }

    }
    #endregion
}
