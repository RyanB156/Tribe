using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tribe
{
    public static class Utilities
    {
        public static Random Rng;
        public static string ResourceDirectory;
        public static Bitmap DefaultImage;

        // Dimensions of the view window.
        public static int ViewWidth = 10;
        public static int ViewHeight = 10;

        public static int WorldWidth = 10;
        public static int WorldHeight = 10;

        static Utilities()
        {
            Rng = new Random();
        }

        public static OrderedPair<int> GetRandomPointCloseToPoint(int x, int y, int dist)
        {
            return new OrderedPair<int>(x + Rng.Next(dist) * 2 - dist, y + Rng.Next(dist) * 2 - dist);
        }

        public static OrderedPair<int> GetRandomPoint()
        {
            return new OrderedPair<int>(Rng.Next(5, WorldWidth - 4), Rng.Next(5, WorldHeight - 4));
        }

        public static bool OutofBounds(OrderedPair<int> point)
        {
            return point.X < 0 || point.X > WorldWidth || point.Y < 0 || point.Y > WorldHeight;
        }

        public static string ToString(this CraftingComponent[] components)
        {
            StringBuilder sb = new StringBuilder();
            if (components.Length == 1)
            {
                sb.Append(string.Format($"{components[0].Type}: {components[0].Amount}"));
            }
            else
            {
                for (int i = 0; i < components.Length - 1; i++)
                {
                    sb.Append(string.Format($"{components[i].Type}: {components[i].Amount}, "));
                }
                sb.Append(string.Format($"{components.Last().Type}: {components.Last().Amount}"));
            }

            return sb.ToString();
        }

        // Get a random point inside the width and height analytically using polar coordinates.
        // Generate a random angle based on the collision direction and then use trigonmetry to calculate the maximum distance within the screen.
        // Point is (distance * sin(angle), distance * cos(angle))

        // Breaks when they get close to (0, 0) for some reason...
        public static OrderedPair<int> GetDirectionalPoint(Direction direction, OrderedPair<double> position)
        {
            if (direction == Direction.None)
            {
                Console.WriteLine("Direction was None inside Utilities.GetDirectionalPoint");
                return new OrderedPair<int>((int)position.X, (int)position.Y);
            }

            double theta1;
            double theta2;
            double angle;
            double distance;

            if (direction == Direction.Left)
            {
                theta1 = Math.Atan((position.Y - ViewHeight) / ViewWidth);
                theta2 = Math.Atan(position.Y / ViewWidth);
                angle = Rng.Next(-89, 90) * Math.PI / 180.0;

                if (angle > theta2)
                    distance = Math.Abs(position.Y / Math.Sin(angle));
                else if (angle < theta1)
                    distance = Math.Abs((ViewHeight - position.Y) / Math.Sin(angle));
                else
                    distance = Math.Abs(ViewWidth / Math.Cos(angle));
            }
            else if (direction == Direction.Up)
            {
                theta1 = -Math.PI + Math.Atan(ViewHeight / position.X);
                theta2 = Math.Atan(-ViewHeight / (ViewWidth - position.X));
                angle = Rng.Next(181, 360) * Math.PI / 180.0;
                angle -= (2.0 * Math.PI);

                if (angle > theta2)
                    distance = Math.Abs((ViewWidth - position.X) / Math.Cos(angle));
                else if (angle < theta1)
                    distance = Math.Abs(position.X / Math.Cos(angle));
                else
                    distance = Math.Abs(ViewHeight / Math.Sin(angle));
            }
            else if (direction == Direction.Right)
            {
                theta1 = Math.PI + Math.Atan(position.Y / -ViewWidth);
                theta2 = -Math.PI + Math.Atan((ViewHeight - position.Y) / ViewWidth);
                angle = Rng.Next(91, 270) * Math.PI / 180.0;
                

                if (angle > theta2 + 2.0 * Math.PI) // Adjust theta2 to positive again.
                    distance = Math.Abs((ViewHeight - position.Y) / Math.Sin(angle));
                else if (angle < theta1)
                    distance = Math.Abs(position.Y / Math.Sin(angle));
                else
                    distance = Math.Abs(ViewWidth / Math.Cos(angle));
            }
                    
            else // Direction.Down.
            {
                theta1 = Math.Atan(ViewHeight / (ViewWidth - position.X));
                theta2 = Math.PI - Math.Atan(ViewHeight / position.X);
                angle = Rng.Next(1, 180) * Math.PI / 180.0;

                if (angle > theta2)
                    distance = Math.Abs(position.X / Math.Cos(angle));
                else if (angle < theta1)
                    distance = Math.Abs((ViewWidth - position.X) / Math.Cos(angle));
                else
                    distance = Math.Abs(ViewHeight / Math.Sin(angle));

            }

            int x = Math.Max(2, (int)(Math.Abs(distance * Math.Cos(angle))));
            int y = Math.Max(2, (int)(Math.Abs(distance * Math.Sin(angle))));

            //int x = (int)(Math.Abs(distance * Math.Cos(angle)));
            //int y = (int)(Math.Abs(distance * Math.Sin(angle)));

            OrderedPair<int> destination = new OrderedPair<int>(x, y);
            //Console.WriteLine("Heading to {0},{1}", destination.X, destination.Y);
            //Console.WriteLine("T1: {0}, T2: {1}, angle: {2}, position: {3}", theta1 * 180.0 / Math.PI, theta2 * 180.0 / Math.PI, angle * 180.0 / Math.PI, position.ToString());
            return destination;
        }

        public static double SquaredDistance(OrderedPair<double> p1, OrderedPair<double> p2)
        {
            double xDist = p1.X - p2.X;
            double yDist = p1.Y - p2.Y;

            return xDist * xDist + yDist * yDist;
        }

        public static Bitmap GetResourceImage(string path)
        {
            try
            {
                return new Bitmap(ResourceDirectory + path);
            }
            catch (System.IO.FileNotFoundException)
            {
                return new Bitmap(ResourceDirectory + "default.png");
            }
        }

        public static bool AreSameBaseType(object a, object b)
        {
            if (a == null || b == null)
                return false;
            else
                return a.GetType().Equals(b.GetType());
        }

        public static bool SameBaseType(this object a, object b)
        {
            if (a == null || b == null)
                return false;
            else
                return a.GetType().Equals(b.GetType());
        }

    }

    public struct OrderedPair<T>
    {
        public readonly T X;
        public readonly T Y;
        public OrderedPair(T x, T y) { X = x; Y = y; }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }
    }


    public struct BoundingBox
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Buffer;

        public BoundingBox(int width, int height, int buffer)
        {
            Width = width;
            Height = height;
            Buffer = buffer;
        }
    }

    public class MyPanel : Panel
    {

        public MyPanel()
        {
            DoubleBuffered = true;
        }

    }

}
