using CommandLine;
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

            Console.WriteLine("Operation Completed");

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
                        if (opts.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                        {
                            var request = HttpWebRequest.Create(opts.Url);
                            request.Method = opts.Method.ToUpper();

                            using (var reqStream = request.GetRequestStream())
                            {
                                if (string.IsNullOrWhiteSpace(opts.QueryString) == false)
                                {
                                    reqStream.Write(Encoding.UTF8.GetBytes(opts.QueryString));
                                }
                            }

                            var rsp = request.GetResponse();
                            if (rsp != null)
                            {
                                using (var sr = new StreamReader(rsp.GetResponseStream()))
                                {
                                    Console.WriteLine(sr.ReadToEnd());
                                }
                            }
                        }
                        else
                        {
                            var request = HttpWebRequest.Create(opts.Url + "?" + opts.QueryString);
                            request.Method = opts.Method.ToUpper();
                            var rsp = request.GetResponse();
                            if (rsp != null)
                            {
                                using (var sr = new StreamReader(rsp.GetResponseStream()))
                                {
                                    Console.WriteLine(sr.ReadToEnd());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Agent {current}, Request {i}: Error: {ex.Message}" );
                    }
                    Console.WriteLine($"Agent {current}, Request {i}: End");
                }
            });
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
