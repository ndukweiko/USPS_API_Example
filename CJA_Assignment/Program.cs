using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CJA_Assignment
{
    class MainClass
    {
        static HttpClient client = new HttpClient();
        

        public static void Main(string[] args)
        {
            string fileName = System.Configuration.ConfigurationManager.AppSettings["InputFileName"];
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            string path = Path.Combine(projectDirectory, fileName);

            var url = System.Configuration.ConfigurationManager.AppSettings["API_URL"];
            var user_id = System.Configuration.ConfigurationManager.AppSettings["UserID"];
        
            var max_length = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["MaxRequestLength"]);

            //parse csv into list of objects
            List<CsvModel> csv_list = ParseCSV(path);
            
            //get list of Address objects
            List<Address> address_list = ConvertToAddressModel(csv_list);

            //split list of lists into sublists of length 5 to reduce number of api calls necessary
            List<List<Address>> separated_list = Create_sublists(address_list, max_length);
            List<XDocument> Doc_list = new List<XDocument>();
            foreach(var list in separated_list)
            {

                Doc_list.Add(FormatRequests(list, user_id));
                
            
            }

            //make requests for each document
            List<string> result_list = new List<string>();
            foreach(var document in Doc_list)
            {
                var res = GetAsync(url + document).GetAwaiter().GetResult();
                if (res != null)
                {
                    var result = res.Content.ReadAsStringAsync();
               
                    result_list.Add(result.Result);
                }

            }
            //re iterate through results to print xml in a more readable manner.
            foreach(var res in result_list)
            {
                Console.Write(FormatXml(res));
            }

           

            

        }

        public static string FormatXml(string xml)
        {
            try
            {
                //reformat
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                // Handle and throw if fatal exception here; don't just ignore them
                return xml;
            }
        }
        public static XDocument FormatRequests(List<Address> list, string user_id)
        {
            //initialize document object
            XDocument doc = new XDocument();
            //initialize Zipcode request object
            XElement zipsearch = new XElement("ZipCodeLookupRequest",
                new XAttribute("USERID", user_id)

                );
            int ndx = 0;
            foreach (var datarow in list)
            {
                //For each address object -- create xml for request and read string as async
                XElement child = CreateXMLForRequest(datarow, ndx);
                //add child to zipsearch tag
                zipsearch.Add(child);
                ndx++;
            }
            //add zipsearch tag to document
            doc.Add(zipsearch);
            return doc;
        }

        public static List<List<Address>> Create_sublists(List<Address> addresses, int nSize)
        {
            var list = new List<List<Address>>();

            for (int i = 0; i < addresses.Count; i += nSize)
            {
                list.Add(addresses.GetRange(i, Math.Min(nSize, addresses.Count - i)));
            }

            return list;
        }

        public static List<Address> ConvertToAddressModel(List<CsvModel> temp_list)
        {
            List<Address> address_list = new List<Address>();
            foreach(var element in temp_list)
            {
                //convert
                Address address = new Address(element.StreetNumber, element.StreetName, element.City, element.State);
                address_list.Add(address);

            }
            return address_list;
        }
   
        public static List<CsvModel> ParseCSV(string path)
        {
            CsvHelper.Configuration.CsvConfiguration config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture);
            TextReader reader = File.OpenText(path);
            config.HasHeaderRecord = false;
            List<CsvModel> csv_list = new List<CsvModel>();
            var csvReader = new CsvReader(reader, config);
            try
            {
                csv_list = csvReader.GetRecords<CsvModel>().Skip(1).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException.Message);
            }
            return csv_list;
        }


        static async Task<HttpResponseMessage> GetAsync(string path)
        {
            
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }



            return null;


        }

        public static XElement CreateXMLForRequest(Address address, int new_id)
        {
            //create xelement that will be added to xdocument
           XElement xElement =
                new XElement("Address",
                new XAttribute("ID", new_id.ToString()),
                    new XElement("Address1", address.Address1),
                        new XElement("Address2", ""),
                        new XElement("City", address.City),
                        new XElement("State", address.State),
                        new XElement("Zip5", ""),
                        new XElement("Zip4", "")



                );
            return xElement;

        }


        

 


    }
}
