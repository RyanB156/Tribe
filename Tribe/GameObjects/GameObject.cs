using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tribe
{

	public class ScheduleEventArgs : EventArgs
	{

		public int SecondsUntilAction { get; }
		public GameObjectAction Action { get; }
		public bool SetScheduleFlag { get; }
		public ScheduleEventArgs(int seconds, GameObjectAction action, bool setScheduleFlag = true)
		{
			SecondsUntilAction = seconds;
			Action = action;
			SetScheduleFlag = setScheduleFlag;
		}
	}

	// Has a position and size. Can be rendered to the screen. Contains actions that can be scheduled.
	public abstract class GameObject
	{

		private static int nextObjectID = 0;

		public delegate void ScheduleEventHandler(GameObject gameObject, ScheduleEventArgs e);
		public event ScheduleEventHandler ScheduleEvent;

		public OrderedPair<double> Position { get; protected set; }
		public int CollisionDistance { get; protected set; }
		public int Size { get; protected set; }
		public int ObjectID { get; protected set; } // Unique ID for each gameobject created.

		public RenderContext RenderContext { get; protected set; }
		public enum RenderContextMode { AccessoryFirst, ObjectFirst }
		public RenderContextMode RenderMode { get; protected set; }

		public GameObject(double x, double y, int size)
		{
			Position = new OrderedPair<double>(x, y);
			Size = size;
			CollisionDistance = Size / 2;

			RenderContext = new RenderContext(this);
			RenderMode = RenderContextMode.AccessoryFirst;

			ObjectID = nextObjectID;
			nextObjectID++;
		}


		// All GameObjects have to advertise a list of actions to Persons.
		public abstract List<ActionUtility> GetAdvertisedActions(Entity entity, double mod);


		public virtual RectangleF GetRectangleF()
		{
			return new RectangleF((float)Position.X - CollisionDistance, (float)Position.Y - CollisionDistance, Size, Size);
		}

		public virtual RectangleF GetRectangleF(double scale)
		{
			float scaledSize = (float)(Size * scale);
			float scaledOffset = (float)(CollisionDistance * scale);
			return new RectangleF((float)Position.X - scaledOffset, (float)(Position.Y - scaledOffset), scaledSize, scaledSize);
		}

		public virtual Rectangle GetRectangle()
		{
			return new Rectangle((int)Position.X - CollisionDistance, (int)Position.Y - CollisionDistance, Size, Size);
		}

		public virtual Rectangle GetRectangle(double scale)
		{
			int scaledSize = (int)(Size * scale);
			int scaledOffset = scaledSize / 2;
			return new Rectangle((int)Position.X - scaledOffset, (int)Position.Y - scaledOffset, scaledSize, scaledSize);
		}

		// Returns the rectangle bounding box for the GameObject shifted by the x and y offsets away from the source position. The offsets are scaled.
		public virtual RectangleF GetRectangleFWithOffset(OrderedPair<double> sourcePosition, double xOffset, double yOffset, double scale)
		{
			return new RectangleF((float)(sourcePosition.X + (xOffset - CollisionDistance) * scale), 
				(float)(sourcePosition.Y + (yOffset - CollisionDistance) * scale), 
				(float)(scale * Size), (float)(scale * Size));
		}

		public abstract Bitmap GetImage();

		public virtual void OnScheduleEvent(GameObject gameObject, ScheduleEventArgs e)
		{
			//Console.WriteLine("GameObject.OnScheduleEvent");

			ScheduleEventHandler handler = ScheduleEvent;
			handler?.Invoke(gameObject, e);
		}

	}

	public interface IMortal
	{
		int GetHealth();
		void TakeDamage(int damage, Entity attacker);
		void Heal(int amount);
	}

	public class DataChangedArgs : EventArgs
	{
		
		public readonly ChangeType Type;
		public readonly string ElementName;
		public readonly object Value; // Only used for NewItem and UpdateItem.

		public DataChangedArgs(ChangeType type, string elementName, object value)
		{
			Type = type;
			ElementName = elementName;
			Value = value;
		}
	}

	public enum ChangeType
	{
		NewElement, // Create new row with event mappings and use "Value".
		UpdateElement, // Change existing row using "Value" as the modifier +-. Ensure the value always > 0.
		RemoveElement // Delete mappings and let display panel remove the row.
	}

	public delegate void DataChangedHandler(object sender, DataChangedArgs e);

	public class ObjectData
	{
		public List<Tuple<string, object>> DataList { get; private set; }
		public int FixedCount { get; private set; }

		public ObjectData(List<Tuple<string, object>> dataList, int fixedCount)
		{
			DataList = dataList;
			FixedCount = fixedCount;
		}
	}

	public interface IGetData
	{
		ObjectData GetData();
		event DataChangedHandler UpdateElement;
		event EventHandler CancelData;
		int GetItemIndex();
		void IncrementItemIndex();
		void DecrementItemIndex();
	}

	public class House : GameObject
	{
		private readonly Bitmap image;

		public House(double x, double y, int size) : base(x, y, size)
		{
			image = new Bitmap(Utilities.ResourceDirectory + "house.png");
		}

		public override List<ActionUtility> GetAdvertisedActions(Entity entity, double mod)
		{
			List<ActionUtility> actionAds = new List<ActionUtility>();

			if (entity is Person)
			{
				actionAds.Add(new ActionUtility(new GuardBaseAction(Position, 800), new Tuple<Need, double>[]
				{
					ActionUtility.NewPair(Need.JobFullfilment, DDeltaConfig.guardBaseDelta)
				}));
			}
			
			return actionAds;
		}

		public override Bitmap GetImage()
		{
			return image;
		}
	}

	public class Halo : GameObject
	{
		private readonly Bitmap image;

		public Halo(double x, double y, int size) : base(x, y, size)
		{
			image = Utilities.GetResourceImage("radialgradient.png");
		}

		public override List<ActionUtility> GetAdvertisedActions(Entity entity, double mod)
		{
			return new List<ActionUtility>();
		}

		public override Bitmap GetImage()
		{
			return image;
		}
	}

	// Campfire object that contains a campfire image with light halo effect. It applies warmth to people in proximity and scares away animals.
	public class Campfire : GameObject
	{
		private readonly Bitmap image;

		public ActionUtility FleeFromThisUtility { get; private set; }

		public Campfire(int x, int y) : base(x, y, 10)
		{
			Size = 25;
			RenderMode = RenderContextMode.ObjectFirst;
			image = Utilities.GetResourceImage("campfirelit.png");
			RenderContext.AddAccessory(new Halo(13, 13, 200));

			FleeFromThisUtility = new ActionUtility(new FleeAction(this), new Tuple<Need, double>[]
			{
			    ActionUtility.NewPair(Need.JobFullfilment, 0.0025)
			});
		}

		public override List<ActionUtility> GetAdvertisedActions(Entity entity, double mod)
		{
			List<ActionUtility> actionAds = new List<ActionUtility>();

			// Make bears and wolves run from the campfire.
			if (entity is Animal a)
			{
				if (a.Type == AnimalType.Bear || a.Type == AnimalType.Wolf)
				{
					actionAds.Add(FleeFromThisUtility);
				}
			}

			return actionAds;
		}

		public override Bitmap GetImage()
		{
			return image;
		}
	}

}
