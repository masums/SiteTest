using CommandLine;
using RestSharp;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpAttacker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Site Tester");

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => HandleCommand(opts))
                .WithNotParsed((errs) => HandleParseError(errs));

            Console.WriteLine("Operation started");

            Console.ReadKey();
        }


        private static void HandleCommand(Options opts)
        {
            Console.WriteLine($"URL : {opts.Url})");
            Parallel.For(0, opts.AgentNumber, (current) =>
            {
                Console.WriteLine($"Aget: {current}");
                
                for (int i = 0; i < opts.NumberOfRequest; i++)
                {
                    Console.WriteLine($"Agent {current}, Request {i}: Start");
                    try
                    {
                        var client = new RestClient(opts.Url);
                        var request = new RestRequest(GetMethod(opts.Method));
                        AddParamiters(request, opts.QueryString);

                        client.ExecuteAsync(request, response => {
                            Console.WriteLine($"Agent {current}, Request {i}: Response: ");
                            Console.WriteLine(response.Content);
                        });

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Agent {current}, Request {i}: Error: {ex.Message}" );
                    }
                    Console.WriteLine($"Agent {current}, Request {i}: End");
                }
            });
        }

        private static Method GetMethod(string method)
        {
            if(method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return Method.POST;
            }
            else if(method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                return Method.POST;
            }
            else
            {
                throw new Exception("Method conversion not implemented.");
            }
        }

        private static void AddParamiters(RestRequest request, string queryString)
        {
            if(string.IsNullOrWhiteSpace(queryString) == false)
            {
                var parts = queryString.Split("&", StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in parts)
                {
                    var pPart = item.Split("=", StringSplitOptions.RemoveEmptyEntries);
                    if(pPart.Length == 2)
                    {
                        request.AddParameter(pPart[0], pPart[1]);
                    }
                }
            }
        }

        private static void HandleParseError(IEnumerable errs)
        {
            Console.WriteLine("Command Line parameters provided were not valid!");
            foreach (var item in errs)
            {
                Console.WriteLine(item);
            }
        }
    }
}
