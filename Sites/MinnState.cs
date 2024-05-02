using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; // Updated from System.Data.SqlClient for .NET Core
using Microsoft.EntityFrameworkCore; // Using Entity Framework Core for database operations
using System.Net;
using NewDotnet.Models;
using NewDotnet.Code;
using NewDotnet.Context;

namespace NewDotnet.Controllers
{
    [ApiController]
    [Route("api/site/minnstate")]
    public class MNSUSiteController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OODBModelContext _context;
        private readonly  OODBModel m;
        public MNSUSiteController(IConfiguration configuration, OODBModelContext context)
        {
            _configuration = configuration;
            _context = context;
            m = new OODBModel(_context);

        }
        [HttpGet("semester_days_elapsed")]

        public IActionResult DaysSinceSemesterStarted()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("OODBModel")))
            {
                connection.Open();
                var command = new SqlCommand("SELECT DATEDIFF(day, (SELECT TOP 1 BeginOfSemesterDate FROM Semester WHERE BeginOfSemesterDate <= CAST(GETDATE() AS Date) ORDER BY BeginOfSemesterDate DESC), CAST(GETDATE() AS Date))", connection);
                int days = (int)command.ExecuteScalar();
                return Ok(days.ToString());
            }
        }

        [HttpGet("analytics")]
        public IActionResult SiteAnalytics()
        {
            BearerTokenContents btc = Services.GetTokenDataFromUserPrincipal(User);
            var flagIdForMilitary = _context.Flags.FirstOrDefault(x => x.FlagName == "oe_veteran");

            var baseQuery = _context.Assignments.Where(x => x.AssignedPlaylist == 0).ToList();
            string starIds = string.Join(", ", baseQuery.Select(x => $"'{x.StarId}'").Distinct());

          
           
            var entireSeries = baseQuery.Select(x => new
            {
                userId = x.StarId
                
            }).ToList();

            List<string[]> headers = new List<string[]>
            {
                
                new string[] { "isMilitary", "Military?" }
            };

            if (btc.Playlist == 0)
            {
                headers.Add(new string[] { "location", "Location" });
            }

            return Ok(new
            {
                error = 0,
                data = new
                {
                    headers = headers,
                    rows = entireSeries
                }
            });
        }

        public class StarIdTechIdMapping
        {
            public string starId { get; set; }
            public string techId { get; set; }
        }
    }
}
