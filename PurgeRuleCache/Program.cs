using System;
using System.Net;
using Newtonsoft.Json.Linq;

namespace PurgeRuleCache
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            if (args.Length == 0)
            {
                PrintUsage();
            }

            var connectionType = GetConnectionType(args[0]);
            RestClient client;

            var isServiceAccount = true;
            switch (connectionType)
            {
                case RestClient.ConnectionType.NtlmUserViaProxy:
                    if (args.Length != 2)
                    {
                        PrintUsage();
                        return;
                    }

                    client = new RestClient(args[1]);
                    client.AsNtlmUserViaProxy();
                    break;
                case RestClient.ConnectionType.DirectConnection:
                    if (args.Length != 3 && args.Length != 5)
                    {
                        PrintUsage();
                        return;
                    }

                    client = new RestClient(args[1]);
                    int port;
                    if (!int.TryParse(args[2], out port))
                    {
                        PrintUsage();
                    }

                    var certificates = client.LoadCertificateFromStore();
                    var userDir = "INTERNAL";
                    var userId = "sa_repository";
                    if (args.Length == 5)
                    {
                        userDir = args[3];
                        userId = args[4];
                        isServiceAccount = false;
                    }
                    Console.WriteLine("Using direct connection as {0}\\{1}", userDir, userId);
                    client.AsDirectConnection(userDir, userId, port, false, certificates);
                    break;
                default:
                    PrintUsage();
                    return;
            }

            Console.WriteLine("Connecting to {0} using {1}", client.Url, connectionType);
            try
            {
                client.Get("/qrs/about");
                Console.WriteLine("Connection successfully established.");
                if (isServiceAccount)
                {
                    Console.WriteLine("Using internal resetcache endpoint.");
                    client.Post("/qrs/systemrule/security/resetcache", "");
                }
                else
                {
                    Console.Write("Creating dummy rule... ");
                    var response = client.Post("/qrs/systemrule", MakeDummyRuleBody());
                    var id = JObject.Parse(response).Property("id").Value.ToString();
                    Console.WriteLine("Rule successfully created, id=" + id);
                    Console.Write("Deleting rule id=" + id + "... ");
                    client.Delete("/qrs/systemrule/" + id);
                    Console.WriteLine("Rule deleted");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Operation failed with message: " + e.Message + " ");
                throw;
            }
        }

        private static RestClient.ConnectionType GetConnectionType(string arg)
        {
            switch (arg)
            {
                case "--ntlm": return RestClient.ConnectionType.NtlmUserViaProxy;
                case "--direct": return RestClient.ConnectionType.DirectConnection;
                default:
                    Console.WriteLine("Unknown connection type: " + arg);
                    PrintUsage();
                    return default(RestClient.ConnectionType);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:   PurgeRuleCache.exe --ntlm   <url>");
            Console.WriteLine("         PurgeRuleCache.exe --direct <url> <port> [<userDir> <userId>]");
            Console.WriteLine("Example: PurgeRuleCache.exe --ntlm   https://my.server.url");
            Console.WriteLine("         PurgeRuleCache.exe --direct https://localhost 4242");
            Console.WriteLine("         PurgeRuleCache.exe --direct https://my.server.url 4242 MyUserDir MyUserId");
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
