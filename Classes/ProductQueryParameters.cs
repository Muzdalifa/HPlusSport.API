using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HPlusSport.API.Classes
{
    public class ProductQueryParameters:QueryParameters //we dont want to loose the prpoerty that we have in query paramet class
    {
        //this class contains data that will be passesd by user in th URL concering the product
        public string Sku { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Name { get; set; }


    }
}
