using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaseStudy.Application.Models;
using CaseStudy.Application.Models.Holiday;
using CaseStudy.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CaseStudy.API.Controllers
{
    [Route("core/api/[controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class BayTahminController : ControllerBase
    {
        private readonly IBayTahminService _bayTahminService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<BayTahminController> _logger;


        public BayTahminController(
            IBayTahminService bayTahminService,
            ILogger<BayTahminController> logger)
        {
            _bayTahminService = bayTahminService;
            _logger = logger;
        }

       
    }
}
