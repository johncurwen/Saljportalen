using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SakraStats.Controllers
{
    [ValidateAntiForgeryToken]
    [NoCache]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class StoresController : Controller
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
        // GET: Stores
        public ActionResult Manage(string releasedto, string employeeid, string accessid, string branchid, string submit, List<bool> DeliverProduct, List<int> StatsID, 
            string type="search", string customer=null, List<string> ProductID = null, string AddProductID = null, int? turnover = null, int? service = null  )
        {
            ViewBag.employeeid = employeeid;
            ViewBag.name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            ViewBag.accessid = accessid;
            ViewBag.branchid = branchid;
            if (submit== "LÄGG TILL")
            {
                if (ProductID==null)
                {
                    ProductID = new List<string>();
                }
                ProductID.Add(AddProductID);
                List<product> selectedproducts = new List<product>();
                foreach (var item in ProductID)
                {
                    selectedproducts.Add(db.products.Where(r => r.ProductID == item).ToList().First());
                }
                ViewBag.selectedproducts = selectedproducts;
                ViewBag.title = "Nedmontering";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "techmain";
                ViewBag.type = type;
                var products = db.products.ToList();
                List<SelectListItem> selectlistproducts = new List<SelectListItem>();
                foreach (var item in products)
                {
                    var temp = new SelectListItem();
                    temp.Text = item.Name;
                    temp.Value = item.ProductID;
                    selectlistproducts.Add(temp);
                }
                ViewBag.selectlistproduct = selectlistproducts;
                //Get delivered stores details
                IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid, delivered: true, filter: customer);
                ModelState.Clear();
                return View("ViewStores_Delivered", storemodel.ToList());
            }
            else if (submit == "NY SERVICE")
            {
                ViewBag.title = "Ny service";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "techmain";
                return View("AddService");
            }
            else if (submit == "SPARA SERVICE")
            {
                if (turnover==null)
                {
                    turnover = 0;
                    try { int.Parse(turnover.ToString()); } catch { turnover = 0; }
                }
                if (service == null)
                {
                    service = 0;
                    try { int.Parse(service.ToString()); } catch { service = 0; }
                }
                try
                {
                    var newstat = new stat();
                    newstat.Contacts = 0;
                    newstat.Demos = 0;
                    newstat.Sales = 0;
                    newstat.Turnover = turnover;
                    newstat.Date = DateTime.Now.Date;
                    newstat.Service = service;
                    newstat.EmployeeID = employeeid;
                    db.stats.Add(newstat);
                    db.SaveChanges();

                    ViewBag.lastpage = "techmain";
                    ViewBag.title = "Succè!";
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
                catch
                {
                    ViewBag.lastpage = "techmain";
                    ViewBag.title = "Fel!";
                    ViewBag.message = "Något gick snätt!";
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    return View("Changes");
                }
            }
            //Back from editing or from main menu
            else if (submit=="NY INSTALLATION" || submit == "Tillbaka")
            {
                ViewBag.title = "Ny installation";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "techmain";
                //Get stores details for individual
                IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid);
                return View("ViewStores", storemodel.ToList());
            }
            else if (submit=="NEDMONTERING") //Move alarm from old installation
            {
                ViewBag.title = "Nedmontering";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "techmain";
                ViewBag.type = type;
                //Get delivered stores details
                IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid, delivered:true);
                return View("ViewStores_Delivered", storemodel.ToList());
            }
            else if (submit == "SÖK") //Move alarm from old installation
            {
                ViewBag.title = "Nedmontering";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "techmain";
                ViewBag.type = type;
                var products = db.products.ToList();
                List<SelectListItem> selectlistproducts = new List<SelectListItem>();
                foreach (var item in products)
                {
                    var temp = new SelectListItem();
                    temp.Text = item.Name;
                    temp.Value = item.ProductID;
                    selectlistproducts.Add(temp);
                }
                ViewBag.selectlistproduct = selectlistproducts;
                //Get delivered stores details
                IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid, delivered: true, filter:customer);
                return View("ViewStores_Delivered", storemodel.ToList());
            }
            //Refresh stores view
            else if (submit=="UPPDATERA LISTAN")
            {
                ViewBag.title = "Ny installation";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "techmain";
                //Get stores details for individual
                IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid);
                return View("ViewStores", storemodel.ToList());
            }
            else if (submit == "SPARA NEDMONTERING")
            {
                if (DeliverProduct == null || DeliverProduct.Count == 0)
                {
                    if (ProductID == null || ProductID.Count == 0)
                    {

                        ViewBag.title = "Installation/Service";
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        ViewBag.lastpage = "main";
                        return View("ManageTech");
                    }
                }
                if (ProductID == null || ProductID.Count == 0)
                {
                    for (int i = (DeliverProduct.Count - 1); i >= 0; i--)
                    {
                        if (DeliverProduct[i] == true)
                        {
                            DeliverProduct.RemoveAt(i + 1);
                        }
                    }
                    for (int i = DeliverProduct.Count - 1; i >= 0; i--)
                    {
                        if (DeliverProduct[i] == false)
                        {
                            DeliverProduct.RemoveAt(i);
                            StatsID.RemoveAt(i);
                        }
                    }
                }
                ModelState.Clear();
                if (type=="choose")
                {
                    try
                    {
                        if (DeliverProduct != null && DeliverProduct.Count != 0)
                        {
                            IEnumerable<SakraStats.carstat> model = (from m in db.carstats where StatsID.Any(id => id.Equals(m.StatsID)) select m);
                            try
                            {
                                foreach (var row in model)
                                {
                                    row.ReleasedDate = null;
                                    row.ReleasedTo = "";
                                    row.EmployeeID = employeeid;
                                    row.Approved = false;
                                    row.AssignedDate = DateTime.Now;
                                }
                                db.SaveChanges();
                            }
                            catch
                            {
                                ViewBag.title = "Installation/Service";
                                ViewBag.action = "Main";
                                ViewBag.controller = "Home";
                                ViewBag.buttonvalue = "Tillbaka";
                                ViewBag.lastpage = "main";
                                return View("ManageTech");
                            }
                        }
                        else
                        {
                            try
                            {
                                foreach (var product in ProductID)
                                {
                                    carstat row = new carstat()
                                    {
                                        ProductID = product,
                                        ReleasedDate = null,
                                        ReleasedTo = "",
                                        EmployeeID = employeeid,
                                        Approved = false,
                                        AssignedDate = DateTime.Now
                                    };
                                    db.carstats.Add(row);
                                }
                                db.SaveChanges();
                            }
                            catch
                            {
                                ViewBag.title = "Installation/Service";
                                ViewBag.action = "Main";
                                ViewBag.controller = "Home";
                                ViewBag.buttonvalue = "Tillbaka";
                                ViewBag.lastpage = "main";
                                return View("ManageTech");
                            }
                        }
                        ViewBag.employeeid = employeeid;
                        ViewBag.accessid = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].Role;
                        ViewBag.lastpage = "techmain";
                        ViewBag.title = "Succè!";
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        return View("Changes");
                    }
                    catch
                    {
                        ViewBag.title = "Installation/Service";
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        ViewBag.lastpage = "main";
                        return View("ManageTech");
                    }
                }
                else
                {
                    try
                    {
                        IEnumerable<SakraStats.carstat> model = (from m in db.carstats where StatsID.Any(id => id.Equals(m.StatsID)) select m);
                        if (model.Count() != 0)
                        {
                            ViewBag.title = "Leverera produkter";
                            ViewBag.action = "Manage";
                            ViewBag.controller = "Stores";
                            ViewBag.buttonvalue = "Avbryt";
                            return View("EditStores", model.ToList());
                        }
                        else
                        {
                            ViewBag.title = "Ny installation";
                            ViewBag.action = "Main";
                            ViewBag.controller = "Home";
                            ViewBag.buttonvalue = "Tillbaka";
                            ViewBag.lastpage = "techmain";
                            //Get stores details for individual
                            IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid);
                            return View("ViewStores", storemodel.ToList());
                        }
                    }
                    catch
                    {
                        ViewBag.title = "Ny installation";
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        ViewBag.lastpage = "techmain";
                        //Get stores details for individual
                        IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid);
                        return View("ViewStores", storemodel.ToList());
                    }
                }
            }
            //Deliver products to customer
            else if (submit == "FORTSÄTT")
            {
                if (DeliverProduct == null || DeliverProduct.Count == 0)
                {
                    if (ProductID == null || ProductID.Count == 0)
                    {

                        ViewBag.title = "Installation/Service";
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        ViewBag.lastpage = "main";
                        return View("ManageTech");
                    }
                }
                if (ProductID == null || ProductID.Count == 0)
                {
                    for (int i = (DeliverProduct.Count - 1); i >= 0; i--)
                    {
                        if (DeliverProduct[i] == true)
                        {
                            DeliverProduct.RemoveAt(i + 1);
                        }
                    }
                    for (int i = DeliverProduct.Count - 1; i >= 0; i--)
                    {
                        if (DeliverProduct[i] == false)
                        {
                            DeliverProduct.RemoveAt(i);
                            StatsID.RemoveAt(i);
                        }
                    }
                }
                ModelState.Clear();
                if (type=="choose")
                {
                    try
                    {
                        if (DeliverProduct != null && DeliverProduct.Count != 0)
                        {
                            IEnumerable<SakraStats.carstat> model = (from m in db.carstats where StatsID.Any(id => id.Equals(m.StatsID)) select m);
                            try
                            {
                                foreach (var row in model)
                                {
                                    row.ReleasedDate = null;
                                    row.ReleasedTo = "";
                                    row.EmployeeID = employeeid;
                                    row.Approved = false;
                                    row.AssignedDate = DateTime.Now;
                                }
                                db.SaveChanges();
                            }
                            catch
                            {
                                ViewBag.title = "Installation/Service";
                                ViewBag.action = "Main";
                                ViewBag.controller = "Home";
                                ViewBag.buttonvalue = "Tillbaka";
                                ViewBag.lastpage = "main";
                                return View("ManageTech");
                            }
                        }
                        else
                        {
                            try
                            {
                                foreach (var product in ProductID)
                                {
                                    carstat row = new carstat()
                                    {
                                        ProductID = product,
                                        ReleasedDate = null,
                                        ReleasedTo = "",
                                        EmployeeID = employeeid,
                                        Approved = false,
                                        AssignedDate = DateTime.Now
                                    };
                                    db.carstats.Add(row);
                                }
                                db.SaveChanges();
                            }
                            catch
                            {
                                ViewBag.title = "Installation/Service";
                                ViewBag.action = "Main";
                                ViewBag.controller = "Home";
                                ViewBag.buttonvalue = "Tillbaka";
                                ViewBag.lastpage = "main";
                                return View("ManageTech");
                            }
                        }
                        ViewBag.employeeid = employeeid;
                        ViewBag.accessid = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].Role;
                        ViewBag.lastpage = "techmain";
                        ViewBag.title = "Succè!";
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        return View("Changes");
                    }
                    catch
                    {
                        ViewBag.title = "Installation/Service";
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        ViewBag.lastpage = "main";
                        return View("ManageTech");
                    }
                }
                else
                {
                    try
                    {
                        IEnumerable<SakraStats.carstat> model = (from m in db.carstats where StatsID.Any(id => id.Equals(m.StatsID)) select m);
                        if (model.Count() != 0)
                        {
                            ViewBag.title = "Leverera produkter";
                            ViewBag.action = "Manage";
                            ViewBag.controller = "Stores";
                            ViewBag.buttonvalue = "Avbryt";
                            return View("EditStores", model.ToList());
                        }
                        else
                        {
                            ViewBag.title = "Ny installation";
                            ViewBag.action = "Main";
                            ViewBag.controller = "Home";
                            ViewBag.buttonvalue = "Tillbaka";
                            ViewBag.lastpage = "techmain";
                            //Get stores details for individual
                            IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid);
                            return View("ViewStores", storemodel.ToList());
                        }
                    }
                    catch
                    {
                        ViewBag.title = "Ny installation";
                        ViewBag.action = "Main";
                        ViewBag.controller = "Home";
                        ViewBag.buttonvalue = "Tillbaka";
                        ViewBag.lastpage = "techmain";
                        //Get stores details for individual
                        IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid);
                        return View("ViewStores", storemodel.ToList());
                    }
                }
            }
            //Cancel delivery
            else if (submit == "Avbryt")
            {
                ViewBag.title = "Ny installation";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "techmain";
                //Get stores details for individual
                IEnumerable<SakraStats.carstat> storemodel = GetStores(employeeid);
                ViewBag.accessid = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].Role;
                return View("ViewStores", storemodel.ToList());
            }
            //Confirm delivery
            else if (releasedto != "" && DeliverProduct != null)
            {
                var selcarstat = (from m in db.carstats
                                  where StatsID.Any(id => id.Equals(m.StatsID))
                                  select m).ToList();
                try
                {
                    foreach (var row in selcarstat)
                    {
                        row.ReleasedDate = DateTime.Now;
                        row.ReleasedTo = releasedto;
                    }
                    var newstat = new stat();
                    newstat.Contacts = 0;
                    newstat.Demos = 0;
                    newstat.Sales = 0;
                    newstat.Turnover = turnover;
                    newstat.Date = DateTime.Now.Date;
                    newstat.Service = service;
                    newstat.EmployeeID = employeeid;
                    db.stats.Add(newstat);
                    db.SaveChanges();
                }
                catch
                {
                    ViewBag.title = "Installation/Service";
                    ViewBag.action = "Main";
                    ViewBag.controller = "Home";
                    ViewBag.buttonvalue = "Tillbaka";
                    ViewBag.lastpage = "main";
                    //Get stores details for individual
                    IEnumerable<SakraStats.carstat> storemodel2 = GetStores(employeeid);
                    ViewBag.accessid = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].Role;
                    return View("ViewStores", storemodel2.ToList());
                }
                ViewBag.employeeid = employeeid;
                ViewBag.accessid = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].Role;
                ViewBag.lastpage = "techmain";
                ViewBag.title = "Succè!";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                return View("Changes");
            }
            //Error
            else
            {
                ViewBag.title = "Installation/Service";
                ViewBag.action = "Main";
                ViewBag.controller = "Home";
                ViewBag.buttonvalue = "Tillbaka";
                ViewBag.lastpage = "main";
                //Get stores details for individual
                IEnumerable<SakraStats.carstat> storemodel2 = GetStores(employeeid);
                ViewBag.accessid = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].Role;
                return View("ViewStores", storemodel2.ToList());
            }
        }
//View stores
        private IEnumerable<SakraStats.carstat> GetStores(string employeeid, bool delivered = false, string filter = null)
        {
            if (delivered)
            {
                if (filter!=null)
                {
                    IEnumerable<SakraStats.carstat> model = db.carstats.Where(r => r.Approved == true && r.ReleasedTo==filter).ToList().OrderByDescending(r => r.ReleasedDate).ThenBy(r => r.EmployeeID).ThenBy(r => r.ReleasedTo).ThenBy(r => r.product.Name);
                    return model.AsEnumerable();
                }
                else
                {
                    IEnumerable<SakraStats.carstat> model = db.carstats.Where(r => r.Approved == true).ToList().OrderByDescending(r => r.ReleasedDate).ThenBy(r => r.EmployeeID).ThenBy(r => r.ReleasedTo).ThenBy(r => r.product.Name);
                    return model.AsEnumerable();
                }
            }
            else
            {
                IEnumerable<SakraStats.carstat> model = db.carstats.Where(r => r.EmployeeID == employeeid && r.ReleasedTo == "" && r.Approved == false).ToList().OrderBy(r => r.ProductID);
                return model.AsEnumerable();
            }
        }
    }
}