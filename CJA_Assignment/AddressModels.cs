using System;
using System.Xml.Serialization;
using CsvHelper.Configuration;

namespace CJA_Assignment
{
    public class ZipCodeLookupRequest
    {
        public Address Address { get; set; }
        public string USERID { get; set; }
        public ZipCodeLookupRequest(Address addressQuery, string id)
        {
            Address = addressQuery;
            USERID = id;
        }
        public ZipCodeLookupRequest()
        {
            
        }
    }

    public class CsvModel
    {
        public string StreetNumber { get; set; }
             
        public string StreetName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
    public class Address
    {
        //public string StreetName { get; set; }
        public string Address1 { get; set; }
        //public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }


        public Address(string StreetName, string Address, string City, string State)
        {
            //this.StreetName = StreetName;
            this.Address1 = StreetName + " " + Address;
            this.City = City;
            this.State = State;


        }
        public Address()
        {

        }
    }





}