using System;
using System.Collections.Generic;
using System.Linq;

namespace Tribe
{
    public class ObjectMesh
    {
        private List<GameObject>[,] gameObjectMesh;
        private int width;
        private int height;
        private int boxSize;
        private int meshWidth;
        private int meshHeight;

        // Initialize a mesh with the given dimensions and cell sizes and populate it with the given game objects.
        // Limitations: Adds objects only to the cell based on position and not their bounding box.
        //      This has one bonus! Don't have to check if the current object has already been checked because it can only exist in one cell.
        //      Fix this if you want to add collisions later...

        public ObjectMesh(int width, int height, int boxSize, List<GameObject> objects)
        {
            this.width = width;
            this.height = height;
            this.boxSize = boxSize;
            meshHeight = (int)Math.Ceiling(height / (double)boxSize) + 1;
            meshWidth = (int)Math.Ceiling(width / (double)boxSize) + 1;


            gameObjectMesh = new List<GameObject>[meshWidth, meshHeight];

            // Create collision/search mesh and populate it with entities, plants, etc.
            for (int i = 0; i < meshWidth; i++) // Access mesh using [object.X, object.Y]
            {
                for (int j = 0; j < meshHeight; j++)
                {
                    gameObjectMesh[i, j] = new List<GameObject>();
                }
            }

            foreach (GameObject o in objects)
            {
                if (o != null)
                {
                    int x = (int)(o.Position.X / boxSize);
                    int y = (int)(o.Position.Y / boxSize);

                    gameObjectMesh[x, y].Add(o);
                }
                
            }
        }

        public List<GameObject> GetCollisionObjects(double xPos, double yPos)
        {
            return GetObjectsInRange(xPos, yPos, boxSize);
        }

        // Get a list of game objects filtered to only include the specified type, and each object is casted to that type.
        public List<T> GetTypedObjectsInRange<T>(Entity entity, int range) where T : GameObject
        {
            return GetObjectsInRange(entity, range).Where(g => g is T).Select(g => (T)g).ToList();
        }

        public List<IMortal> GetMortalObjectsInRange(Entity entity, int range)
        {
            return GetObjectsInRange(entity, range).Where(g => g is IMortal).Select(g => (IMortal)g).ToList();
        }

        public List<GameObject> GetObjectsInRange(GameObject gameObject, int searchRadius) 
            => GetObjectsInRange(gameObject.Position.X, gameObject.Position.Y, searchRadius);

        public List<GameObject> GetObjectsInRange(OrderedPair<int> position, int searchRadius) => GetObjectsInRange(position.X, position.Y, searchRadius);

        public List<GameObject> GetObjectsInRange(OrderedPair<double> position, int searchRadius) => GetObjectsInRange(position.X, position.Y, searchRadius);

        public List<GameObject> GetObjectsInRange(double xPos, double yPos, int searchRadius) => GetObjectsInRange((int)xPos, (int)yPos, searchRadius);
        

        public List<GameObject> GetObjectsInRange(int xPos, int yPos, int searchRadius)
        {
            List<GameObject> nearbyObjects = new List<GameObject>();

            int boxRadius = searchRadius / boxSize; // Flatten boxes into boxes with zero width to simplify finding the correct indices to fill.
            boxRadius = boxRadius > 0 ? boxRadius : 1; // Make sure that some area is checked even if the searchRadius was smaller than the boxSize.
            int boxRSquared = boxRadius * boxRadius;
            int xBox = (int)Math.Round(xPos / (double)boxSize);
            int yBox = (int)Math.Round(yPos / (double)boxSize);


            for (int x = xBox - boxRadius; x < xBox; x++) // Check 2nd quadrant first then use symmetry to find the other 3.
            {
                for (int y = yBox - boxRadius; y < yBox; y++)
                {
                    int xDist = x - xBox + 1;
                    int yDist = y - yBox + 1;
                    if (xDist * xDist + yDist * yDist + boxRadius <= boxRSquared)
                    {

                        int xSym = xBox + xBox - x - 1;
                        int ySym = yBox + yBox - y - 1;

                        // 1st Quadrant.
                        if (xSym >= 0 && y >= 0 && xSym < meshWidth && y < meshHeight)
                        {
                            nearbyObjects.AddRange(gameObjectMesh[xSym, y]);
                        }
                        // 2nd Quadrant.
                        if (x >= 0 && y >= 0 && x < meshWidth && y < meshHeight)
                        {
                            nearbyObjects.AddRange(gameObjectMesh[x, y]);
                        }
                        // 3rd Quadrant.
                        if (x >= 0 && ySym >= 0 && x < meshWidth && ySym < meshHeight)
                        {
                            nearbyObjects.AddRange(gameObjectMesh[x, ySym]);
                        }
                        // 4th Quadrant.
                        if (xSym >= 0 && ySym >= 0 && xSym < meshWidth && ySym < meshHeight)
                        {
                            nearbyObjects.AddRange(gameObjectMesh[xSym, ySym]);
                        }
                    }
                }
            }

            return nearbyObjects;

        }

    }
}
