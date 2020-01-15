using System;

namespace Tribe
{
    public class ActionGroup : IComparable
    {

        public static int Compare(ActionGroup a1, ActionGroup a2)
        {
            if (a1.time < a2.time)
                return -1;
            else if (a1.time > a2.time)
                return 1;
            else
                return 0;
        }

        public int CompareTo(object obj)
        {
            return Compare(this, (ActionGroup)obj);
        }

        public readonly GameTime time;
        public readonly GameObjectAction action;
        public readonly GameObject gameObject;

        public ActionGroup(GameTime t, GameObjectAction a, GameObject e)
        {
            time = t;
            action = a;
            gameObject = e;
        }
    }
}
