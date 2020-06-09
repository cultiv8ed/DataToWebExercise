using System;

namespace DataToWeb.Data
{
    public class Details
    {
        public string name { get; set; }
        public Address address { get; set; }
    }

    public class Address
    {
        public string line1 { get; set; }
        public string line2 { get; set; }
    }
}
