using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        #region Private Fields

        private readonly ReportsDbContext _context;

        #endregion
        public ChartsController(ReportsDbContext reportsDbContext)
        {
            _context = reportsDbContext;
        }

        [HttpGet("GetCarByType")]
        public IActionResult GetCarByType()
        {
            var qu = from c in _context.Cars
                     where c.Type != null
                     group c by c.Type into g
                     select new
                     {
                         Type = g.Key,
                         Count = g.Count()
                     };
            return Ok(qu.ToList());
        }
        [HttpGet("ReportsByUser")]
        public IActionResult GetReportsByUser()
        {
            var ExistsByAuthor = from EE in _context.EntryExitReports
                                 where EE.Author != null
                                 group EE by EE.Author into g
                                 select new
                                 {
                                     Author = g.Key,
                                     Count = g.Count()
                                 };
            var issuesByAuthor = from EE in _context.IssueReports
                                 where EE.Author != null
                                 group EE by EE.Author into g
                                 select new
                                 {
                                     Author = g.Key,
                                     Count = g.Count()
                                 };
            var crashesByAuthor = from EE in _context.CrashReports
                                  where EE.Author != null
                                  group EE by EE.Author into g
                                  select new
                                  {
                                      Author = g.Key,
                                      Count = g.Count()
                                  };

            return Ok(new
            {
                ExistsByAuthor = ExistsByAuthor.ToList(),
                IssuesByAuthor = issuesByAuthor.ToList(),
                CrashesByAuthor = crashesByAuthor.ToList()
            });
        }

        [HttpGet("ReportsByCars")]
        public IActionResult GetReportsByCars()
        {
            var cars = _context.Cars.ToList();

            var results = cars.Select(car => new
            {
                CarId = car.CarId,
                Brand = car.Brand,
                Model = car.Model,
                Plate = car.Plate,
                Exits = _context.EntryExitReports.Count(e => e.CarId == car.CarId),
                Issues = _context.IssueReports.Count(i => i.CarId == car.CarId),
                Crashes = _context.CrashReports.Count(c => c.CarId == car.CarId),
                Returns = _context.VehicleReturns.Count(r => r.CarId == car.CarId),
                Bookings = _context.Bookings.Count(b => b.CarId == car.CarId),
            }).ToList();

            return Ok(results);
        }

        [HttpGet("ReportsByDate")]
        public IActionResult GetReportsByDate(DateTime start, DateTime End)
        {
            var Exits = from C in _context.EntryExitReports
                        where C.Created.Date >= start.Date && C.Created.Date <= End.Date
                        group C by C.CarId into e
                        select e.Count();
            var Issues = from C in _context.IssueReports
                         where C.Created.Date >= start.Date && C.Created.Date <= End.Date
                         group C by C.CarId into e
                         select e.Count();
            var Crashes = from C in _context.CrashReports
                         where C.Created.Date >= start.Date && C.Created.Date <= End.Date
                         group C by C.CarId into e
                         select e.Count();

            return Ok(new
            {
                StartDate= start,
                EndDate = End,
                Exits = Exits.Count(),
                Issues = Issues.Count(),
                Crashes = Crashes.Count()
            });
        }

        [HttpGet("ReportsByYear")]
        public IActionResult GetReportsByYear(int year)
        {
            // Reportes de entradas/salidas por mes
            var exitsInfo = from EE in _context.EntryExitReports
                            where EE.Created.Year == year
                            select new
                            {
                                EE.ReportId,
                                EE.CarId,
                                EE.Created,
                            };


            var exits = _context.EntryExitReports
                .Where(c => c.Created.Year == year)
                .GroupBy(c => c.Created.Month)
                .Select(g => new {
                    Month = g.Key,
                    Count = g.Count(),
                });

            // Reportes de incidencias por mes
            var issuesInfo = from EE in _context.IssueReports
                             where EE.Created.Year == year
                             select new
                             {
                                 EE.ReportId,
                                 EE.CarId,
                                 EE.Created,
                             };
            var issues = _context.IssueReports
                .Where(c => c.Created.Year == year)
                .GroupBy(c => c.Created.Month)
                .Select(g => new {
                    Month = g.Key,
                    Count = g.Count(),

                });

            // Reportes de accidentes por mes
            var crashesInfo = from EE in _context.CrashReports
                              where EE.Created.Year == year
                              select new
                              {
                                  EE.ReportId,
                                  EE.CarId,
                                  EE.Created,
                              };
            var crashes = _context.CrashReports
                .Where(c => c.Created.Year == year)
                .GroupBy(c => c.Created.Month)
                .Select(g => new {
                    Month = g.Key,
                    Count = g.Count()
                });

            var report = new
            {
                Year = year,
                Exits = new
                {
                    Info = exits,
                    List = exitsInfo.ToList() 
                },
                Issues = new{
                    Info= issues,
                    List= issuesInfo.ToList()
                },
                Crashes = new
                {
                    Info=crashes,
                    List= issuesInfo.ToList()
                },
            };


            return Ok(report);
        }

        [HttpGet("ReportsByCarByYear")]
        public IActionResult GetReportsByYear(int year, string CarId)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == CarId);
            // Reportes de entradas/salidas por mes
            var exits = _context.EntryExitReports
                .Where(c => c.Created.Year == year && c.CarId == CarId)
                .GroupBy(c => c.Created.Month)
                .Select(g => new {
                    Month = g.Key,
                    Type = "Exit",
                    Count = g.Count()
                });

            // Reportes de incidencias por mes
            var issues = _context.IssueReports
                .Where(c => c.Created.Year == year && c.CarId== CarId)
                .GroupBy(c => c.Created.Month)
                .Select(g => new {
                    Month = g.Key,
                    Type = "Issue",
                    Count = g.Count()
                });

            // Reportes de accidentes por mes
            var crashes = _context.CrashReports
                .Where(c => c.Created.Year == year && c.CarId == CarId)
                .GroupBy(c => c.Created.Month)
                .Select(g => new {
                    Month = g.Key,
                    Type = "Crash",
                    Count = g.Count()
                });

            // Unir todos los resultados
            var result = exits
                .Concat(issues)
                .Concat(crashes)
                .ToList();

            var carInfo = new
            {
                Year = year,
                Brand = car.Brand,
                Model = car.Model,
                Plate = car.Plate,
                Reports = result
            };

            return Ok(carInfo);
        }

        [HttpGet("ReportsByCar/{id}")]
        public IActionResult GetReportsByCar(string id)
        {
            var car = _ = _context.Cars.FirstOrDefault(c => c.CarId == id);
            var Exits = from EE in _context.EntryExitReports
                        where EE.CarId == id
                        group EE by EE.CarId into g
                        select g.Count();

            var Issues = from EE in _context.IssueReports
                         where EE.CarId == id
                         group EE by EE.CarId into g
                         select g.Count();
            var crashes = from EE in _context.CrashReports
                          where EE.CarId == id
                          group EE by EE.CarId into g
                          select g.Count();
            var Returns = from EE in _context.VehicleReturns
                          where EE.CarId == id
                          group EE by EE.CarId into g
                          select g.Count();
            var reminders = from EE in _context.Reminders
                            where EE.CarId == id
                            group EE by EE.CarId into g
                            select g.Count();
            return Ok(new
            {
                Brand = car.Brand,
                Model = car.Model,
                Plate = car.Plate,
                Exits = Exits.Count(),
                Issues = Issues.Count(),
                Crashes = crashes.Count(),
                Returns = Returns.Count(),
                Reminders = reminders.Count(),
            });
        }

        [HttpGet("CarsByBrand")]
        public IActionResult Get()
        {
            var qu = from c in _context.Cars
                     where c.Brand != null
                     group c by c.Brand into g
                     select new
                     {
                         Brand = g.Key,
                         Count = g.Count(),
                         Cars = g.Select(c => new
                         {
                             c.CarId,
                             c.Model,
                             c.Plate,
                             c.Year
                         }).ToList()
                     };

            return Ok(qu.ToList());
        }

        [HttpGet("CarsByYear")]
        public IActionResult CarsByYear()
        {
            var qu = from c in _context.Cars
                     where c.Year != 0
                     group c by c.Year into g
                     select new
                     {
                         Year = g.Key,
                         Count = g.Count(),
                         Cars = g.Select(c => new
                         {
                             c.CarId,
                             c.Brand,
                             c.Model,
                             c.Plate
                         }).ToList()
                     };

            return Ok(qu.ToList());
        }
    }
}
