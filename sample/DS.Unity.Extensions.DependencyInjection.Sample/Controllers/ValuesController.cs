using System.Collections.Generic;
using DS.Unity.Extensions.DependencyInjection.Sample.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DS.Unity.Extensions.DependencyInjection.Sample.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IEchoService _echoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ValuesController(IEchoService echoService, IHttpContextAccessor httpContextAccessor)
        {
            _echoService = echoService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] { _echoService.Echo("value1"), _echoService.Echo("value2"), _httpContextAccessor.HttpContext.Request.Path.ToString() };
        }
    }
}