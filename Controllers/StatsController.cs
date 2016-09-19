using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Data.Entity.SqlServer;
using System.Data;
using System.Web.Helpers;
using System.ComponentModel;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Web.UI.DataVisualization.Charting;

namespace SakraStats.Controllers
{
    public class LeaderBoardType
    {
        public int? sumsales { get; set; }
        public int? sumturnover { get; set; }
        public string sumemployeeid { get; set; }
        public string sumname { get; set; }
    }
    public class CompType
    {
        public string CompName { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime? StatDate { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Role { get; set; }
        public int Contacts { get; set; }
        public int Sales { get; set; }
        public int Demos { get; set; }
        public int Turnover { get; set; }
    }
    public class CompRequirements
    {
        public string CompName { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int TotContacts { get; set; }
        public int TotSales { get; set; }
        public int TotDemos { get; set; }
        public int TotTurnover { get; set; }
        public int DayContacts { get; set; }
        public int DaySales { get; set; }
        public int DayDemos { get; set; }
        public int DayTurnover { get; set; }
        public string Role { get; set; }
        public int TopBoard { get; set; }
    }
    [ValidateAntiForgeryToken]
    [NoCache]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class StatsController : Controller
    {
        HomeContext db = new HomeContext();
        private class NoCacheAttribute : ActionFilterAttribute
        {
            public override void OnResultExecuting(ResultExecutingContext filterContext)
            {
                filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
                filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                filterContext.HttpContext.Response.Cache.SetNoStore();

                base.OnResultExecuting(filterContext);
            }
        }
//Main menu options
        [HttpPost]
        public ActionResult DayReport(DateTime? datestart, DateTime? dateend, string[] Name, string[] Address, string[] Telephone, string[] Email, 
            string[] Result, string[] Notes, DateTime? Date, string branchid, string employeeid, string accessid, string submit, int Contacts = 0, 
            int Demos = 0, int Sales = 0, int Turnover = 0, DateTime? repdate = null, string repemployee = "")
        {
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
            ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
            ViewBag.accessid = accessid;
            if (submit== "Rapport")
            {
                var namesearchrep = db.employees.Where(r => r.EmployeeID == repemployee).ToList()[0];
                ViewBag.repemployee = namesearchrep.FirstName + " " + namesearchrep.LastName;
                ViewBag.datestart = datestart;
                ViewBag.dateend = dateend;
                ViewBag.title = "Dagrapport";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "statdayforday";
                var search = db.demreports.Where(r => r.Date == repdate && r.EmployeeID == repemployee).OrderByDescending(r=>r.Date).ThenBy(r=>r.employee.BranchID).ThenBy(r=>r.employee.LastName).ToList();
                return View("DayReportView",search);
            }
            else
            {
                try
                {
                    for (int i = 0; i < Name.Length; i++)
                    {
                        demreport newdemreport = new demreport()
                        {
                            EmployeeID = employeeid,
                            Date = (DateTime)Date,
                            Name = Name[i],
                            Address = Address[i],
                            Telephone = "",
                            Email = "",
                            Result = Result[i],
                            Notes = Notes[i],
                        };
                        db.demreports.Add(newdemreport);
                    }
                    db.SaveChanges();
                    ViewBag.title = "Succè!";
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    ViewBag.lastpage = "statsmain";
                    return View("PostedStats");
                }
                catch (Exception ex)
                {
                    ViewBag.date = Date;
                    ViewBag.title = "Fel!";
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    ViewBag.lastpage = "statsmain";
                    return View("ExistStats");
                }
            }
        }
        [HttpPost]
        public ActionResult Send(DateTime? Date, string branchid, string employeeid, string accessid, string submit, int Contacts = 0, int Demos = 0, int Sales = 0, int Turnover = 0)
        {
            if (Date==null)
            {
                //Check sell stats for individual
                SakraStats.stat checkmodel = CheckStats(employeeid);
                ViewBag.employeeid = employeeid;
                ViewBag.branchid = branchid;
                var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
                ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
                ViewBag.accessid = accessid;
                checkmodel.Date = DateTime.Now;
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.title = "Skicka statistik";
                ViewBag.lastpage = "statsmain";
                return View("AddStats", checkmodel);
            }
            else
            {
                ViewBag.employeeid = employeeid;
                var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
                ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                if (submit == "Avbryt")
                {
                    if (accessid == "Tekniker")
                    {
                        ViewBag.title = "Installation/Service";
                        return View("ManageTech");
                    }
                    else
                    {
                        ViewBag.title = "Säljportal";
                        return View("ManageSell");
                    }
                }
                else
                {
                    if (employeeid != "")
                    {
                        try
                        {
                            DateTime PostDate = (DateTime)Date;
                            //SaveData(EmployeeID, Contacts, Demos, Sales, Turnover, PostDate);
                            int day = PostDate.Day;
                            int month = PostDate.Month;
                            int year = PostDate.Year;

                            //Search database to see if already posted
                            var searchstats = db.stats.Where(r =>
                            r.EmployeeID == employeeid && 
                            r.Sales > -1 &&
                            SqlFunctions.DatePart("day", r.Date) == day &&
                            SqlFunctions.DatePart("month", r.Date) == month &&
                            SqlFunctions.DatePart("year", r.Date) == year).ToList();
                            if (searchstats.Count == 0 || accessid=="Tekniker" || accessid=="Chefstekniker" || accessid == "TempTech")
                            {
                                if (Demos>0)
                                {
                                    try
                                    {
                                        stat statsrow2 = new stat()
                                        {
                                            EmployeeID = employeeid,
                                            Contacts = Contacts,
                                            Demos = Demos,
                                            Sales = Sales,
                                            Turnover = Turnover,
                                            Date = PostDate,
                                            Service = 0
                                        };
                                        db.stats.Add(statsrow2);
                                        db.SaveChanges();
                                    }
                                    catch
                                    {
                                        ViewBag.date = PostDate;
                                        ViewBag.kk = searchstats[0].Contacts;
                                        ViewBag.dem = searchstats[0].Demos;
                                        ViewBag.sal = searchstats[0].Sales;
                                        ViewBag.oms = searchstats[0].Turnover;
                                        ViewBag.title = "Fel!";
                                        ViewBag.action = "Main";
                                        ViewBag.controller = "Home";
                                        ViewBag.buttonvalue = "Tillbaka";
                                        ViewBag.lastpage = "statsmain";
                                        return View("ExistStats");
                                    }
                                    ViewBag.Contacts = Contacts;
                                    ViewBag.Demos = Demos;
                                    ViewBag.Sales = Sales;
                                    ViewBag.Turnover = Turnover;
                                    ViewBag.Date = PostDate;
                                    ViewBag.title = "Skicka statistik";
                                    ViewBag.action = "Main";
                                    ViewBag.controller = "Home";
                                    ViewBag.buttonvalue = "Avbryt";
                                    ViewBag.lastpage = "statsmain";
                                    return View("DayReport");
                                }
                                stat statsrow = new stat()
                                {
                                    EmployeeID = employeeid,
                                    Contacts = Contacts,
                                    Demos = Demos,
                                    Sales = Sales,
                                    Turnover = Turnover,
                                    Date = PostDate,
                                    Service = 0
                                };
                                db.stats.Add(statsrow);
                                db.SaveChanges();
                                ViewBag.title = "Succè!";
                                ViewBag.action = "Main";
                                ViewBag.controller = "Home";
                                ViewBag.buttonvalue = "Tillbaka";
                                ViewBag.lastpage = "statsmain";
                                return View("PostedStats");
                            }
                            else
                            {
                                ViewBag.date = PostDate;
                                ViewBag.kk = searchstats[0].Contacts;
                                ViewBag.dem = searchstats[0].Demos;
                                ViewBag.sal = searchstats[0].Sales;
                                ViewBag.oms = searchstats[0].Turnover;
                                ViewBag.title = "Fel!";
                                ViewBag.action = "Main";
                                ViewBag.controller = "Home";
                                ViewBag.buttonvalue = "Tillbaka";
                                ViewBag.lastpage = "statsmain";
                                return View("ExistStats");
                            }
                        }
                        catch
                        {
                            if (accessid == "Tekniker")
                            {
                                ViewBag.title = "Installation/Service";
                                return View("ManageTech");
                            }
                            else
                            {
                                ViewBag.title = "Säljportal";
                                return View("ManageSell");
                            }
                        }
                    }
                    else
                    {
                        ViewBag.title = "Fel!";
                        ViewBag.action = "Login";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        return View("Failed");
                    }
                }
            }
        }
        public ActionResult History(string lastpage, string branchid, string employeeid, string accessid, string button, string submit, 
            string weekstart = "1", string weekend = "52", string yearstart = "2010", string yearend = "2010")
        {
            ViewBag.employeeid = employeeid;
            var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
            ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
            ViewBag.accessid = accessid;
            ViewBag.branchid = branchid;
            if (submit == "HISTORIK" || submit=="Tillbaka" & lastpage!="statstable")
            {
                //Return history menu
                ViewBag.title = "Historik";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "statsmain";
                return View("ViewStats");
            }
            else if (submit == "REDIGERA" || submit=="Avbryt" & weekstart == "" || button=="Avbryt" & weekstart=="" || lastpage=="statstable")
            {
                //Get sell stats for individual
                IEnumerable<SakraStats.stat> statmodeltable = GetStats(employeeid);
                ViewBag.title = "Redigera";
                ViewBag.action = "History";
                ViewBag.controller = "Stats";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "statsmain";
                return View("ViewStatsTable", statmodeltable);
            }
            else if (submit == "GRAF" || submit=="OK")
            {
                if (weekstart == "1" & weekend == "52" & yearstart == "2010" & yearend == "2010")
                {
                    IEnumerable<SakraStats.stat> statmodelgraph = GetStats(employeeid);
                    var moldeldate = statmodelgraph
                    .OrderByDescending(a => a.Date)
                    .GroupBy(r => GetWeekOfYear(r.Date)[0])
                    .Select(a => a.Min(b => GetWeekOfYear(b.Date)[0]))
                    .ToList();
                    var moldelyear = statmodelgraph
                        .OrderByDescending(a => a.Date)
                        .GroupBy(r => GetWeekOfYear(r.Date)[0])
                        .Select(a => a.Min(b => GetWeekOfYear(b.Date)[1]))
                        .ToList();
                    try
                    {
                        ViewBag.weekstart = moldeldate[moldeldate.Count - 1];
                        ViewBag.weekend = moldeldate[0];
                        ViewBag.yearstart = moldelyear[moldelyear.Count - 1];
                        ViewBag.yearend = moldelyear[0];
                    }
                    catch
                    {
                        ViewBag.weekstart = weekstart;
                        ViewBag.weekend = weekend;
                        ViewBag.yearstart = yearstart;
                        ViewBag.yearend = yearend;
                    }
                }
                else
                {
                    ViewBag.weekstart = weekstart;
                    ViewBag.weekend = weekend;
                    ViewBag.yearstart = yearstart;
                    ViewBag.yearend = yearend;
                }
                ViewBag.title = "Graf";
                ViewBag.action = "History";
                ViewBag.controller = "Stats";
                ViewBag.buttonvalue = "Tillbaka";
                return View("CallChart");
            }
            else if (submit == "Avbryt")
            {
                if (ViewBag.lastpage=="statsgraf")
                {
                    ViewBag.weekstart = weekstart;
                    ViewBag.weekend = weekend;
                    ViewBag.yearstart = yearstart;
                    ViewBag.yearend = yearend;
                    ViewBag.title = "Graf";
                    ViewBag.action = "History";
                    ViewBag.controller = "Stats";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("CallChart");
                }
                else
                {
                    //Get sell stats for individual
                    IEnumerable<SakraStats.stat> statmodeltable = GetStats(employeeid);
                    ViewBag.title = "Redigera";
                    ViewBag.action = "History";
                    ViewBag.controller = "Stats";
                    ViewBag.buttonvalue = "Tillbaka";
                    ViewBag.lastpage = "statsmain";
                    return View("ViewStatsTable", statmodeltable);
                }
            }
            else if (submit=="Ändra")
            {
                ViewBag.weekstart = weekstart;
                ViewBag.weekend = weekend;
                ViewBag.yearstart = yearstart;
                ViewBag.yearend = yearend;
                ViewBag.title = "Ändra period";
                ViewBag.action = "History";
                ViewBag.controller = "Stats";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "statsgraf";
                return View("EditWeek");
            }
            else
            {
                //Return to main menu
                if (accessid == "Tekniker")
                {
                    ViewBag.title = "Installation/Service";
                    return View("ManageTech");
                }
                else
                {
                    ViewBag.title = "Säljportal";
                    return View("ManageSell");
                }
            }
        }
        public ActionResult Edit(string accessid, string branchid, string submit, string button, string employeeid, string statsid, string statid
            , string[] Name, string[] Address, string[] Telephone, string[] Email, string[] Result, string[] Notes, 
            DateTime? date = null, int contacts = 0, int demos = 0, int sales = 0, int turnover = 0, int service = 0)
        {
            if (submit=="Redigera")
            {
                var selstat = db.stats.Where(item => item.StatsID.ToString() == statid).ToList().First();

                var demrepsearch = db.demreports.Where(r => r.Date == selstat.Date && r.EmployeeID == employeeid).ToList();
                ViewBag.demreps = demrepsearch;

                ViewBag.index = selstat.StatsID;
                ViewBag.employeeid = employeeid;
                var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
                ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                ViewBag.statsid = statid;
                ViewBag.title = "Redigera statistik";
                ViewBag.action = "History";
                ViewBag.controller = "Stats";
                ViewBag.buttonvalue = "Avbryt";
                return View("EditStats", selstat);
            }
            else if (submit == "Avbryt")
            {
                //Get sell stats for individual
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                ViewBag.employeeid = employeeid;
                ViewBag.title = "Historik";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "statsmain";
                return View("ViewStats");
            }
            else
            {
                var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
                ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                ViewBag.employeeid = employeeid;
                try
                {
                    DateTime.Parse(date.ToString());
                }
                catch
                {
                    //Get sell stats for individual
                    ViewBag.title = "Historik";
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    ViewBag.lastpage = "statsmain";
                    return View("ViewStats");
                }
                var selstat = db.stats.Where(item => item.StatsID.ToString() == statsid).ToList().First();
                var demrepsearch = db.demreports.Where(r => r.Date == selstat.Date && r.EmployeeID == employeeid).ToList();

                string delid = selstat.EmployeeID;
                DateTime deldate = (DateTime)selstat.Date;
                DateTime PostDate = (DateTime)date;
                try
                {
                    int day = PostDate.Day;
                    int month = PostDate.Month;
                    int year = PostDate.Year;
                    //Search database to see if already posted
                    var searchstats = db.stats.Where(r =>
                    r.EmployeeID == employeeid && 
                    r.Sales > -1 &&
                    SqlFunctions.DatePart("day", r.Date) == day &&
                    SqlFunctions.DatePart("month", r.Date) == month &&
                    SqlFunctions.DatePart("year", r.Date) == year &&
                    r.StatsID.ToString() != statsid).ToList();
                    if (searchstats.Count == 0 || accessid=="Tekniker" || accessid=="Chefstekniker")
                    {
                        selstat.Date = date;
                        selstat.Contacts = int.Parse(Server.HtmlEncode(contacts.ToString()));
                        selstat.Demos = int.Parse(Server.HtmlEncode(demos.ToString()));
                        selstat.Sales = int.Parse(Server.HtmlEncode(sales.ToString()));
                        selstat.Turnover = int.Parse(Server.HtmlEncode(turnover.ToString()));
                        selstat.Service = int.Parse(Server.HtmlEncode(service.ToString()));
                        int demcount = 0;
                        foreach (var demrep in demrepsearch)
                        {
                            try
                            {

                                demrep.Name = Name[demcount];
                                demrep.Address = Address[demcount];
                                demrep.Result = Result[demcount];
                                demrep.Notes = Notes[demcount];
                                demrep.Date = (DateTime)date;
                            }
                            catch
                            {

                            }
                            demcount++;
                        }
                        if (demrepsearch.Count < selstat.Demos)
                        {
                            db.SaveChanges();
                            ViewBag.Demos = selstat.Demos- demrepsearch.Count;
                            ViewBag.Date = PostDate;
                            ViewBag.Sales = 9999999;
                            ViewBag.Turnover = demrepsearch.Count + 9999999;
                            ViewBag.title = "Skicka statistik";
                            ViewBag.action = "Main";
                            ViewBag.controller = "Home";
                            ViewBag.buttonvalue = "Avbryt";
                            ViewBag.lastpage = "statsmain";
                            ModelState.Clear();
                            return View("DayReport",new demreport() { Date = (DateTime)date, EmployeeID = employeeid });
                        }
                    }
                    else
                    {
                        ViewBag.date = PostDate;
                        ViewBag.kk = searchstats[0].Contacts;
                        ViewBag.dem = searchstats[0].Demos;
                        ViewBag.sal = searchstats[0].Sales;
                        ViewBag.oms = searchstats[0].Turnover;
                        ViewBag.title = "Fel!";
                        ViewBag.action = "History";
                        ViewBag.controller = "Stats";
                        ViewBag.buttonvalue = "Tillbaka";
                        ViewBag.lastpage = "statstable";
                        return View("ExistStats");
                    }
                }
                catch
                {
                    //Get sell stats for individual
                    ViewBag.title = "Historik";
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    ViewBag.lastpage = "statsmain";
                    return View("ViewStats");
                }
                db.SaveChanges();
                ViewBag.lastpage = "statstable";
                ViewBag.title = "Succè!";
                ViewBag.action = "History";
                ViewBag.controller = "Stats";
                ViewBag.buttonvalue = "Tillbaka";
                return View("Changes");
            }
        }
        public ActionResult Leaderboards(string accessid, string branchid, string employeeid, string submit)
        {
            ViewBag.branchid = branchid;
            ViewBag.employeeid = employeeid;
            var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
            ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
            ViewBag.accessid = accessid;
            ViewBag.title = "Topplistor";
            ViewBag.action = "Main";
            ViewBag.controller = "Home";
            ViewBag.buttonvalue = "Tillbaka";
            ViewBag.lastpage = "statsmain";
            var leadermodel = Leaderboard(employeeid, branchid, accessid);
            return View("Leaderboard", leadermodel);
        }
        public ActionResult Competitions(string accessid, string branchid, string employeeid, string submit)
        {
            ViewBag.branchid = branchid;
            ViewBag.employeeid = employeeid;
            var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
            ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
            ViewBag.accessid = accessid;
            ViewBag.title = "Tävlingar";
            ViewBag.action = "Main";
            ViewBag.controller = "Home";
            ViewBag.buttonvalue = "Tillbaka";
            ViewBag.lastpage = "statsmain";
            //var compmodel = Competition();
            var compmodel = GetCompText();
            return View("Competitions2", compmodel);
        }
        public ActionResult Statistics(string submit, string employeeid, string branchid, string accessid, DateTime? datestart = null, DateTime? dateend = null)
        {
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
            ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
            if (submit == "Ändra")
            {
                SetViewBag(title: "Ändra period", action: "Statistics", controller: "Stats", buttonvalue: "Avbryt", lastpage: "stats");
                SetViewBag(datestart: datestart, dateend: dateend);
                return View("EditDate");
            }
            if (datestart == null)
            {
                datestart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dateend = DateTime.Now;
            }
            SetViewBag(datestart: datestart, dateend: dateend);
            var statsearch = CollateStats(employeeid, accessid, branchid, datestart, dateend); //Also sets ViewBag.daterange
            ViewBag.title = "Statistik";
            ViewBag.action = "Main";
            ViewBag.controller = "Home";
            ViewBag.buttonvalue = "Tillbaka";
            ViewBag.lastpage = "stats";
            try
            {
                return View(statsearch.ToList());
            }
            catch
            {
                return View("Statistics", null);
            }
        }
        public ActionResult Key(string submit, string employeeid, string branchid, string accessid, bool showseller = true, DateTime? datestart = null, DateTime? dateend = null, string showsellerfromview = "true")
        {
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
            ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
            if (submit == "Ändra")
            {
                ViewBag.title = "Ändra period";
                ViewBag.action = "Key";
                ViewBag.controller = "Stats";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "key";
                ViewBag.datestart = datestart;
                ViewBag.dateend = dateend;
                return View("EditDate");
            }
            if (datestart == null)
            {
                datestart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dateend = DateTime.Now;
            }
            ViewBag.datestart = datestart;
            ViewBag.dateend = dateend;
            var keysearch = CollateKey(employeeid, accessid, branchid, datestart, dateend); //Also sets ViewBag.daterange
            ViewBag.title = "Nyckeltal";
            ViewBag.action = "Main";
            ViewBag.controller = "Home";
            ViewBag.buttonvalue = "Tillbaka";
            ViewBag.lastpage = "stats";
            try
            {
                return View(keysearch.ToList());
            }
            catch
            {
                return View("Key", null);
            }
        }
//Functions
        private SakraStats.stat CheckStats(string employeeid)
        {
            //Check for previous entries that day
            int day = DateTime.Now.Day;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            var searchstats = db.stats.Where(r =>
                                r.EmployeeID == employeeid && 
                                r.Sales > -1 &&
                                SqlFunctions.DatePart("day", r.Date) == day &&
                                SqlFunctions.DatePart("month", r.Date) == month &&
                                SqlFunctions.DatePart("year", r.Date) == year).ToList();
            stat model = new stat();
            List<employee> nameinfo = db.employees.Where(a => a.EmployeeID == employeeid).ToList();
            string name = nameinfo[0].FirstName + " " + nameinfo[0].LastName;
            model.EmployeeID = employeeid;
            ViewBag.employeeid = employeeid;
            ViewBag.GetName = name;
            return model;
        }
        private IEnumerable<SakraStats.stat> GetStats(string employeeid)
        {
            var model = db.stats.Where(r => r.EmployeeID == employeeid && r.Sales > -1).ToList().OrderByDescending(r => r.Date);
            return model.AsEnumerable();
        }
        private List<IEnumerable<LeaderBoardType>> Leaderboard(string employeeid, string branchid, string accessid)
        {
            List<LeaderBoardType> databydate = new List<LeaderBoardType>();
            List<LeaderBoardType> checksales = new List<LeaderBoardType>();
            string accessfilter1 = "Försäljningschef";
            string accessfilter2 = "Säljare";
            string accessfilter3 = "Filialchef";
            string accessfilter4 = "Regionschef";
            string accessfilter5 = "Filialchef/Lager";
            string accessfilter6 = "Gruppchef";
            string accessfilter7 = "Gruppchef";

            if (accessid=="Chefstekniker" || accessid == "Tekniker")
            {
                accessfilter1 = "Tekniker";
                accessfilter2 = "Chefstekniker";
                accessfilter3 = "";
                accessfilter4 = "";
                accessfilter5 = "";
                accessfilter6 = "";
                accessfilter7 = "";
            }
            int inittake = 10;
            List <IEnumerable<LeaderBoardType>> modellist = new List<IEnumerable<LeaderBoardType>>();
            //Get branch list
            for (int i = 0; i < 2; i++)
            {
                DateTime datestart = new DateTime(DateTime.Now.AddMonths(-i).Year, DateTime.Now.AddMonths(-i).Month,1);
                DateTime dateend = new DateTime(DateTime.Now.AddMonths(-i).Year, DateTime.Now.AddMonths(-i).Month, DateTime.DaysInMonth(DateTime.Now.AddMonths(-i).Year,DateTime.Now.AddMonths(-i).Month));

                checksales = db.stats
                    .Where(r => r.employee.Role == accessfilter7 | r.employee.Role == accessfilter1 | r.employee.Role == accessfilter2 | r.employee.Role == accessfilter3 | r.employee.Role == accessfilter4 | r.employee.Role == accessfilter5 | r.employee.Role == accessfilter6 && r.Date >= datestart & r.Date <= dateend & r.employee.BranchID == branchid)
                    .GroupBy(r => new { r.employee.FirstName, r.employee.LastName, r.employee.Role })
                    .Select(a => new LeaderBoardType { sumsales = a.Sum(b => b.Sales), sumturnover = a.Sum(b=> b.Turnover), sumemployeeid = a.Min(b => b.EmployeeID), sumname = a.Min(b => b.employee.FirstName + " " + b.employee.LastName) })
                    .OrderByDescending(a => a.sumsales).ThenByDescending(a => a.sumturnover)
                    .ToList();
                if (checksales.Count<=inittake)
                {
                    databydate = checksales;
                }
                else
                {
                    int newtake = inittake;
                    int? prevsale = checksales[inittake-1].sumsales;
                    for (int j = inittake; j < checksales.Count; j++)
                    {
                        if (checksales[j].sumsales==prevsale)
                        {
                            newtake++;
                        }
                    }
                    databydate = db.stats
                        .Where(r => r.employee.Role == accessfilter7 | r.employee.Role == accessfilter1 | r.employee.Role == accessfilter2 | r.employee.Role == accessfilter3 | r.employee.Role == accessfilter4 | r.employee.Role == accessfilter5 | r.employee.Role == accessfilter6 && r.Date >= datestart & r.Date <= dateend & r.employee.BranchID == branchid)
                        .GroupBy(r => new { r.employee.FirstName, r.employee.LastName, r.employee.Role })
                        .Select(a => new LeaderBoardType { sumsales = a.Sum(b => b.Sales), sumturnover = a.Sum(b => b.Turnover), sumemployeeid = a.Min(b => b.EmployeeID), sumname = a.Min(b => b.employee.FirstName + " " + b.employee.LastName) })
                        .OrderByDescending(a => a.sumsales).ThenByDescending(a => a.sumturnover)
                        .Take(newtake)
                        .ToList();
                }
                modellist.Add(databydate.AsEnumerable());
            }
            //Get country list
            for (int i = 0; i < 2; i++)
            {
                DateTime datestart = new DateTime(DateTime.Now.AddMonths(-i).Year, DateTime.Now.AddMonths(-i).Month, 1);
                DateTime dateend = new DateTime(DateTime.Now.AddMonths(-i).Year, DateTime.Now.AddMonths(-i).Month, DateTime.DaysInMonth(DateTime.Now.AddMonths(-i).Year, DateTime.Now.AddMonths(-i).Month));

                checksales = db.stats
                        .Where(r => r.employee.Role == accessfilter7 | r.employee.Role == accessfilter1 | r.employee.Role == accessfilter2 | r.employee.Role == accessfilter3 | r.employee.Role == accessfilter4 | r.employee.Role == accessfilter5 | r.employee.Role == accessfilter6 && r.Date >= datestart & r.Date <= dateend)
                        .GroupBy(r => new { r.employee.FirstName, r.employee.LastName, r.employee.Role })
                        .Select(a => new LeaderBoardType { sumsales = a.Sum(b => b.Sales), sumturnover = a.Sum(b => b.Turnover), sumemployeeid = a.Min(b => b.EmployeeID), sumname = a.Min(b => b.employee.FirstName + " " + b.employee.LastName) })
                        .OrderByDescending(a => a.sumsales).ThenByDescending(a => a.sumturnover)
                        .ToList();

                if (checksales.Count <= inittake)
                {
                    databydate = checksales;
                }
                else
                {
                    int newtake = inittake;
                    int? prevsale = checksales[inittake - 1].sumsales;
                    for (int j = inittake; j < checksales.Count; j++)
                    {
                        if (checksales[j].sumsales == prevsale)
                        {
                            newtake++;
                        }
                    }
                    databydate = db.stats
                                .Where(r => r.employee.Role == accessfilter7 | r.employee.Role == accessfilter1 | r.employee.Role == accessfilter2 | r.employee.Role == accessfilter3 | r.employee.Role == accessfilter4 | r.employee.Role == accessfilter5 | r.employee.Role == accessfilter6 && r.Date >= datestart & r.Date <= dateend)
                                .GroupBy(r => new { r.employee.FirstName, r.employee.LastName, r.employee.Role })
                                .Select(a => new LeaderBoardType { sumsales = a.Sum(b => b.Sales), sumturnover = a.Sum(b => b.Turnover), sumemployeeid = a.Min(b => b.EmployeeID), sumname = a.Min(b => b.employee.FirstName + " " + b.employee.LastName) })
                                .OrderByDescending(a => a.sumsales).ThenByDescending(a=>a.sumturnover)
                                .Take(newtake)
                                .ToList();
                }
                modellist.Add(databydate.AsEnumerable());
            }
            return modellist;
        }
        private List<List<CompType>> Competition()
        {
            var competitions = GetCompetitions();
            List<List<CompType>> compresults = new List<List<CompType>>();
            ViewBag.competitions = competitions;
            foreach (var comp in competitions)
            {

                List<CompType> result = new List<CompType>();
                //Add day results
                result.AddRange(
                    db.stats
                    .Where(
                        r => r.Date >= comp.DateStart &&
                        r.Date <= comp.DateEnd && 
                        r.Sales > -1 &&
                        comp.Role == "" | (comp.Role == "Säljare" ? r.employee.Role == "Säljare" | r.employee.Role == "Gruppchef" : r.employee.Role == comp.Role) &&
                        r.Contacts >= comp.DayContacts | r.Demos >= comp.DayDemos | r.Sales >= comp.DaySales | r.Turnover >= comp.DayTurnover)
                    .Select(
                        r => new CompType()
                        {
                            CompName = comp.CompName,
                            DateStart = comp.DateStart,
                            DateEnd = comp.DateEnd,
                            StatDate = r.Date,
                            EmployeeID = r.EmployeeID,
                            EmployeeName = r.employee.FirstName + " " + r.employee.LastName,
                            Role = r.employee.Role,
                            Contacts = (int)r.Contacts,
                            Demos = (int)r.Demos,
                            Sales = (int)r.Sales,
                            Turnover = (int)r.Turnover
                        })
                    .OrderByDescending(r => comp.DaySales != 999999999 ? r.Sales : comp.DayTurnover != 999999999 ? r.Turnover : comp.DayDemos != 999999999 ? r.Demos : r.Contacts)
                    .Take(comp.TopBoard)
                    .ToList());
                //Add period results
                result.AddRange(
                    db.stats
                    .Where(
                        r => r.Date >= comp.DateStart &&
                        r.Date <= comp.DateEnd && 
                        r.Sales > -1 &&
                        comp.Role == "" | (comp.Role == "Säljare" ? r.employee.Role == "Säljare" | r.employee.Role == "Gruppchef" : r.employee.Role == comp.Role))
                    .GroupBy(r => r.EmployeeID)
                    .Select(
                        r => new CompType()
                        {
                            CompName = comp.CompName,
                            DateStart = comp.DateStart,
                            DateEnd = comp.DateEnd,
                            StatDate = null,
                            EmployeeID = r.Min (a => a.EmployeeID),
                            EmployeeName = r.Min(a => a.employee.FirstName) + " " + r.Min(a => a.employee.LastName),
                            Role = r.Min(a => a.employee.Role),
                            Contacts = r.Sum(a => (int)a.Contacts),
                            Demos = r.Sum(a => (int)a.Demos),
                            Sales = r.Sum(a => (int)a.Sales),
                            Turnover = r.Sum(a => (int)a.Turnover),
                        })
                    .Where(r => r.Contacts >= comp.TotContacts | r.Demos >= comp.TotDemos | r.Sales >= comp.TotSales | r.Turnover >= comp.TotTurnover)
                    .OrderByDescending(r => comp.TotSales!=999999999 ? r.Sales : comp.TotTurnover!=999999999 ? r.Turnover : comp.TotDemos!=999999999 ? r.Demos : r.Contacts)
                    .Take(comp.TopBoard)
                    .ToList());
                compresults.Add(result);
            }
            return compresults;
        }
        /// <summary>
        /// Input competitions details
        /// </summary>
        /// <returns></returns>
        private List<CompRequirements> GetCompetitions()
        {
            List<CompRequirements> AllComps = new List<CompRequirements>();
            CompRequirements competition = new CompRequirements();

            //LAS VEGAS SÄLJARE
            competition = new CompRequirements()
            {
                CompName = "Las Vegas - Säljare",
                DateStart = new DateTime (2016,6,1),
                DateEnd = new DateTime(2016, 8, 31,23,59,59),
                TotContacts = 999999999,
                TotSales = 60,
                TotDemos = 999999999,
                TotTurnover = 1300000,
                DayContacts = 999999999,
                DaySales = 999999999,
                DayDemos = 999999999,
                DayTurnover = 999999999,
                Role = "Säljare",
                TopBoard = 999999999
            };
            AllComps.Add(competition);

            //LAS VEGAS TEKNIKER
            competition = new CompRequirements()
            {
                CompName = "Las Vegas - Tekniker",
                DateStart = new DateTime(2016, 6, 1),
                DateEnd = new DateTime(2016, 12, 31, 23, 59, 59),
                TotContacts = 999999999,
                TotSales = 999999999,
                TotDemos = 999999999,
                TotTurnover = 0,
                DayContacts = 999999999,
                DaySales = 999999999,
                DayDemos = 999999999,
                DayTurnover = 999999999,
                Role = "Tekniker",
                TopBoard = 1
            };
            AllComps.Add(competition);

            //PRAGUE SÄLJARE
            competition = new CompRequirements()
            {
                CompName = "Prague - Säljare",
                DateStart = new DateTime(2016, 6, 1),
                DateEnd = new DateTime(2016, 8, 31, 23, 59, 59),
                TotContacts = 999999999,
                TotSales = 30,
                TotDemos = 999999999,
                TotTurnover = 700000,
                DayContacts = 999999999,
                DaySales = 999999999,
                DayDemos = 999999999,
                DayTurnover = 999999999,
                Role = "Säljare",
                TopBoard = 999999999
            };
            AllComps.Add(competition);

            //PRAGUE SÄLJARE
            competition = new CompRequirements()
            {
                CompName = "Prague - Tekniker",
                DateStart = new DateTime(2016, 6, 1),
                DateEnd = new DateTime(2016, 8, 31, 23, 59, 59),
                TotContacts = 999999999,
                TotSales = 999999999,
                TotDemos = 999999999,
                TotTurnover = 90000,
                DayContacts = 999999999,
                DaySales = 999999999,
                DayDemos = 999999999,
                DayTurnover = 999999999,
                Role = "Tekniker",
                TopBoard = 999999999
            };
            AllComps.Add(competition);
            return AllComps;
        }
        public static DateTime FirstDateOfWeekISO8601(int weekOfYear, int year)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }
        private IEnumerable<SakraStats.carstat> GetStores(string employeeid)
        {
            IEnumerable<SakraStats.carstat> model = db.carstats.Where(r => r.EmployeeID == employeeid && r.ReleasedTo == "" && r.Approved == false).ToList().OrderBy(r => r.ProductID);
            ViewBag.GetName = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            ViewBag.EmployeeID = employeeid;
            return model.AsEnumerable();
        }
        public static int[] GetWeekOfYear(DateTime? date)
        {
            DateTime dateproper = (DateTime)date;
            int[] week = new int[2] { 1, dateproper.Year };
            int dayofyear = (dateproper).DayOfYear;
            int firstday = (int)(new DateTime(dateproper.Year, 1, 1).DayOfWeek);
            int lastday = (int)(new DateTime(dateproper.Year, 12, 31).DayOfWeek);
            int extradays = 0;
            DateTime endofyearweeks = new DateTime(dateproper.Year, 12, 31);
            switch (firstday)
            {
                case 1:
                    extradays = 0;
                    break;
                case 2:
                    extradays = 1;
                    break;
                case 3:
                    extradays = 2;
                    break;
                case 4:
                    extradays = 3;
                    break;
                case 5:
                    extradays = -3;
                    break;
                case 6:
                    extradays = -2;
                    break;
                case 7:
                    extradays = -1;
                    break;
            }
            switch (lastday)
            {
                case 1:
                    endofyearweeks = new DateTime(dateproper.Year, 12, 31).AddDays(-1);
                    break;
                case 2:
                    endofyearweeks = new DateTime(dateproper.Year, 12, 31).AddDays(-2);
                    break;
                case 3:
                    endofyearweeks = new DateTime(dateproper.Year, 12, 31).AddDays(-3);
                    break;
                case 4:
                    endofyearweeks = new DateTime(dateproper.Year, 12, 31).AddDays(3);
                    break;
                case 5:
                    endofyearweeks = new DateTime(dateproper.Year, 12, 31).AddDays(2);
                    break;
                case 6:
                    endofyearweeks = new DateTime(dateproper.Year, 12, 31).AddDays(1);
                    break;
                case 7:
                    endofyearweeks = new DateTime(dateproper.Year, 12, 31).AddDays(0);
                    break;
            }
            if (dateproper > endofyearweeks)
            {
                week[0] = 1;
                week[1] = dateproper.Year + 1;
            }
            else if (dayofyear + extradays < 1)
            {
                week = GetWeekOfYear(new DateTime(((DateTime)date).Year, 12, 31));
                week[0]++;
                week[1] = dateproper.Year - 1;
            }
            else
            {
                double weekdouble = ((dayofyear + extradays) - 0.5) / 7;
                week[0] = (int)weekdouble;
                week[0]++;
                week[1] = dateproper.Year;
            }
            return week;
        }
        private List<stat> CollateStats(string employeeid, string accessid, string branchid, DateTime? datestart = null, DateTime? dateend = null, bool showseller = true)
        {
            if (datestart == null) { datestart = new DateTime(2016, 1, 1); }
            if (dateend == null) { dateend = DateTime.Now; }

            var collated = db.stats
                   .Where(r => r.Date >= datestart && r.Date <= dateend && r.EmployeeID == employeeid)
                   .GroupBy(r => r.Date)
                   .Select(r => new
                   {
                       StatsID = r.Min(a => a.StatsID),
                       EmployeeID = r.Min(a => a.EmployeeID),
                       Contacts = r.Sum(a => a.Contacts),
                       Demos = r.Sum(a => a.Demos),
                       Sales = r.Sum(a => a.Sales),
                       Turnover = r.Sum(a => a.Turnover),
                       Service = r.Sum(a=>a.Service),
                       Date = r.Min(a => a.Date),
                   })
                   .OrderByDescending(r => r.Date)
                   .ToList();
            List<stat> collateconvert = new List<stat>();
            foreach (var item in collated)
            {
                var newstat = new stat()
                {
                    StatsID = item.StatsID,
                    EmployeeID = item.EmployeeID,
                    Contacts = item.Contacts,
                    Demos = item.Demos,
                    Sales = item.Sales,
                    Turnover = item.Turnover,
                    Service = item.Service,
                    Date = item.Date,
                    employee = db.employees.Where(r => r.EmployeeID == item.EmployeeID).First()
                };
                collateconvert.Add(newstat);
            }
            return collateconvert;
        }
        private List<CollatedKey> CollateKey(string employeeid, string accessid, string branchid, DateTime? datestart = null, DateTime? dateend = null)
        {
            if (datestart == null) { datestart = new DateTime(2016, 1, 1); }
            if (dateend == null) { dateend = DateTime.Now; }
            var collated = db.stats
                    .Where(r => r.Date >= datestart && r.Date <= dateend && r.EmployeeID == employeeid)
                    .GroupBy(r => r.Date)
                    .Select(r => new
                    {
                        StatsID = r.Min(a => a.StatsID),
                        EmployeeID = r.Min(a => a.EmployeeID),
                        Contacts = r.Sum(a => a.Contacts),
                        Demos = r.Sum(a => a.Demos),
                        Sales = r.Sum(a => a.Sales),
                        Turnover = r.Sum(a => a.Turnover),
                        Date = r.Min(a => a.Date),
                        BranchID = r.Min(a => a.employee.BranchID)
                    })
                    .Select(a => new CollatedKey
                    {
                        sumcontacts= a.Contacts == 0 || a.Demos == 0 ? null : (decimal?)a.Contacts/ (decimal?)a.Demos,
                        sumdemos = a.Demos == 0 || a.Sales == 0 ? null : (decimal?)a.Demos / (decimal?)a.Sales,
                        sumturnover = a.Turnover == 0 || a.Sales == 0 ? null : (int?)a.Turnover / (int?)a.Sales,
                        sumemployeeid = a.EmployeeID,
                        sumdate = a.Date,
                        sumbranchid = a.BranchID
                    })
                    .OrderByDescending(a=>a.sumdate).ThenByDescending(a => a.sumdemos.HasValue).ThenBy(a => a.sumdemos).ThenByDescending(a => a.sumturnover).ThenBy(a => a.sumcontacts).ThenBy(a => a.sumbranchid)
                    .ToList();
            var totallist = CollateStats(employeeid, accessid, branchid, datestart, dateend);
            var totals = new CollatedKey()
            {
                sumemployeeid = "SNITT : ",
                sumbranchid = totallist.Min(a => a.employee.BranchID),
                sumcontacts = totallist.Sum(b => b.Contacts) == 0 || totallist.Sum(b => b.Demos) == 0 ? null : (totallist.Sum(b => (decimal?)b.Contacts) / totallist.Sum(b => (decimal?)b.Demos)),
                sumdemos = totallist.Sum(b => b.Demos) == 0 || totallist.Sum(b => b.Sales) == 0 ? null : (totallist.Sum(b => (decimal?)b.Demos) / totallist.Sum(b => (decimal?)b.Sales)),
                sumturnover = totallist.Sum(b => b.Turnover) == 0 || totallist.Sum(b => b.Sales) == 0 ? null : (totallist.Sum(b => b.Turnover) / totallist.Sum(b => b.Sales)),
            };
            collated.Add(totals);
            return collated;
        }
        public List<string> GetCompText()
        {
            List<string> comptext = new List<string>();
            foreach (var comp in db.competitions)
            {
                comptext.Add(comp.CompText);
            }
            return comptext;
        }
        public void SetViewBag(
            string title = null,
            string action = null,
            string controller = null,
            string lastpage = null,
            string buttonvalue = null,
            string showsellerfromview = null,

            string employeeid = null,
            string accessid = null,
            string branchid = null,
            string name = null,

            int? weekstart = null,
            int? weekend = null,
            int? yearstart = null,
            int? yearend = null,
            DateTime? datestart = null,
            DateTime? dateend = null,

            string totalcontacts = null,
            string totaldemos = null,
            string totalsales = null,
            string totalturnover = null

            )
        {
            if (title != null) { ViewBag.title = title; }
            if (action != null) { ViewBag.action = action; }
            if (controller != null) { ViewBag.controller = controller; }
            if (lastpage != null) { ViewBag.lastpage = lastpage; }
            if (buttonvalue != null) { ViewBag.buttonvalue = buttonvalue; }
            if (showsellerfromview != null) { ViewBag.showsellerfromview = showsellerfromview; }

            if (employeeid != null) { ViewBag.employeeid = employeeid; }
            if (accessid != null) { ViewBag.accessid = accessid; }
            if (branchid != null) { ViewBag.branchid = branchid; }
            if (name != null) { ViewBag.name = name; }

            if (weekstart != null) { ViewBag.weekstart = weekstart; }
            if (weekend != null) { ViewBag.weekend = weekend; }
            if (yearstart != null) { ViewBag.yearstart = yearstart; }
            if (yearend != null) { ViewBag.yearend = yearend; }
            if (datestart != null) { ViewBag.datestart = datestart; }
            if (dateend != null) { ViewBag.dateend = dateend; }

            if (totalcontacts != null) { ViewBag.totalcontacts = totalcontacts; }
            if (totaldemos != null) { ViewBag.totaldemos = totaldemos; }
            if (totalsales != null) { ViewBag.totalsales = totalsales; }
            if (totalturnover != null) { ViewBag.totalturnover = totalturnover; }

        }
    }
}