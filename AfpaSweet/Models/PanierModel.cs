using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfpaSweet.Models
{
    public class PanierModel : List<ItemPanier>
    {
        private AfpEatEntities db = new AfpEatEntities();

        public int IdRestaurant { get; set; }

        public int Quantite { get; set; }

        public decimal Montant { get; set; }

        public void GetQuantite()
        {
            Quantite = 0;

            Parcourir();
        }

        public void GetMontant()
        {
            Montant = 0;

            Parcourir();
        }

        public bool AddItem(ItemPanier item)
        {
            int idProduit = item.GetIdProduit();
            int idMenu = item.GetIdMenu();
            bool isReturnOk = true;

            //la parenthèse à l'intéreur de la parenthèse du if indique qu'une des deux 
            //conditions doit etre vraie, et que toutes les hypothèses hors des parenthèses doivent être vraies
            if (item != null && (item is ProduitPanier || item is ProduitComposePanier) && idProduit > 0) //Ici je suis sur un produit
            {
                //on vérifie si le produit ajouté est déjà présent dans le panier
                ItemPanier itemPanier = this.FirstOrDefault(p => p.GetIdProduit() == idProduit);

                if (itemPanier != null)
                {
                    itemPanier.Quantite++;
                }
                else
                {
                    this.Add(itemPanier);
                }

            }

            else if (item != null && item is MenuPanier && idMenu > 0) //Ici je suis sur un menu
            {
                //on vérifie si le menu ajouté est déjà présent dans le panier
                ItemPanier itemPanier = this.FirstOrDefault(p => p.GetIdMenu() == idMenu);

                if (itemPanier != null)
                {
                    itemPanier.Quantite++;
                }
                else
                {
                    this.Add(itemPanier);
                }
            }

            else //gérer l'erreur si l'item est null
            {
                isReturnOk = false;
            }

            return isReturnOk; //true si pas d'erreur, false si erreur
        }

        public bool RemoveItem(int? idProduit, int? idMenu)
        {
            bool isReturnOk = false;
            ItemPanier itemPanier = null;

            if (idProduit != null && idProduit > 0)
            {
                itemPanier = this.FirstOrDefault(p => p.GetIdProduit() == idProduit);
            }
            else if (idMenu != null && idMenu > 0)
            {
                itemPanier = this.FirstOrDefault(p => p.GetIdMenu() == idMenu);
            }

            if (itemPanier != null)
            {
                this.Remove(itemPanier);
                isReturnOk = true;
            }

            return isReturnOk;
        }

        private void Parcourir()
        {
            foreach (ItemPanier itemPanier in this)
            {
                Montant += itemPanier.Prix * itemPanier.Quantite;
                Quantite += itemPanier.Quantite;
                IdRestaurant = itemPanier.IdRestaurant;
            }
        }
    }
}