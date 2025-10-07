using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using CosmeticShopWeb.Models;

public class CartController : Controller
{
    private const string CartSessionKey = "CartSession";

    public IActionResult Index()
    {
        var cart = GetCart();
        return View(cart);
    }

    [HttpPost]
    public IActionResult Add(int productId, string name, decimal price, string imageUrl, int quantity = 1)
    {
        try
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItemViewModel
                {
                    ProductId = productId,
                    Name = name,
                    Price = price,
                    ImageUrl = imageUrl, 
                    Quantity = quantity
                });
            }

            SaveCart(cart);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, cartCount = cart.TotalItems });
            }

            return RedirectToAction("Index", "Cart");
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, error = ex.Message });
            }

            return RedirectToAction("Index", "Products");
        }
    }

    [HttpPost]
    public IActionResult Update(int productId, int quantity)
    {
        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Items.RemoveAll(i => i.ProductId == productId);
            }
            else
            {
                item.Quantity = quantity;
            }
        }
        SaveCart(cart);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new
            {
                success = true,
                total = cart.TotalPrice,
                cartCount = cart.TotalItems 
            });
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Remove(int productId)
    {
        var cart = GetCart();
        cart.Items.RemoveAll(i => i.ProductId == productId);
        SaveCart(cart);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new
            {
                success = true,
                total = cart.TotalPrice,
                cartCount = cart.TotalItems
            });
        }

        return RedirectToAction("Index");
    }

    [HttpPost] 
    public IActionResult Clear()
    {
        SaveCart(new CartViewModel());

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true });
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult GetCartCount()
    {
        var cart = GetCart();
        return Json(new { count = cart.TotalItems });
    }

    private CartViewModel GetCart()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        return json == null
            ? new CartViewModel()
            : JsonConvert.DeserializeObject<CartViewModel>(json);
    }

    private void SaveCart(CartViewModel cart)
    {
        var json = JsonConvert.SerializeObject(cart);
        HttpContext.Session.SetString(CartSessionKey, json);
    }
}