using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HPlusSport.API.Classes;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Controllers
{
    [Route("[controller]")] //we have removed api/ so that the route to this controller is just the name of the controller which is the classname minus word Conroller
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsController(ShopContext context)
        {
            _context = context;

            _context.Database.EnsureCreated(); //this method ensure that the db has been created, if not it will create it
        }
        /*
        [HttpGet]
        public IEnumerable<Product> GetAllProducts() //this is first approach to return something from api, 
        {                                            //but what if something goes wrong in the db? what we can say then? 
            return _context.Products.ToArray();      //because of that we use the scong approach
        }
        */
        /* have been commented to make the method async
        //Getting all data from table
        [HttpGet]
        public IActionResult GetAllProducts() // IActionResult give acess to convinience method to return specific HTTP status codes
        {
            var products = _context.Products.ToArray();
            return Ok(products); //we can also write return OK(_context.Products.ToArray()); OK is a helper method
        }
        */
        /*
        //Getting specific data using id
        [HttpGet("{id}")] //this id will be taken from url and passed to the method.
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.Find(id); //look for a specific product
            return Ok(product);
        }
        */
        /*
        //Handling error, what if the id selected by the user is not in the table
        //Getting specific data using id
        [HttpGet("{id}")] 
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.Find(id); 
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        */

        /*
        //Getting specific data using id with explecitly specifying the data type of id, if the id provided is of different type it will give nothing, meaning the id you have provided did not match anyting
        [HttpGet("{id:int}")]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        */

        //Getting all data by once result into problem if the data is too large, so we divide them using size and we can see other using pagination. 
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductQueryParameters queryParameters) //to see the amount of product you want you need to specify that amount as int here, and to see others you need to specify page number 
        {
            IQueryable<Product> products = _context.Products; //querying all data

            //filtering by price
            if (queryParameters.MinPrice != null && queryParameters.MaxPrice != null) //we check if MaxPrice and MinPrice has been set
            {
                products = products.Where(
                    p => p.Price >= queryParameters.MinPrice.Value &&
                    p.Price <= queryParameters.MaxPrice.Value);
            }

            //filtering by sku
            if(!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p=>p.Sku == queryParameters.Sku);
            }

            //serching items example you can pass this parameters: https://localhost:44388/products?size=10&page=1&name=Orange mineral water
            if (!string.IsNullOrEmpty(queryParameters.Name)) //!string.IsNullOrEmpty() check if only this paramter has been set, ie has been specified by the user.
            {
                products = products.Where(
                    p => p.Name.ToLower().Contains(queryParameters.Name.ToLower()));
            }

            //Sorting items example: https://localhost:44388/products?sortBy=Price&sortOrder=desc
            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if(typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(queryParameters.SortBy, queryParameters.SortOrder);
                }
            }

            products = products
                .Skip(queryParameters.Size * (queryParameters.Page - 1)) // example, for page number 2 we have to skip one times the size given items, show from the next item number follow
                .Take(queryParameters.Size);

            return Ok(await products.ToArrayAsync());
        }

        //Getting specific data
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
    }
}
