using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace BestStoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            var products = this.context.Products.ToList();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto, IFormFile ImageFile)
        {
            // Validate the ImageFile is provided
            if (ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The Image File is Required");
            }

            // Check for any validation errors
            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            // Generate a unique file name based on current timestamp
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(ImageFile.FileName);

            // Define the file path to save the image in the 'wwwroot/products' directory
            string imageFullPath = Path.Combine(environment.WebRootPath, "products", newFileName);

            // Save the uploaded image to the server
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                ImageFile.CopyTo(stream);
            }

            // Create a new Product entity to save to the database
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName, // Save the generated image file name
                CreatedAt = DateTime.Now
            };

            // Add the new product to the database
            context.Products.Add(product);
            context.SaveChanges();

            // Redirect to the Index action to display the list of products
            return RedirectToAction("Index", "Products");
        }
    }
}
