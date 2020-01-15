using System;

namespace Tribe
{
    public class GameTime
    {
        public event EventHandler DayNightToggle;

        private int hoursInDay = 4;

        public bool IsDay { get; private set; }
        public int MinutesInDay { get; private set; }
        public int Day { get; private set; }
        public int Hour { get; private set; }
        public int Minute { get; private set; }
        public int Second { get; private set; }

        public GameTime(int minutesInDay)
        {
            IsDay = true;
            MinutesInDay = minutesInDay;
        }

        public GameTime(int minutesInDay, int day, int hour, int minute, int second)
        {
            MinutesInDay = minutesInDay;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
        }

        public GameTime(int minutesInDay, int day, int hour, int minute, int second, bool isDay) 
            : this(minutesInDay, day, hour, minute, second)
        {
            IsDay = isDay;
        }

        public GameTime Copy() { return new GameTime(MinutesInDay, Day, Hour, Minute, Second, IsDay); }

        public GameTime(int minutesInDay, DateTime time)
        {
            IsDay = true;
            MinutesInDay = minutesInDay;
            Day = time.Day;
            Hour = time.Hour;
            Minute = time.Minute;
            Second = time.Second;
        }

        public void AddSeconds(int seconds)
        {
            Second += seconds;
            Minute += Second / 60;
            Second %= 60;
            
            while (Minute >= MinutesInDay)
            {
                IsDay = !IsDay;
                DayNightToggle?.Invoke(this, null); // Raise event to let listeners know the day has switched.
                    
                Hour++;
                Minute -= MinutesInDay;
            }

            Day += Hour / hoursInDay;
            Hour %= hoursInDay;
        }

        public override string ToString()
        {
            return String.Format("{0,2}:{1,2}:{2,2}:{3,2}", Day.ToString("D2"), Hour.ToString("D2"), Minute.ToString("D2"), Second.ToString("D2"));
            //return $"{Day}:{Hour}:{Minute}:{Second}";
        }

        public static bool operator >=(GameTime time1, GameTime time2)
        {
            if (time1.Day > time2.Day)
                return true;
            else if (time1.Day == time2.Day)
            {
                if (time1.Minute > time2.Minute)
                    return true;
                else if (time1.Minute == time2.Minute)
                {
                    return time1.Second >= time2.Second;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static bool operator <=(GameTime time1, GameTime time2)
        {
            if (time1.Day < time2.Day)
                return true;
            else if (time1.Day == time2.Day)
            {
                if (time1.Minute < time2.Minute)
                    return true;
                else if (time1.Minute == time2.Minute)
                {
                    return time1.Second <= time2.Second;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static bool operator >(GameTime time1, GameTime time2)
        {
            if (time1.Day > time2.Day)
                return true;
            else if (time1.Day == time2.Day)
            {
                if (time1.Minute > time2.Minute)
                    return true;
                else if (time1.Minute == time2.Minute)
                {
                    return time1.Second > time2.Second;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static bool operator <(GameTime time1, GameTime time2)
        {
            if (time1.Day < time2.Day)
                return true;
            else if (time1.Day == time2.Day)
            {
                if (time1.Minute < time2.Minute)
                    return true;
                else if (time1.Minute == time2.Minute)
                {
                    return time1.Second < time2.Second;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is GameTime time2 && obj != null)
            {
                return Day == time2.Day & Hour == time2.Hour & Minute == time2.Minute & Second == time2.Second;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            var hashCode = -918288665;
            hashCode = hashCode * -1521134295 + hoursInDay.GetHashCode();
            hashCode = hashCode * -1521134295 + IsDay.GetHashCode();
            hashCode = hashCode * -1521134295 + MinutesInDay.GetHashCode();
            hashCode = hashCode * -1521134295 + Day.GetHashCode();
            hashCode = hashCode * -1521134295 + Hour.GetHashCode();
            hashCode = hashCode * -1521134295 + Minute.GetHashCode();
            hashCode = hashCode * -1521134295 + Second.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(GameTime time1, GameTime time2)
        {
            if (time1 is null)
            {
                return time2 is null;
            }

            return time1.Equals(time2);
        }

        public static bool operator !=(GameTime time1, GameTime time2)
        {
            if (time1 == null || time2 == null)
                return true;
            return time1.Day != time2.Day | time1.Hour != time2.Hour | time1.Minute != time2.Minute | time1.Second != time2.Second;
        }
    }
}
