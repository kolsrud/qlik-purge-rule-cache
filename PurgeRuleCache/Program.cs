using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PurgeRuleCache
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            if (args.Length < 1)
            {
                PrintUsage();
            }

            var url = args[0];
            Console.WriteLine("Connecting to " + url);
            var client = new RestClient(url);
            client.AsNtlmUserViaProxy();
            try
            {
                client.Get("/qrs/about");
                Console.WriteLine("Connection successfully established.");
                Console.Write("Creating dummy rule... ");
                var response = client.Post("/qrs/systemrule", MakeDummyRuleBody());
                var id = JObject.Parse(response).Property("id").Value.ToString();
                Console.WriteLine("Rule successfully created, id=" + id);
                Console.Write("Deleting rule id=" + id + "... ");
                client.Delete("/qrs/systemrule/" + id);
                Console.WriteLine("Rule deleted");
            }
            catch (Exception e)
            {
                Console.WriteLine("Operation failed with message: " + e.Message + " ");
                throw;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:   PurgeRuleCache.exe <url>");
            Console.WriteLine("Example: PurgeRuleCache.exe https://my.server.url");
            Environment.Exit(1);
        }

        private static string MakeDummyRuleBody()
        {
            return "{"
                   + "\"category\": \"Security\","
                   + "\"name\": \"Dummy rule for clearing rule cache\","
                   + "\"rule\": \"((user.name!=\\\"*\\\"))\","
                   + "\"actions\": 2,"
                   + "\"resourceFilter\": \"*\","
                   + "\"comment\": \"Dummy rule for clearing rule cache\","
                   + "\"ruleContext\": 0"
                   + "}";
        }
    }
}
