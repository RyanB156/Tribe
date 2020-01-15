using System;
using System.Drawing;

namespace Tribe
{

    public enum Direction { None, Up, Right, Down, Left }


    public abstract class Entity : GameObject, IMortal
    {
        public static BoundingBox Boundary { get; protected set; }
        public static void SetBoundingBox(int width, int height)
        {
            Boundary = new BoundingBox(width, height, 5);
        }
        public static OrderedPair<int> HomePosition { get; protected set; }
        public static void SetHomePosition(int x, int y)
        {
            HomePosition = new OrderedPair<int>(x, y);
        }

        public abstract void SetBaseColor();

        public delegate void EffectEventHandler(object o, EffectEventArgs e);
        public event EffectEventHandler CreateEffect;

        public static readonly int adultSize = 10;
        public static readonly int childSize = 5;

        public EntityAction Action { get; protected set; }
        public bool ActionLocked { get; protected set; }
        public EntityAction IdleAction { get; protected set; }

        public EntityAction AttackThisAction { get; protected set; }
        public EntityAction FollowThisAction { get; protected set; }

        public int FollowDistance { get; protected set; } = 30;

        public int Health { get; protected set; } = 100;
        public virtual double Strength { get; protected set; }
        public int Damage { get; protected set; }
        public readonly int BaseDamage = 20;
        public readonly int AttackRange = 50;
        public Entity Attacker { get; protected set; }

        public double BaseSpeed { get; protected set; }
        public double Speed { get; protected set; }
        public bool CanMove { get; protected set; } = true;
        public OrderedPair<double> Velocity { get; protected set; }
        public Direction CollisionDirection { get; protected set; } = Direction.None;

        public double Hunger { get; protected set; } = 1.0;
        public double Social { get; protected set; } = 1.0;
        public double Lust { get; protected set; } = 1.0;
        public double Tiredness { get; protected set; } = 1.0;
        public double Boredom { get; protected set; } = 1.0;
        public double JobFullfilment { get; protected set; } = 1.0;
        public double Warmth { get; protected set; } = 1.0;

        public Color Color { get; protected set; }

        public int VisionRange { get; protected set; } = 100;
        public bool IsAdult { get; protected set; } = false;

        public Entity(double speed, int size, double x, double y, Color color, int ageSeconds)
            : base(x, y, size)
        {
            Speed = speed;
            BaseSpeed = speed;
            Damage = BaseDamage;
            IdleAction = new WanderAction();
            Color = color;
            Velocity = new OrderedPair<double>(0, 0);

            AttackThisAction = new AttackAction(this);
            FollowThisAction = new FollowAction(this, FollowDistance);

            base.OnScheduleEvent(this, new ScheduleEventArgs(ageSeconds, new AgeAction()));
        }

        public void Attack(IMortal mortal)
        {
            if (mortal == null)
            {
                Console.WriteLine("Mortal was null in Entity.Attack");
                return;
            }
            mortal.TakeDamage((int)(Damage * Strength), this); // Let the object receiving damage know which object is attacking it.

            //OnCreateEffect(EffectType.Attack);
            Color = Color.Black;
            base.OnScheduleEvent(this, new ScheduleEventArgs(1, new ResetColorAction()));
        }

        public int GetHealth()
        {
            return Health;
        }

        public void Heal(int amount)
        {
            Health += amount;
        }

        public virtual void TakeDamage(int damage, Entity attacker)
        {
            // If the entity is waiting and the action that triggered the wait can be interrupred, e.g. sleeping, playing, talking
            // then the action will be wiped so PersonBrain can asign a new one. 
            // It should flee from the attacker, then it can decide to fight back or not.
            if (Action != null)
                if (Action.IsInterruptible)
                {
                    ActionLocked = false;
                    Action.End();
                }

            // Turn entity red temporarily to show damage taken.
            SetColor(Color.Red);
            base.OnScheduleEvent(this, new ScheduleEventArgs(1, new ResetColorAction()));
            Health -= damage;
            Attacker = attacker;
        }

        public virtual void SetActionLockState(bool lockState) { ActionLocked = lockState; }

        public virtual bool TrySetAction(EntityAction newAction, bool locked = false)
        {
            if (Utilities.AreSameBaseType(Action, newAction)) // Don't assign an action of the same type.
            {
                return false;
            }
            else
            {
                if (Action != null && !Action.IsActive)
                    Action.End();

                Action = newAction;
                ActionLocked = locked;
                return true;
            }
        }

        public void DoActionOnce(EntityAction action)
        {
            action.Do(this);
        }

        // Make the entity do its action. Checks if the action was completed. 
        // If so, trys to set a new one from the stack or set to default WanderAction.
        public virtual void CarryOutAction(ObjectMesh objectMesh, GameTime gameTime)
        {
            if (Action != null) // Entity controller can set actions to null or change them if it wants.
            {
                if (Action is ICheckNearbyObjectsWithTime ct)
                {
                    ct.DoWithSearchAndTime(this, objectMesh, gameTime);
                }
                else if (Action is EntityTimeAction t)
                {
                    t.DoWithTime(this, gameTime);
                }
                else if (Action is ICheckNearbyObjects c)
                {
                    c.DoWithSearch(this, objectMesh);
                }
                else // Action is normal EntityAction. 
                {
                    Action.Do(this);
                }

                if (!Action.IsActive) // The action has reached an ending condition.
                {
                    ActionLocked = false;
                    CanMove = true; // Allow the person to move here just in case I forget to reset this inside of an action...
                    Action = null;
                }
            }
        }

        public virtual double GetNeedValue(Need need)
        {
            switch (need)
            {
                case Need.Hunger: return Hunger;
                case Need.Social: return Social;
                case Need.Lust: return Lust;
                case Need.Tiredness: return Tiredness;
                case Need.Boredom: return Boredom;
                case Need.JobFullfilment: return JobFullfilment;
                default: return double.MinValue;
            }
        }

        public virtual void ApplySleepDelta()
        {
            Tiredness = 1.0;
            Health = 100; // Restore health when the Person sleeps.   
        }

        public void ApplyNeedDeltas(Tuple<Need, double>[] needDeltas)
        {
            if (needDeltas == null)
            {
                //Console.WriteLine("Need Deltas were null for game object {0}", this.ObjectID);
                return;
            }
            foreach (var deltaPair in needDeltas)
            {
                double delta = deltaPair.Item2;
                switch (deltaPair.Item1)
                {
                    case Need.Hunger: Hunger = Math.Min(1.0, Hunger + delta); break;
                    case Need.Social: Social = Math.Min(1.0, Social + delta); break;
                    case Need.Lust: Lust = Math.Min(1.0, Lust + delta); break;
                    case Need.Tiredness: Tiredness = Math.Min(1.0, Tiredness + delta); break;
                    case Need.Boredom: Boredom = Math.Min(1.0, Boredom + delta); break;
                    case Need.JobFullfilment: JobFullfilment = Math.Min(1.0, JobFullfilment + delta); break;
                }
            }
        }

        // Raise the CreateEffect event to let the entity controller know that an animation graphic should be added to the game.
        public void OnCreateEffect(EffectType type, bool isFinite)
        {
            EffectEventHandler handler = CreateEffect;
            Effect e = new Effect((int)Position.X, (int)Position.Y, type, CollisionDistance, isFinite);
            handler?.Invoke(this, new EffectEventArgs(e));
        }

        public void OnCreateEffect(Effect effect)
        {
            EffectEventHandler handler = CreateEffect;
            handler?.Invoke(this, new EffectEventArgs(effect));
        }

        public void Age() { IsAdult = true; }

        public int GetSize() => IsAdult ? adultSize : childSize;

        public void Stop()
        {
            Velocity = new OrderedPair<double>(0, 0);
        }

        public void SetCanMove(bool canMove)
        {
            CanMove = canMove;
        }

        public void RemoveAttacker()
        {
            Attacker = null;
        }

        // This might not be necessary. Relative rendering with the scale should stretch the perceived distance without changing the absolute distance.
        public void ScaleSpeed(double scale)
        {
            Speed = BaseSpeed;
        }

        public bool Move()
        {
            if (!CanMove)
                return false;

            double nextXPosition = Position.X + Speed * Velocity.X;
            double nextYPosition = Position.Y + Speed * Velocity.Y;
            bool notCollided = true;

            // Project the next position to ensure it does not cross a boundary.
            if (nextXPosition < 0 + Boundary.Buffer) // About to cross left boundary: clear x velocity.
            {
                Velocity = new OrderedPair<double>(0, Velocity.Y);
                notCollided = false;
                CollisionDirection = Direction.Left;
            }
            else if (nextXPosition > Boundary.Width - Boundary.Buffer) // About to cross right boundary: clear x velocity.
            {
                Velocity = new OrderedPair<double>(0, Velocity.Y);
                notCollided = false;
                CollisionDirection = Direction.Right;
            }

            if (nextYPosition < 0 + Boundary.Buffer) // About to cross top boundary: clear y velocity.
            {
                Velocity = new OrderedPair<double>(Velocity.X, 0);
                notCollided = false;
                CollisionDirection = Direction.Up;
            }
            else if (nextYPosition > Boundary.Height - Boundary.Buffer) // About to cross bottom boundary: clear y velocity.
            {
                Velocity = new OrderedPair<double>(Velocity.X, 0);
                notCollided = false;
                CollisionDirection = Direction.Down;
            }

            if (notCollided)
                Position = new OrderedPair<double>(nextXPosition, nextYPosition); // Next Position is within the boundary.
            return notCollided;
        }

        public void SetVelocity(OrderedPair<double> velocity)
        {
            Velocity = velocity;
        }

        public bool Goto(double x, double y) => GotoWithDistance(x, y, CollisionDistance);

        // Apply velocity to an entity to make it move. Calls the move method to apply the velocity.
        public bool GotoWithDistance(double x, double y, int distance)
        {
            if (!GetRectangleF().IntersectsWith(new RectangleF((float)x, (float)y, distance, distance)))
            {
                double dx = x - Position.X;
                double dy = y - Position.Y;

                double radAngle = Math.Atan(dy / dx);

                double vx = Speed * Math.Cos(radAngle);
                double vy = Speed * Math.Sin(radAngle);

                if (dx < 0)
                {
                    vx = -vx;
                    vy = -vy;
                }

                // Set Velocity and make the entity move.
                SetVelocity(new OrderedPair<double>(vx, vy));
                return Move();
            }
            return false;
        }

        public RectangleF GetSearchRadius(int radius)
        {
            return new RectangleF((float)Position.X - radius, (float)Position.Y - radius, radius * 2, radius * 2);
        }

        public void SetColor(Color color) { Color = color; }

        
    }
}
