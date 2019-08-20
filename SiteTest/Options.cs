using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpAttacker
{
    public sealed class Options
    {
        [Option('u', "url", Required = true, HelpText = "Website URL https://osl.one")]
        public string Url { get; set; }

        [Option('m', "method", Required = true, HelpText = "Request method GET/POST", Default = "GET")]
        public string Method { get; set; }

        [Option('q', "querystring", Required = false, HelpText = "& seperated query string inside \" \" ")]
        public string QueryString { get; set; }

        [Option('a', "agent", Required = false, HelpText = "Number of concurrent request agent", Default = 1)]
        public int AgentNumber { get; set; }

        [Option('n', "request", Required = false, HelpText = "Number of request per agent", Default = 100)]
        public int NumberOfRequest{ get; set; }

        [Option('r', "rspshow", Required = false, HelpText = "Show response output. 1 for show 0 for hide", Default = 1)]
        public int ShowResponse { get; set; }
    }
}
