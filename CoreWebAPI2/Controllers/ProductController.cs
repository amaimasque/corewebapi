using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreWebAPI2.Models;
using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CoreWebAPI2.Controllers
{
    [Route("api/products")]
    [ApiController]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        // GET: api/products/all
        [HttpGet]
        [Route("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetProductModel()
        {
            ApplicationUser applicationUser = await userManager.GetUserAsync(User);

            if (applicationUser == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var results = await _context.ProductModel.Where(item => item.UserID == applicationUser.Id).ToListAsync();

            return Ok(new { message = "Get products list", total = results.Count, results });
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductModel>> GetProductModel(int id)
        {
            ApplicationUser applicationUser = await userManager.GetUserAsync(User);
            var productModel = await _context.ProductModel.Where(item => item.UserID == applicationUser.Id && item.ID == id).FirstOrDefaultAsync();

            if (productModel == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(new { message = "Get product by ID", result = productModel });
        }

        // PUT: api/Product/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PutProductModel([FromRoute]int id, [FromForm]ProductModel productModel)
        {
            try
            {
                if (id != productModel.ID)
                {
                    return BadRequest(new { message = "Updating of product failed" });
                }

                ApplicationUser applicationUser = await userManager.GetUserAsync(User);

                var product = await _context.ProductModel.AsNoTracking().Where(item => item.UserID == applicationUser.Id && item.ID == id).FirstOrDefaultAsync();

                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                if (!ModelState.IsValid || applicationUser == null)
                {
                    return new BadRequestObjectResult(new { message = "Updating of product failed" });
                }
                _context.Entry(productModel).State = EntityState.Modified;

                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    if (file.Length > 0)
                    {
                        using (var dataStream = new MemoryStream())
                        {
                            await file.CopyToAsync(dataStream);
                            productModel.DisplayImage = dataStream.ToArray();
                        }
                    }
                    else productModel.DisplayImage = product.DisplayImage;
                } 
                if (Request.Form.Files.Count == 0 || !Request.Form.Files.Where(item => item.Name == "DisplayImage").Any())
                {
                    productModel.DisplayImage = product.DisplayImage;
                }

                productModel.UserID = applicationUser.Id;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Updating of product success", productModel });
            }
            catch (Exception exception)
            {
                return new BadRequestObjectResult(new { message = exception.Message });
            }
        }

        // POST: api/Product
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost(Name = "add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductModel>> PostProductModel([FromForm] ProductModel productModel)
        {
            ApplicationUser applicationUser = await userManager.GetUserAsync(User);

            if (!ModelState.IsValid || applicationUser == null)
            {
                return new BadRequestObjectResult(new { Message = "Adding of product failed"});
            }

            var product = new ProductModel()
            {
                Title = productModel.Title,
                Description = productModel.Description,
                UserID = applicationUser.Id,
            };

            if (Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                using (var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    product.DisplayImage = dataStream.ToArray();
                }
            }

            _context.ProductModel.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { message ="Adding of product success", product });
        }

        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteProductModel(int id)
        {
            ApplicationUser applicationUser = await userManager.GetUserAsync(User);
            var productModel = await _context.ProductModel.Where(item => item.UserID == applicationUser.Id && item.ID == id).FirstOrDefaultAsync();

            if (productModel == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            _context.ProductModel.Remove(productModel);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product successfully deleted", result = productModel });
        }

        private bool ProductModelExists(int id)
        {
            return _context.ProductModel.Any(e => e.ID == id);
        }
    }
}
