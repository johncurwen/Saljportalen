using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SakraStats.Controllers
{
    [ValidateAntiForgeryToken]
    [NoCache]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class AdminController : Controller
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
        // GET: Admin
        public ActionResult Employees(string employeeidstr, string accessidstr, string branchidstr, string submit, string id, 
            string EmployeeID=null, string FirstName = null, string LastName=null, string Address1 = null, string Address2 = null, string Address3 = null,
            string BranchID = null, string Postcode = null, string Tel = null, DateTime? DateEmployed = null, DateTime? DateFinished = null, 
            string Role=null)
        {
            if (submit=="Save")
            {
                employee changedrow;
                try
                {
                    changedrow = db.employees.Where(r => r.EmployeeID == EmployeeID).ToList()[0];
                }
                catch
                {
                    changedrow = db.employees.Where(r => r.FirstName == FirstName && r.LastName == LastName && r.BranchID == BranchID).ToList()[0];
                }
                changedrow.EmployeeID = EmployeeID;
                changedrow.FirstName = FirstName;
                changedrow.LastName = LastName;
                changedrow.Address1 = Address1;
                changedrow.Address2 = Address2;
                changedrow.Address3 = Address3;
                changedrow.BranchID = BranchID;
                changedrow.Postcode = Postcode;
                changedrow.Tel = Tel;
                changedrow.DateEmployed = DateEmployed;
                changedrow.DateFinished = DateFinished;
                changedrow.Role = Role;
                db.SaveChanges();
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Anställda";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.employees.ToList());
            }
            else if (submit == "Create")
            {
                employee newrow = new employee()
                {
                    EmployeeID = EmployeeID,
                    FirstName = FirstName,
                    LastName = LastName,
                    Address1 = Address1,
                    Address2 = Address2,
                    Address3 = Address3,
                    BranchID = BranchID,
                    Postcode = Postcode,
                    Tel = Tel,
                    DateEmployed = DateEmployed,
                    DateFinished = DateFinished,
                    Role = Role
                };
                db.employees.Add(newrow);
                db.SaveChanges();
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Anställda";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.employees.ToList());
            }
            if (submit == "Delete")
            {
                employee delrow;
                try
                {
                    delrow = db.employees.Where(r => r.EmployeeID == EmployeeID).ToList()[0];
                    db.employees.Remove(delrow);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                   
                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Anställda";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.employees.ToList());
            }
            else if (submit == "NEW")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Lägg till";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("EmployeesNew", new employee());
            }
            else if (submit == "DELETE")
            {
                ViewBag.employeeid = id;
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Ta bort";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("EmployeesDelete", db.employees.Where(r => r.EmployeeID == id).ToList()[0]);
            }
            else if (submit == "EDIT")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Redigera";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("EmployeesEdit", db.employees.Where(r => r.EmployeeID == id).ToList()[0]);
            }
            else if (submit == "DETAILS")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Detaljer";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View("EmployeesDetails", db.employees.Where(r=>r.EmployeeID==id).ToList()[0]);
            }
            else
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Anställda";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.employees.ToList());
            }
        }
        public ActionResult Users(string employeeidstr, string accessidstr, string branchidstr, string submit, string id,
            string EmployeeID = null, string Username = null, string Pass = null, string Pass1 = null, string Pass2 = null, string Email = null, 
            string selected = null)
        {
            if (submit == "Save")
            {
                user changedrow;
                try
                {
                    changedrow = db.users.Where(r => r.EmployeeID == EmployeeID).ToList()[0];
                }
                catch
                {
                    changedrow = db.users.Where(r => r.Username == Username).ToList()[0];
                }
                if (Pass1==Pass2)
                {
                    if (Pass1!=null && Pass1!="") { Pass = GetHash(Pass1 + Username); }
                    changedrow.EmployeeID = EmployeeID;
                    changedrow.Username = Username;
                    changedrow.Pass = Pass;
                    changedrow.Email = Email;
                    db.SaveChanges();
                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Användare";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.users.ToList());
            }
            else if (submit == "Create")
            {
                if (Pass1 == Pass2) //Check if the checkpass is the same as the pass
                {
                    if (Pass1 != null && Pass1 != "") // Check if a pass has been entyered
                    {
                        var checkusers = db.users.Where(r => r.EmployeeID == EmployeeID || r.Username == Username).ToList(); //Check if user already exists
                        if (checkusers.Count==0)
                        {
                            Pass = GetHash(Pass1 + Username);
                            user newrow = new user()
                            {
                                EmployeeID = EmployeeID,
                                Username = Username,
                                Pass = Pass,
                                Email = Email
                            };
                            db.users.Add(newrow);
                            db.SaveChanges();
                        }
                    }
                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Användare";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.users.ToList());
            }
            if (submit == "Delete")
            {
                user delrow;
                try
                {
                    delrow = db.users.Where(r => r.EmployeeID == EmployeeID).ToList()[0];
                    db.users.Remove(delrow);
                    db.SaveChanges();
                }
                catch
                {

                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Användare";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.users.ToList());
            }
            else if (submit == "NEW")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Lägg till";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                //Get list of employees
                List<employee> employeesmodel = db.employees.Where(r=>r.users.ToList().Count==0)
                        .OrderBy(r => r.BranchID).ThenBy(r => r.LastName).ThenBy(r => r.FirstName)
                        .ToList();
                List<string> tempid = employeesmodel.Select(r => r.EmployeeID).ToList();
                List<string> tempname = employeesmodel.Select(r => r.FirstName + " " + r.LastName).ToList();
                int i = 0;
                List<SelectListItem> selectlist = new List<SelectListItem>();
                foreach (var item in tempname)
                {
                    var temp = new SelectListItem();
                    temp.Text = item;
                    temp.Value = tempid[i];
                    selectlist.Add(temp);
                    i++;
                }
                ViewBag.selectlist = selectlist;
                return View("UsersNew", new user());
            }
            else if (submit == "DELETE")
            {
                ViewBag.employeeid = id;
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Ta bort";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("UsersDelete", db.users.Where(r => r.EmployeeID == id).ToList()[0]);
            }
            else if (submit == "EDIT")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Redigera";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("UsersEdit", db.users.Where(r => r.EmployeeID == id).ToList()[0]);
            }
            else if (submit == "DETAILS")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Detaljer";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View("UsersDetails", db.users.Where(r => r.EmployeeID == id).ToList()[0]);
            }
            else
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Användare";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.users.ToList());
            }
        }
        public ActionResult Branches(string employeeidstr, string accessidstr, string branchidstr, string submit, string id,
                    string BranchID = null, string Address1 = null, string Address2 = null, string Address3 = null, string Postcode = null, 
                    string Tel = null)
        {
            if (submit == "Save")
            {
                branch changedrow;
                try
                {
                    changedrow = db.branches.Where(r => r.BranchID == BranchID).ToList()[0];
                    changedrow.BranchID = BranchID;
                    changedrow.Address1 = Address1;
                    changedrow.Address2 = Address2;
                    changedrow.Address3 = Address3;
                    changedrow.Postcode = Postcode;
                    changedrow.Tel = Tel;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Filialer";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.branches.ToList());
            }
            else if (submit == "Create")
            {
                var checkbranches = db.branches.Where(r => r.BranchID == BranchID).ToList(); //Check if user already exists
                if (checkbranches.Count == 0)
                {
                    branch newrow = new branch()
                    {
                        BranchID = BranchID,
                        Address1 = Address1,
                        Address2 = Address2,
                        Address3 = Address3,
                        Postcode=Postcode,
                        Tel=Tel
                    };
                    db.branches.Add(newrow);
                    db.SaveChanges();
                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Filialer";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.branches.ToList());
            }
            if (submit == "Delete")
            {
                branch delrow;
                try
                {
                    delrow = db.branches.Where(r => r.BranchID == id).ToList()[0];
                    db.branches.Remove(delrow);
                    db.SaveChanges();
                }
                catch
                {

                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Filialer";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.branches.ToList());
            }
            else if (submit == "NEW")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Lägg till";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("BranchesNew", new branch());
            }
            else if (submit == "DELETE")
            {
                ViewBag.branchid = id;
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Ta bort";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("BranchesDelete", db.branches.Where(r => r.BranchID == id).ToList()[0]);
            }
            else if (submit == "EDIT")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Redigera";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("BranchesEdit", db.branches.Where(r => r.BranchID == id).ToList()[0]);
            }
            else if (submit == "DETAILS")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Detaljer";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View("BranchesDetails", db.branches.Where(r => r.BranchID == id).ToList()[0]);
            }
            else
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Filialer";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.branches.ToList());
            }
        }
        public ActionResult Statistics(string employeeidstr, string accessidstr, string branchidstr, string submit, int? id,
                    string EmployeeID = null, int? Contacts = 0, int? Demos = 0, int? Sales = 0, int? Turnover = 0, DateTime? Date = null,
                    string selected = null, int? StatsID = null)
        {
            if (submit == "Save")
            {
                if (Date == null) { Date = DateTime.Now; }
                stat changedrow;
                try
                {
                    changedrow = db.stats.Where(r => r.StatsID == StatsID).ToList()[0];
                    List<demreport> editdem = new List<demreport>();
                    editdem = db.demreports.Where(r => r.EmployeeID == changedrow.EmployeeID && r.Date == changedrow.Date).ToList();
                    foreach (var dem in editdem)
                    {
                        dem.Date=(DateTime)Date;
                    }
                    changedrow.EmployeeID = EmployeeID;
                    changedrow.Contacts = Contacts;
                    changedrow.Demos = Demos;
                    changedrow.Sales = Sales;
                    changedrow.Turnover = Turnover;
                    changedrow.Date = Date;
                    db.SaveChanges();
                    ViewBag.employeeidstr = employeeidstr;
                    ViewBag.accessidstr = accessidstr;
                    ViewBag.branchidstr = branchidstr;
                    ViewBag.title = "Succé!";
                    ViewBag.Message = "Dina ändringar har sparats!";
                    ViewBag.action = "Statistics";
                    ViewBag.controller = "Admin";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
                catch
                {
                    ViewBag.employeeidstr = employeeidstr;
                    ViewBag.accessidstr = accessidstr;
                    ViewBag.branchidstr = branchidstr;
                    ViewBag.title = "Fel!";
                    ViewBag.Message = "Något gick snätt!";
                    ViewBag.action = "Statistics";
                    ViewBag.controller = "Admin";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
            }
            else if (submit == "Create")
            {
                if (Date == null) { Date = DateTime.Now; }
                stat newrow = new stat()
                {
                    EmployeeID = EmployeeID,
                    Contacts = Contacts,
                    Demos = Demos,
                    Sales = Sales,
                    Turnover = Turnover,
                    Date = Date
                };
                db.stats.Add(newrow);
                db.SaveChanges();
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Succé!";
                ViewBag.Message = "Dina ändringar har sparats!";
                ViewBag.action = "Statistics";
                ViewBag.controller = "Admin";
                ViewBag.buttonvalue = "Tillbaka";
                return View("Changes");
            }
            if (submit == "Delete")
            {
                stat delrow;
                List<demreport> deldem = new List<demreport>();
                try
                {
                    delrow = db.stats.Where(r => r.StatsID == id).ToList()[0];
                    deldem = db.demreports.Where(r => r.EmployeeID == delrow.EmployeeID && r.Date == delrow.Date).ToList();
                    db.stats.Remove(delrow);
                    foreach (var dem in deldem)
                    {
                        db.demreports.Remove(dem);
                    }
                    db.SaveChanges();
                    ViewBag.employeeidstr = employeeidstr;
                    ViewBag.accessidstr = accessidstr;
                    ViewBag.branchidstr = branchidstr;
                    ViewBag.title = "Succé!";
                    ViewBag.Message = "Dina ändringar har sparats!";
                    ViewBag.action = "Statistics";
                    ViewBag.controller = "Admin";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
                catch
                {
                    ViewBag.employeeidstr = employeeidstr;
                    ViewBag.accessidstr = accessidstr;
                    ViewBag.branchidstr = branchidstr;
                    ViewBag.title = "Fel!";
                    ViewBag.Message = "Något gick snätt!";
                    ViewBag.action = "Statistics";
                    ViewBag.controller = "Admin";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
            }
            else if (submit == "NEW")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Lägg till";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                //Get list of employees
                List<employee> employeesmodel = db.employees
                        .OrderBy(r => r.BranchID).ThenBy(r => r.LastName).ThenBy(r => r.FirstName)
                        .ToList();
                List<string> tempid = employeesmodel.Select(r => r.EmployeeID).ToList();
                List<string> tempname = employeesmodel.Select(r => r.FirstName + " " + r.LastName).ToList();
                int i = 0;
                List<SelectListItem> selectlist = new List<SelectListItem>();
                foreach (var item in tempname)
                {
                    var temp = new SelectListItem();
                    temp.Text = item;
                    temp.Value = tempid[i];
                    selectlist.Add(temp);
                    i++;
                }
                ViewBag.selectlist = selectlist;
                return View("StatsNew", new stat() { Date = DateTime.Now });
            }
            else if (submit == "DELETE")
            {
                ViewBag.statid = id;
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Ta bort";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("StatsDelete", db.stats.Where(r => r.StatsID == id).ToList()[0]);
            }
            else if (submit == "EDIT")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Redigera";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("StatsEdit", db.stats.Where(r => r.StatsID == id).ToList()[0]);
            }
            else if (submit == "DETAILS")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Detaljer";
                ViewBag.action = "Statistics";
                ViewBag.controller = "Admin";
                ViewBag.buttonvalue = "Tillbaka";
                return View("StatsDetails", db.stats.Where(r => r.StatsID == id).ToList()[0]);
            }
            else
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Statistik";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                if(submit=="LOAD ALL")
                {
                    return View(db.stats.OrderByDescending(r => r.Date).ThenBy(r => r.employee.BranchID).ThenBy(r => r.employee.LastName).ToList());

                }
                else
                {
                    DateTime searchdate = DateTime.Now.Date.AddDays(-14);
                    return View(db.stats.Where(r => r.Date >= searchdate).OrderByDescending(r => r.Date).ThenBy(r => r.employee.BranchID).ThenBy(r => r.employee.LastName).ToList());
                }
            }
        }
        public ActionResult Groups(string employeeidstr, string accessidstr, string branchidstr, string submit, int? id = null,
                    string EmployeeID = null, string MemberID = null, string BranchID = null, int? GroupID = null, string selected = null)
        {
            if (submit == "Save")
            {
                group changedrow;
                try
                {
                    changedrow = db.groups.Where(r => r.GroupID == GroupID).ToList()[0];
                    changedrow.EmployeeID = EmployeeID;
                    changedrow.MemberID = MemberID;
                    db.SaveChanges();
                }
                catch
                {
                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Grupper";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.groups.OrderBy(r => r.employee.BranchID).ThenBy(r => r.employee.LastName).ThenBy(r => r.employee1.LastName).ToList());
            }
            else if (submit == "Create")
            {
                group newrow = new group()
                {
                    EmployeeID = EmployeeID,
                    MemberID = MemberID,
                };
                db.groups.Add(newrow);
                db.SaveChanges();
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Grupper";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.groups.OrderBy(r => r.employee.BranchID).ThenBy(r => r.employee.LastName).ThenBy(r => r.employee1.LastName).ToList());
            }
            if (submit == "Delete")
            {
                group delrow;
                try
                {
                    delrow = db.groups.Where(r => r.GroupID == GroupID).ToList()[0];
                    db.groups.Remove(delrow);
                    db.SaveChanges();
                }
                catch
                {

                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Grupper";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.groups.OrderBy(r => r.employee.BranchID).ThenBy(r => r.employee.LastName).ThenBy(r => r.employee1.LastName).ToList());
            }
            if (submit=="Next")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Lägg till";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                //Get list of employees
                BranchID = db.employees.Where(r => r.EmployeeID == EmployeeID).ToList()[0].BranchID;
                List<employee> employeesmodel = db.employees.Where(r => r.BranchID == BranchID)
                        .OrderBy(r => r.LastName).ThenBy(r => r.FirstName)
                        .ToList();
                List<string> tempid = employeesmodel.Select(r => r.EmployeeID).ToList();
                List<string> tempname = employeesmodel.Select(r => r.FirstName + " " + r.LastName).ToList();
                int i = 0;
                List<SelectListItem> selectlist = new List<SelectListItem>();
                foreach (var item in tempname)
                {
                    var temp = new SelectListItem();
                    temp.Text = item;
                    temp.Value = tempid[i];
                    selectlist.Add(temp);
                    i++;
                }
                ViewBag.selectlist = selectlist;
                return View("GroupsNewMember", new group() { EmployeeID = EmployeeID });
            }
            else if (submit == "NEW")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Lägg till";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                //Get list of employees
                List<employee> employeesmodel = db.employees
                        .OrderBy(r => r.LastName).ThenBy(r => r.FirstName)
                        .ToList();
                List<string> tempid = employeesmodel.Select(r => r.EmployeeID).ToList();
                List<string> tempname = employeesmodel.Select(r => r.FirstName + " " + r.LastName).ToList();
                int i = 0;
                List<SelectListItem> selectlist = new List<SelectListItem>();
                foreach (var item in tempname)
                {
                    var temp = new SelectListItem();
                    temp.Text = item;
                    temp.Value = tempid[i];
                    selectlist.Add(temp);
                    i++;
                }
                ViewBag.selectlist = selectlist;
                return View("GroupsNewOwner");
            }
            else if (submit == "DELETE")
            {
                ViewBag.groupid = id;
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Ta bort";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("GroupsDelete", db.groups.Where(r => r.GroupID == id).ToList()[0]);
            }
            else if (submit == "EDIT")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Redigera";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                //Get list of employees
                BranchID = db.groups.Where(r => r.GroupID == id).ToList()[0].employee.BranchID;
                List<employee> employeesmodel = db.employees.Where(r => r.BranchID == BranchID)
                        .OrderBy(r => r.LastName).ThenBy(r => r.FirstName)
                        .ToList();
                List<string> tempid = employeesmodel.Select(r => r.EmployeeID).ToList();
                List<string> tempname = employeesmodel.Select(r => r.FirstName + " " + r.LastName).ToList();
                int i = 0;
                List<SelectListItem> selectlist = new List<SelectListItem>();
                foreach (var item in tempname)
                {
                    var temp = new SelectListItem();
                    temp.Text = item;
                    temp.Value = tempid[i];
                    selectlist.Add(temp);
                    i++;
                }
                ViewBag.selectlist = selectlist;
                return View("GroupsEdit", db.groups.Where(r => r.GroupID == id).ToList()[0]);
            }
            else if (submit == "DETAILS")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Detaljer";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View("GroupsDetails", db.groups.Where(r => r.GroupID == id).ToList()[0]);
            }
            else
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Grupper";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.groups.OrderBy(r => r.employee.BranchID).ThenBy(r => r.employee.LastName).ThenBy(r => r.employee1.LastName).ToList());
            }
        }
        public ActionResult Roles(string employeeidstr, string accessidstr, string branchidstr, string submit, string id = null,
                    string AccessID = null)
        {
            if (submit == "Create")
            {
                access newrow = new access()
                {
                    AccessID = AccessID
                };
                db.accesses.Add(newrow);
                db.SaveChanges();
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Roll";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.accesses.OrderBy(r => r.AccessID).ToList());
            }
            if (submit == "Delete")
            {
                access delrow;
                try
                {
                    delrow = db.accesses.Where(r => r.AccessID == AccessID).ToList()[0];
                    db.accesses.Remove(delrow);
                    db.SaveChanges();
                }
                catch
                {

                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Roll";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.accesses.OrderBy(r => r.AccessID).ToList());
            }
            else if (submit == "NEW")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Lägg till";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("RolesNew", new access());
            }
            else if (submit == "DELETE")
            {
                ViewBag.accessid = id;
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Ta bort";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("RolesDelete", db.accesses.Where(r => r.AccessID == id).ToList()[0]);
            }
            else
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Roll";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.accesses.OrderBy(r => r.AccessID).ToList());
            }
        }
        public ActionResult  BranchStores(string employeeidstr, string accessidstr, string branchidstr, string submit, string id = null, string BranchID = null, string StoreID = null)
        {
            if (submit == "Create")
            {
                try
                {
                    store newrow = new store()
                    {
                        StoreID = StoreID,
                        BranchID = BranchID
                    };
                    db.stores.Add(newrow);
                    db.SaveChanges();
                }
                catch
                {

                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Filial lager";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.stores.OrderBy(r => r.BranchID).ThenBy(r=>r.StoreID).ToList());
            }
            if (submit == "Delete")
            {
                store delrow;
                try
                {
                    delrow = db.stores.Where(r => r.StoreID == id).ToList()[0];
                    db.stores.Remove(delrow);
                    db.SaveChanges();
                }
                catch
                {

                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Roll";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.stores.OrderBy(r => r.BranchID).ThenBy(r => r.StoreID).ToList());
            }
            else if (submit == "NEW")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Lägg till";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("BranchStoresNew", new store());
            }
            else if (submit == "DELETE")
            {
                ViewBag.storeid = id;
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Ta bort";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("BranchStoresDelete", db.stores.Where(r => r.StoreID == id).ToList()[0]);
            }
            else
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Roll";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.stores.OrderBy(r => r.BranchID).ThenBy(r => r.StoreID).ToList());
            }
        }
        public ActionResult Products(string employeeidstr, string accessidstr, string branchidstr, string submit, string id="",
                    string ProductID = null, decimal? Price = 0, string Name = "")
        {
            if (submit == "Save")
            {
                product changedrow;
                try
                {
                    changedrow = db.products.Where(r => r.ProductID== ProductID).ToList()[0];
                    changedrow.ProductID=ProductID;
                    changedrow.Price=Price;
                    changedrow.Name=Name;
                    db.SaveChanges();
                }
                catch
                {
                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Produkt";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.products.OrderBy(r => r.Name).ThenBy(r => r.ProductID).ThenBy(r => r.Price).ToList());
            }
            else if (submit == "Create")
            {
                product newrow = new product()
                {
                    ProductID = ProductID,
                    Price = Price,
                    Name = Name
                };
                db.products.Add(newrow);
                db.SaveChanges();
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Produkt";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.products.OrderBy(r => r.Name).ThenBy(r => r.ProductID).ThenBy(r => r.Price).ToList());
            }
            if (submit == "Delete")
            {
                product delrow;
                try
                {
                    delrow = db.products.Where(r => r.ProductID == id).ToList()[0];
                    db.products.Remove(delrow);
                    db.SaveChanges();
                }
                catch
                {

                }
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Produkt";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.products.OrderBy(r => r.Name).ThenBy(r => r.ProductID).ThenBy(r => r.Price).ToList());
            }
            else if (submit == "NEW")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Lägg till";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("ProductsNew", new product());
            }
            else if (submit == "DELETE")
            {
                ViewBag.id = id;
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Ta bort";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("ProductsDelete", db.products.Where(r => r.ProductID== id).ToList()[0]);
            }
            else if (submit == "EDIT")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Redigera";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "admin";
                return View("ProductsEdit", db.products.Where(r => r.ProductID == id).ToList()[0]);
            }
            else if (submit == "DETAILS")
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Detaljer";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View("ProductsDetails", db.products.Where(r => r.ProductID == id).ToList()[0]);
            }
            else
            {
                ViewBag.employeeidstr = employeeidstr;
                ViewBag.accessidstr = accessidstr;
                ViewBag.branchidstr = branchidstr;
                ViewBag.title = "Produkt";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "admin";
                return View(db.products.OrderBy(r => r.Name).ThenBy(r => r.ProductID).ThenBy(r => r.Price).ToList());
            }
        }
        public ActionResult Competitions(string submit, string employeeidstr, string accessidstr, string branchidstr, string id = null, string CompText = null, string CompName = null, string CompDate = null, string CompReq1 = null, string CompReq2 = null, string CompReq3 = null, string CompReq4 = null, string CompReq5 = null, string List1 = null, string List2 = null, string List3 = null, string List4 = null, string List5 = null)
        {
            ViewBag.employeeidstr = employeeidstr;
            ViewBag.accessidstr = accessidstr;
            ViewBag.branchidstr = branchidstr;
            ViewBag.title = "Tävlingar";
            ViewBag.action = "Competitions";
            ViewBag.controller = "Admin";
            ViewBag.buttonvalue = "Tillbaka";
            ViewBag.lastpage = "comp";
            if (submit=="TÄVLINGAR" || submit=="Tillbaka" || submit=="Avbryt")
            {
                ViewBag.title = "Competitions";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "comp";
                return View(db.competitions);
            }
            else if (submit == "Add")
            {
                ViewBag.title = "Add competition";
                ViewBag.action = "Competitions";
                ViewBag.controller = "Admin";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "comp";
                return View("CompetitionsAdd");
            }
            else if (submit == "Edit")
            {
                ViewBag.title = "Edit competition";
                ViewBag.action = "Competitions";
                ViewBag.controller = "Admin";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "comp";
                ViewBag.id = id;
                var editmodel = db.competitions.Where(r => r.CompetitionsID == id).ToList()[0];
                return View("CompetitionsEdit", editmodel);
            }
            else if (submit == "Delete")
            {
                ViewBag.title = "Delete competition";
                ViewBag.action = "Competitions";
                ViewBag.controller = "Admin";
                ViewBag.buttonvalue = "Avbryt";
                ViewBag.lastpage = "comp";
                ViewBag.id = id;
                var deletemodel = db.competitions.Where(r => r.CompetitionsID == id).ToList()[0];
                return View("CompetitionsDelete", deletemodel);
            }
            else if (submit == "Save Edits")
            {
                var searchedit = db.competitions.Where(r => r.CompetitionsID == id).ToList()[0];
                searchedit.CompText = CompText;
                try
                {
                    db.SaveChanges();
                }
                catch { }
                ViewBag.title = "Competitions";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "comp";
                return View(db.competitions);
            }
            else if (submit == "Save Delete")
            {
                var searchdelete = db.competitions.Where(r => r.CompetitionsID == id).ToList()[0];
                try
                {
                    db.competitions.Remove(searchdelete);
                    db.SaveChanges();
                }
                catch { }
                ViewBag.title = "Competitions";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "comp";
                return View(db.competitions);
            }
            else if (submit == "Save New")
            {
                string comptextstring = "";
                comptextstring += CompName;
                if (CompReq1 != "") { comptextstring += "# •" + CompReq1; }
                if (CompReq2 != "") { comptextstring += "# •" + CompReq2; }
                if (CompReq3 != "") { comptextstring += "# •" + CompReq3; }
                if (CompReq4 != "") { comptextstring += "# •" + CompReq4; }
                if (CompReq5 != "") { comptextstring += "# •" + CompReq5; }
                comptextstring += "#*";
                if (List1 != "") { comptextstring += "#" + List1; }
                if (List2 != "") { comptextstring += "#" + List2; }
                if (List3 != "") { comptextstring += "#" + List3; }
                if (List4 != "") { comptextstring += "#" + List4; }
                if (List5 != "") { comptextstring += "#" + List5; }
                competition newcomp = new competition()
                {
                    CompText = comptextstring,
                    CompetitionsID = CompName,
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now
                };
                try
                {
                    db.competitions.Add(newcomp);
                    db.SaveChanges();
                }
                catch { }
                ViewBag.title = "Competitions";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "comp";
                return View(db.competitions);
            }
            return View(db.competitions);
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
    }
}