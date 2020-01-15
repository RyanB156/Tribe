using System.Collections.Generic;
using System.Drawing;

namespace Tribe
{

    /*
     *      All GameObjects are drawn to the screen using their rectangle and color/image
     *      AppleTrees have apples on them that have to be rendered separately using a loop in PlantController
     *      
     *      Abstracting this to a RenderContext object, with associated rendering classes would make this more extensible
     *      Draw apple tree and all of its apples with one call in PlantController with appleTree.RenderContext
     *      
     *      Could also be used to draw items and weapons when a person is holding them
     *      
     *      Source - base of the Render. Start drawing relative to Source.Position
     *      Accessories - list of things that need to be rendered above Source, like apples
     *          May or not need custom sizing and offsets for this
     *      
     *      
     */


    public class RenderContext
    {
        public GameObject Source { get; protected set; }
        public List<GameObject> Accessories { get; protected set; }

        public RenderContext(GameObject source)
        {
            Source = source;
            Accessories = new List<GameObject>();
        }

        public RenderContext(GameObject source, List<GameObject> gameObjects)
        {
            Source = source;
            Accessories = gameObjects;
        }

        public void AddAccessory(GameObject gameObject)
        {
            Accessories.Add(gameObject);
        }

        public void RemoveAccessory(GameObject gameObject)
        {
            Accessories.Remove(gameObject);
        }

        public void ClearAccessories()
        {
            Accessories.Clear();
        }

    }

    /*
     *      Class to apply the RenderContext for each entity/plant in the respective controller classes
     *      May or not be necessary because animals will be drawn without accessories
     */

    public abstract class Renderer
    {
        public abstract void DrawRender(RenderContext renderContext, double scale, OrderedPair<double> playerPosition, 
            OrderedPair<double> screenCenter, Graphics graphics);
    }

    public class DefaultEntityRenderer : Renderer
    {
        public override void DrawRender(RenderContext renderContext, double scale, OrderedPair<double> playerPosition, 
            OrderedPair<double> screenCenter, Graphics graphics)
        {
            Entity source = (Entity)renderContext.Source;
            double xOffset = source.Position.X - playerPosition.X;
            double yOffset = source.Position.Y - playerPosition.Y;

            graphics.FillRectangle(new SolidBrush(source.Color), source.GetRectangleFWithOffset(screenCenter, xOffset, yOffset, scale));

            foreach (GameObject acc in renderContext.Accessories)
            {
                // Draw accessories relative to the center of each Entity. The center will not move under scaling.
                if (acc != null)
                    graphics.DrawImage(acc.GetImage(), acc.GetRectangleFWithOffset(screenCenter, acc.Position.X + xOffset, 
                        acc.Position.Y + yOffset, scale));
            }
        }
    }

    public class DefaultPictureRenderer : Renderer
    {
        public override void DrawRender(RenderContext renderContext, double scale, OrderedPair<double> playerPosition, 
            OrderedPair<double> screenCenter, Graphics graphics)
        {
            // OrderedPair<double> offsetPosition = new OrderedPair<double>(source.Position.X + xOffset * scale, source.Position.Y + yOffset * scale);

            GameObject source = renderContext.Source;
            double xOffset = source.Position.X - playerPosition.X;
            double yOffset = source.Position.Y - playerPosition.Y;

            // Draw source object with accessories on top.
            if (source.RenderMode == GameObject.RenderContextMode.AccessoryFirst)
            {
                graphics.DrawImage(source.GetImage(), source.GetRectangleFWithOffset(screenCenter, xOffset, yOffset, scale));
                foreach (GameObject acc in renderContext.Accessories)
                {
                    if (acc != null)
                        graphics.DrawImage(acc.GetImage(), acc.GetRectangleFWithOffset(screenCenter, acc.Position.X + xOffset,
                            acc.Position.Y + yOffset, scale));
                }
            }
            else // Draw accessories with source object on top.
            {
                foreach (GameObject acc in renderContext.Accessories)
                {
                    if (acc != null)
                        graphics.DrawImage(acc.GetImage(), acc.GetRectangleFWithOffset(screenCenter, acc.Position.X + xOffset,
                            acc.Position.Y + yOffset, scale));
                }
                graphics.DrawImage(source.GetImage(), source.GetRectangleFWithOffset(screenCenter, xOffset, yOffset, scale));
            }
        }
    }
}
