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
            PanierModel panierModel = null;

            if (sessionUtilisateur != null)
            {
                if (HttpContext.Application[idSession] != null)
                {
                    panierModel = (PanierModel)HttpContext.Application[idSession];
                }
                else
                {
                    panierModel = new PanierModel();
                }


                //si le produit n'est pas présent dans le panier je le récupère
                Produit produit = db.Produits.Find(idProduit);

                ProduitPanier produitPanier = FindProduit(idProduit);

                if (produitPanier != null)
                {
                    panierModel.Add(produitPanier);
                }


                panierModel.GetQuantite();

                //Je sauve mon panier en session
                HttpContext.Application[idSession] = panierModel;
            }

            //Je retourne en Json le nombre de lignes
            return Json(panierModel.Quantite, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddMenu(int idMenu, List<int> idProduits, string idSession)
        {
            SessionUtilisateur sessionUtilisateur = db.SessionUtilisateurs.Find(Session.SessionID);
            PanierModel panierModel = null;

            if (sessionUtilisateur != null)
            {
                if (HttpContext.Application[idSession] != null)
                {
                    panierModel = (PanierModel)HttpContext.Application[idSession];
                }
                else
                {
                    panierModel = new PanierModel();
                }


                //si le menu n'est pas présent dans le panier je le récupère
                Menu menu = db.Menus.Find(idMenu);

                MenuPanier menuPanier = new MenuPanier();
                menuPanier.IdMenu = idMenu;

                if (menu != null)
                {
                    foreach (int idProduit in idProduits)
                    {
                        ProduitPanier produitPanier = FindProduit(idProduit);

                        if (produitPanier != null)
                        {
                            menuPanier.produits.Add(produitPanier);
                        }
                    }

                    panierModel.Add(menuPanier);
                }

                panierModel.GetQuantite();

                //Je sauve mon panier en session
                HttpContext.Application[idSession] = panierModel;
            }

            //Je retourne en Json le nombre de lignes
            return Json(panierModel.Quantite, JsonRequestBehavior.AllowGet);
        }

        private ProduitPanier FindProduit(int idProduit)
        {
            Produit produit = db.Produits.Find(idProduit);

            ProduitPanier produitPanier = new ProduitPanier();

            if (produit != null)
            {
                produitPanier.IdProduit = idProduit;
                produitPanier.Nom = produit.Nom;
                produitPanier.Description = produit.Description;
                produitPanier.Quantite = 1;
                produitPanier.Prix = produit.Prix;
                //produitPanier.Photo = produit.Photos.First().Nom;
                produitPanier.IdRestaurant = produit.IdRestaurant;
            }

            return produitPanier;
        }

        public JsonResult SubQte(int idProduit, string idSession)
        {
            SessionUtilisateur sessionUtilisateur = db.SessionUtilisateurs.Find(Session.SessionID);

            PanierModel panier = null;

            panier = (PanierModel)HttpContext.Application[idSession];
            ItemPanier itemPanier = panier.FirstOrDefault(p => p.GetIdProduit() == idProduit);


            if (itemPanier.Quantite > 1)
            {
                itemPanier.Quantite -= 1;
                return Json(itemPanier.Quantite, JsonRequestBehavior.AllowGet);
            }

            else
            {
                panier.Remove(itemPanier);
                return Json(0, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult AddQte(int idProduit, string idSession)
        {
            SessionUtilisateur sessionUtilisateur = db.SessionUtilisateurs.Find(Session.SessionID);

            PanierModel panier = null;

            panier = (PanierModel)HttpContext.Application[idSession];
            ItemPanier itemPanier = panier.FirstOrDefault(p => p.GetIdProduit() == idProduit);


            itemPanier.Quantite += 1;

            return Json(itemPanier.Quantite, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPanier(string idSession)
        {
            PanierModel panier = null;

            SessionUtilisateur sessionUtilisateur = db.SessionUtilisateurs.Find(Session.SessionID);

            if (sessionUtilisateur != null)
            {
                if (HttpContext.Application[idSession] != null)
                {
                    panier = (PanierModel)HttpContext.Application[idSession];
                }
            }

            return Json(panier, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveCommande(string idSession)
        {
            PanierModel panierModel = null;

            SessionUtilisateur sessionUtilisateur = db.SessionUtilisateurs.Find(Session.SessionID);

            string message = "";

            //On vérifie l'existence d'un utilisateur en session
            if (sessionUtilisateur != null)
            {
                //On récupère son panier
                if (HttpContext.Application[idSession] != null)
                {
                    panierModel = (PanierModel)HttpContext.Application[idSession];
                }
            }

            //On récupère l'utilisateur en base de données à partir de son idsession
            Utilisateur utilisateur = db.Utilisateurs.First(u => u.IdSession == idSession);

            if (utilisateur != null && utilisateur.Solde > 0 && panierModel != null && panierModel.Count > 0)
            {
                decimal prixTotal = 0;
                int idRestaurant = 0;

                //Calcul du montant de la commande
                panierModel.GetMontant();
                prixTotal = panierModel.Montant;

                //On vérifie si le montant de la commande est inférieur au solde avant de sauvegarder la commande
                if (prixTotal <= utilisateur.Solde)
                {
                    //On crée la commande
                    Commande commande = new Commande();
                    commande.IdUtilisateur = utilisateur.IdUtilisateur;
                    commande.IdRestaurant = panierModel.IdRestaurant;
                    commande.Date = DateTime.Now;
                    commande.Prix = prixTotal;
                    commande.IdEtatCommande = 1;

                    utilisateur.Solde -= prixTotal;


                    //On parcourt tous les produits dans le panierModel pour les enregistrer en bdd
                    foreach (ItemPanier itemPanier in panierModel)
                    {
                        if (itemPanier is ProduitPanier)
                        {
                            CommandeProduit commandeProduit = new CommandeProduit();

                            commandeProduit.IdProduit = itemPanier.GetIdProduit();
                            commandeProduit.Prix = itemPanier.Prix;
                            commandeProduit.Quantite = itemPanier.Quantite;

                            commande.CommandeProduits.Add(commandeProduit);
                        }

                        else if (itemPanier is MenuPanier menuPanier)
                        {
                            List<ProduitPanier> produitPaniers = menuPanier.produits;
                            Menu menu = db.Menus.Find(itemPanier.GetIdMenu());

                            foreach (ProduitPanier itemProduits in produitPaniers)
                            {
                                CommandeProduit commandeProduit = new CommandeProduit();

                                commandeProduit.IdProduit = itemPanier.GetIdProduit();
                                commandeProduit.Prix = itemPanier.Prix;
                                commandeProduit.Quantite = itemPanier.Quantite;
                                commandeProduit.Menus.Add(menu);
                                commande.CommandeProduits.Add(commandeProduit);
                            }
                        }

                        else if(itemPanier is ProduitComposePanier produitComposePanier)
                        {
                            List<ProduitPanier> produitPaniers = produitComposePanier.produits;
                            Menu menu = db.Menus.Find(itemPanier.GetIdMenu());

                            foreach (ProduitPanier itemProduits in produitPaniers)
                            {
                                CommandeProduit commandeProduit = new CommandeProduit();

                                commandeProduit.IdProduit = itemPanier.GetIdProduit();
                                commandeProduit.Prix = itemPanier.Prix;
                                commandeProduit.Quantite = itemPanier.Quantite;
                                commandeProduit.Menus.Add(menu);
                                commande.CommandeProduits.Add(commandeProduit);
                            }
                        }

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