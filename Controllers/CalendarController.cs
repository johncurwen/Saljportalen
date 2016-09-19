using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SakraStats.Controllers
{
    public class CalType
    {
        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
    public class CalendarController : Controller
    {
        public List<string> exceptions = new List<string>()
            {
                "Tekniker",
                "Chefstekniker",
                "Kontor",
                "TempTech"
            };
        public List<SelectListItem> abtypes = new List<SelectListItem>()
        {
            new SelectListItem{Text = "Ledig", Value = "LDG" },
            new SelectListItem{Text = "Semester", Value = "SEM" },
            new SelectListItem{Text = "Sjuk", Value = "SJK" },
            new SelectListItem{Text = "Vård av barn", Value = "VAB" },
            new SelectListItem{Text = "Föräldrarledig", Value = "FÖR" },
            new SelectListItem{Text = "Utbildning", Value = "UTB" },
            new SelectListItem{Text = "Gruppjobb", Value = "GRP" }
        };
        HomeContext db = new HomeContext();
        // GET: Calendar
        public ActionResult View(string[] selectlist, string manager, string submit, string employeeid, string accessid, string branchid, DateTime? date = null, 
            DateTime? EventStart = null, DateTime? EventEnd = null, string EventName ="", string EventType="", int? currmonth = null, int? curryear = null)
        {
            ViewBag.employeeid = employeeid;
            ViewBag.manager = manager;
            ViewBag.branchid = branchid;
            var namesearch = db.employees.Select(r=>new {FirstName = r.FirstName, LastName=r.LastName, EmployeeID=r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
            ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
            ViewBag.accessid = accessid;
            if (submit=="MIN KALENDER" || submit == "KALENDER" || submit == "OK" || submit == "Avbryt" || submit == "Tillbaka" || submit==">" || submit == "<" || submit == "GO")
            {
                if (submit==">")
                {
                    date = ((DateTime)date).AddMonths(1);
                }
                if (submit == "<")
                {
                    date = ((DateTime)date).AddMonths(-1);
                }
                if (submit == "GO")
                {
                    date = new DateTime((int)curryear, (int)currmonth,1).Date;
                }
                var curr = new List<int>();
                if (date == null)
                {
                    curr = BuildMonth();
                }
                else
                {
                    curr = BuildMonth(date);
                }

                List<user> users = new List<user>();

                if (manager=="yes")
                {
                    users = db.users.Where(r => r.employee.BranchID == branchid && !(exceptions.Any(a=> r.employee.Role==a))).ToList();
                }
                else
                {
                    users = db.users.Where(r => r.EmployeeID == employeeid && !(exceptions.Any(a => r.employee.Role == a))).ToList();
                }
                DateTime calstart = new DateTime(curr[2], curr[1], 1);
                DateTime calend = new DateTime(curr[2], curr[1], curr[3]);
                List<IEnumerable<CalType>> calall = new List<IEnumerable<CalType>>();

                foreach (var row in users)
                {
                    List<CalType> cal = new List<CalType>();

                    var searchcal = db.calendars.Where(r => r.EmployeeID == row.EmployeeID &&
                    (r.EventStart >= calstart & r.EventStart <= calend ||
                    r.EventEnd >= calstart & r.EventEnd <= calend)).ToList();

                    if (searchcal.Count==0)
                    {
                        string empname = row.employee.FirstName + " " + row.employee.LastName;
                        CalType calday = new CalType()
                        {
                            day = 99,
                            month = 99,
                            year = 9999,
                            type = "",
                            name = empname,
                            description = ""
                        };
                        cal.Add(calday);
                    }

                    foreach (var evnt in searchcal)
                    {
                        string empname = row.employee.FirstName + " " + row.employee.LastName;
                        for (DateTime i = evnt.EventStart; i <= evnt.EventEnd; i = i.AddDays(1))
                        {
                            if (i >= calstart && i <= calend)
                            {
                                string addname = "";
                                if (users.Count>1)
                                {
                                    addname = empname + "\r\n";
                                }
                                CalType calday = new CalType()
                                {
                                    day = i.Day,
                                    month = i.Month,
                                    year = i.Year,
                                    type = evnt.EventType,
                                    name = empname,
                                    description = evnt.EventStart.ToString("dd MMM yyyy") + " - " + evnt.EventEnd.ToString("dd MMM yyyy") + "\r\n" + addname  + evnt.EventName
                                };
                                cal.Add(calday);
                            }
                        }
                        cal = cal.OrderBy(r => r.day).ThenBy(r => r.month).ToList();
                    }
                    calall.Add(cal.AsEnumerable());
                }
                calall = calall.OrderBy(r => r.Min(a => a.name.Split(' ')[1])).ToList();
                ViewBag.month = Month(curr[1], curr[2]);
                ViewBag.intmonth = curr[1];
                ViewBag.intyear = curr[2];
                ViewBag.daysinmonth = curr[3];
                ViewBag.daysinlastmonth = curr[4];
                ViewBag.monday = curr[5];
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                if (manager=="yes")
                {
                    ViewBag.title = "Kalender";
                    ViewBag.lastpage = "manager";
                }
                else
                {
                    ViewBag.title = "Min kalender";
                    ViewBag.lastpage = "minkalender";
                }
                return View(calall);
            }
            else if (submit=="SPARA")
            {
                try
                {
                    if (EventStart > EventEnd)
                    {
                        ViewBag.manager = manager;
                        ViewBag.title = "Fel!";
                        ViewBag.message = "Startdatum kan inte vara efter slutdatum!";
                        ViewBag.action = "View";
                        ViewBag.controller = "Calendar";
                        ViewBag.buttonvalue = "Tillbaka";
                        return View("Changes");
                    }
                    else
                    {
                        var checkold = new List<calendar>();
                        if (manager == "yes")
                        {
                            string errors = "";
                            int errorcount = 0;
                            foreach (var employee in selectlist)
                            {
                                checkold = db.calendars.Where(r => r.EmployeeID == employee &&
                                (EventStart >= r.EventStart & EventStart <= r.EventEnd ||
                                EventEnd >= r.EventStart & EventEnd <= r.EventEnd)).ToList();

                                if (checkold.Count > 0)
                                {
                                    errors += checkold.Min(r => r.employee.FirstName) + " " + checkold.Min(r => r.employee.LastName) + ", ";
                                    errorcount++;
                                }
                                else
                                {
                                    calendar newcal = new calendar()
                                    {
                                        EventType = EventType,
                                        EventStart = (DateTime)EventStart,
                                        EventEnd = (DateTime)EventEnd,
                                        EventName = EventName,
                                        EmployeeID = employee
                                    };
                                    try
                                    {
                                        db.calendars.Add(newcal);
                                        db.SaveChanges();
                                    }
                                    catch
                                    {

                                    }

                                }
                            }
                            ViewBag.manager = manager;
                            if (errorcount != 0)
                            {
                                ViewBag.title = "Fel!";
                                ViewBag.message = "Kunde inte spara: " + errors.TrimEnd(new char[]{ ',',' '});
                                ViewBag.action = "View";
                                ViewBag.controller = "Calendar";
                                ViewBag.buttonvalue = "Tillbaka";
                                return View("Changes");
                            }
                            else
                            {
                                ViewBag.title = "Succé!";
                                ViewBag.message = "Dina ändringar har sparats!";
                                ViewBag.action = "View";
                                ViewBag.controller = "Calendar";
                                ViewBag.buttonvalue = "Tillbaka";
                                return View("Changes");
                            }
                        }
                        else
                        {
                            checkold = db.calendars.Where(r => r.EmployeeID == employeeid &&
                            (EventStart >= r.EventStart & EventStart <= r.EventEnd ||
                            EventEnd >= r.EventStart & EventEnd <= r.EventEnd)).ToList();
                        }
                        if (checkold.Count > 0)
                        {
                            ViewBag.manager = manager;
                            ViewBag.title = "Fel!";
                            ViewBag.message = "Du har redan lagt till en kalenderpost mellan " + ((DateTime)EventStart).ToString("d MMM") + " och " + ((DateTime)EventEnd).ToString("d MMM");
                            ViewBag.action = "View";
                            ViewBag.controller = "Calendar";
                            ViewBag.buttonvalue = "Tillbaka";
                            return View("Changes");
                        }
                        else
                        {
                            calendar newcal = new calendar()
                            {
                                EventType = EventType,
                                EventStart = (DateTime)EventStart,
                                EventEnd = (DateTime)EventEnd,
                                EventName = EventName,
                                EmployeeID = employeeid
                            };
                            db.calendars.Add(newcal);
                            db.SaveChanges();
                            ViewBag.manager = manager;
                            ViewBag.title = "Succé!";
                            ViewBag.message = "Dina ändringar har sparats!";
                            ViewBag.action = "View";
                            ViewBag.controller = "Calendar";
                            ViewBag.buttonvalue = "Tillbaka";
                            return View("Changes");
                        }
                    }
                }
                catch
                {
                    ViewBag.manager = manager;
                    ViewBag.title = "Fel!";
                    ViewBag.message = "Något gick snätt";
                    ViewBag.action = "View";
                    ViewBag.controller = "Calendar";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
            }
            //All else fails
            return View();
        }
        public ActionResult New(string manager, string submit, string employeeid, string accessid, string branchid)
        {
            ViewBag.manager = manager;
            ViewBag.title = "Ny kalenderpost";
            ViewBag.action = "View";
            ViewBag.controller = "Calendar";
            ViewBag.buttonvalue = "Avbryt";
            ViewBag.lastpage = "minkalender";
            ViewBag.typelist = abtypes;
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
            ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
            ViewBag.accessid = accessid;
            var selectlist = new List<SelectListItem>();
            if (manager == "yes")
            {
                var usersearch = db.users.Where(r => r.employee.BranchID == branchid && !(exceptions.Any(a => r.employee.Role == a))).ToList();
                usersearch = usersearch.OrderBy(r => r.employee.LastName).ToList();
                foreach (var userrow in usersearch)
                {
                    selectlist.Add(new SelectListItem { Text = userrow.employee.FirstName + " " + userrow.employee.LastName, Value = userrow.EmployeeID });
                }
                ViewBag.selectlist = selectlist;
            }
            if (submit == "LÄGG TILL" || submit == "OK")
            {
                return View(new calendar() {EventStart=DateTime.Now.Date, EventEnd=DateTime.Now.Date });
            }
            return View();
        }
        public ActionResult Delete(string manager, string selected, string submit, string employeeid, string accessid, string branchid, int? month, int? year, int? last)
        {
            ViewBag.manager = manager;
            ViewBag.title = "Ta bort kalenderpost";
            ViewBag.action = "View";
            ViewBag.controller = "Calendar";
            ViewBag.buttonvalue = "Avbryt";
            if (manager=="yes")
            {
                ViewBag.lastpage = "manager";
            }
            else
            {
                ViewBag.lastpage = "minkalender";
            }
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
            ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
            ViewBag.accessid = accessid;
            if (submit=="RADERA")
            {

                try
                {
                    int id = int.Parse(selected);
                    db.calendars.Remove(db.calendars.Where(r => r.EventID == id).ToList().First());
                    db.SaveChanges();
                    ViewBag.title = "Succé!";
                    ViewBag.message = "Dina ändringar har sparats!";
                    ViewBag.action = "View";
                    ViewBag.controller = "Calendar";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
                catch (Exception ex)
                {
                    ViewBag.title = "Fel!";
                    ViewBag.message = "Något gick snätt!";
                    ViewBag.action = "View";
                    ViewBag.controller = "Calendar";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
            }
            else
            {
                DateTime firstday = new DateTime((int)year, (int)month, 1).Date;
                DateTime lastday = new DateTime((int)year, (int)month, (int)last).Date;
                List<user> users = new List<user>();
                var selectlist = new List<SelectListItem>();

                if (manager == "yes")
                {
                    var delsearch = db.calendars.Where(r => r.employee.BranchID == branchid &&
                    (r.EventStart >= firstday & r.EventStart <= lastday ||
                    r.EventEnd >= firstday & r.EventEnd <= lastday)).ToList();
                    delsearch = delsearch.OrderBy(r => r.employee.LastName).ThenBy(r=> r.EventStart).ToList();
                    foreach (var delrow in delsearch)
                    {
                        string details = delrow.EventStart.ToString("d MMM yy") + " - " + delrow.EventEnd.ToString("d MMM yy") + " :: " + delrow.employee.FirstName + " " + delrow.employee.LastName + " :: " + delrow.EventName;
                        selectlist.Add(new SelectListItem { Text = details, Value = delrow.EventID.ToString() });
                    }
                    ViewBag.selected = selectlist;
                }
                else
                {
                    var delsearch = db.calendars.Where(r => r.EmployeeID == employeeid &&
                    (r.EventStart >= firstday & r.EventStart <= lastday ||
                    r.EventEnd >= firstday & r.EventEnd <= lastday)).ToList();
                    delsearch = delsearch.OrderBy(r => r.EventStart).ToList();
                    foreach (var delrow in delsearch)
                    {
                        string details = delrow.EventStart.ToString("d MMM yy") + " - " + delrow.EventEnd.ToString("d MMM yy") + " :: " + delrow.EventName;
                        selectlist.Add(new SelectListItem { Text = details, Value = delrow.EventID.ToString() });
                    }
                    ViewBag.selected = selectlist;
                }
                
                return View();
            }
        }
        public ActionResult Edit(string manager, string selected, string submit, string employeeid, string accessid, string branchid, int? month, int? year, int? last, DateTime? EventStart=null, DateTime? EventEnd = null, string EventType="", string EventName="")
        {
            ViewBag.typelist = abtypes;
            ViewBag.manager = manager;
            ViewBag.title = "Redigera kalenderpost";
            ViewBag.action = "View";
            ViewBag.controller = "Calendar";
            ViewBag.buttonvalue = "Avbryt";
            if (manager == "yes")
            {
                ViewBag.lastpage = "manager";
            }
            else
            {
                ViewBag.lastpage = "minkalender";
            }
            ViewBag.employeeid = employeeid;
            ViewBag.branchid = branchid;
            var namesearch = db.employees.Select(r => new { FirstName = r.FirstName, LastName = r.LastName, EmployeeID = r.EmployeeID }).Where(r => r.EmployeeID == employeeid).ToList()[0];
            ViewBag.name = namesearch.FirstName + " " + namesearch.LastName;
            ViewBag.accessid = accessid;
            if (submit == "SPARA")
            {
                try
                {
                    int id = int.Parse(selected);
                    var editrec = db.calendars.Where(r => r.EventID == id).ToList();
                    string employee = editrec[0].EmployeeID;

                    List <CalType> cal = new List<CalType>();

                    var searchcal = db.calendars.Where(r => r.EmployeeID == employee &&
                    (r.EventStart >= EventStart & r.EventStart <= EventEnd||
                    r.EventEnd >= EventStart & r.EventEnd <= EventEnd)).ToList();
                    
                    if (searchcal.Count==0 || searchcal.Count == 1 & searchcal[0].EventID==id)
                    {
                        if (searchcal.Count == 0)
                        {
                            searchcal = editrec;
                        }
                        searchcal[0].EventStart = (DateTime)EventStart;
                        searchcal[0].EventType = EventType;
                        searchcal[0].EventName = EventName;
                        searchcal[0].EventEnd = (DateTime)EventEnd;
                        db.SaveChanges();
                        ViewBag.title = "Succé!";
                        ViewBag.message = "Dina ändringar har sparats!";
                        ViewBag.action = "View";
                        ViewBag.controller = "Calendar";
                        ViewBag.buttonvalue = "Tillbaka";
                        return View("Changes");
                    }
                    else
                    {
                        ViewBag.title = "Fel!";
                        ViewBag.message = "Något gick snätt!";
                        ViewBag.action = "View";
                        ViewBag.controller = "Calendar";
                        ViewBag.buttonvalue = "Tillbaka";
                        return View("Changes");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.title = "Fel!";
                    ViewBag.message = "Något gick snätt!";
                    ViewBag.action = "View";
                    ViewBag.controller = "Calendar";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
            }
            else
            {
                ModelState.Clear();
                DateTime firstday = new DateTime((int)year, (int)month, 1).Date;
                DateTime lastday = new DateTime((int)year, (int)month, (int)last).Date;
                List<user> users = new List<user>();
                var selectlist = new List<SelectListItem>();

                if (manager == "yes")
                {
                    var delsearch = db.calendars.Where(r => r.employee.BranchID == branchid &&
                    (r.EventStart >= firstday & r.EventStart <= lastday ||
                    r.EventEnd >= firstday & r.EventEnd <= lastday)).ToList();
                    delsearch = delsearch.OrderBy(r => r.employee.LastName).ThenBy(r => r.EventStart).ToList();
                    foreach (var delrow in delsearch)
                    {
                        string details = delrow.EventStart.ToString("d MMM yy") + " - " + delrow.EventEnd.ToString("d MMM yy") + " :: " + delrow.employee.FirstName + " " + delrow.employee.LastName + " :: " + delrow.EventName;
                        selectlist.Add(new SelectListItem { Text = details, Value = delrow.EventID.ToString() });
                    }
                    ViewBag.calitems = delsearch;
                    ViewBag.selected = selectlist;
                    return View("Edit", delsearch[0]);
                }
                else
                {
                    var delsearch = db.calendars.Where(r => r.EmployeeID == employeeid &&
                    (r.EventStart >= firstday & r.EventStart <= lastday ||
                    r.EventEnd >= firstday & r.EventEnd <= lastday)).ToList();
                    delsearch = delsearch.OrderBy(r => r.EventStart).ToList();
                    foreach (var delrow in delsearch)
                    {
                        string details = delrow.EventStart.ToString("d MMM yy") + " - " + delrow.EventEnd.ToString("d MMM yy") + " :: " + delrow.EventName;
                        selectlist.Add(new SelectListItem { Text = details, Value = delrow.EventID.ToString() });
                    }
                    ViewBag.calitems = delsearch;
                    ViewBag.selected = selectlist;
                    try
                    {
                        return View("Edit", delsearch[0]);
                    }
                    catch
                    {
                        ViewBag.title = "Fel!";
                        ViewBag.message = "Finns inget att redigera!";
                        ViewBag.action = "View";
                        ViewBag.controller = "Calendar";
                        ViewBag.buttonvalue = "Tillbaka";
                        return View("Changes");
                    }
                }
            }
        }

        private List<int> BuildMonth(DateTime? date = null)
        {
            DateTime usedate = DateTime.Now.Date;
            if (date != null)
            {
                usedate = ((DateTime)date).Date;
            }
            int currentday = usedate.Day;
            int currentmonth = usedate.Month;
            int currentyear = usedate.Year;
            int currentmonthlastday = DateTime.DaysInMonth(currentyear, currentmonth);
            int lastmonthlastday = DateTime.DaysInMonth(usedate.AddMonths(-1).Year, usedate.AddMonths(-1).Month);
            DayOfWeek firstday = new DateTime(currentyear, currentmonth, 1).DayOfWeek;
            int firstmonday = 0;
            switch (firstday)
            {
                case DayOfWeek.Monday:
                    firstmonday = 1;
                    break;
                case DayOfWeek.Tuesday:
                    firstmonday = 7;
                    break;
                case DayOfWeek.Wednesday:
                    firstmonday = 6;
                    break;
                case DayOfWeek.Thursday:
                    firstmonday = 5;
                    break;
                case DayOfWeek.Friday:
                    firstmonday = 4;
                    break;
                case DayOfWeek.Saturday:
                    firstmonday = 3;
                    break;
                case DayOfWeek.Sunday:
                    firstmonday = 2;
                    break;
            }
            return new List<int> { currentday, currentmonth, currentyear, currentmonthlastday, lastmonthlastday, firstmonday };
        }
        private string Month(int month, int year)
        {
            string monthstr = "";
            switch (month)
            {
                case 1:
                    monthstr = "januari " + year;
                    break;
                case 2:
                    monthstr = "februari " + year;
                    break;
                case 3:
                    monthstr = "mars " + year;
                    break;
                case 4:
                    monthstr = "april " + year;
                    break;
                case 5:
                    monthstr = "maj " + year;
                    break;
                case 6:
                    monthstr = "juni " + year;
                    break;
                case 7:
                    monthstr = "juli " + year;
                    break;
                case 8:
                    monthstr = "augusti " + year;
                    break;
                case 9:
                    monthstr = "september " + year;
                    break;
                case 10:
                    monthstr = "oktober " + year;
                    break;
                case 11:
                    monthstr = "november " + year;
                    break;
                case 12:
                    monthstr = "december " + year;
                    break;
            }
            return monthstr;
        }
        [HttpPost]
        public ActionResult GetEdit(int selectedid)
        {
            var selectedcal = db.calendars.Where(r=>r.EventID== selectedid).ToList().First();
            List<dynamic> result = new List<dynamic>()
            {
                selectedcal.EventName,
                selectedcal.EventStart.ToString("yyyy-MM-dd"),
                selectedcal.EventEnd.ToString("yyyy-MM-dd"),
                selectedcal.EventType.Substring(0,3)
            };
            return Json (result);
        }
    }
}