using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AfpaSweet.Models;

namespace AfpaSweet.Controllers
{
    public class RestaurantsController : Controller
    {
        private AfpEatEntities db = new AfpEatEntities();

        // GET: Restaurants
        public ActionResult Index()
        {
            ViewBag.restaurants = db.Restaurants.ToList();
            ViewBag.typeCuisines = db.TypeCuisines.ToList();

            return View();
        }
        public ActionResult Menu(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.DetailRestaurant = db.Restaurants.Find(id);
            ViewBag.Entree = db.Produits.Where(p => p.IdCategorie == 1 && p.IdRestaurant == id).ToList();
            ViewBag.Plat = db.Produits.Where(p => p.IdCategorie == 2 && p.IdRestaurant == id).ToList();
            ViewBag.Dessert = db.Produits.Where(p => p.IdCategorie == 3 && p.IdRestaurant == id).ToList();

            if (ViewBag.DetailRestaurant == null)
            {
                return HttpNotFound();
            }

            if (ViewBag.Entree == null)
            {
                return HttpNotFound();
            }

            if (ViewBag.Plat == null)
            {
                return HttpNotFound();
            }

            if (ViewBag.Dessert == null)
            {
                return HttpNotFound();
            }


            return View();
        }

        public ActionResult AddPanier(int? idProduit, int? idRestaurant)
        {
            if (idProduit == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Produit produitPanier = db.Produits.Find(idProduit);

            if (produitPanier == null)
            {
                return HttpNotFound();
            }

            List<Produit> produits = new List<Produit>();

            if(Session["panier"] == null)
            {
                Session["panier"] = produitPanier;
            }
            else
            {
                produits = (List<Produit>)Session["panier"];
                produits.Add(produitPanier);
                Session["panier"] = produits;
            }

            return RedirectToAction("Menu/" + idRestaurant);

        }

        public ActionResult Cuisine(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.typeRestaurants = db.Restaurants.Where(p => p.IdTypeCuisine == id).ToList();

            if (ViewBag.typeRestaurants == null)
            {
                return HttpNotFound();
            }

            return View();
        }

        // GET: Restaurants/Create
        public ActionResult Create()
        {
            ViewBag.IdTypeCuisine = new SelectList(db.TypeCuisines, "IdTypeCuisine", "Nom");
            return View();
        }

        // POST: Restaurants/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdRestaurant,Nom,IdTypeCuisine,Budget,Description,Adresse,CodePostal,Ville,Tel,Mobile,Email,Login,Password")] Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                db.Restaurants.Add(restaurant);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdTypeCuisine = new SelectList(db.TypeCuisines, "IdTypeCuisine", "Nom", restaurant.IdTypeCuisine);
            return View(restaurant);
        }

        // GET: Restaurants/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdTypeCuisine = new SelectList(db.TypeCuisines, "IdTypeCuisine", "Nom", restaurant.IdTypeCuisine);
            return View(restaurant);
        }

        // POST: Restaurants/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdRestaurant,Nom,IdTypeCuisine,Budget,Description,Responsable,Adresse,CodePostal,Ville,Tel,Mobile,Email,Login,Password")] Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                db.Entry(restaurant).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdTypeCuisine = new SelectList(db.TypeCuisines, "IdTypeCuisine", "Nom", restaurant.IdTypeCuisine);
            return View(restaurant);
        }

        // GET: Restaurants/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            return View(restaurant);
        }

        // POST: Restaurants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Restaurant restaurant = db.Restaurants.Find(id);
            db.Restaurants.Remove(restaurant);
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
