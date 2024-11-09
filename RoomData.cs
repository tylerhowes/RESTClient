using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoomData
{
    

    internal class RoomData
    {
        public int id {  get; set; }
        public string name { get; set; }
        public Location location {  get; set; }
        public Details details { get; set; }
        public float price_per_month_gbp { get; set; }
        public string availability_date { get; set; }
        public string[] spoken_languages {  get; set; }
    }

    public class Location
    {
        public string city { get; set; }
        public string county {  get; set; }
        public string postcode {  get; set; }
    }

    public class Details
    {
        public bool furnished {  get; set; }
        public string[] amenities { get; set; }
        public bool live_in_landlord {  get; set; }
        public int shared_with {  get; set; }
        public bool bills_included {  get; set; }
        public bool bathroom_shared {  get; set; }
    }


}
