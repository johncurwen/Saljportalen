using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace SakraStats.Controllers
{
    [NoCache]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class ChartController : Controller
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
        // GET: Chart
        [ValidateAntiForgeryToken]
        public ActionResult CallChart()
        {
            return View();
        }
        public ActionResult MakeChart(string employeeid, string weekstart = "1", string weekend = "52", string yearstart = "2010", string yearend = "2010")
        {
            IEnumerable<SakraStats.stat> statmodelgraph = GetStats(employeeid);
            if (weekstart!="1" & weekend != "52" & yearstart != "2010" & yearend != "2010")
            {
                DateTime datestart = FirstDateOfWeekISO8601(int.Parse(weekstart),int.Parse(yearstart));
                DateTime dateend = FirstDateOfWeekISO8601(int.Parse(weekend), int.Parse(yearend)).AddDays(6.9999999);
                statmodelgraph = statmodelgraph.Where(r=>r.Date>=datestart && r.Date <= dateend).ToList().AsEnumerable();
            }
            var budgetmodel = db.budgets.Where(r => r.EmployeeID == employeeid);

            //Group stats by date
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
            var moldelsales = statmodelgraph
                .OrderByDescending(a => a.Date)
                .GroupBy(r => GetWeekOfYear(r.Date)[0])
                .Select(a => a.Sum(b => b.Sales))
                .ToList();
            var moldeldemos = statmodelgraph
                .OrderByDescending(a => a.Date)
                .GroupBy(r => GetWeekOfYear(r.Date)[0])
                .Select(a => a.Sum(b => b.Demos))
                .ToList();
            var moldelcontacts = statmodelgraph
                .OrderByDescending(a => a.Date)
                .GroupBy(r => GetWeekOfYear(r.Date)[0])
                .Select(a => a.Sum(b => b.Contacts))
                .ToList();
            var moldelturnover = statmodelgraph
                .OrderByDescending(a => a.Date)
                .GroupBy(r => GetWeekOfYear(r.Date)[0])
                .Select(a => a.Sum(b => b.Turnover))
                .ToList();
            List<int?> moldelbudget = new List<int?>();
            for (int i = 0; i < moldeldate.Count; i++)
            {
                int budget = 0;
                int tempyear = moldelyear[i];
                int tempdate = moldeldate[i];
                var checkbudget = budgetmodel.Where(r => r.Week == tempdate && r.BudgetStart.Year == tempyear).ToList();
                if (checkbudget.Count > 0)
                {
                    try
                    {
                        budget = checkbudget[0].Budget1;
                    }
                    catch { }
                }
                moldelbudget.Add(budget);
            }
            List<dynamic> listedmodel = new List<dynamic>();

            //Fill in any gaps in reporting
            try
            {
                int lastweek = GetWeekOfYear(new DateTime(moldelyear[0], 12, 31))[0];
                int difference = 0;
                int lastyear = moldelyear[0];
                int firstyear = moldelyear[moldelyear.Count - 1];

                //Work out difference in weeks if stats spans years
                if (moldelyear[0]!= firstyear)
                {
                    for (int i = firstyear; i<= lastyear;i++)
                    {
                        if (i == firstyear)
                        {
                            difference = difference + (
                                GetWeekOfYear(new DateTime(i, 12, 31))[0] - moldeldate[moldeldate.Count-1]
                                );
                        }
                        else if(i!=lastyear)
                        {
                            difference = difference + GetWeekOfYear(new DateTime(i, 12, 31))[0];
                        }
                        else
                        {
                            difference = difference + moldeldate[0];
                        }
                    }
                }
                else
                {
                    difference = moldeldate[0] - moldeldate[moldeldate.Count - 1];
                }
                int currentweek = moldeldate[0];
                int currentyear = moldelyear[0];
                for (int i = 0; i < difference; i++)
                {
                    var weeksearch = moldeldate.IndexOf(currentweek);
                    if (weeksearch == -1)
                    {
                        moldeldate.Insert(i, currentweek);
                        moldelyear.Insert(i, currentyear);
                        moldelsales.Insert(i, 0);
                        moldeldemos.Insert(i, 0);
                        moldelcontacts.Insert(i, 0);
                        moldelturnover.Insert(i, 0);
                        moldelbudget.Insert(i, 0);
                    }
                    currentweek--;
                    if (currentweek == 0) { currentyear--; currentweek = GetWeekOfYear(new DateTime(currentyear, 12, 31))[0]; }
                }
            }
            catch { }

            List<string> moldelweek = new List<string>();
            for (int i=0; i< moldeldate.Count;i++)
            {
                moldelweek.Add(moldeldate[i].ToString() + " (" + moldelyear[i].ToString() + ")");
            }
            
            listedmodel.Add(moldelweek);
            listedmodel.Add(moldelsales);
            listedmodel.Add(moldeldemos);
            listedmodel.Add(moldelcontacts);
            listedmodel.Add(moldelturnover);
            listedmodel.Add(moldelbudget);

            ViewBag.employeeid = employeeid;
            ViewBag.accessid = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].Role;
            ModelState.Clear();

            return View("MakeChart", listedmodel);
        }
//Funtions
        private IEnumerable<SakraStats.stat> GetStats(string employeeid)
        {
            var model = db.stats.Where(r => r.EmployeeID == employeeid && r.Sales > -1).ToList().OrderByDescending(r => r.Date);
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
            DateTime endofyearweeks = new DateTime(dateproper.Year,12,31);
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
            if (dateproper> endofyearweeks)
            {
                week[0] = 1;
                week[1] = dateproper.Year + 1;
            }
            else if (dayofyear + extradays<1)
            {
                week=GetWeekOfYear(new DateTime(((DateTime)date).Year,12,31));
                week[0]++;
                week[1] = dateproper.Year-1;
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

    }
}