using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tribe
{
    static class ExtensionMethods
    {

        public static List<object> GetValues(this IGetData dataObject)
        {
            return dataObject.GetData().DataList.Select(pair => pair.Item2).ToList();
        }

        public static T RandomChoice<T>(this List<T> list, Random rng)
        {
            return list[rng.Next(list.Count)];
        }

        public static void Print<T>(this IEnumerable<T> e)
        {
            if (e.Count() == 0)
            {
                Console.WriteLine("Collection is empty");
                return;
            }

            for (int i = 0; i < e.Count() - 1; i++)
            {
                Console.Write("{0}, ", e.ElementAt(i));
            }
            Console.WriteLine(e.ElementAt(e.Count() - 1));
        }

        // Convert any IEnumerable collection to a string. [] -> "", [1, 2, 3] -> "1, 2, 3".
        public static string CollectionToString<T>(this IEnumerable<T> e)
        {
            StringBuilder sb = new StringBuilder();

            if (e.Count() == 0)
                return "";
            else
            {
                for (int i = 0; i < e.Count() - 1; i++)
                {
                    sb.Append(string.Format("{0}, ", e.ElementAt(i).ToString()));
                }
                sb.Append(e.ElementAt(e.Count() - 1).ToString());

                return sb.ToString();
            }
        }

        public static int Dot(this OrderedPair<int> p1, OrderedPair<int> p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }

        public static double Dot(this OrderedPair<double> p1, OrderedPair<double> p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }

        public static OrderedPair<int> Plus(this OrderedPair<int> p1, OrderedPair<int> p2)
        {
            return new OrderedPair<int>(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static OrderedPair<double> Plus(this OrderedPair<double> p1, OrderedPair<double> p2)
        {
            return new OrderedPair<double>(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static OrderedPair<int> Minus(this OrderedPair<int> p1, OrderedPair<int> p2)
        {
            return new OrderedPair<int>(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static OrderedPair<double> Minus(this OrderedPair<double> p1, OrderedPair<double> p2)
        {
            return new OrderedPair<double>(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static bool OneSatisfies<T>(this List<T> list, Predicate<T> predicate)
        {
            foreach (T elem in list)
            {
                if (predicate(elem))
                    return true;
            }

            return false;
        }

        public static bool AllSatisfy<T>(this IEnumerable<T> e, Predicate<T> predicate)
        {
            foreach (T elem in e)
            {
                if (!predicate(elem))
                    return false;
            }
            return true;
        }
    }
}
