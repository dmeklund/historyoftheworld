using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using MwParserFromScratch.Nodes;

namespace ParseWiki
{
    public class Coord
    {
        public static Coord FromWikitext(string text)
        {
            var numbers = new List<double>();
            double[] north = null, east = null;
            text = text.Replace('{', ' ').Replace('}', ' ');
            foreach (var token in text.Split('|'))
            {
                if (double.TryParse(token, out var val))
                {
                    numbers.Add(val);
                }
                else
                {
                    var lowered = token.Trim().ToLower();
                    if (lowered == "n")
                    {
                        if (north != null)
                        {
                            throw new ArgumentException("Invalid coord string: latitude found more than once: " + text);
                        }
                        north = numbers.ToArray();
                        numbers.Clear();
                    }
                    else if (lowered == "s")
                    {
                        if (north != null)
                        {
                            throw new ArgumentException("Invalid coord string: latitude found more than once: " + text);
                        }
                        for (var index = 0; index < numbers.Count; ++index)
                        {
                            numbers[index] = -numbers[index];
                        }
                        north = numbers.ToArray();
                        numbers.Clear();
                    }
                    else if (lowered == "e")
                    {
                        if (east != null)
                        {
                            throw new ArgumentException("Invalid coord string: longitude found more than once: " + text);
                        }
                        east = numbers.ToArray();
                        numbers.Clear();
                    }
                    else if (lowered == "w")
                    {
                        if (east != null)
                        {
                            throw new ArgumentException("Invalid coord string: longitude found more than once: " + text);
                        }
                        for (var index = 0; index < numbers.Count; ++index)
                        {
                            numbers[index] = -numbers[index];
                        }
                        east = numbers.ToArray();
                        numbers.Clear();
                    }
                }
            }

            if (north == null && east == null && numbers.Count == 2)
            {
                return new Coord(numbers[0], numbers[1]);
            }
            else if (north != null && east != null)
            {
                var lat = ToDecimal(north);
                var lng = ToDecimal(east);
                return new Coord(lat, lng);                
            }
            else
            {
                throw new ArgumentException("Couldn't parse coordinate: " + text);
            }
        }

        public static double ToDecimal(double[] values)
        {
            if (values.Length == 0 || values.Length > 3)
            {
                throw new ArgumentException("Invalid input length: " + values.Length);
            }
            double val = values[0];
            if (values.Length > 1)
                val += values[1] / 60;
            if (values.Length > 2)
                val += values[2] / 3600;
            return val;
        }
        
        public Coord(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }
        
        public double Latitude { get; }
        public double Longitude { get; }
        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Latitude >= 0)
            {
                builder.AppendFormat("{0} N, ", Latitude);
            }
            else
            {
                builder.AppendFormat("{0} S, ", -Latitude);
            }

            if (Longitude >= 0)
            {
                builder.AppendFormat("{0} E", Longitude);
            }
            else
            {
                builder.AppendFormat("{0} W", -Longitude);
            }

            return builder.ToString();
        }
    }
}