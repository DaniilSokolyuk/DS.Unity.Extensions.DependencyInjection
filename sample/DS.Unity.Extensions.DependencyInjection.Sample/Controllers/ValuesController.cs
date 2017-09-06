using System.Collections.Generic;
using DS.Unity.Extensions.DependencyInjection.Sample.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DS.Unity.Extensions.DependencyInjection.Sample.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IEchoService _echoService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(IEchoService echoService, IHttpContextAccessor httpContextAccessor, ILogger<ValuesController> logger)
        {
            _echoService = echoService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogCritical("HELP");
            return new[] { _echoService.Echo("value1"), _echoService.Echo("value2"), _httpContextAccessor.HttpContext.Request.Path.ToString() };
        }
    }
}