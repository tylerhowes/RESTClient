using RoomData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrimeReport
{

    internal class CrimeReportObject
    {
        public Dictionary<string, int> crimePairs {  get; set; }
    }
    //internal class CrimeReportObject
    //{
    //    public string Category { get; set; }
    //    public string PersistentID { get; set; }
    //    public string LocationSubtype { get; set; }
    //    public long ID { get; set; }
    //    public Location Location { get; set; }
    //    public string Context { get; set; }
    //    public string Month { get; set; }
    //    public string LocationType { get; set; }
    //    public OutcomeStatus OutcomeStatus { get; set; }
    //}

    //public class OutcomeStatus
    //{
    //    public string Category { get; set; }
    //    public string Date { get; set; }
    //}

    //public class Location
    //{
    //    public string Latitude { get; set; }
    //    public Street Street { get; set; }
    //    public string Longitude { get; set; }
    //}

    //public class Street
    //{
    //    public long id {  get; set; }
    //    public string name {  get; set; }
    //}
}