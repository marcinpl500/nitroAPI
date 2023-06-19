using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;

using System.Net.Http;
using Newtonsoft.Json;

namespace linux2.Controllers
{
    

    [ApiController]
    [Route("[controller]")]

    public class DiscordController :ControllerBase
    {
       

        [HttpGet]
        public int[] Get()
        {
           
            var s = Program.Returned;
            return s.ToArray();


        }
    }

   
}
