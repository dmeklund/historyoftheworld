using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;

namespace ParseWiki
{
    public enum DateGranularity
    {
        Minute,
        Hour,
        Day,
        Month,
        Year
    }

    public enum DateComponent
    {
        Time,
        Day,
        Month,
        Year,
        Epoch,
        Timezone
    }

    public enum Epoch
    {
        BC,
        AD
    }

    public class Range<T>
    {
        public Range(T start, T end)
        {
            End = end;
            Start = start;
        }

        public T Start { get; }
        public T End { get; }
    }

    public class AmbiguousDateTime
    {
        public static void Combine(AmbiguousDateTime comp1, AmbiguousDateTime comp2)
        {
            CopyTo(comp1, comp2);
            CopyTo(comp2, comp1);
        }
        public static void CopyTo(AmbiguousDateTime from, AmbiguousDateTime to)
        {
            to.Year ??= from.Year;
            to.Month ??= from.Month;
            to.Day ??= from.Day;
            to.Hour ??= from.Hour;
            to.Minute ??= from.Minute;
            to.Epoch ??= from.Epoch;
        }
        
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public int? Hour { get; set; }
        public int? Minute { get; set; }
        public Epoch? Epoch { get; set; }

        public bool FullyAmbiguous => Minute == null && Hour == null && Day == null && Month == null && Year == null;

        public DateGranularity Granularity
        {
            get
            {
                if (Minute != null) return DateGranularity.Minute;
                if (Hour != null) return DateGranularity.Hour;
                if (Day != null) return DateGranularity.Day;
                if (Month != null) return DateGranularity.Month;
                if (Year != null) return DateGranularity.Year;
                else throw new Exception("Fully ambiguous");
            }
        }
        
        public void Set(DateComponent component, object value)
        {
            switch (component)
            {
                case DateComponent.Time:
                    var time = (DateTime) value;
                    Hour = time.Hour;
                    Minute = time.Minute;
                    break;
                case DateComponent.Day:
                    Day = (int) value;
                    break;
                case DateComponent.Month:
                    Month = (int) value;
                    break;
                case DateComponent.Year:
                    Year = (int) value;
                    break;
                case DateComponent.Epoch:
                    Epoch = (Epoch) value;
                    break;
                case DateComponent.Timezone:
                    throw new NotImplementedException("timezone");
                default:
                    throw new ArgumentOutOfRangeException(nameof(component), component, null);
            }
        }

        public PWDateTime ToPWDateTime()
        {
            return new PWDateTime(
                Year.GetValueOrDefault(1),
                Month.GetValueOrDefault(1),
                Day.GetValueOrDefault(1),
                Hour.GetValueOrDefault(0),
                Minute.GetValueOrDefault(0),
                0,
                Epoch.GetValueOrDefault(ParseWiki.Epoch.AD));
        }
    }

    public readonly struct PWDateTime
    {
        public PWDateTime(
            int year,
            int month = 1,
            int day = 1,
            int hour = 0,
            int minute = 0,
            int second = 0,
            Epoch epoch = Epoch.AD)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
            Epoch = epoch;
        }

        public PWDateTime(DateTime time)
        {
            Year = time.Year;
            Month = time.Month;
            Day = time.Day;
            Hour = time.Hour;
            Minute = time.Minute;
            Second = time.Second;
            Epoch = Epoch.AD;
        }

        public int YearWithEpoch
        {
            get
            {
                switch (Epoch)
                {
                    case Epoch.AD:
                        return Year;
                    case Epoch.BC:
                        return -Year;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public int Year { get; }
        public int Month { get; }
        public int Day { get; }
        public int Hour { get; }
        public int Minute { get; }
        public int Second { get; }
        public Epoch Epoch { get; }

        public override string ToString()
        {
            try
            {
                var result = new DateTime(Year, Month, Day, Hour, Minute, Second).ToString(CultureInfo.InvariantCulture);
                if (Epoch == Epoch.BC)
                    result = result + " BC";
                return result;
            }
            catch (ArgumentOutOfRangeException exc)
            {
                Console.WriteLine();
                return null;
            }
        }
    }
    
    public class DateRange
    {
        public static PWDateTime MinimumForGranularity(PWDateTime time, DateGranularity granularity)
        {
            return granularity switch
            {
                DateGranularity.Minute => new PWDateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, epoch:time.Epoch),
                DateGranularity.Hour => new PWDateTime(time.Year, time.Month, time.Day, time.Hour, epoch:time.Epoch),
                DateGranularity.Day => new PWDateTime(time.Year, time.Month, time.Day, epoch:time.Epoch),
                DateGranularity.Month => new PWDateTime(time.Year, time.Month, epoch:time.Epoch),
                DateGranularity.Year => new PWDateTime(time.Year, epoch:time.Epoch),
                _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, null)
            };
        }

        public static PWDateTime MaximumForGranularity(PWDateTime time, DateGranularity granularity)
        {
            return granularity switch
            {
                DateGranularity.Minute => new PWDateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 59, epoch:time.Epoch),
                DateGranularity.Hour => new PWDateTime(time.Year, time.Month, time.Day, time.Hour, 59, 59, epoch:time.Epoch),
                DateGranularity.Day => new PWDateTime(time.Year, time.Month, time.Day, 23, 59, 59, epoch:time.Epoch),
                DateGranularity.Month => new PWDateTime(time.Year, time.Month, DateTime.DaysInMonth(time.Year, time.Month), 23, 59, 59, epoch:time.Epoch),
                DateGranularity.Year => new PWDateTime(time.Year, 12, 31, 23, 59, 59, epoch:time.Epoch),
                _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, null)
            };
        }
        
        public static DateRange Parse(string input)
        {
            DateGranularity? granularity = null;
            var tokens = Tokenize(input);
            if (DateTime.TryParse(input, out var dateTime))
            {
                if (tokens.Count == 1)
                    granularity = DateGranularity.Year;
                else if (tokens.Count == 2)
                    granularity = DateGranularity.Month;
                else if (tokens.Count == 3)
                    granularity = DateGranularity.Day;
                if (granularity != null)
                {
                    var startTime = MinimumForGranularity(new PWDateTime(dateTime), granularity.Value);
                    var endTime = MaximumForGranularity(startTime, granularity.Value);
                    return new DateRange(startTime, endTime, granularity.Value);
                }
            }

            var separator = input.IndexOfAny(new char[] {'-', '–'});
            AmbiguousDateTime start, end;
            if (separator == -1)
            {
                start = AmbiguousParse(input);
                end = start;
            }
            else
            {
                start = AmbiguousParse(input.Substring(0, separator));
                end = AmbiguousParse(input.Substring(separator + 1));
                AmbiguousDateTime.Combine(start, end);
            }

            if (start.FullyAmbiguous)
            {
                // Console.WriteLine($"{input} is fully ambiguous");
                return null;
            }
            granularity = start.Granularity;
            return new DateRange(
                MinimumForGranularity(start.ToPWDateTime(), granularity.Value),
                MaximumForGranularity(end.ToPWDateTime(), granularity.Value),
                granularity.Value
            );

        }

        private static AmbiguousDateTime AmbiguousParse(string input)
        {
            var datetime = new AmbiguousDateTime();
            var tokens = Tokenize(input);
            var foundDay = false;
            foreach (var token in tokens)
            {
                // FIXME
                if (!token.IsCertain)
                {
                    if (token.CouldBe(DateComponent.Day) && !foundDay)
                    {
                        token.SetIsCertainly(DateComponent.Day, int.Parse(token.Contents));
                        foundDay = true;
                    }
                    else if (token.CouldBe(DateComponent.Year))
                    {
                        token.SetIsCertainly(DateComponent.Year, int.Parse(token.Contents));
                    }
                }

                if (token.IsCertain)
                {
                    datetime.Set(token.CertainComponent, token.CertainValue);
                    if (token.CertainComponent == DateComponent.Day)
                        foundDay = true;
                }
            }
            return datetime;
        }

        private static IReadOnlyList<Token> Tokenize(string input)
        {
            var tokens = input.Split(" ");
            var result = new List<Token>(
                tokens.Select(token => new Token(token))
            );
            return result;
        }

        private class Token
        {
            public string Contents { get; }
            public bool IsCertain => _certainVal.Count > 0;
            private Dictionary<DateComponent, object> _possibleValues;
            private Dictionary<DateComponent, object> _certainVal;
            
            public bool CouldBe(DateComponent component) => _possibleValues.ContainsKey(component);
            public void SetCouldBe(DateComponent component, object val)
            {
                if (_certainVal.Count == 0)
                    _possibleValues[component] = val;
            }
            public bool IsCertainly(DateComponent component) => _certainVal.ContainsKey(component);
            public void SetIsCertainly(DateComponent component, object val)
            {
                if (_certainVal.Count > 0)
                {
                    Console.WriteLine(
                        $"Trying to set {component} as certainly {val}, but {_certainVal.Keys.First()} is already certainly {_certainVal.Values.First()}");
                    throw new ArgumentException("Token is already certain");
                }
                _certainVal[component] = val;
                _possibleValues.Clear();
                _possibleValues[component] = val;
            }

            public DateComponent CertainComponent => _certainVal.Keys.First();
            public object CertainValue => _certainVal.Values.First();

            private static readonly Dictionary<string, int> MonthStrToInt = new Dictionary<string, int>
            {
                {"january", 1},
                {"february", 2},
                {"march", 3},
                {"april", 4},
                {"may", 5},
                {"june", 6},
                {"july", 7},
                {"august", 8},
                {"september", 9},
                {"october", 10},
                {"november", 11},
                {"december", 12},
            };

            private static readonly Dictionary<string, Epoch> EpochStrToVal = new Dictionary<string, Epoch>
            {
                {"bc", Epoch.BC},
                {"ad", Epoch.AD},
                {"bce", Epoch.BC},
                {"ce", Epoch.AD},
            };

            public Token(string contents)
            {
                this.Contents = contents;
                _possibleValues = new Dictionary<DateComponent, object>();
                _certainVal = new Dictionary<DateComponent, object>();
                if (int.TryParse(contents, out var intVal))
                {
                    if (intVal > 0 && intVal <= 12)
                        SetCouldBe(DateComponent.Month, intVal);
                    if (intVal > 0)
                        SetCouldBe(DateComponent.Year, intVal);
                    if (intVal > 0 && intVal <= 31)
                        SetCouldBe(DateComponent.Day, intVal);
                    if (intVal <= 0)
                        throw new ArgumentException($"No known date component for non-positive number: {intVal}");
                }

                if (MonthStrToInt.TryGetValue(contents.ToLower(), out var month))
                    SetIsCertainly(DateComponent.Month, month);
                if (contents.Contains(":") && DateTime.TryParse(contents, out var time))
                    SetIsCertainly(DateComponent.Time, time);
                if (EpochStrToVal.TryGetValue(contents.ToLower(), out var epoch))
                    SetIsCertainly(DateComponent.Epoch, epoch);

                if (_possibleValues.Count == 1 && !IsCertain)
                    SetIsCertainly(_possibleValues.Keys.First(), _possibleValues.Values.First());
            }
        }
        
        public DateRange(PWDateTime startTime, PWDateTime endTime, DateGranularity granularity)
        {
            StartTime = startTime;
            EndTime = endTime;
            Granularity = granularity;
        }

        public PWDateTime StartTime { get; set; }

        public PWDateTime EndTime { get; set; }
        
        public DateGranularity Granularity { get; set; }

        public override string ToString()
        {
            return $"{StartTime} - {EndTime}";
        }
    }
}