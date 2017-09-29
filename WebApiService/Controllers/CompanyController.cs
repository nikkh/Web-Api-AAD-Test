using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AADApiTest_ApI.Controllers
{
    [Authorize]
    public class CompanyController : ApiController
    {
        public IEnumerable<string> Get()
        {

            var date = DateTime.Now.ToShortDateString();
            var time = DateTime.Now.ToShortTimeString();
            return new string[] { "CompanyController", date, time };
        }
    }
}