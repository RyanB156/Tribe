using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tribe
{

    public enum EffectType { Love, Attack, Death, Sleep }

    public class EffectEventArgs : EventArgs
    {
        public readonly Effect Effect;
        public EffectEventArgs(Effect effect) { Effect = effect; }
    }

    public class Effect : GameObject, IDisposable
    {
        public static Bitmap loveEffectImage = new Bitmap(Utilities.ResourceDirectory + "heart.png");
        public static Bitmap attackEffectImage = new Bitmap(Utilities.ResourceDirectory + "attack.gif");
        public static Bitmap sleepEffectImage = new Bitmap(Utilities.ResourceDirectory + "sleep.png");

        private static readonly int size = 20;
        private static readonly int attackSize = 50;

        public static Bitmap GetBitmap(EffectType type)
        {
            switch (type)
            {
                case EffectType.Love: return loveEffectImage;
                case EffectType.Attack: return attackEffectImage;
                case EffectType.Sleep: return sleepEffectImage;
                default: return Utilities.DefaultImage;
            }
        }

        public static int GetSize(EffectType type)
        {
            if (type == EffectType.Attack)
                return attackSize;
            else
                return size;
        }

        public event EventHandler Disposed;

        public EffectType Type { get; private set; }
        public readonly bool IsFinite;

        public Effect(int x, int y, EffectType type, int offset, bool isFinite) : base(x, y, GetSize(type))
        {
            Type = type;
            Position = new OrderedPair<double>(x + offset - CollisionDistance, y - offset - CollisionDistance);
            IsFinite = isFinite;
        }

        public void TrySetAction(EffectAction action)
        {
            action.Do(this);
        }

        private void OnDispose()
        {
            EventHandler handler = Disposed;
            handler?.Invoke(this, null);
        }

        public void Dispose()
        {
            OnDispose();
        }

        public override Bitmap GetImage()
        {
            return GetBitmap(Type);
        }

        public override List<ActionUtility> GetAdvertisedActions(Entity entity, double mod)
        {
            return new List<ActionUtility>();
        }
    }
}
