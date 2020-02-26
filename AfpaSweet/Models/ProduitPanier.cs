using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfpaSweet.Models
{
    public class ProduitPanier
    {
        public int IdProduit { get; set; }

        public int IdRestaurant { get; set; }

        public string Nom { get; set; }

        public decimal Prix { get; set; }

        public string Description { get; set; }

        public int Quantite { get; set; }

        public string Photo { get; set; }
    }
}