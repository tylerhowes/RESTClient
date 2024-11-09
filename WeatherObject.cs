using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTClientService
{
    internal class WeatherObject
    {
        public string weather { get; set; }
        public long windSpeed { get; set; }
        public string temperature { get; set; }
    }


    //public class Datasery
    //{
    //    public long timepoint { get; set; }
    //    public long cloudcover { get; set; }
    //    public long liftedIndex { get; set; }
    //    public PrecType precType { get; set; }
    //    public long precAmount { get; set; }
    //    public long temp2M { get; set; }
    //    public string rh2M { get; set; }
    //    public Wind10M wind10M { get; set; }
    //    public string weather {  get; set; }
    //}


    //public enum PrecType
    //{
    //    None,
    //    Rain
    //}


    //public static class PrecTypeExtensions
    //{

    //    public static PrecType Value(string value)
    //    {
    //        switch (value)
    //        {
    //            case "none":
    //                return PrecType.None;
    //            case "rain":
    //                return PrecType.Rain;
    //            default:
    //                throw new InvalidDataException("Cannot deserialize PrecType");
    //        }
    //    }
    //}


    //public class Wind10M
    //{
    //    public Direction Direction { get; set; }
    //    public long Speed { get; set; }
    //}


    //public enum Direction
    //{
    //    E, N, NE, NW, SW, W, S, SE
    //}

    //public static class DirectionExtensions
    //{
    //    public static string ToValue(this Direction direction)
    //    {
    //        switch (direction)
    //        {
    //            case Direction.E: return "E";
    //            case Direction.N: return "N";
    //            case Direction.NE: return "NE";
    //            case Direction.NW: return "NW";
    //            case Direction.SW: return "SW";
    //            case Direction.W: return "W";
    //            case Direction.S: return "S";
    //            case Direction.SE: return "SW";
    //            default: return null;
    //        }
    //    }

    //    public static Direction ForValue(string value)
    //    {
    //        switch (value)
    //        {
    //            case "E": return Direction.E;
    //            case "N": return Direction.N;
    //            case "NE": return Direction.NE;
    //            case "NW": return Direction.NW;
    //            case "SW": return Direction.SW;
    //            case "W": return Direction.W;
    //            default: throw new InvalidDataException("Cannot deserialize Direction");
    //        }
    //    }
    //}

}