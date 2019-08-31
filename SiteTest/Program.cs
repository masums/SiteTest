using CommandLine;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpAttacker
{
    class Program
    {
        volatile static int TotalAgents = 0, RunningAgents = 0, RequestCount = 0, Success = 0, Failed = 0, Error = 0, AgentIndex = 1;
        volatile static bool IsRunning = false;

        static void Main(string[] args)
        {
            IsRunning = true;
            Console.WriteLine("Hello Site Tester");

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => HandleCommand(opts))
                .WithNotParsed((errs) => HandleParseError(errs));

            GC.Collect();
            Console.ReadKey();
            IsRunning = false;
            Console.WriteLine("Operation completed");
        }

        private static void UpdateDisplay()
        {
            new Task(() =>
            {

                while (IsRunning)
                {
                    Console.Clear();
                    Console.WriteLine($"Agents: {RunningAgents} / {TotalAgents}, Total Request: {RequestCount}, Success: {Success} / Failed: {Failed} / Error: {Error}");
                    Thread.Sleep(1000);
                }

            }).Start();
        }

        private static void HandleCommand(Options opts)
        {
            Console.WriteLine($"URL : {opts.Url}");
            var taskList = new List<Thread>();

            UpdateDisplay();

            //Parallel.For(0, opts.AgentNumber,new ParallelOptions() {MaxDegreeOfParallelism = opts.AgentNumber }, (agentIndex) =>
            for (int i = 0; i < opts.AgentNumber; i++)
            {
                var task = new Thread(() =>
                {
                    RunningAgents++;

                    if (opts.ShowResponse == 1)
                    {
                        Console.WriteLine($"Aget: {AgentIndex}");
                    }

                    TotalAgents = opts.AgentNumber;

                    for (int i = 0; i < opts.RequestNumber; i++)
                    {
                        RequestCount++;

                        if (opts.ShowResponse == 1)
                        {
                            Console.WriteLine($"Agent {AgentIndex}, Request {i}: Start");
                        }

                        try
                        {
                            var client = new RestClient(opts.Url);
                            var request = new RestRequest(GetMethod(opts.Method));
                            request.Timeout = opts.Timeout * 60 * 1000;

                            AddParamiters(request, opts.QueryString);
                            MakeRequest(AgentIndex, i, client, request, opts);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Agent {AgentIndex}, Request {i}: Error: {ex.Message}");
                            Error++;
                        }

                        if (opts.ShowResponse == 1)
                        {
                            Console.WriteLine($"Agent {AgentIndex}, Request {i}: End");
                        }
                    };

                    AgentIndex++;
                    RunningAgents--;
                });

                taskList.Add(task);                
                task.Start();
            }

            while (RunningAgents > 0)
            {
                Thread.Sleep(1000);
            }
        }

        private static void MakeRequest(int agetIndex, int reqIndex, RestClient client, RestRequest request, Options opts)
        {
            if(opts.RequestMode.Equals("async", StringComparison.OrdinalIgnoreCase))
            {
                client.ExecuteAsync(request, response =>
                {

                    try
                    {
                        if (response.StatusCode == HttpStatusCode.OK && string.IsNullOrWhiteSpace(response.Content) == false)
                        {
                            Success++;
                        }
                        else
                        {
                            Failed++;
                            if (opts.ShowResponse == 1)
                            {
                                Console.WriteLine($"Agent {agetIndex}, Request {reqIndex}: Error: {response.ErrorException}");
                            }
                        }

                        if (opts.ShowResponse == 1)
                        {
                            Console.WriteLine($"Agent {agetIndex}, Request {reqIndex}: Response: ");
                            Console.WriteLine(response.Content);
                        }
                    }
                    catch (Exception ex)
                    {
                        Failed++;
                    }
                });
            }
            else
            {
                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK && string.IsNullOrWhiteSpace(response.Content) == false)
                {
                    Success++;
                }
                else
                {
                    Failed++;
                    if (opts.ShowResponse == 1)
                    {
                        Console.WriteLine($"Agent {agetIndex}, Request {reqIndex}: Error: {response.ErrorException}");
                    }
                }

                if (opts.ShowResponse == 1)
                {
                    Console.WriteLine($"Agent {agetIndex}, Request {reqIndex}: Response: ");
                    Console.WriteLine(response.Content);
                }
            }
        }

        private static Method GetMethod(string method)
        {
            if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return Method.POST;
            }
            else if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
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
            if (string.IsNullOrWhiteSpace(queryString) == false)
            {
                var parts = queryString.Split("&", StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in parts)
                {
                    var pPart = item.Split("=", StringSplitOptions.RemoveEmptyEntries);
                    if (pPart.Length == 2)
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
