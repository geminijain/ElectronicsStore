using ElectronicsStore.DAL;
using ElectronicsStore.Models;
using ElectronicsStore.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace ElectronicsStore.Controllers
{
    public class ProductsController : Controller
    {
        private StoreContext db = new StoreContext();

        // GET: Products
        public ActionResult Index(string category, string search, string sortBy, int? page)
        {
            //instantiate a new view model
            ProductIndexViewModel viewModel = new ProductIndexViewModel();

            //select the products
            var products = db.Products.Include(p => p.Category);

            //perform the search and save the search string to the viewModel
            if (!String.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search) || p.Description.Contains(search) || p.Category.Name.Contains(search));
                viewModel.Search = search;
            }

            //group search results into categories and count how many items in each category
            viewModel.CatsWithCount = from matchingProducts in products
                                      where
                                      matchingProducts.CategoryID != null
                                      group matchingProducts by
                                       matchingProducts.Category.Name into
                                      catGroup
                                      select new CategoryWithCount()
                                      {
                                          CategoryName = catGroup.Key,
                                          ProductCount = catGroup.Count()
                                      };

            if (!String.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category.Name == category);
                viewModel.Category = category;
            }

            //sort the results
            switch (sortBy)
            {
                case "price_lowest":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_highest":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            int currentPage = (page ?? 1);
            viewModel.Products = products.ToPagedList(currentPage, Constants.PageItems);
            viewModel.SortBy = sortBy;
            viewModel.Sorts = new Dictionary<string, string>
            {
                {"Price low to high", "price_lowest" },
                {"Price high to low", "price_highest" }
            };

            return View(viewModel);
        }


        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ProductViewModel viewModel = new ProductViewModel();
            viewModel.CategoryList = new SelectList(db.Categories, "ID", "Name");
            viewModel.ImageLists = new List<SelectList>();
            for (int i = 0; i < Constants.NumberOfProductImages; i++)
            {
                viewModel.ImageLists.Add(new SelectList(db.ProductImages, "ID", "FileName"));
            }
            return View(viewModel);
        }

        //POST: Products/Create
        //To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductViewModel viewModel)
        {
            //check the user has entered a file
            if (file != null)
            {
                //check if the file is valid
                if (Utility.ValidateFile(file))
                {
                    try
                    {
                        Utility.SaveFileToDisk(file);
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("FileName", "Sorry an error occurred saving the file to disk, please try again");
                    }
                }
                else
                {
                    ModelState.AddModelError("FileName", "The file must be gif, png, jpeg or jpg and less than 2MB in size");
                }
            }
            else
            {
                //if the user has not entered a file return an error message
                ModelState.AddModelError("FileName", "Please choose a file");
            }
            if (ModelState.IsValid)
            {
                db.ProductImages.Add(new ProductImage { FileName = file.FileName });
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }


        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", product.CategoryID);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Description,Price,CategoryID")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", product.CategoryID);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}