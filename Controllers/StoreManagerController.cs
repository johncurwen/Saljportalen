using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SakraStats.Controllers
{
    public class UniqueBranchList
    {
        public string BranchID { get; set; }
    }
    public class StorestatDropDown
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public int? Qty { get; set; }
    }
    public class CollatedBranch
    {
        public int StatsID { get; set; }
        public string BranchID { get; set; }
        public string StoreID { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public int ProductQty { get; set; }
        public decimal ProductValue { get; set; }
    }
    public class CollatedTech
    {
        public int? StatsID { get; set; }
        public string BranchID { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public int ProductQty { get; set; }
        public decimal ProductValue { get; set; }
        public bool? Approved { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReleaseDate { get; set; }
        public string Customer { get; set; }
    }
    [ValidateAntiForgeryToken]
    [NoCache]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class StoreManagerController : Controller
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
        [HttpPost]
        public ActionResult GetStores(string selectedbranchid)
        {
            List<SelectListItem> selectliststores = new List<SelectListItem>();
            var stores = db.stores.Where(r => r.BranchID == selectedbranchid).ToList();
            foreach (var store in stores)
            {
                selectliststores.Add(new SelectListItem { Text = store.StoreID, Value = store.StoreID});
            }
            return Json(selectliststores);
        }
        [HttpPost]
        public ActionResult Get(string storeid, string branchid, string accessid)
        {
            List<StorestatDropDown> PRODselectitems = new List<StorestatDropDown>();

            if (storeid==null || storeid=="")
            {
                var storestats = db.products.ToList();
                foreach (var stat in storestats)
                {
                    PRODselectitems.Add(new StorestatDropDown { Text = stat.Name, Value = "PROD**" + stat.ProductID, Qty = null });
                }
            }
            else
            {
                List<storestat> storestats = new List<storestat>();
                if (accessid=="Chefstekniker" || accessid=="Administrator")
                {
                    storestats = db.storestats.Where(r => r.StoreID == storeid).ToList();
                }
                else
                {
                    storestats = db.storestats.Where(r => r.StoreID == storeid && r.store.BranchID==branchid).ToList();
                }
                foreach (var stat in storestats)
                {
                    PRODselectitems.Add(new StorestatDropDown { Text = stat.product.Name, Value = stat.StatsID.ToString(), Qty = stat.ProductQty });
                }
            }

            return Json(PRODselectitems);
        }
        [HttpPost]
        public ActionResult GetMax(string statsid)
        {
            int intid = int.Parse(statsid);
            var storestats = db.storestats.Where(r => r.StatsID == intid).ToList();

            List<StorestatDropDown> PRODselectitems = new List<StorestatDropDown>();

            foreach (var stat in storestats)
            {
                PRODselectitems.Add(new StorestatDropDown { Text = stat.product.Name, Value = stat.StatsID.ToString(), Qty = stat.ProductQty });
            }

            return Json(PRODselectitems);
        }
        public ActionResult Home(string employeeid, string accessid, string branchid)
        {
            string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            SetViewBag("Chefstekniker", "Main", "Home", null, "Tillbaka", employeeid, accessid, branchid, name);
            return View("ManageStoreManager");
        }
        public ActionResult BranchStores(string employeeid, string accessid, string branchid)
        {
            var branchsearch = CollateBranch(employeeid,accessid,branchid);
            string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
            SetViewBag("Filial lager", "Home", "StoreManager",null, "Tillbaka", employeeid, accessid, branchid, name);
            return View(branchsearch);
        }
        public ActionResult TechStores(string submit, string employeeid, string accessid, string branchid, List<int> StatsID, List<bool> Approved, string type = "tekstore")
        {
            if (submit=="TEKNIKER LAGER")
            {
                var techsearch = CollateTech(employeeid, accessid, branchid);
                string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                SetViewBag("Tekniker lager", "Home", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                return View(techsearch);
            }
            else if (submit == "ATT GODKÄNNA")
            {
                var techsearch = CollateTech(employeeid, accessid, branchid, current:false,unapproved:true);
                string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                SetViewBag("Att godkänna", "Home", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                return View("TechStores_Pending", techsearch);
            }
            else if(submit == "LEVERERAD")
            {
                var techsearch = CollateTech(employeeid, accessid, branchid, current: false, delivered: true);
                string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                SetViewBag("Levererad", "Home", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                return View("TechStores_Delivered",techsearch);
            }
            else if (submit=="SPARA" && Approved!=null && Approved.Count>0)
            {

                try
                {
                    for (int i = (Approved.Count - 1); i >= 0; i--)
                    {
                        if (Approved[i] == true)
                        {
                            Approved.RemoveAt(i + 1);
                        }
                    }
                    for (int i = Approved.Count - 1; i >= 0; i--)
                    {
                        if (Approved[i] == false)
                        {
                            Approved.RemoveAt(i);
                            StatsID.RemoveAt(i);
                        }
                    }
                }
                catch { }
                ModelState.Clear();
                try
                {
                    IEnumerable<SakraStats.carstat> model = (from m in db.carstats where StatsID.Any(id => id.Equals(m.StatsID)) select m);
                    if (model.Count() != 0)
                    {
                        try
                        {
                            foreach (var row in model)
                            {
                                row.Approved = true;
                            }
                            db.SaveChanges();
                        }
                        catch { }
                    }
                }catch { }
                var techsearch = CollateTech(employeeid, accessid, branchid, current: false, unapproved: true);
                string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                SetViewBag("Att godkänna", "Home", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                return View("TechStores_Pending", techsearch);
            }
            else
            {
                if (type == "delivered")
                {
                    var techsearch = CollateTech(employeeid, accessid, branchid, current: false, delivered: true);
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Levererad", "Home", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    return View("TechStores_Delivered", techsearch);
                }
                else
                {
                    var techsearch = CollateTech(employeeid, accessid, branchid);
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Tekniker lager", "Home", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    return View(techsearch);
                }
            }
        }
        public ActionResult EditBranch(string submit, string employeeid, string accessid, string branchid, int statsid = 0, int productqtyedit = 1, 
            string productbranch="", string storeid="", string StoreID = "", string productid ="", int productqty =0)
        {
            if (submit == "OK")
            {
                var editstorestat = db.storestats.Where(r => r.StatsID == statsid).ToList().First();
                db.storestats.Remove(editstorestat);
                db.SaveChanges();
                ViewBag.message = "Dina ändringar har sparats!";
                string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                SetViewBag("Succè!", "BranchStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                return View("EditBranch", null);
            }
            else if (submit == "X")
            {
                ViewBag.message = "Vill du verkligen ta bort produkten?";
                ViewBag.statsid = statsid;
                ViewBag.confirm = true;
                string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                SetViewBag("Ta bort produkt", "BranchStores", "StoreManager", null, "Avbryt", employeeid, accessid, branchid, name);
                return View("EditBranch", null);
            }
            else if (submit == "SPARA")
            {
                try
                {
                    var existingstorestat = db.storestats.Where(r => r.StoreID == storeid && r.ProductID == productid).ToList();
                    if (existingstorestat.Count == 0)
                    {
                        var newrow = new storestat()
                        {
                            StoreID = storeid,
                            ProductID = productid,
                            ProductQty = productqty
                        };
                        db.storestats.Add(newrow);
                    }
                    else
                    {
                        existingstorestat[0].ProductQty += productqty;
                    }
                    db.SaveChanges();
                    ViewBag.message = "Dina ändringar har sparats!";
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Succè!", "BranchStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    return View("EditBranch", null);
                }
                catch
                {
                    ViewBag.message = "Något gick snätt!";
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Fel!", "BranchStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    return View("EditBranch", null);
                }
            }
            else if(submit=="Lägg till produkt")
            {
                List<UniqueBranchList> branches = new List<UniqueBranchList>();
                if (accessid == "Chefstekniker" || accessid == "Administrator")
                {
                     branches = db.stores.GroupBy(r=>r.BranchID).Select(r=> new UniqueBranchList { BranchID =r.Min(a=>a.BranchID)}).OrderBy(r => r.BranchID).ToList();
                }
                else
                {
                    branches = db.stores.Where(r => r.BranchID == branchid).GroupBy(r => r.BranchID).Select(r => new UniqueBranchList { BranchID = r.Min(a => a.BranchID)}).OrderBy(r => r.BranchID).ToList();
                }
                string firstbranch = branches[0].BranchID;
                var stores = db.stores.Where(r=>r.BranchID==firstbranch).OrderBy(r => r.StoreID).ToList();
                var products = db.products.OrderBy(r=>r.Name).ToList();
                List<SelectListItem> selectlistbranches = new List<SelectListItem>();
                List<SelectListItem> selectliststores = new List<SelectListItem>();
                List<SelectListItem> selectlistproducts = new List<SelectListItem>();
                foreach (var item in branches)
                {
                    var temp = new SelectListItem();
                    temp.Text = item.BranchID;
                    temp.Value = item.BranchID;
                    selectlistbranches.Add(temp);
                }
                ViewBag.selectlistbranch = selectlistbranches;
                foreach (var item in products)
                {
                    var temp = new SelectListItem();
                    temp.Text = item.Name;
                    temp.Value = item.ProductID;
                    selectlistproducts.Add(temp);
                }
                ViewBag.selectlistproduct = selectlistproducts;
                foreach (var item in stores)
                {
                    var temp = new SelectListItem();
                    temp.Text = item.StoreID;
                    temp.Value = item.StoreID;
                    selectliststores.Add(temp);
                }
                ViewBag.selectliststore = selectliststores;
                string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                SetViewBag("Lägg till produkt", "BranchStores", "StoreManager", null, "Avbryt", employeeid, accessid, branchid, name);
                return View(new storestat());
            }
            else
            {
                try
                {
                    ViewBag.message = "Dina ändringar har sparats!";
                    var editstorestat = db.storestats.Where(r => r.StatsID == statsid).ToList().First();
                    editstorestat.ProductQty += productqtyedit;
                    db.SaveChanges();
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Succè!", "BranchStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    return View("EditBranch", null);
                }
                catch
                {
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Fel!", "BranchStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    ViewBag.message = "Något gick snätt!";
                    return View("EditBranch",null);
                }
            }
            
        }
        public ActionResult EditTech(string submit, string employeeid, string accessid, string branchid, int statsid = 0, bool? usestore = null, 
            string StoreID=null, string ProductID=null, string TechID=null,int? ProductQty=null, string from = "move", string type="tekstore")
        {
            if (submit == "OK")
            {
                var findstat = new List<carstat>();
                if (type=="delivered")
                {
                    findstat = db.carstats.Where(r => r.StatsID == statsid).ToList();
                }
                else
                {
                    findstat = db.carstats.Where(r => r.ProductID == ProductID && r.EmployeeID == TechID).ToList();
                }
                foreach (var stat in findstat)
                {
                    db.carstats.Remove(stat);
                }
                db.SaveChanges();
                ViewBag.message = "Dina ändringar har sparats!";
                ViewBag.type = type;
                string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                SetViewBag("Succè!", "TechStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                return View("EditTech", null);
            }
            else if (submit == "X")
            {
                ViewBag.message = "Vill du verkligen ta bort produkten?";
                ViewBag.TechID = TechID;
                ViewBag.ProductID = ProductID;
                ViewBag.statsid = statsid;
                ViewBag.type = type;
                ViewBag.confirm = true;
                string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                SetViewBag("Ta bort produkt", "TechStores", "StoreManager", null, "Avbryt", employeeid, accessid, branchid, name);
                return View("EditTech", null);
            }
            else if (submit == "SPARA")
            {
                try
                {
                    if ((bool)usestore)
                    {
                        statsid = int.Parse(ProductID);
                        try
                        {
                            var findstorestat = db.storestats.Where(r => r.StatsID == statsid).ToList()[0];
                            findstorestat.ProductQty = findstorestat.ProductQty - (int)ProductQty;
                        }
                        catch { }
                        ProductID = db.storestats.Where(r => r.StatsID == statsid).ToList()[0].ProductID;
                    }
                    else
                    {
                        ProductID = ProductID.Substring(6, ProductID.Length - 6);
                    }
                    for (int i = 0; i < ProductQty; i++)
                    {
                        var newrow = new carstat()
                        {
                            EmployeeID = TechID,
                            ProductID = ProductID,
                            AssignedDate = DateTime.Now,
                            ReleasedTo = ""
                        };
                        db.carstats.Add(newrow);
                    }
                    db.SaveChanges();
                    ViewBag.message = "Dina ändringar har sparats!";
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Succè!", "TechStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    return View("EditTech", null);
                }
                catch
                {
                    ViewBag.message = "Något gick snätt!";
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Fel!", "TechStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    return View("EditTech", null);
                }
            }
            else if (submit == "Dela ut produkt")
            {
                var storestats = db.products.ToList();
                var stores = db.stores.ToList();
                List<employee> sellers = new List<employee>(); ;

                if (accessid == "Chefstekniker" || accessid == "Administrator")
                {
                    sellers = db.employees.Where(r=>r.Role=="Tekniker" || r.Role=="Chefstekniker").ToList();
                }
                else
                {
                    sellers = db.employees.Where(r=>r.BranchID==branchid && r.Role == "Tekniker" | r.Role == "Chefstekniker").ToList();
                }

                List<SelectListItem> SELLselectitems = new List<SelectListItem>();
                List<StorestatDropDown> PRODselectitems = new List<StorestatDropDown>();
                List<StorestatDropDown> STORselectitems = new List<StorestatDropDown>();

                foreach (var seller in sellers)
                {
                    SELLselectitems.Add(new SelectListItem { Text = seller.FirstName + " " + seller.LastName, Value = seller.EmployeeID});
                }
                foreach (var stat in storestats)
                {
                    PRODselectitems.Add(new StorestatDropDown { Text = stat.Name, Value = "PROD**" + stat.ProductID, Qty = null });
                }

                foreach (var store in stores)
                {
                    STORselectitems.Add(new StorestatDropDown { Text = store.StoreID + "(" + store.BranchID + ")", Value = store.StoreID });
                }

                ViewBag.SELLselectlist = SELLselectitems;
                ViewBag.PRODselectlist = PRODselectitems;
                ViewBag.STORselectlist = STORselectitems;

                string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                SetViewBag("Dela ut produkt", "TechStores", "StoreManager", null, "Avbryt", employeeid, accessid, branchid, name);

                return View(new SakraStats.storestat());
            }
            else if(submit=="GO" && from == "edit")
            {
                try
                {
                    ViewBag.message = "Dina ändringar har sparats!";
                    if (ProductQty<0)
                    {
                        var findstat = db.carstats.Where(r => r.ProductID == ProductID && r.EmployeeID == TechID && r.Approved==false).ToList();
                        int i = (int)ProductQty;
                        foreach (var stat in findstat)
                        {
                            db.carstats.Remove(stat);
                            i++;
                            if (i==0)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < ProductQty; i++)
                        {
                            var newrow = new carstat()
                            {
                                EmployeeID = TechID,
                                ProductID = ProductID,
                                AssignedDate = DateTime.Now,
                                ReleasedTo = "",
                                Approved=false
                            };
                            db.carstats.Add(newrow);
                        }
                    }
                    
                    db.SaveChanges();
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Succè!", "TechStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    return View("EditTech", null);
                }
                catch
                {
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Fel!", "TechStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    ViewBag.message = "Något gick snätt!";
                    return View("EditTech", null);
                }
            }
            else if(submit=="GO" && from=="move")
            {
                if (ProductQty !=null && ProductQty > 0)
                {
                    List<SelectListItem> selectliststores = new List<SelectListItem>();
                    List<SelectListItem> selectlistbranches = new List<SelectListItem>();
                    List<UniqueBranchList> branches = new List<UniqueBranchList>();
                    if (accessid == "Chefstekniker" || accessid == "Administrator")
                    {
                        branches = db.stores.GroupBy(r => r.BranchID).Select(r => new UniqueBranchList { BranchID = r.Min(a => a.BranchID) }).ToList();
                    }
                    else
                    {
                        branches = db.stores.Where(r => r.BranchID == branchid).GroupBy(r => r.BranchID).Select(r => new UniqueBranchList { BranchID = r.Min(a => a.BranchID) }).ToList();
                    }
                    string firstbranch = branches[0].BranchID;
                    var stores = db.stores.Where(r => r.BranchID == firstbranch).ToList();
                    foreach (var item in branches)
                    {
                        var temp = new SelectListItem();
                        temp.Text = item.BranchID;
                        temp.Value = item.BranchID;
                        selectlistbranches.Add(temp);
                    }
                    ViewBag.selectlistbranch = selectlistbranches;
                    foreach (var item in stores)
                    {
                        var temp = new SelectListItem();
                        temp.Text = item.StoreID;
                        temp.Value = item.StoreID;
                        selectliststores.Add(temp);
                    }
                    ViewBag.type = type;
                    ViewBag.statsid = statsid;
                    ViewBag.selectliststore = selectliststores;
                    ViewBag.TechID = TechID;
                    ViewBag.ProductID = ProductID;
                    ViewBag.ProductQty = ProductQty;
                    ViewBag.tostore = true;
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Flytta produkt till lager", "TechStores", "StoreManager", null, "Avbryt", employeeid, accessid, branchid, name);
                    return View("EditTech", null);
                }
                else if (type=="delivered")
                {
                    List<SelectListItem> selectliststores = new List<SelectListItem>();
                    List<SelectListItem> selectlistbranches = new List<SelectListItem>();
                    List<UniqueBranchList> branches = new List<UniqueBranchList>();
                    if (accessid == "Chefstekniker" || accessid == "Administrator")
                    {
                        branches = db.stores.GroupBy(r => r.BranchID).Select(r => new UniqueBranchList { BranchID = r.Min(a => a.BranchID) }).ToList();
                    }
                    else
                    {
                        branches = db.stores.Where(r => r.BranchID == branchid).GroupBy(r => r.BranchID).Select(r => new UniqueBranchList { BranchID = r.Min(a => a.BranchID) }).ToList();
                    }
                    string firstbranch = branches[0].BranchID;
                    var stores = db.stores.Where(r => r.BranchID == firstbranch).ToList();
                    foreach (var item in branches)
                    {
                        var temp = new SelectListItem();
                        temp.Text = item.BranchID;
                        temp.Value = item.BranchID;
                        selectlistbranches.Add(temp);
                    }
                    ViewBag.selectlistbranch = selectlistbranches;
                    foreach (var item in stores)
                    {
                        var temp = new SelectListItem();
                        temp.Text = item.StoreID;
                        temp.Value = item.StoreID;
                        selectliststores.Add(temp);
                    }
                    ViewBag.type = type;
                    ViewBag.statsid = statsid;
                    ViewBag.selectliststore = selectliststores;
                    ViewBag.TechID = TechID;
                    ViewBag.ProductID = ProductID;
                    ViewBag.ProductQty = ProductQty;
                    ViewBag.tostore = true;
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Flytta produkt till lager", "TechStores", "StoreManager", null, "Avbryt", employeeid, accessid, branchid, name);
                    return View("EditTech", null);
                }
                else
                {
                    string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                    SetViewBag("Fel!", "TechStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                    ViewBag.message = "Något gick snätt!";
                    return View("EditTech", null);
                }
            }
            else //If submit=="FLYTTA"
            {
                if (type=="delivered")
                {
                    try
                    {
                        var product = db.carstats.Where(r => r.StatsID==statsid).ToList()[0];
                        var instore = db.storestats.Where(r => r.StoreID == StoreID && r.ProductID == product.ProductID).ToList();
                        if (instore.Count == 0)
                        {
                            db.storestats.Add(new storestat() { StoreID = StoreID, ProductID = product.ProductID, ProductQty = 1 });
                        }
                        else
                        {
                            instore[0].ProductQty += 1;
                        }
                        db.carstats.Remove(product);
                        db.SaveChanges();
                        string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                        SetViewBag("Succè!", "TechStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                        ViewBag.type = type;
                        ViewBag.message = "Dina ändringar har sparats!";
                        return View("EditTech", null);
                    }
                    catch
                    {
                        string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                        SetViewBag("Fel!", "TechStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                        ViewBag.type = type;
                        ViewBag.message = "Något gick snätt!";
                        return View("EditTech", null);
                    }
                }
                else
                {
                    try
                    {
                        var products = db.carstats.Where(r => r.ProductID == ProductID && r.EmployeeID == TechID && r.Approved == false).ToList();
                        var instore = db.storestats.Where(r => r.StoreID == StoreID && r.ProductID == ProductID).ToList();
                        if (instore.Count == 0)
                        {
                            db.storestats.Add(new storestat() { StoreID = StoreID, ProductID = ProductID, ProductQty = (int)ProductQty });
                        }
                        else
                        {
                            instore[0].ProductQty += (int)ProductQty;
                        }
                        int i = -((int)ProductQty);
                        foreach (var item in products)
                        {
                            db.carstats.Remove(item);
                            i++;
                            if (i == 0)
                            {
                                break;
                            }
                        }
                        db.SaveChanges();
                        string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                        SetViewBag("Succè!", "TechStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                        ViewBag.message = "Dina ändringar har sparats!";
                        return View("EditTech", null);
                    }
                    catch
                    {
                        string name = db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].FirstName + " " + db.employees.Where(r => r.EmployeeID == employeeid).ToList()[0].LastName;
                        SetViewBag("Fel!", "TechStores", "StoreManager", null, "Tillbaka", employeeid, accessid, branchid, name);
                        ViewBag.message = "Något gick snätt!";
                        return View("EditTech", null);
                    }
                }
                
            }
        }
        private List<CollatedBranch> CollateBranch(string employeeid, string accessid, string branchid)
        {
            List<CollatedBranch> branchstats = new List<CollatedBranch>();
            if (accessid=="Chefstekniker" || accessid == "Administrator")
            {
                branchstats = db.storestats
                    .Select(a=> new CollatedBranch
                    {
                        StatsID=a.StatsID,
                        BranchID = a.store.BranchID,
                        StoreID=a.StoreID,
                        ProductID=a.ProductID,
                        ProductName=a.product.Name,
                        ProductQty=a.ProductQty,
                        ProductValue= (a.ProductQty)*((decimal)a.product.Price)
                    })
                    .OrderBy(r => r.BranchID).ThenBy(r=>r.StoreID).ThenBy(r=>r.ProductName)
                    .ToList();
            }
            else
            {
                branchstats = db.storestats.Where(r=>r.store.BranchID==branchid)
                    .Select(a => new CollatedBranch
                    {
                        StatsID = a.StatsID,
                        BranchID = a.store.BranchID,
                        StoreID = a.StoreID,
                        ProductID = a.ProductID,
                        ProductName = a.product.Name,
                        ProductQty = a.ProductQty,
                        ProductValue = (a.ProductQty) * ((decimal)a.product.Price)
                    })
                    .OrderBy(r => r.BranchID).ThenBy(r => r.StoreID).ThenBy(r => r.ProductName)
                    .ToList();
            }
            return branchstats;
        }
        private List<CollatedTech> CollateTech(string employeeid, string accessid, string branchid, bool current=true, bool unapproved = false, bool delivered = false)
        {
            List<CollatedTech> techstats = new List<CollatedTech>();
            if (accessid == "Chefstekniker" || accessid == "Administrator")
            {
                if (current)
                {
                    techstats = db.carstats.Where(r=>r.ReleasedDate==null)
                    .GroupBy(r => new { r.employee.BranchID, r.EmployeeID, r.ProductID })
                    .Select(a => new CollatedTech
                    {
                        BranchID = a.Min(b => b.employee.BranchID),
                        EmployeeID = a.Min(b => b.EmployeeID),
                        EmployeeName = a.Min(b => b.employee.FirstName) + " " + a.Min(b => b.employee.LastName),
                        ProductID = a.Min(b => b.ProductID),
                        ProductName = a.Min(b => b.product.Name),
                        ProductQty = a.Count(),
                        ProductValue = (a.Count()) * (decimal)(a.Min(b => b.product.Price))
                    })
                    .OrderBy(r => r.BranchID).ThenBy(r => r.EmployeeID).ThenBy(r => r.ProductName)
                    .ToList();
                }
                else if (unapproved)
                {
                    techstats = db.carstats.Where(r => r.ReleasedDate!= null && r.Approved==false)
                    .Select(a => new CollatedTech
                    {
                        StatsID = a.StatsID,
                        BranchID = a.employee.BranchID,
                        EmployeeID = a.EmployeeID,
                        EmployeeName = a.employee.FirstName + " " + a.employee.LastName,
                        ProductID = a.ProductID,
                        ProductName = a.product.Name,
                        ProductQty = 0,
                        ProductValue = ((decimal)a.product.Price),
                        Approved = a.Approved,
                        ReleaseDate = a.ReleasedDate,
                        Customer = a.ReleasedTo
                    })
                    .OrderByDescending(r => r.ReleaseDate).ThenBy(r => r.Customer).ThenBy(r => r.ProductName)
                    .ToList();
                }
                else if (delivered)
                {
                    techstats = db.carstats.Where(r => r.ReleasedDate != null && r.Approved == true)
                    .Select(a => new CollatedTech
                    {
                        StatsID = a.StatsID,
                        BranchID = a.employee.BranchID,
                        EmployeeID = a.EmployeeID,
                        EmployeeName = a.employee.FirstName + " " + a.employee.LastName,
                        ProductID = a.ProductID,
                        ProductName = a.product.Name,
                        ProductQty = 0,
                        ProductValue = ((decimal)a.product.Price),
                        Approved = a.Approved,
                        ReleaseDate = a.ReleasedDate,
                        Customer = a.ReleasedTo
                    })
                    .OrderByDescending(r => r.ReleaseDate).ThenBy(r => r.Customer).ThenBy(r => r.ProductName)
                    .ToList();
                }
            }
            else
            {
                if (current)
                {
                    techstats = db.carstats.Where(r => r.employee.BranchID == branchid && r.ReleasedDate == null)
                    .GroupBy(r => new { r.employee.BranchID, r.EmployeeID, r.ProductID })
                    .Select(a => new CollatedTech
                    {
                        BranchID = a.Min(b => b.employee.BranchID),
                        EmployeeID = a.Min(b => b.EmployeeID),
                        EmployeeName = a.Min(b => b.employee.FirstName) + " " + a.Min(b => b.employee.LastName),
                        ProductID = a.Min(b => b.ProductID),
                        ProductName = a.Min(b => b.product.Name),
                        ProductQty = a.Count(),
                        ProductValue = (a.Count()) * (decimal)(a.Min(b => b.product.Price))
                    })
                    .OrderBy(r => r.BranchID).ThenBy(r => r.EmployeeID).ThenBy(r => r.ProductName)
                    .ToList();
                }
                else if (unapproved)
                {
                    techstats = db.carstats.Where(r => r.employee.BranchID == branchid && r.ReleasedDate != null && r.Approved == false)
                    .Select(a => new CollatedTech
                    {
                        StatsID = a.StatsID,
                        BranchID = a.employee.BranchID,
                        EmployeeID = a.EmployeeID,
                        EmployeeName = a.employee.FirstName + " " + a.employee.LastName,
                        ProductID = a.ProductID,
                        ProductName = a.product.Name,
                        ProductQty = 0,
                        ProductValue = ((decimal)a.product.Price),
                        Approved = a.Approved,
                        ReleaseDate = a.ReleasedDate,
                        Customer = a.ReleasedTo
                    })
                    .OrderByDescending(r => r.ReleaseDate).ThenBy(r => r.Customer).ThenBy(r => r.ProductName)
                    .ToList();
                }
                else if (delivered)
                {
                    techstats = db.carstats.Where(r => r.employee.BranchID == branchid && r.ReleasedDate != null && r.Approved == true)
                    .Select(a => new CollatedTech
                    {
                        StatsID = a.StatsID,
                        BranchID = a.employee.BranchID,
                        EmployeeID = a.EmployeeID,
                        EmployeeName = a.employee.FirstName + " " + a.employee.LastName,
                        ProductID = a.ProductID,
                        ProductName = a.product.Name,
                        ProductQty = 0,
                        ProductValue = ((decimal)a.product.Price),
                        Approved = a.Approved,
                        ReleaseDate = a.ReleasedDate,
                        Customer = a.ReleasedTo
                    })
                    .OrderByDescending(r => r.ReleaseDate).ThenBy(r => r.Customer).ThenBy(r => r.ProductName)
                    .ToList();
                }
            }
            return techstats;
        }
        public void SetViewBag(
            string title = null,
            string action = null,
            string controller = null,
            string lastpage = null,
            string buttonvalue = null,

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