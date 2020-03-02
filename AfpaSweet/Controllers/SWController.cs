using AfpaSweet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfpaSweet.Controllers
{
    public class SWController : Controller
    {
        private AfpEatEntities db = new AfpEatEntities();
        //GET: SW
        public JsonResult AddProduit(int idProduit, string idSession)
        {
            SessionUtilisateur sessionUtilisateur = db.SessionUtilisateurs.Find(Session.SessionID);
            List<ProduitPanier> panier = null;
            decimal panierQte = 0;

            if (sessionUtilisateur != null)
            {
                if (HttpContext.Application[idSession] != null)
                {
                    panier = (List<ProduitPanier>)HttpContext.Application[idSession];
                }
                else
                {
                    panier = new List<ProduitPanier>();
                }

                Produit produit = db.Produits.Find(idProduit);
                ProduitPanier produitPanier = null;

                foreach (var item in panier)
                {
                    panierQte += item.Quantite;
                }

                if (panier.Where(p => p.IdProduit == idProduit).Count() > 0)
                {
                    produitPanier = panier.Where(p => p.IdProduit == idProduit).First();
                    produitPanier.Quantite += 1;
                }

                else
                {
                    produitPanier = new ProduitPanier();

                    produitPanier.IdProduit = idProduit;
                    produitPanier.Nom = produit.Nom;
                    produitPanier.Description = produit.Description;
                    produitPanier.Quantite = 1;
                    produitPanier.Prix = produit.Prix;
                    //produitPanier.Photo = produit.Photos.First().Nom;
                    produitPanier.IdRestaurant = produit.IdRestaurant;

                    panier.Add(produitPanier);

                }

                HttpContext.Application[idSession] = panier;
            }

            return Json(panierQte + 1, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SubQte(int idProduit, string idSession)
        {
            SessionUtilisateur sessionUtilisateur = db.SessionUtilisateurs.Find(Session.SessionID);

            List<ProduitPanier> panier = null;

            panier = (List<ProduitPanier>)HttpContext.Application[idSession];
            ProduitPanier produitPanier = panier.Where(p => p.IdProduit == idProduit).First();


            if (produitPanier.Quantite > 1)
            {
                produitPanier.Quantite -= 1;
                return Json(produitPanier.Quantite, JsonRequestBehavior.AllowGet);
            }

            else
            {
                panier.Remove(produitPanier);
                return Json(0, JsonRequestBehavior.AllowGet);
            }

        }        
        
        public JsonResult AddQte(int idProduit, string idSession)
        {
            SessionUtilisateur sessionUtilisateur = db.SessionUtilisateurs.Find(Session.SessionID);

            List<ProduitPanier> panier = null;

            panier = (List<ProduitPanier>)HttpContext.Application[idSession];
            ProduitPanier produitPanier = panier.Where(p => p.IdProduit == idProduit).First();


                produitPanier.Quantite += 1;

                return Json(produitPanier.Quantite, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPanier(string idSession)
        {
            List<ProduitPanier> panier = null;

            SessionUtilisateur sessionUtilisateur = db.SessionUtilisateurs.Find(Session.SessionID);

            if (sessionUtilisateur != null)
            {
                if (HttpContext.Application[idSession] != null)
                {
                    panier = (List<ProduitPanier>)HttpContext.Application[idSession];
                }
            }

            return Json(panier, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveCommande(string idSession)
        {
            List<ProduitPanier> panier = null;

            SessionUtilisateur sessionUtilisateur = db.SessionUtilisateurs.Find(Session.SessionID);

            string message = "";

            if (sessionUtilisateur != null)
            {
                if (HttpContext.Application[idSession] != null)
                {
                    panier = (List<ProduitPanier>)HttpContext.Application[idSession];
                }
            }

            Utilisateur utilisateur = db.Utilisateurs.First(u => u.IdSession == idSession);

            if (utilisateur != null && utilisateur.Solde > 0 && panier != null && panier.Count > 0)

            {
                decimal prixTotal = 0;
                int idRestaurant = 0;

                foreach (ProduitPanier produitPanier in panier)
                {
                    prixTotal += produitPanier.Prix * produitPanier.Quantite;
                    idRestaurant = produitPanier.IdRestaurant;
                }

                if (prixTotal <= utilisateur.Solde)
                {
                    Commande commande = new Commande();
                    commande.IdUtilisateur = utilisateur.IdUtilisateur;
                    commande.IdRestaurant = idRestaurant;
                    commande.Date = DateTime.Now;
                    commande.Prix = prixTotal;
                    commande.IdEtatCommande = 1;

                    utilisateur.Solde -= prixTotal;



                    foreach (ProduitPanier produitPanier in panier)
                    {
                        CommandeProduit commandeProduit = new CommandeProduit();
                        //commandeProduit.IdCommande = commande.IdCommande;
                        commandeProduit.IdProduit = produitPanier.IdProduit;
                        commandeProduit.Prix = produitPanier.Prix;
                        commandeProduit.Quantite = produitPanier.Quantite;

                        commande.CommandeProduits.Add(commandeProduit);
                    }

                    db.Commandes.Add(commande);
                    db.SaveChanges();
                }
            }

            return Json(message, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoginUtilisateur(string idSession, string matricule, string password)
        {
            Utilisateur utilisateur = db.Utilisateurs.FirstOrDefault(u => u.Matricule == matricule && u.Password == password);

            if (utilisateur != null)
            {
                utilisateur.IdSession = idSession;

                db.SaveChanges();

                return Json(new { error = 0, message = "Vous êtes connecté" }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { error = 1, message = "Erreur de connection" }, JsonRequestBehavior.AllowGet);

        }
    }
}