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
    public class CollatedStats
    {
        public DateTime? sumdate { get; set; }
        public int? sumdemos { get; set; }
        public int? sumturnover { get; set; }
        public int? sumservice { get; set; }
        public int? sumcontacts { get; set; }
        public int? sumsales { get; set; }
        public int? budget { get; set; }
        public string sumemployeeid { get; set; }
        public string sumname { get; set; }
        public string sumbranchid { get; set; }

    }
    public class CollatedKey
    {
        public decimal? sumdemos { get; set; }
        public int? sumturnover { get; set; }
        public decimal? sumcontacts { get; set; }
        public string sumemployeeid { get; set; }
        public string sumname { get; set; }
        public string sumbranchid { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime? sumdate { get; set; }
    }
    [ValidateAntiForgeryToken]
    [NoCache]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class ManagerController : Controller
    {
        public List<string> exceptions = new List<string>()
            {
                "Tekniker",
                "Chefstekniker",
                "Kontor",
                "TempTech"
            };
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
        // GET: Manager
        public ActionResult Cancellations(string submit, string employeeid, string branchid, string accessid, string stat = null, DateTime? selecteddate = null, string selectedemployeeid = null, int? value = null, int? qty = null, string showsellerfromview = "", DateTime? datestart=null, DateTime? dateend = null)
        {
            bool showseller = false;
            if (submit == "FILIAL ÅNGER") { showseller = false; }
            else
            if (submit == "SÄLJARE ÅNGER") { showseller = true; }
            else
            if (showsellerfromview == "false") { showseller = false; }
            else
            if (showsellerfromview == "true") { showseller = true; }
            if (submit=="SPARA")
            {
                SetViewBag(employeeid: employeeid, branchid: branchid, accessid: accessid);
                if (stat != "Ingen sälj hittat!")
                {
                    try
                    {
                        var newrow = new stat()
                        {
                            EmployeeID = selectedemployeeid,
                            Date = selecteddate,
                            Contacts = 0,
                            Demos = 0,
                            Sales = -(qty),
                            Turnover = -(value),
                            Service=0
                        };
                        db.stats.Add(newrow);
                        db.SaveChanges();

                        ViewBag.lastpage = "manager";
                        ViewBag.title = "Succè!";
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        return View("Changes");
                    }
                    catch
                    {
                        ViewBag.lastpage = "manager";
                        ViewBag.title = "Fel!";
                        ViewBag.message = "Något gick snätt!";
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        return View("Changes");
                    }
                }
                else
                {
                    ViewBag.lastpage = "manager";
                    ViewBag.title = "Fel!";
                    ViewBag.message = "Något gick snätt!";
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
            }
            else if (submit == "Ändra")
            {
                SetViewBag(title: "Ändra period", action: "Cancellations", controller: "Manager", buttonvalue: "Avbryt", lastpage: "filialcancel", showsellerfromview: showsellerfromview);
                SetViewBag(datestart: datestart, dateend: dateend);
                
                ViewBag.employeeid = employeeid;
                ViewBag.branchid = branchid;
                ViewBag.accessid = accessid;
                ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                return View("EditDate");
            }
            else if (submit == "NY ÅNGER")
            {
                var employees = new List<employee>();
                if (accessid == "Administrator" || accessid == "Försäljningschef" || accessid == "Regionschef" || accessid == "Kontor")
                {
                    employees = db.employees.OrderBy(r => r.LastName).ThenBy(r => r.FirstName).ToList();
                }
                else
                {
                    employees = db.employees.Where(r => r.BranchID == branchid).OrderBy(r => r.LastName).ThenBy(r => r.FirstName).ToList();
                }

                List<SelectListItem> selectlistemployee = new List<SelectListItem>();
                foreach (var employee in employees)
                {
                    var temp = new SelectListItem()
                    {
                        Text = employee.FirstName + " " + employee.LastName,
                        Value = employee.EmployeeID
                    };
                    selectlistemployee.Add(temp);
                }
                string temp2 = selectlistemployee[0].Value;
                DateTime current = DateTime.Now.Date;
                try
                {
                    var result = db.stats.Where(r => r.EmployeeID == temp2 && r.Date == current).GroupBy(r => r.EmployeeID).Select(r => new
                    {
                        Sales = r.Sum(a => a.Sales),
                        Turnover = r.Sum(a => a.Turnover)
                    }).First();
                    if (result.Sales == 0 && result.Turnover == 0)
                    {
                        ViewBag.stat = "Ingen sälj hittat!";
                        ViewBag.qty = null;
                        ViewBag.value = null;
                    }
                    else
                    {
                        ViewBag.stat = "Sälj:" + result.Sales + "   Omsättning:" + result.Turnover + "kr";
                        ViewBag.qty = result.Sales;
                        ViewBag.value = result.Turnover;
                    }
                }
                catch
                {
                    ViewBag.stat = "Ingen sälj hittat!";
                    ViewBag.qty = null;
                    ViewBag.value = null;
                }
                ViewBag.selectlistemployee = selectlistemployee;

                SetViewBag(title: "Lägg till ånger", action: "Main", controller: "Home", buttonvalue: "Avbryt", lastpage: "manager");
                SetViewBag(employeeid: employeeid, branchid: branchid, accessid: accessid);
                return View();
            }
            if (datestart == null)
            {
                datestart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dateend = DateTime.Now;
            }
            SetViewBag(datestart: datestart, dateend: dateend);
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            var statsearch = CollateStats(employeeid, accessid, branchid, datestart, dateend, showseller, cancel:true); //Also sets ViewBag.daterange
            if (showseller == true)
            {
                ViewBag.title = "Säljare ånger";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View("StatisticsCancel", statsearch.ToList());
            }
            else
            {
                int? totalcontacts = 0;
                int? totaldemos = 0;
                int? totalsales = 0;
                int? totalturnover = 0;
                int? totalservice = 0;
                int? totalbudgets = 0;
                foreach (var item in statsearch)
                {
                    totalcontacts += item.Sum(r => r.sumcontacts);
                    totaldemos += item.Sum(r => r.sumdemos);
                    totalsales += item.Sum(r => r.sumsales);
                    totalturnover += item.Sum(r => r.sumturnover);
                    totalservice += item.Sum(r => r.sumservice);
                    totalbudgets += item.Sum(r => r.budget);
                }
                ViewBag.totalcontacts = totalcontacts;
                ViewBag.totaldemos = totaldemos;
                ViewBag.totalsales = totalsales;
                ViewBag.totalturnover = totalturnover;
                ViewBag.totalservice = totalservice;
                ViewBag.totalbudgets = totalbudgets;
                ViewBag.title = "Filial ånger";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View("BranchstatisticsCancel", statsearch.ToList());
            }
        }
        public ActionResult Statistics(string submit, string employeeid, string branchid, string accessid, bool showseller = true, DateTime? datestart = null, DateTime? dateend = null, string showsellerfromview = "true")
        {
            if (submit == "FILIAL STATISTIK" || submit == "GRUPP STATISTIK") { showseller = false; }
            else
            if (submit == "SÄLJARE STATISTIK" || submit == "TEKNIKER STATISTIK") { showseller = true; }
            else
            if (showsellerfromview == "false") { showseller = false; }
            else
            if (showsellerfromview == "true") { showseller = true; }
            if (submit == "Ändra")
            {
                SetViewBag(title: "Ändra period", action:"Statistics",controller:"Manager",buttonvalue:"Avbryt",lastpage:"filialstats",showsellerfromview:showsellerfromview);
                SetViewBag(datestart:datestart,dateend:dateend);

                ViewBag.datestart = datestart;
                ViewBag.dateend = dateend;
                ViewBag.employeeid = employeeid;
                ViewBag.branchid = branchid;
                ViewBag.accessid = accessid;
                ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                return View("EditDate");
            }
            if (datestart==null)
            {
                datestart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dateend = DateTime.Now;
            }
            ViewBag.datestart = datestart;
            ViewBag.dateend = dateend;
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            var statsearch = CollateStats(employeeid, accessid, branchid,datestart,dateend,showseller); //Also sets ViewBag.daterange
            if (showseller==true)
            {
                if (accessid == "Chefstekniker" || accessid == "Tekniker")
                {
                    ViewBag.title = "Tekniker statistik";
                }
                else
                {
                    ViewBag.title = "Säljare statistik";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View(statsearch.ToList());
            }
            else
            {
                int? totalcontacts = 0;
                int? totaldemos = 0;
                int? totalsales = 0;
                int? totalturnover = 0;
                int? totalservice = 0;
                int? totalbudgets = 0;
                foreach (var item in statsearch)
                {
                    totalcontacts += item.Sum(r => r.sumcontacts);
                    totaldemos += item.Sum(r => r.sumdemos);
                    totalsales += item.Sum(r => r.sumsales);
                    totalturnover += item.Sum(r => r.sumturnover);
                    totalservice += item.Sum(r => r.sumservice);
                    totalbudgets += item.Sum(r => r.budget);
                }
                ViewBag.totalcontacts = totalcontacts;
                ViewBag.totaldemos = totaldemos;
                ViewBag.totalsales = totalsales;
                ViewBag.totalturnover = totalturnover;
                ViewBag.totalservice = totalservice;
                ViewBag.totalbudgets = totalbudgets;
                if (accessid=="Gruppchef")
                {
                    ViewBag.title = "Grupp statistik";
                }
                else
                {
                    ViewBag.title = "Filial statistik";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View("Branchstatistics",statsearch.ToList());
            }
        }
        public ActionResult StatisticsDay(string submit, string employeeid, string branchid, string accessid, bool showseller = true, DateTime? datestart = null, DateTime? dateend = null, string showsellerfromview = "true")
        {
            if (submit == "FILIAL DAG FÖR DAG" || submit == "GRUPP DAG FÖR DAG") { showseller = false; }
            else
            if (submit == "SÄLJARE DAG FÖR DAG" || submit == "TEKNIKER DAG FÖR DAG") { showseller = true; }
            else
            if (showsellerfromview == "false") { showseller = false; }
            else
            if (showsellerfromview == "true") { showseller = true; }
            if (submit == "Ändra")
            {
                SetViewBag(title: "Ändra period", action: "StatisticsDay", controller: "Manager", buttonvalue: "Avbryt", lastpage: "filialstats", showsellerfromview: showsellerfromview);
                SetViewBag(datestart: datestart, dateend: dateend);

                ViewBag.datestart = datestart;
                ViewBag.dateend = dateend;
                ViewBag.employeeid = employeeid;
                ViewBag.branchid = branchid;
                ViewBag.accessid = accessid;
                ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                return View("EditDateDay");
            }
            if (datestart == null)
            {
                datestart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dateend = DateTime.Now;
            }
            ViewBag.datestart = datestart;
            ViewBag.dateend = dateend;
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            var statsearch = CollateStatsDay(employeeid, accessid, branchid, datestart, dateend, showseller); //Also sets ViewBag.daterange
            if (showseller == true)
            {
                if (accessid == "Chefstekniker" || accessid == "Tekniker")
                {
                    ViewBag.title = "Tekniker dag för dag";
                }
                else
                {
                    ViewBag.title = "Säljare dag för dag";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View(statsearch.ToList());
            }
            else
            {
                int? totalcontacts = 0;
                int? totaldemos = 0;
                int? totalsales = 0;
                int? totalturnover = 0;
                int? totalservice = 0;
                int? totalbudgets = 0;
                foreach (var item in statsearch)
                {
                    totalcontacts += item.Sum(r => r.sumcontacts);
                    totaldemos += item.Sum(r => r.sumdemos);
                    totalsales += item.Sum(r => r.sumsales);
                    totalturnover += item.Sum(r => r.sumturnover);
                    totalservice += item.Sum(r => r.sumservice);
                    totalbudgets += item.Sum(r => r.budget);
                }
                ViewBag.totalcontacts = totalcontacts;
                ViewBag.totaldemos = totaldemos;
                ViewBag.totalsales = totalsales;
                ViewBag.totalturnover = totalturnover;
                ViewBag.totalservice = totalservice;
                ViewBag.totalbudgets = totalbudgets;
                if (accessid == "Gruppchef")
                {
                    ViewBag.title = "Grupp dag för dag";
                }
                else
                {
                    ViewBag.title = "Filial dag för dag";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View("Branchstatisticsday", statsearch.ToList());
            }
        }
        public ActionResult Key(string submit, string employeeid, string branchid, string accessid, bool showseller = true, DateTime? datestart = null, DateTime? dateend = null, string showsellerfromview = "true")
        {
            if (submit == "FILIAL NYCKELTAL" || submit == "GRUPP NYCKELTAL") { showseller = false; }
            else
            if (submit == "SÄLJARE NYCKELTAL") { showseller = true; }
            else
            if (showsellerfromview == "false") { showseller = false; }
            else
            if (showsellerfromview == "true") { showseller = true; }
            if (submit=="Ändra")
            {
                ViewBag.title = "Ändra period";
                ViewBag.action = "Key";
                ViewBag.controller = "Manager";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "filialkey";
                ViewBag.showsellerfromview = showsellerfromview;
                ViewBag.datestart = datestart;
                ViewBag.dateend = dateend;
                ViewBag.employeeid = employeeid;
                ViewBag.branchid = branchid;
                ViewBag.accessid = accessid;
                ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                return View("EditDate");
            }
            if (datestart == null)
            {
                datestart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dateend = DateTime.Now;
            }
            ViewBag.datestart = datestart;
            ViewBag.dateend = dateend;
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            var keysearch = CollateKey(employeeid, accessid, branchid, datestart, dateend, showseller); //Also sets ViewBag.daterange
            if (showseller == true) {
                ViewBag.title = "Säljare nyckeltal";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                try
                {
                    return View(keysearch.ToList());
                }
                catch
                {
                    return View("Key",null);
                }
            } else
            {
                if (accessid == "Gruppchef")
                {
                    ViewBag.title = "Grupp nyckeltal";
                }
                else
                {
                    ViewBag.title = "Filial nyckeltal";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                try
                {
                    return View("Branchkey", keysearch.ToList());
                }
                catch
                {
                    return View("Branchkey", null);
                }
            }
        }
        public ActionResult Users(string employeeid, string branchid, string accessid)
        {
            var usersearch = CollateUsers(accessid, employeeid,branchid);
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            if (branchid == "Huvudkontor")
            {
                ViewBag.title = "Användare";
            }
            else
            {
                ViewBag.title = "Användare (" + branchid + ")";
            }
            ViewBag.action = "Main";
            ViewBag.controller = "Home";
            ViewBag.buttonvalue = "Tillbaka";
            ViewBag.lastpage = "manager";
            return View(usersearch.ToList());
        }
        public ActionResult Budgets(string employeeid, string branchid, string accessid, string weekstart=null, string yearstart = null, string submit = null, int? budgetid=null, string alertmessage=null, string confirmaction = null)
        {
            if (weekstart==null)
            {
                int[] temp = GetWeekOfYear(DateTime.Now);
                weekstart = temp[0].ToString();
                yearstart = temp[1].ToString();
            }
            ViewBag.weekstart = weekstart.ToString();
            ViewBag.yearstart = yearstart.ToString();
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            if (submit == "Ändra")
            {
                ViewBag.title = "Ändra period";
                ViewBag.action = "Budgets";
                ViewBag.controller = "Manager";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "budgets";
                return View("EditWeek");
            }
            if (submit== "OK")
            {
                var budgetsearch = CollateBudgets(branchid,weekstart,yearstart);
                if (branchid == "Huvudkontor")
                {
                    ViewBag.title = "Budgets";
                }
                else
                {
                    ViewBag.title = "Budgets (" + branchid + ")";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View(budgetsearch.ToList());
            }
            else if (submit == "X")
            {
                if (alertmessage!=null)
                {
                    if (confirmaction=="OK")
                    {
                        var budgetdelete = db.budgets.Where(r => r.BudgetID == budgetid).ToList()[0];
                        db.budgets.Remove(budgetdelete);
                        db.SaveChanges();
                        var budgetsearch = CollateBudgets(branchid, weekstart, yearstart);
                        if (branchid == "Huvudkontor")
                        {
                            ViewBag.title = "Budgets";
                        }
                        else
                        {
                            ViewBag.title = "Budgets (" + branchid + ")";
                        }
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        ViewBag.lastpage = "manager";
                        return View(budgetsearch.ToList());
                    }
                    else
                    {
                        var budgetsearch = CollateBudgets(branchid, weekstart, yearstart);
                        if (branchid == "Huvudkontor")
                        {
                            ViewBag.title = "Budgets";
                        }
                        else
                        {
                            ViewBag.title = "Budgets (" + branchid + ")";
                        }
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        ViewBag.lastpage = "manager";
                        return View(budgetsearch.ToList());
                    }
                }
                else
                {
                    ViewBag.buttonpress = "X";
                    ViewBag.budgetid = budgetid;
                    ViewBag.alertmessage = "Vill du verkligen radera raden?";
                    ViewBag.title = "Bekräfta!";
                    ViewBag.action = "Budgets";
                    ViewBag.controller = "Manager";
                    ViewBag.buttonvalue = "Avbryt";
                    return View("Confirmation");
                }
            }
            else if (submit == "REDIGERA")
            {
                var budgetsearch = CollateBudgets(branchid, weekstart, yearstart);
                ViewBag.title = "Redigera budgets";
                ViewBag.action = "EditBudgets";
                ViewBag.controller = "Manager";
                ViewBag.buttonvalue = "Avbryt";
                return View("EditBudgets",budgetsearch.ToList());
            }
            else if (submit == "LÄGG TILL NY")
            {
                var temp = GetWeekOfYear(DateTime.Now);
                ViewBag.currentweek = temp[0];
                ViewBag.currentyear = temp[1];
                ViewBag.employeeidlist = new List<string>();
                if (branchid=="Huvudkontor")
                {
                    var employeesmodel = db.employees.Where(r => r.Role == "Säljare" | r.Role == "Filialchef" | r.Role == "Regionschef" | r.Role == "Försäljningschef" | r.Role == "Gruppchef")
                        .OrderBy(r=>r.BranchID).ThenBy(r=>r.LastName).ThenBy(r=>r.FirstName)
                        .ToList();
                    if (branchid == "Huvudkontor")
                    {
                        ViewBag.title = "Lägg till";
                    }
                    else
                    {
                        ViewBag.title = "Lägg till (" + branchid + ")";
                    }
                    ViewBag.action = "AddBudgets";
                    ViewBag.controller = "Manager";
                    ViewBag.buttonvalue = "Avbryt";
                    return View("AddBudgetsAll", employeesmodel);
                }
                else
                {
                    var employeesmodel = db.employees.Where(r => r.BranchID == branchid && r.Role == "Säljare" | r.Role == "Filialchef" | r.Role == "Regionschef" | r.Role == "Försäljningschef" | r.Role == "Gruppchef")
                        .OrderBy(r => r.LastName).ThenBy(r => r.FirstName)
                        .ToList();
                    if (branchid == "Huvudkontor")
                    {
                        ViewBag.title = "Lägg till";
                    }
                    else
                    {
                        ViewBag.title = "Lägg till (" + branchid + ")";
                    }
                    ViewBag.action = "AddBudgets";
                    ViewBag.controller = "Manager";
                    ViewBag.buttonvalue = "Avbryt";
                    return View("AddBudgetsAll", employeesmodel);
                }
            }
            else
            {
                var budgetsearch = CollateBudgets(branchid, weekstart, yearstart);
                if (branchid == "Huvudkontor")
                {
                    ViewBag.title = "Budgets";
                }
                else
                {
                    ViewBag.title = "Budgets (" + branchid + ")";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View(budgetsearch.ToList());
            }
        }
        [HttpPost]
        public ActionResult EditBudgets(int[] Budget1, int[] BudgetID, int[] Week, int[] Year, string employeeid, string branchid, string accessid, string weekstart = null, string yearstart = null, string submit = null, string alertmessage = null)
        {
            //Save or discard edits to budgets
            ViewBag.weekstart = weekstart.ToString();
            ViewBag.yearstart = yearstart.ToString();
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            if (submit=="SPARA")
            {
                try
                {
                    int i = 0;
                    foreach (var item in BudgetID)
                    {
                        var temp = db.budgets.Where(r => r.BudgetID == item).ToList()[0];
                        temp.Budget1 = Budget1[i];
                        temp.Week = Week[i];
                        temp.BudgetStart = FirstDateOfWeekISO8601(Week[i], Year[i]);
                        temp.BudgetEnd = temp.BudgetStart.AddDays(6.999999);
                        i++;
                    }
                    db.SaveChanges();
                }
                catch { }
                var budgetsearch = CollateBudgets(branchid, weekstart, yearstart);
                if (branchid == "Huvudkontor")
                {
                    ViewBag.title = "Budgets";
                }
                else
                {
                    ViewBag.title = "Budgets (" + branchid + ")";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View("Budgets",budgetsearch.ToList());
            }
            else
            {
                var budgetsearch = CollateBudgets(branchid, weekstart, yearstart);
                if (branchid == "Huvudkontor")
                {
                    ViewBag.title = "Budgets";
                }
                else
                {
                    ViewBag.title = "Budgets (" + branchid + ")";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View("Budgets", budgetsearch.ToList());
            }
        }
        [HttpPost]
        public ActionResult AddBudgets(string select, int[] Budget1, int[] BudgetID, int[] Week, int[] Year, string employeeid, string branchid, string accessid, string[] employeeidlist = null, string weekstart = null, string yearstart = null, string submit = null, string alertmessage = null, string selected = null)
        {
            List<string> templist = new List<string>();
            if (employeeidlist != null)
            {
                templist = employeeidlist.ToList();
            }
            ViewBag.weekstart = weekstart.ToString();
            ViewBag.yearstart = yearstart.ToString();
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            if (submit == "Avbryt")
            {
                var budgetsearch = CollateBudgets(branchid, weekstart, yearstart);
                if (branchid == "Huvudkontor")
                {
                    ViewBag.title = "Budgets";
                }
                else
                {
                    ViewBag.title = "Budgets (" + branchid + ")";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View("Budgets", budgetsearch.ToList());
            }
            else if (submit=="LÄGG TILL")
            {
                var temp = GetWeekOfYear(DateTime.Now);
                ViewBag.currentweek = temp[0];
                ViewBag.currentyear = temp[1];
                ViewBag.budgets = Budget1;
                ViewBag.weeks = Week;
                ViewBag.years = Year;
                templist.Add(selected);
                ViewBag.employeeidlist = templist;
                if (branchid == "Huvudkontor")
                {
                    var employeesmodel = db.employees.Where(r => r.Role == "Säljare" | r.Role == "Filialchef" | r.Role == "Regionschef" | r.Role == "Försäljningschef" | r.Role == "Gruppchef")
                        .OrderBy(r => r.BranchID).ThenBy(r => r.LastName).ThenBy(r => r.FirstName)
                        .ToList();
                    ViewBag.title = "Lägg till";
                    ViewBag.action = "AddBudgets";
                    ViewBag.controller = "Manager";
                    ViewBag.buttonvalue = "Avbryt";
                    return View("AddBudgetsAll", employeesmodel);
                }
                else
                {
                    var employeesmodel = db.employees.Where(r => r.BranchID == branchid && r.Role == "Säljare" | r.Role == "Filialchef" | r.Role == "Regionschef" | r.Role == "Försäljningschef" | r.Role == "Gruppchef")
                        .OrderBy(r => r.LastName).ThenBy(r => r.FirstName)
                        .ToList();
                    ViewBag.title = "Lägg till (" + branchid + ")";
                    ViewBag.action = "AddBudgets";
                    ViewBag.controller = "Manager";
                    ViewBag.buttonvalue = "Avbryt";
                    return View("AddBudgetsAll", employeesmodel);
                }
            }
            else
            {
                int i = 0;
                foreach (var item in employeeidlist)
                {
                    DateTime firstdate = FirstDateOfWeekISO8601(Week[i], Year[i]);
                    var exists = db.budgets.Where(r => r.EmployeeID == item && r.BudgetStart == firstdate).ToList();
                    if (exists.Count==0)
                    {
                        budget newrow = new budget
                        {
                            EmployeeID = item,
                            Budget1 = Budget1[i],
                            Week = Week[i],
                            BudgetStart = firstdate,
                            BudgetEnd = firstdate.AddDays(6.9999999),
                            employee = db.employees.Where(r => r.EmployeeID == item).First()
                        };
                        db.budgets.Add(newrow);
                        db.SaveChanges();
                    }
                    i++;
                }
                var budgetsearch = CollateBudgets(branchid, weekstart, yearstart);
                if (branchid == "Huvudkontor")
                {
                    ViewBag.title = "Budgets";
                }
                else
                {
                    ViewBag.title = "Budgets (" + branchid + ")";
                }
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "manager";
                return View("Budgets", budgetsearch.ToList());
            }
        }
        public ActionResult Unreported(string employeeid, string branchid, string accessid)
        {
            DateTime datestart = DateTime.Now.Date.AddDays(-6);
            DateTime dateend = DateTime.Now.Date.AddDays(0.999999);
            List<List<user>> collateall = new List<List<user>>();
            List<DateTime> dates = new List<DateTime>();
            List<bool> reported = new List<bool>();
            List<bool[]> allreported = new List<bool[]>();
            var usersearch = CollateUsers(accessid, employeeid, branchid).ToList();
            for (int i = 0; i < usersearch.Count; i++)
            {
                reported.Add(false);
            }
            for (DateTime i=dateend;i>=datestart;i=i.AddDays(-1))
            {
                bool[] newreported = new bool[reported.Count];
                reported.CopyTo(newreported, 0);
                DateTime tempend = i.AddDays(-0.999999);
                var statsearch = db.stats.Where
                    (r => r.Sales > -1 && r.Date >= tempend & r.Date <= i
                    ).ToList();
                foreach (var stat in statsearch)
                {
                    for (int j=usersearch.Count-1;j>=0;j--)
                    {
                        if (stat.EmployeeID==usersearch[j].EmployeeID)
                        {
                            newreported[j]=true;
                        }
                    }
                }
                dates.Add(i.Date);
                collateall.Add(usersearch);
                allreported.Add(newreported);
            }
            ViewBag.reported = allreported;
            ViewBag.dates = dates.ToArray();
            ViewBag.datestart = datestart;
            ViewBag.dateend = dateend;
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            ViewBag.accessid = accessid;
            ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            ViewBag.title = "Utestående rapport";
            ViewBag.action = "Main";
            ViewBag.controller = "Home";
            ViewBag.buttonvalue = "Tillbaka";
            ViewBag.lastpage = "manager";
            return View(collateall.ToList());
        }
//Functions
        public ActionResult GetStatFromId(string employeeid, DateTime date)
        {
            try
            {
                var result = db.stats.Where(r => r.EmployeeID == employeeid && r.Date == date.Date).GroupBy(r => r.EmployeeID).Select(r=> new {
                    Sales = r.Sum(a => a.Sales),
                    Turnover = r.Sum(a => a.Turnover)
                }).First();
                if (result.Sales==0 && result.Turnover==0)
                {
                    return Json(new { Sales = "Ingen", Turnover = "sälj hittat!" });
                }
                return Json(result);
            }
            catch
            {
                return Json(new { Sales = "Ingen", Turnover = "sälj hittat!" });
            }
        }
        private int GetWorkDays(DateTime datestart, DateTime dateend, params DateTime[] bankHolidays)
        {
            datestart = datestart.Date;
            dateend = dateend.Date;
            TimeSpan span = dateend - datestart;
            int workdays = span.Days + 1;
            int fullWeekCount = workdays / 7;
            // find out if there are weekends during the time exceedng the full weeks
            if (workdays > fullWeekCount * 7)
            {
                // we are here to find out if there is a 1-day or 2-days weekend
                // in the time interval remaining after subtracting the complete weeks
                int firstDayOfWeek = datestart.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)datestart.DayOfWeek;
                int lastDayOfWeek = dateend.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dateend.DayOfWeek;
                if (lastDayOfWeek < firstDayOfWeek)
                    lastDayOfWeek += 7;
                if (firstDayOfWeek <= 6)
                {
                    if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
                        workdays -= 2;
                    else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
                        workdays -= 1;
                }
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
                    workdays -= 1;
            }

            // subtract the weekends during the full weeks in the interval
            workdays -= fullWeekCount + fullWeekCount;

            // subtract the number of bank holidays during the time interval
            foreach (DateTime bankHoliday in bankHolidays)
            {
                DateTime bh = bankHoliday.Date;
                if (datestart <= bh && bh <= dateend)
                    --workdays;
            }

            return workdays;
        }
        private List<List<CollatedStats>> CollateStats(string employeeid, string accessid, string branchid, DateTime? datestart = null, 
            DateTime? dateend = null, bool showseller = true, bool cancel = false)
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

            if (datestart == null) { datestart = new DateTime(2016,1,1); }
            if (dateend == null) { dateend = DateTime.Now; }

            ViewBag.daterange = ((DateTime)datestart).ToString("d MMM yyyy") + " - " + ((DateTime)dateend).ToString("d MMM yyyy");

            if (accessid == "Gruppchef")
            {
                var members = db.groups.Where(r => r.EmployeeID == employeeid).ToList();
                foreach (var member in members)
                {
                    decimal budgetsum = 0;
                    int budgetdays = 0;
                    try
                    {
                        var budget = new List<budget>();
                        if (showseller)
                        {
                            budget = db.budgets.Where(r => r.EmployeeID == member.EmployeeID).Where(r => r.BudgetStart >= datestart & r.BudgetStart <= dateend || r.BudgetEnd >= datestart & r.BudgetEnd <= dateend).ToList();
                            budget.AddRange(db.budgets.Where(r => r.EmployeeID == member.EmployeeID).Where(r => datestart > r.BudgetStart && dateend < r.BudgetEnd).ToList());
                        }
                        else
                        {
                            budget = db.budgets.Where(r => r.employee.BranchID == member.employee.BranchID).Where(r => r.BudgetStart >= datestart & r.BudgetStart <= dateend || r.BudgetEnd >= datestart & r.BudgetEnd <= dateend).ToList();
                            budget.AddRange(db.budgets.Where(r => r.employee.BranchID == member.employee.BranchID).Where(r => datestart > r.BudgetStart && dateend < r.BudgetEnd).ToList());
                        }
                        var partialbudgets = budget.Where(r => r.BudgetStart < datestart | r.BudgetEnd > dateend).ToList();
                        int workdays = 0;
                        if (showseller)
                        {
                            foreach (var partial in partialbudgets)
                            {
                                DateTime workstart;
                                DateTime workend;
                                if (datestart > partial.BudgetStart) { workstart = (DateTime)datestart; } else { workstart = partial.BudgetStart; }
                                if (dateend < partial.BudgetEnd) { workend = (DateTime)dateend; } else { workend = partial.BudgetEnd; }

                                workdays += GetWorkDays(workstart, workend);
                            }
                        }
                        else
                        {
                            foreach (var partial in partialbudgets.GroupBy(r => r.BudgetStart))
                            {
                                DateTime workstart;
                                DateTime workend;
                                if (datestart > partial.Min(r => r.BudgetStart)) { workstart = (DateTime)datestart; } else { workstart = partial.Min(r => r.BudgetStart); }
                                if (dateend < partial.Min(r => r.BudgetEnd)) { workend = (DateTime)dateend; } else { workend = partial.Min(r => r.BudgetEnd); }

                                workdays += GetWorkDays(workstart, workend);
                            }
                        }
                        if (showseller)
                        {
                            budgetdays = (partialbudgets.Count()) * 5;
                        }
                        else
                        {
                            budgetdays = (partialbudgets.GroupBy(r => r.BudgetStart).Count()) * 5;
                        }
                        budgetsum = (budget.Sum(r => r.Budget1)) - (partialbudgets.Sum(r => r.Budget1));
                        if (partialbudgets.Count != 0)
                        {
                            budgetsum += (((partialbudgets.Sum(r => r.Budget1)) / budgetdays) * workdays);
                        }
                    }
                    catch
                    {
                        budgetsum = 0;
                    }
                    var tempcollated = new CollatedStats();
                    try
                    {
                       tempcollated = db.stats
                       .Where(r => r.Date >= datestart && r.Date <= dateend && r.EmployeeID == member.MemberID)
                       .GroupBy(r => r.EmployeeID)
                       .Select(a => new CollatedStats
                       {
                           budget = (int)budgetsum,
                           sumcontacts = a.Sum(b => b.Contacts),
                           sumdemos = a.Sum(b => b.Demos),
                           sumsales = a.Sum(b => b.Sales),
                           sumturnover = a.Sum(b => b.Turnover),
                           sumservice = a.Sum(b => b.Service),
                           sumemployeeid = a.Key,
                           sumname = a.Min(b => b.employee.FirstName + " " + b.employee.LastName),
                           sumbranchid = a.Min(b => b.employee.BranchID)
                       }).ToList().First();
                    }
                    catch
                    {
                        tempcollated = null;
                    }
                    if (tempcollated!=null)
                    {
                        collated.Add(tempcollated);
                    }
                }
                collated = collated.OrderByDescending(a => a.sumsales).ThenByDescending(a => a.sumturnover).ThenByDescending(a => a.sumdemos).ThenByDescending(a => a.sumcontacts).ThenBy(a => a.sumname).ThenBy(a => a.sumbranchid).ToList();
                collatedall.Add(collated);
            }
            else
            {
                foreach (var branch in db.branches)
                {
                    collated = db.stats
                    .Where(r =>
                        r.employee.Role == accessfilter7 | r.employee.Role==accessfilter1 | r.employee.Role == accessfilter2 | r.employee.Role == accessfilter3 | r.employee.Role == accessfilter4 | r.employee.Role == accessfilter5 | r.employee.Role == accessfilter6 &&
                        r.Date >= datestart & r.Date <= dateend & r.employee.BranchID == branch.BranchID)
                    .GroupBy(r => showseller ? r.EmployeeID : r.employee.BranchID)
                    .Select(a => new CollatedStats
                    {
                        budget = 0,
                        sumcontacts = a.Sum(b => b.Contacts),
                        sumdemos = a.Sum(b => b.Demos),
                        sumsales = a.Sum(b => b.Sales),
                        sumturnover = a.Sum(b => b.Turnover),
                        sumservice = a.Sum(b => b.Service),
                        sumemployeeid = a.Key,
                        sumname = a.Min(b => b.employee.FirstName + " " + b.employee.LastName),
                        sumbranchid = a.Min(b => b.employee.BranchID)
                    })
                    .OrderByDescending(a => a.sumsales).ThenByDescending(a => a.sumturnover).ThenByDescending(a => a.sumdemos).ThenByDescending(a => a.sumcontacts).ThenBy(a => a.sumname).ThenBy(a => a.sumbranchid)
                    .ToList();
                    if (cancel)
                    {
                        foreach (var stat in collated)
                        {
                            stat.sumsales = stat.sumsales + (-(stat.sumsales * 2));
                            stat.sumturnover = stat.sumturnover + (-(stat.sumturnover * 2));
                        }
                    }
                    else
                    {
                        foreach (var employee in collated)
                        {
                            decimal budgetsum = 0;
                            int budgetdays = 0;
                            try
                            {
                                var budget = new List<budget>();
                                if (showseller)
                                {
                                    budget = db.budgets.Where(r => r.EmployeeID == employee.sumemployeeid).Where(r => r.BudgetStart >= datestart & r.BudgetStart <= dateend || r.BudgetEnd >= datestart & r.BudgetEnd <= dateend).ToList();
                                    budget.AddRange(db.budgets.Where(r => r.EmployeeID == employee.sumemployeeid).Where(r => datestart > r.BudgetStart && dateend < r.BudgetEnd).ToList());
                                }
                                else
                                {
                                    budget = db.budgets.Where(r => r.employee.BranchID == employee.sumbranchid).Where(r => r.BudgetStart >= datestart & r.BudgetStart <= dateend || r.BudgetEnd >= datestart & r.BudgetEnd <= dateend).ToList();
                                    budget.AddRange(db.budgets.Where(r => r.employee.BranchID == employee.sumbranchid).Where(r => datestart > r.BudgetStart && dateend < r.BudgetEnd).ToList());
                                }
                                var partialbudgets = budget.Where(r => r.BudgetStart < datestart | r.BudgetEnd > dateend).ToList();
                                int workdays = 0;
                                if (showseller)
                                {
                                    foreach (var partial in partialbudgets)
                                    {
                                        DateTime workstart;
                                        DateTime workend;
                                        if (datestart > partial.BudgetStart) { workstart = (DateTime)datestart; } else { workstart = partial.BudgetStart; }
                                        if (dateend < partial.BudgetEnd) { workend = (DateTime)dateend; } else { workend = partial.BudgetEnd; }

                                        workdays += GetWorkDays(workstart, workend);
                                    }
                                }
                                else
                                {
                                    foreach (var partial in partialbudgets.GroupBy(r => r.BudgetStart))
                                    {
                                        DateTime workstart;
                                        DateTime workend;
                                        if (datestart > partial.Min(r => r.BudgetStart)) { workstart = (DateTime)datestart; } else { workstart = partial.Min(r => r.BudgetStart); }
                                        if (dateend < partial.Min(r => r.BudgetEnd)) { workend = (DateTime)dateend; } else { workend = partial.Min(r => r.BudgetEnd); }

                                        workdays += GetWorkDays(workstart, workend);
                                    }
                                }
                                if (showseller)
                                {
                                    budgetdays = (partialbudgets.Count()) * 5;
                                }
                                else
                                {
                                    budgetdays = (partialbudgets.GroupBy(r => r.BudgetStart).Count()) * 5;
                                }
                                budgetsum = (budget.Sum(r => r.Budget1)) - (partialbudgets.Sum(r => r.Budget1));
                                if (partialbudgets.Count != 0)
                                {
                                    budgetsum += (((partialbudgets.Sum(r => r.Budget1)) / budgetdays) * workdays);
                                }
                                employee.budget = (int)budgetsum;
                            }
                            catch
                            {
                                employee.budget = 0;
                            }
                        }
                    }
                    if (collated.Count!=0)
                    {
                        collatedall.Add(collated);
                    }
                }
            }
            if (collatedall.Count!=0)
            {
                collatedall = collatedall.OrderByDescending(r => r.Sum(a => a.sumsales)).ThenByDescending(r => r.Sum(a => a.sumturnover)).ThenByDescending(r => r.Sum(a => a.sumdemos)).ToList();
            }
            return collatedall;
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
                if (branchid!="Huvudkontor")
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
        private List<List<CollatedKey>> CollateKey(string employeeid, string accessid, string branchid, DateTime? datestart = null, DateTime? dateend = null, bool showseller = true)
        {
            List<CollatedKey> collated = new List<CollatedKey>();
            List<List<CollatedKey>> collatedall = new List<List<CollatedKey>>();

            if (datestart == null) { datestart = new DateTime(2016, 1, 1); }
            if (dateend == null) { dateend = DateTime.Now; }

            ViewBag.daterange = ((DateTime)datestart).ToString("d MMM yyyy") + " - " + ((DateTime)dateend).ToString("d MMM yyyy");

            if (accessid == "Gruppchef")
            {
                var members = db.groups.Where(r => r.EmployeeID == employeeid).ToList();
                foreach (var member in members)
                {
                    var tempcollated = new CollatedKey();
                    try
                    {
                        tempcollated = db.stats
                    .Where(r => r.Date >= datestart && r.Date <= dateend && r.EmployeeID == member.MemberID)
                    .GroupBy(r => r.EmployeeID)
                    .Select(r => new
                    {
                        StatsID = r.Min(a => a.StatsID),
                        EmployeeID = r.Min(a => a.EmployeeID),
                        Contacts = r.Sum(a => a.Contacts),
                        Demos = r.Sum(a => a.Demos),
                        Sales = r.Sum(a => a.Sales),
                        Turnover = r.Sum(a => a.Turnover),
                        Date = r.Min(a => a.Date),
                        BranchID = r.Min(a => a.employee.BranchID),
                        Name = r.Min(a => a.employee.FirstName) + " " + r.Min(a => a.employee.LastName),
                    })
                    .GroupBy(r => r.EmployeeID)
                    .Select(a => new CollatedKey
                    {
                        sumcontacts = a.Sum(b => b.Contacts) == 0 || a.Sum(b => b.Demos) == 0 ? null : (a.Sum(b => (decimal?)b.Contacts) / a.Sum(b => (decimal?)b.Demos)),
                        sumdemos = a.Sum(b => b.Demos) == 0 || a.Sum(b => b.Sales) == 0 ? null : (a.Sum(b => (decimal?)b.Demos) / a.Sum(b => (decimal?)b.Sales)),
                        sumturnover = a.Sum(b => b.Turnover) == 0 || a.Sum(b => b.Sales) == 0 ? null : (a.Sum(b => b.Turnover) / a.Sum(b => b.Sales)),
                        sumemployeeid = a.Key,
                        sumname = a.Min(b => b.Name),
                        sumbranchid = a.Min(b => b.BranchID)
                    })
                    .ToList().First();
                    }
                    catch
                    {
                        tempcollated = null;
                    }
                    if (tempcollated!=null)
                    {
                        collated.Add(tempcollated);
                    }
                }
                collated = collated.OrderByDescending(a => a.sumdemos != null).ThenByDescending(a => a.sumturnover != null).ThenByDescending(a => a.sumcontacts != null).ThenBy(a => a.sumdemos).ThenByDescending(a => a.sumturnover).ThenBy(a => a.sumcontacts).ThenBy(a => a.sumname).ThenBy(a => a.sumbranchid).ToList();
                collatedall.Add(collated);
            }
            else
            {
                foreach (var branch in db.branches)
                {
                    collated = db.stats
                    .Where(r => r.Date >= datestart && r.Date <= dateend && r.employee.BranchID== branch.BranchID)
                    .GroupBy(r => showseller ? r.EmployeeID : r.employee.BranchID)
                    .Select(r => new
                    {
                        StatsID = r.Min(a => a.StatsID),
                        EmployeeID = r.Min(a => a.EmployeeID),
                        Contacts = r.Sum(a => a.Contacts),
                        Demos = r.Sum(a => a.Demos),
                        Sales = r.Sum(a => a.Sales),
                        Turnover = r.Sum(a => a.Turnover),
                        Date = r.Min(a => a.Date),
                        BranchID = r.Min(a => a.employee.BranchID),
                        Name = r.Min(a => a.employee.FirstName) + " " + r.Min(a => a.employee.LastName),
                    })
                    .GroupBy(r => showseller ? r.EmployeeID : r.BranchID)
                    .Select(a => new CollatedKey
                    {
                        sumcontacts = a.Sum(b => b.Contacts) == 0 || a.Sum(b => b.Demos) == 0 ? null : (a.Sum(b => (decimal?)b.Contacts) / a.Sum(b => (decimal?)b.Demos)),
                        sumdemos = a.Sum(b => b.Demos) == 0 || a.Sum(b => b.Sales) == 0 ? null : (a.Sum(b => (decimal?)b.Demos) / a.Sum(b => (decimal?)b.Sales)),
                        sumturnover = a.Sum(b => b.Turnover) == 0 || a.Sum(b => b.Sales) == 0 ? null : (a.Sum(b => b.Turnover) / a.Sum(b => b.Sales)),
                        sumemployeeid = a.Key,
                        sumname = a.Min(b => b.Name),
                        sumbranchid = a.Min(b => b.BranchID)
                    })
                    .OrderByDescending(a => a.sumdemos != null).ThenByDescending(a => a.sumturnover != null).ThenByDescending(a => a.sumcontacts != null).ThenBy(a => a.sumdemos).ThenByDescending(a => a.sumturnover).ThenBy(a => a.sumcontacts).ThenBy(a => a.sumname).ThenBy(a => a.sumbranchid)
                    .ToList();
                    if (collated.Count != 0)
                    {
                        collatedall.Add(collated);
                    }
                }
            }
            collatedall = collatedall.OrderByDescending(r => r.Sum(a => a.sumdemos)!=0).ThenByDescending(r => r.Sum(a => a.sumturnover) != 0).ThenByDescending(r => r.Sum(a => a.sumcontacts) != 0).ThenBy(r => r.Sum(a => a.sumdemos)).ThenByDescending(r => r.Sum(a => a.sumturnover)).ThenBy(r => r.Sum(a => a.sumcontacts)).ThenBy(r => r.Min(a => a.sumname)).ToList();
            var totallist = CollateStats(employeeid, accessid, branchid, datestart, dateend, showseller);
            if (showseller)
            {
                foreach (var item in totallist)
                {
                    var totals = new CollatedKey()
                    {
                        sumemployeeid = "SNITT : ",
                        sumbranchid = item.Min(a => a.sumbranchid),
                        sumcontacts = item.Sum(b => b.sumcontacts) == 0 || item.Sum(b => b.sumdemos) == 0 ? null : (item.Sum(b => (decimal?)b.sumcontacts) / item.Sum(b => (decimal?)b.sumdemos)),
                        sumdemos = item.Sum(b => b.sumcontacts) == 0 || item.Sum(b => b.sumsales) == 0 ? null : (item.Sum(b => (decimal?)b.sumdemos) / item.Sum(b => (decimal?)b.sumsales)),
                        sumturnover = item.Sum(b => b.sumcontacts) == 0 || item.Sum(b => b.sumsales) == 0 ? null : (item.Sum(b => b.sumturnover) / item.Sum(b => b.sumsales)),
                    };
                    try
                    {
                        foreach (var branch in collatedall)
                        {
                            if (branch[0].sumbranchid == totals.sumbranchid)
                            {
                                branch.Add(totals);
                            }
                        }
                    }

                    catch
                    {

                    }
                }
            }
            else
            {
                int? totalcontacts = 0;
                int? totaldemos = 0;
                int? totalturnover = 0;
                int? totalsales = 0;
                foreach (var item in totallist)
                {
                    totalcontacts += item.Sum(a => a.sumcontacts);
                    totaldemos += item.Sum(a => a.sumdemos);
                    totalsales += item.Sum(a => a.sumsales);
                    totalturnover += item.Sum(a => a.sumturnover);
                }
                ViewBag.totalcontacts = totalcontacts;
                ViewBag.totaldemos = totaldemos;
                ViewBag.totalsales = totalsales;
                ViewBag.totalturnover = totalturnover;
            }
            return collatedall;
        }
        private IEnumerable<SakraStats.user> CollateUsers(string accessid, string employeeid, string branchid)
        {
            if (branchid == "Huvudkontor")
            {
                return db.users
                    .Where(r => r.employee.Role == "Säljare" | r.employee.Role == "Filialchef" | r.employee.Role == "Regionschef" | r.employee.Role == "Försäljningschef" | r.employee.Role == "Gruppchef")
                    .OrderBy(a => a.employee.BranchID).ThenBy(a => a.employee.LastName).ThenBy(a => a.employee.FirstName)
                    .ToList();
            }
            else
            {
                if (accessid=="Gruppchef")
                {
                    var members = db.groups.Where(r=>r.EmployeeID==employeeid).ToList();
                    List<user> usersearch = new List<user>();
                    foreach (var member in members)
                    {
                        usersearch.AddRange
                        (db.users
                        .Where(r => r.employee.EmployeeID == member.MemberID)
                        .OrderBy(a => a.employee.BranchID).ThenBy(a => a.employee.LastName).ThenBy(a => a.employee.FirstName)
                        .ToList());
                    }
                    return usersearch;
                }
                else
                {
                    return db.users
                    .Where(r => r.employee.BranchID == branchid && r.employee.Role == "Säljare" | r.employee.Role == "Filialchef" | r.employee.Role == "Regionschef" | r.employee.Role == "Försäljningschef" | r.employee.Role == "Gruppchef")
                    .OrderBy(a => a.employee.BranchID).ThenBy(a => a.employee.LastName).ThenBy(a => a.employee.FirstName)
                    .ToList();
                }
                
            }
        }
        private IEnumerable<SakraStats.budget> CollateBudgets(string branchid, string week, string year)
        {
            DateTime datestart = FirstDateOfWeekISO8601(int.Parse(week),int.Parse(year));
            DateTime dateend = datestart.AddDays(6);
            if (branchid == "Huvudkontor")
            {
                return db.budgets
                    .Where(r => r.BudgetStart >= datestart & r.BudgetEnd <= dateend)
                    .OrderByDescending(a => a.BudgetStart).ThenBy(a => a.employee.BranchID).ThenBy(a => a.employee.LastName).ThenBy(a => a.employee.FirstName)
                    .ToList();
            }
            else
            {
                return db.budgets
                    .Where(r => r.employee.BranchID == branchid && r.BudgetStart >= datestart & r.BudgetEnd <= dateend)
                    .OrderByDescending(a => a.BudgetStart).ThenBy(a => a.employee.BranchID).ThenBy(a => a.employee.LastName).ThenBy(a => a.employee.FirstName)
                    .ToList();
            }
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
        public void SetViewBag(
            string title=null,
            string action = null,
            string controller = null,
            string lastpage = null,
            string buttonvalue = null,
            string showsellerfromview=null,

            string employeeid=null,
            string accessid=null,
            string branchid=null,
            string name=null,

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
            if (title!=null) { ViewBag.title = title; }
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