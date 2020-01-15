using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tribe
{
    class Wearable : GameObject
    {

        public Wearable(double x, double y, int size) : base(x, y, size)
        {

        }

        public override List<ActionUtility> GetAdvertisedActions(Entity entity, double mod)
        {
            return new List<ActionUtility>();
        }

        public override Bitmap GetImage()
        {
            throw new NotImplementedException();
        }
    }
}
