using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VSProfiler.Models;

namespace VSProfiler.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger _logger;
        private readonly ILogger<HomeController> _logger;
        private static Action<ILogger<HomeController>, string, string, string, string, string, string, Exception> _logIterationGeneric6;
        private string _s1;
        private string _s2;
        private string _s3;
        private string _s4;
        private string _s5;
        private string _s6;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            //_logger = NullLogger.Instance;
            _s1 = "some string";
            _s2 = "some string some string";
            _s3 = "some string some string some string";
            _s4 = "some string some string some string some string";
            _s5 = "some string some string some string some string some string";
            _s6 = "some string some string some string some string some string some string";

            _logIterationGeneric6 = LoggerMessage.Define<string, string, string, string, string, string>(LogLevel.Debug,
                eventId: 6,
                formatString: @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} and {other}");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            for (int i = 0; i < 1000000; i++)
            {
                // profiling first class, then struct then Define
                //_logger.LogCritical("logging critical info");
                _logIterationGeneric6(_logger, _s1, _s2, _s3, _s4, _s5, _s6, null);
                //Log.LogTest(_logger, _s1, _s2, _s3, _s4, _s5, _s6);
            }

            //var sw = new Stopwatch();
            //sw.Start();
            //sw.Stop();
            //Thread.Sleep(1000);

            return View();
        }
    }
}