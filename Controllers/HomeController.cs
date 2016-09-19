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
    [NoCache]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class HomeController : Controller
    {
        HomeContext db = new HomeContext();
        public class NoCacheAttribute : ActionFilterAttribute
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
        // GET: Home
        public ActionResult Login()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        //Verify input from login
        public ActionResult Main(DateTime? datestart, DateTime? dateend, List<bool> DeliverProduct, List<int> StatsID, string lastpage = null, string branchid = null, string accessid = null, string Username = null, string Pass = null, string releasedto = null, string employeeid = null, string submit = null, string employeeidstr=null, string accessidstr = null, string branchidstr = null)
        {
            if (employeeidstr!=null)
            {
                employeeid = employeeidstr;
                branchid = branchidstr;
                accessid = accessidstr;
            }
            try
            {
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.title = "Meny";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            }
            catch { }
            if (submit == "Logga ut") //Return to login page
            {
                ViewBag.employeeid = employeeid;
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                ViewBag.title = "Logga in";
                ViewBag.buttonvalue = "Logga in";
                return RedirectToAction("Login", "Home");
            }
            else if (lastpage != null && lastpage != "") //Sent from back button or cancel button
            {
                if (lastpage == "stores")
                {
                    //Get stores details for individual
                    IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid);
                    ViewBag.employeeid = employeeid;
                    ViewBag.accessid = accessid;
                    ViewBag.branchid = branchid;
                    ViewBag.title = "Ny installation";
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    ViewBag.lastpage = "techmain";
                    return View("ViewStores", storemodel.ToList());
                }
                else if (lastpage == "minkalender")
                {
                    if (accessid=="Säljare")
                    {
                        ViewBag.employeeid = employeeid;
                        ViewBag.accessid = accessid;
                        ViewBag.branchid = branchid;
                        ViewBag.action = "Login";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Logga ut";
                        ViewBag.employeeid = employeeid;
                        ViewBag.title = "Säljportal";
                        return View("ManageSell");
                    }
                    else
                    {
                        ViewBag.employeeid = employeeid;
                        ViewBag.accessid = accessid;
                        ViewBag.branchid = branchid;
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.title = "Säljportal";
                        ViewBag.buttonvalue = "Tillbaka";
                        return View("ManageSell");
                    }
                }
                else if (lastpage == "comp")
                {
                    ViewBag.employeeidstr = employeeid;
                    ViewBag.accessidstr = accessid;
                    ViewBag.branchidstr = branchid;
                    ViewBag.title = "Administrator";
                    return View("ManageAdmin");
                }
                else if (lastpage == "stats")
                {
                    ViewBag.employeeid = employeeid;
                    ViewBag.accessid = accessid;
                    ViewBag.branchid = branchid;
                    ViewBag.title = "Historik";
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    ViewBag.lastpage = "statsmain";
                    return View("ViewStats");
                }
                else if (lastpage == "manager")
                {
                    ViewBag.employeeid = employeeid;
                    ViewBag.accessid = accessid;
                    ViewBag.branchid = branchid;
                    ViewBag.title = "Chefsfunktioner";
                    return View("ManageManager");
                }
                else if (lastpage == "main")
                {
                    ViewBag.employeeid = employeeid;
                    ViewBag.accessid = accessid;
                    ViewBag.branchid = branchid;
                    ViewBag.action = "Login";
                    ViewBag.buttonvalue = "Logga ut";
                    return View("Splash");
                }
                else if (lastpage == "statsmain")
                {
                    if (accessid == "Säljare")
                    {
                        ViewBag.employeeid = employeeid;
                        ViewBag.accessid = accessid;
                        ViewBag.branchid = branchid;
                        ViewBag.action = "Login";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Logga ut";
                        ViewBag.employeeid = employeeid;
                        ViewBag.title = "Säljportal";
                        return View("ManageSell");
                    }
                    ViewBag.employeeid = employeeid;
                    ViewBag.accessid = accessid;
                    ViewBag.branchid = branchid;
                    ViewBag.title = "Säljportal";
                    return View("ManageSell");
                }
                else if (lastpage == "techmain")
                {
                    ViewBag.employeeid = employeeid;
                    ViewBag.accessid = accessid;
                    ViewBag.branchid = branchid;
                    ViewBag.title = "Installation/Service";
                    return View("ManageTech");
                }
                else if (lastpage == "admin")
                {
                    ViewBag.employeeidstr = employeeid;
                    ViewBag.accessidstr = accessid;
                    ViewBag.branchidstr = branchid;
                    ViewBag.title = "Administrator";
                    return View("ManageAdmin");
                }
                else if (lastpage == "statdayforday")
                {
                    var statsearch = CollateStatsDay(employeeid, accessid, branchid, datestart, dateend); //Also sets ViewBag.daterange
                    if (accessid == "Chefstekniker" || accessid == "Tekniker")
                    {
                        ViewBag.title = "Tekniker dag för dag";
                    }
                    else
                    {
                        ViewBag.title = "Säljare dag för dag";
                    }
                    ViewBag.datestart = datestart;
                    ViewBag.dateend = dateend;

                    ViewBag.employeeid = employeeid;
                    ViewBag.accessid = accessid;
                    ViewBag.branchid = branchid;
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    ViewBag.lastpage = "manager";
                    return View("StatisticsDay",statsearch.ToList());
                }
                ViewBag.employeeid = employeeid;
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                ViewBag.title = "Installation/Service";
                return View("ManageTech");
            }
            else if (Username != null && Pass != null) //Sent from login page
            {
                //Get username and pass from view (Login)
                Username = Server.HtmlEncode(Username);
                Username = Username.ToLower();
                Pass = Server.HtmlEncode(Pass);
                //Hash username and pass
                string passhash = GetHash(Pass + Username);
                //Check db for match
                List<user> searchinfo = db.users.Where(a => a.Username == Username && a.Pass == passhash).ToList();
                if (searchinfo.Count != 0)
                {
                    //Check role details and send to appropriate view
                    ViewBag.accessid = searchinfo[0].employee.Role;
                    ViewBag.employeeid = searchinfo[0].EmployeeID;
                    ViewBag.name = searchinfo[0].employee.FirstName + " " + searchinfo[0].employee.LastName;
                    ViewBag.branchid = searchinfo[0].employee.BranchID;
                    if (ViewBag.accessid == "Säljare")
                    {
                        ViewBag.action = "Login";
                        ViewBag.title = "Säljportal";
                        ViewBag.buttonvalue = "Logga ut";
                        return View("ManageSell");
                    }
                    else
                    {
                        ViewBag.action = "Login";
                        ViewBag.controller = "Home";
                        ViewBag.title = "Meny";
                        ViewBag.buttonvalue = "Logga ut";
                        return View("Splash");
                    }
                }
                else
                //No user match
                {
                    ViewBag.title = "Något gick snätt!";
                    ViewBag.action = "Login";
                    ViewBag.controller = "Home";
                    return View("Failed");
                }
            }
            else if (submit == "ADMINISTRATOR") //Sent from splash screen
            {
                ViewBag.employeeidstr = employeeid;
                ViewBag.accessidstr = accessid;
                ViewBag.branchidstr = branchid;
                ViewBag.title = "Administrator";
                return View("ManageAdmin");
            }
            else if (submit == "CHEFSTEKNIKER") //Sent from splash screen
            {
                ViewBag.employeeid = employeeid;
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                ViewBag.title = "Chefstekniker";
                return View("ManageStoreManager");
            }
            else if (submit == "CHEFSFUNKTIONER") //Sent from splash screen
            {
                ViewBag.employeeid = employeeid;
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                ViewBag.title = "Chefsfunktioner";
                return View("ManageManager");
            }
            else if (submit == "SÄLJPORTAL") //Sent from splash screen
            {
                ViewBag.employeeid = employeeid;
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                ViewBag.title = "Säljportal";
                return View("ManageSell");
            }
            else if (submit == "INSTALLATION/SERVICE") //Sent from splash screen
            {
                ViewBag.employeeid = employeeid;
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                ViewBag.title = "Installation/Service";
                return View("ManageTech");
            }
            else  //Sent from back button
            {
                ViewBag.employeeid = employeeid;
                ViewBag.accessid = accessid;
                ViewBag.branchid = branchid;
                ViewBag.action = "Login";
                ViewBag.controller = "Home";
                ViewBag.title = "Meny";
                ViewBag.buttonvalue = "Logga ut";
                return View("Splash");
            }
        }
//Functions
        private IEnumerable<SakraStats.carstat> GetStores(string employeeid)
        {
            IEnumerable<SakraStats.carstat> model = db.carstats.Where(r => r.EmployeeID == employeeid && r.ReleasedTo == "" && r.Approved == false).ToList().OrderBy(r => r.ProductID);
            ViewBag.GetName = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            ViewBag.EmployeeID = employeeid;
            return model.AsEnumerable();
        }
        private String GetHash(string hashstring)
        {
            SHA256Managed crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(hashstring), 0, Encoding.ASCII.GetByteCount(hashstring));
            foreach (byte item in crypto)
            {
                hash += item.ToString("x2");
            }
            return hash;
        }
        private List<List<CollatedStats>> CollateStatsDay(string employeeid, string accessid, string branchid, DateTime? datestart = null, DateTime? dateend = null, bool showseller = true, bool cancel = false)
        {
            string accessfilter1 = "Försäljningschef";
            string accessfilter2 = "Säljare";
            string accessfilter3 = "Filialchef";
            string accessfilter4 = "Regionschef";
            string accessfilter5 = "Filialchef/Lager";
            string accessfilter6 = "Gruppchef";
            string accessfilter7 = "Administrator";

            if (accessid == "Chefstekniker" || accessid == "Tekniker")
            {
                accessfilter1 = "Tekniker";
                accessfilter2 = "Chefstekniker";
                accessfilter3 = "";
                accessfilter4 = "";
                accessfilter5 = "";
                accessfilter6 = "";
                accessfilter7 = "";
            }
            List<CollatedStats> collated = new List<CollatedStats>();
            List<List<CollatedStats>> collatedall = new List<List<CollatedStats>>();

            if (datestart == null) { datestart = new DateTime(2016, 1, 1); }
            if (dateend == null) { dateend = DateTime.Now; }

            ViewBag.daterange = ((DateTime)datestart).ToString("d MMM yyyy") + " - " + ((DateTime)dateend).ToString("d MMM yyyy");

            if (accessid == "Gruppchef")
            {
                var members = db.groups.Where(r => r.EmployeeID == employeeid).ToList();
                foreach (var member in members)
                {
                    var tempcollated = new List<CollatedStats>();
                    try
                    {
                        tempcollated = db.stats
                        .Where(r => r.Date >= datestart && r.Date <= dateend && r.EmployeeID == member.MemberID)
                        .GroupBy(r => new { Date = r.Date, Employee = r.EmployeeID })
                        .Select(a => new CollatedStats
                        {
                            sumdate = a.Min(b => (DateTime)b.Date),
                            sumcontacts = a.Sum(b => b.Contacts),
                            sumdemos = a.Sum(b => b.Demos),
                            sumsales = a.Sum(b => b.Sales),
                            sumturnover = a.Sum(b => b.Turnover),
                            sumservice = a.Sum(b => b.Service),
                            sumemployeeid = a.Min(b => b.EmployeeID),
                            sumname = a.Min(b => b.employee.FirstName + " " + b.employee.LastName),
                            sumbranchid = a.Min(b => b.employee.BranchID)
                        }).ToList();
                    }
                    catch
                    {
                        tempcollated = null;
                    }
                    if (tempcollated != null)
                    {
                        collated.AddRange(tempcollated);
                    }
                }
                collated = collated.OrderByDescending(a => a.sumdate).ThenByDescending(a => a.sumsales).ThenByDescending(a => a.sumturnover).ThenByDescending(a => a.sumdemos).ThenByDescending(a => a.sumcontacts).ThenBy(a => a.sumname).ThenBy(a => a.sumbranchid).ToList();
                collatedall.Add(collated);
            }
            else
            {
                if (branchid != "Huvudkontor")
                {
                    collated = db.stats
                    .Where(r =>
                        r.employee.Role == accessfilter7 | r.employee.Role == accessfilter1 | r.employee.Role == accessfilter2 | r.employee.Role == accessfilter3 | r.employee.Role == accessfilter4 | r.employee.Role == accessfilter5 | r.employee.Role == accessfilter6 &&
                        r.Date >= datestart & r.Date <= dateend & r.employee.BranchID == branchid)
                    .GroupBy(r => showseller ? new { Date = r.Date, Employee = r.EmployeeID } : new { Date = r.Date, Employee = r.employee.BranchID })
                    .Select(a => new CollatedStats
                    {
                        sumdate = a.Min(b => (DateTime)b.Date),
                        sumcontacts = a.Sum(b => b.Contacts),
                        sumdemos = a.Sum(b => b.Demos),
                        sumsales = a.Sum(b => b.Sales),
                        sumturnover = a.Sum(b => b.Turnover),
                        sumservice = a.Sum(b => b.Service),
                        sumemployeeid = a.Min(b => b.EmployeeID),
                        sumname = a.Min(b => b.employee.FirstName + " " + b.employee.LastName),
                        sumbranchid = a.Min(b => b.employee.BranchID)
                    })
                    .OrderByDescending(a => a.sumdate).ThenBy(a => a.sumbranchid).ThenByDescending(a => a.sumsales).ThenByDescending(a => a.sumturnover).ThenByDescending(a => a.sumdemos).ThenBy(a => a.sumname)
                    .ToList();
                    if (collated.Count != 0)
                    {
                        collatedall.Add(collated);
                    }
                }
                else
                {
                    foreach (var branch in db.branches)
                    {
                        collated = db.stats
                        .Where(r =>
                            r.employee.Role == accessfilter7 | r.employee.Role == accessfilter1 | r.employee.Role == accessfilter2 | r.employee.Role == accessfilter3 | r.employee.Role == accessfilter4 | r.employee.Role == accessfilter5 | r.employee.Role == accessfilter6 &&
                            r.Date >= datestart & r.Date <= dateend & r.employee.BranchID == branch.BranchID)
                        .GroupBy(r => showseller ? new { Date = r.Date, Employee = r.EmployeeID } : new { Date = r.Date, Employee = r.employee.BranchID })
                        .Select(a => new CollatedStats
                        {
                            sumdate = a.Min(b => (DateTime)b.Date),
                            sumcontacts = a.Sum(b => b.Contacts),
                            sumdemos = a.Sum(b => b.Demos),
                            sumsales = a.Sum(b => b.Sales),
                            sumturnover = a.Sum(b => b.Turnover),
                            sumservice = a.Sum(b => b.Service),
                            sumemployeeid = a.Min(b => b.EmployeeID),
                            sumname = a.Min(b => b.employee.FirstName + " " + b.employee.LastName),
                            sumbranchid = a.Min(b => b.employee.BranchID)
                        })
                        .OrderByDescending(a => a.sumdate).ThenBy(a => a.sumbranchid).ThenByDescending(a => a.sumsales).ThenByDescending(a => a.sumturnover).ThenByDescending(a => a.sumdemos).ThenBy(a => a.sumname)
                        .ToList();
                        if (collated.Count != 0)
                        {
                            collatedall.Add(collated);
                        }
                    }
                }
            }
            collatedall = collatedall.OrderByDescending(r => r.Sum(a => a.sumsales)).ThenByDescending(r => r.Min(a => a.sumbranchid)).ThenByDescending(r => r.Sum(a => a.sumturnover)).ThenByDescending(r => r.Sum(a => a.sumdemos)).ToList();
            return collatedall;
        }

    }
}