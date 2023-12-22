using AmazonCloneMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AmazonCloneMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyDbContext _context;

        public HomeController(ILogger<HomeController> logger, MyDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index([Bind("searchString")] string searchString)
        {
            IEnumerable<Produit> myDbContext;
            if (!string.IsNullOrEmpty(searchString))
            {
                _logger.LogInformation("Logger!!! it sounds like someone is searchin a product that contains: {searchString}", searchString);
                myDbContext = await _context.Produits.Include(p => p.Categorie).Where(p => p.Description.Contains(searchString)).ToListAsync();
                if (!myDbContext.Any())
                {
                    _logger.LogWarning("Logger!!! No product contains the string: {searchString}", searchString);
                
                }
            }
            else {
                _logger.LogInformation("Logger!!! Someone has visited the home page at {DT}", DateTime.UtcNow.ToLongTimeString());
                myDbContext = await _context.Produits.Include(p => p.Categorie).ToListAsync(); 
            }

            return View(myDbContext);
        }
        public async Task<IActionResult> AddToCard([Bind("ProduitId")] int ProduitId)
        {
            var product = await _context.Produits.FindAsync(ProduitId);


            if (product != null)
            {
                _logger.LogInformation("Logger!!! Someone added a product to card at: {DT}", DateTime.UtcNow.ToLongTimeString());

                var retrievedCartService = HttpContext.Session.GetCartService();

                retrievedCartService._cartItems.Add(product);
                HttpContext.Session.SetCartService(retrievedCartService);
            }

            return RedirectToAction(nameof(Index));

        }
        public IEnumerable<Produit> CartProduit;
        public IActionResult Cart()
        {
            var retrievedCartService = HttpContext.Session.GetCartService();
            if (retrievedCartService != null)
            {
                CartProduit = retrievedCartService._cartItems;
            }
            return View(CartProduit);
        }

        public IActionResult RemoveProd([Bind("prodId")] int prodId)
        {
            var retrievedCartService = HttpContext.Session.GetCartService();
            if (retrievedCartService != null)
            {
                retrievedCartService._cartItems.Remove(retrievedCartService._cartItems.Where(pro => pro.ProduitID == prodId).First());
                HttpContext.Session.SetCartService(retrievedCartService);

            }
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}