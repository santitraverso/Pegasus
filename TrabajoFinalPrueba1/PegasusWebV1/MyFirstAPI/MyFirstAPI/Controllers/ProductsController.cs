using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFirstAPI.Models;

namespace MyFirstAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsController(ShopContext context)
        {
            _context = context;
            
            //We make sure the DB is seeded
            _context.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            return Ok(await _context.Products.ToListAsync());
        }

        [HttpGet, Route("/products/{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null) 
            {
               return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult> PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(
                "GetProduct",
                new { id = product.Id },
                product);
        }

        //We expect the product from the body in the HTTP request
        [HttpPut("id")]
        public async Task<ActionResult> PutProduct(int id, [FromBody] Product product)
        {
            if(id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            //This exception is called if the item has already been modified by another call
            catch (DbUpdateConcurrencyException)
            {
                if(!_context.Products.Any(p => p.Id == product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

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

        [HttpPost]
        [Route("Delete")]
        public async Task<ActionResult> DeleteMultiple([FromQuery] int[] ids)
        {
            var products = new List<Product>();
            foreach (var id in ids)
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                products.Add(product);
            }

            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();

            return Ok(products);
        }
    }
}
