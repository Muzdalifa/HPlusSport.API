using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HPlusSport.API.Classes;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Controllers
{
    [ApiVersion("1.0")]
    //[Route("v{v:apiVersion}/products")] //we have removed [controller] so that the route to this controller can be acessed directly by the name of the controller.
    [Route("products")] //implementing header versioning add this only and line in startup file. Versioning in Header is MORE PRACTIBLE than other, QueryString is the easiest one to implement. 
    [ApiController]
    [Authorize]
    public class ProductsV1_0Controller : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsV1_0Controller(ShopContext context)
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
            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);
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
                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
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

        //Adding the item
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromBody] Product product)  // by ActionResult<Product> We expect to return Action result of type product, async Task<> , await for asnchronization.
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(         // To get the added item we use CreatedAtAction method which need:  
                "GetProduct",               //name of the function to retrieve a product, in this case GetProduct() that has ben define above,
                new { id = product.Id },    //the argument , which is id of the product, which set autoamtically after adding the product and save the changes
                product                     //the product it self, but here .NET framework will bind everything in the Product class, there are some secret property
                );                          //you dont want to reveal to the user, they you need to create another class view and return it instead of returning the complete data.
        }

        //Updating the item
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct([FromRoute]int id, [FromBody] Product product) //Task<IActionResult> we use this because when everything is right we dont return anything.
        {
            if(id != product.Id) //we have 2 id here id from route when we click send it will be passed from URL and id in the body specified by the user
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if(_context.Products.Find(id)==null) //you might want to check if the id of the product exist in this moment, some time it can happen may be some one delete it at that moment
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent(); //if everything goes OK, return nothing. This is the suggested .NeT method to return when you update items in db
        }

        //Deleting the item
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id) // we use ActionResult<Product> because we want to return product that has been deleted
        {
            var product = await _context.Products.FindAsync(id); //such for the product
            if(product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return product; //there are many approach here, you can also use NoContent() and return nothing after deleting the product,or return a deleted product as done here.
        }
    }

    //SECOND CONTROLLER VERSION_2

    [ApiVersion("2.0")]
    //[Route("v{v:apiVersion}/products")] //Before putting version we were accessing the controller using the name of the controller, but now we access it version and name such as https://localhost:44388/v2.0/products
    [Route("products")]   //iplementing header versioning
    [ApiController]
    public class ProductsV2_0Controller : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsV2_0Controller(ShopContext context)
        {
            _context = context;

            _context.Database.EnsureCreated();
        }
        

        //Getting all data 
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductQueryParameters queryParameters) //to see the amount of product you want you need to specify that amount as int here, and to see others you need to specify page number 
        {
            IQueryable<Product> products = _context.Products.Where(p=>p.IsAvailable == true);  //we make small changes so that it can deffer with Version 1

            //filtering by price
            if (queryParameters.MinPrice != null && queryParameters.MaxPrice != null) 
            {
                products = products.Where(
                    p => p.Price >= queryParameters.MinPrice.Value &&
                    p.Price <= queryParameters.MaxPrice.Value);
            }

            //filtering by sku
            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);
            }

            //serching items example you can pass this parameters: https://localhost:44388/v2.0/products?size=10&page=1&name=Orange mineral water
            if (!string.IsNullOrEmpty(queryParameters.Name)) 
            {
                products = products.Where(
                    p => p.Name.ToLower().Contains(queryParameters.Name.ToLower()));
            }

            //Sorting items example: https://localhost:44388/v2.0/products?sortBy=Price&sortOrder=desc
            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(queryParameters.SortBy, queryParameters.SortOrder);
                }
            }

            products = products
                .Skip(queryParameters.Size * (queryParameters.Page - 1)) 
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

        //Adding the item
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromBody] Product product)  // by ActionResult<Product> We expect to return Action result of type product, async Task<> , await for asnchronization.
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(         
                "GetProduct",               
                new { id = product.Id },    
                product                     
                );                          
        }

        //Updating the item
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct([FromRoute] int id, [FromBody] Product product) //Task<IActionResult> we use this because when everything is right we dont return anything.
        {
            if (id != product.Id) 
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Products.Find(id) == null) 
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent(); 
        }

        //Deleting the item
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id) 
        {
            var product = await _context.Products.FindAsync(id); 
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return product; 
        }
    }
}
