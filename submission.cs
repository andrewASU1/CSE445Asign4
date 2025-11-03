using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;

using Newtonsoft.Json.Linq;


/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/


namespace ConsoleApp1
{


    public class Program
    {
        public static string xmlURL = "https://andrewasu1.github.io/CSE445Asign4/Hotels.xml";
        public static string xmlErrorURL = "https://andrewasu1.github.io/CSE445Asign4/HotelsErrors.xml";
        public static string xsdURL = "https://andrewasu1.github.io/CSE445Asign4/Hotels.xsd";

        public static void Main(string[] args)
        {

            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result == "No Error" ? "No errors are found" : result);

            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result == "No Error" ? "No errors are found" : result);

            result = Xml2Json(xmlURL);
            Console.WriteLine(result);

            /*
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);


            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);


            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
            */
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add(null, xsdUrl);

                XmlReaderSettings settings = new XmlReaderSettings
                {
                    Schemas = schemaSet,
                    ValidationType = ValidationType.Schema
                };
                settings.ValidationFlags =
                    XmlSchemaValidationFlags.ProcessInlineSchema |
                    XmlSchemaValidationFlags.ProcessSchemaLocation |
                    XmlSchemaValidationFlags.ReportValidationWarnings;

                var messages = new System.Collections.Generic.List<string>();
                settings.ValidationEventHandler += (sender, e) =>
                {
                    string pos = e.Exception != null ? $" line {e.Exception.LineNumber}, pos {e.Exception.LinePosition}" : "";
                    string sev = (e.Severity == XmlSeverityType.Error ? "Error" : "Warning");
                    messages.Add($"{sev}{pos}: {e.Message}");
                };

                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read()) { }
                }

                if (messages.Count == 0) return "No Error";
                return string.Join("\n", messages);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string Xml2Json(string xmlUrl)
        {

            string jsonText = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlUrl);

                JObject root = new JObject();
                JObject hotelsObj = new JObject();
                JArray hotelArr = new JArray();

                // <Hotels><Hotel>...</Hotel>...</Hotels>
                XmlNodeList hotelNodes = doc.DocumentElement.SelectNodes("Hotel");
                foreach (XmlNode h in hotelNodes)
                {
                    JObject item = new JObject();

                    XmlNode name = h.SelectSingleNode("Name");
                    if (name != null) item["Name"] = name.InnerText.Trim();

                    // Phones as an array
                    JArray phones = new JArray();
                    XmlNodeList phoneNodes = h.SelectNodes("Phone");
                    foreach (XmlNode p in phoneNodes)
                    {
                        string val = (p.InnerText ?? "").Trim();
                        if (val.Length > 0) phones.Add(val);
                    }
                    item["Phone"] = phones;

                    // Address object
                    XmlNode addrNode = h.SelectSingleNode("Address");
                    if (addrNode != null)
                    {
                        JObject addr = new JObject();
                        string[] fields = { "Number", "Street", "City", "State", "Zip", "NearestAirport" };
                        foreach (string f in fields)
                        {
                            XmlNode n = addrNode.SelectSingleNode(f);
                            if (n != null) addr[f] = (n.InnerText ?? "").Trim();
                        }
                        item["Address"] = addr;
                    }

                    // optional Rating attribute -> "_Rating" only if present
                    XmlAttribute rating = (h.Attributes != null) ? h.Attributes["Rating"] : null;
                    if (rating != null && !string.IsNullOrWhiteSpace(rating.Value))
                    {
                        item["_Rating"] = rating.Value.Trim();
                    }

                    hotelArr.Add(item);
                }

                hotelsObj["Hotel"] = hotelArr;
                root["Hotels"] = hotelsObj;

                jsonText = root.ToString(Newtonsoft.Json.Formatting.None);

                // tiny sanity check so it is de-serializable (does nothing if it fails)
                try { var _ = JsonConvert.DeserializeXmlNode(jsonText); } catch { }

                return jsonText;
            }
            catch (Exception ex)
            {
                // just return the message if error
                return ex.Message;
            }
        }
    }
}
