using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTClientService
{
    internal class ApplicationData
    {
        public string userID { get; set; }
        public int applicationID {  get; set; }
        public int roomID { get; set; }
        public string name { get; set; }

        public string status {  get; set; }
    }
}
