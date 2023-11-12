using Assignment.Models;
using Assignment.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment.Controllers
{
    public class ProductController : Controller
    {
        NorthwindPRN211Context context = new NorthwindPRN211Context();
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ShowProductPage(string selectedCategory, string selectedSort, int page = 1)
        {
            List<string> listCategory = context.Categories.Select(x => x.CategoryName).ToList();
            listCategory.Insert(0, "All categorises");
            ViewBag.listCategory = listCategory;
            if (selectedSort == null)
            {
                selectedSort = "abc";
            }
            ViewBag.selectedSort = selectedSort;
            if (selectedCategory == null)
            {
                List<Product> listProduct = context.Products.Where(x => x.UnitsInStock > 0).ToList();
                int NoOfRecordPerPage = 16;
                int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(listProduct.Count) / Convert.ToDouble(NoOfRecordPerPage)));
                int NoOfSkip = (page - 1) * NoOfRecordPerPage;
                ViewBag.Page = page;
                ViewBag.NoOfPages = NoOfPages;
                ViewBag.listProduct = listProduct.Skip(NoOfSkip).Take(NoOfRecordPerPage);
                ViewBag.selectedCategory = string.Empty;
                return View();
            }
            else
            {
                if (selectedSort.Equals("All"))
                {
                    var listJoin = (from a in context.Products
                                    join b in context.Categories on a.CategoryId equals b.CategoryId
                                    select new
                                    {
                                        a.ProductId,
                                        a.ProductName,
                                        a.UnitPrice,
                                        b.CategoryName,
                                        a.UnitsInStock
                                    }).Where(x => (selectedCategory.Equals("All categorises") ? true : x.CategoryName.Equals(selectedCategory))).Where(x => x.UnitsInStock > 0).ToList();
                    int NoOfRecordPerPage = 16;
                    int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(listJoin.Count) / Convert.ToDouble(NoOfRecordPerPage)));
                    int NoOfSkip = (page - 1) * NoOfRecordPerPage;
                    ViewBag.Page = page;
                    ViewBag.NoOfPages = NoOfPages;
                    ViewBag.listProduct = listJoin.Skip(NoOfSkip).Take(NoOfRecordPerPage);
                    ViewBag.selectedCategory = selectedCategory;
                    return View();
                }
                else
                {
                    var listJoin = (from a in context.Products
                                    join b in context.Categories on a.CategoryId equals b.CategoryId
                                    select new
                                    {
                                        a.ProductId,
                                        a.ProductName,
                                        a.UnitPrice,
                                        b.CategoryName,
                                        a.UnitsInStock
                                    }).Where(x => (selectedCategory.Equals("All categorises") ? true : x.CategoryName.Equals(selectedCategory))).Where(x => x.UnitsInStock > 0).OrderBy(x => selectedSort.Equals("Asc") ? x.UnitPrice : -x.UnitPrice).ToList();
                    int NoOfRecordPerPage = 16;
                    int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(listJoin.Count) / Convert.ToDouble(NoOfRecordPerPage)));
                    int NoOfSkip = (page - 1) * NoOfRecordPerPage;
                    ViewBag.Page = page;
                    ViewBag.NoOfPages = NoOfPages;
                    ViewBag.listProduct = listJoin.Skip(NoOfSkip).Take(NoOfRecordPerPage);
                    ViewBag.selectedCategory = selectedCategory;
                    return View();
                }
            }
        }
        public IActionResult ShowDetail(int idProduct)
        {
            var detailProduct = (from a in context.Products
                                 join b in context.Suppliers on a.SupplierId equals b.SupplierId
                                 select new
                                 {
                                     a.ProductId,
                                     ProductName = a.ProductName,
                                     b.SupplierId,
                                     a.UnitPrice,
                                     a.UnitsInStock,
                                     b.CompanyName
                                 }).FirstOrDefault(x => x.ProductId == idProduct);
            ViewBag.detailProduct = detailProduct;
            return View();
        }
        public IActionResult AddToCart(int idProduct, int quantity, int check, string selectedCategory, string selectedSort)
        {
            if (check == 0) //add luon
            {
                Account account = HttpContext.Session.GetJson<Account>("account");
                int accountId = account.AccountId;
                Cart cartSearch = context.Carts.FirstOrDefault(x => x.AccountId == accountId && x.ProductId == idProduct);
                if (cartSearch == null)
                {
                    Cart cart = new Cart();
                    cart.AccountId = accountId;
                    cart.ProductId = idProduct;
                    cart.Quantity = quantity;
                    context.Carts.Add(cart);
                    context.SaveChanges();
                }
                else
                {
                    cartSearch.Quantity += quantity;
                    context.SaveChanges();
                }
                return RedirectToAction("ShowProductPage", new { selectedCategory = selectedCategory, selectedSort = selectedSort });
            } else //add trong detail
            {
                Account account = HttpContext.Session.GetJson<Account>("account");
                int accountId = account.AccountId;
                Cart cartSearch = context.Carts.FirstOrDefault(x => x.AccountId == accountId && x.ProductId == idProduct);
                if (cartSearch == null)
                {
                    Cart cart = new Cart();
                    cart.AccountId = accountId;
                    cart.ProductId = idProduct;
                    cart.Quantity = quantity;
                    context.Carts.Add(cart);
                    context.SaveChanges();
                }
                else
                {
                    cartSearch.Quantity += quantity;
                    context.SaveChanges();
                }
                return RedirectToAction("ShowDetail", new { idProduct = idProduct });
            }
        }
        public IActionResult ShowCart()
        {
            Account account = HttpContext.Session.GetJson<Account>("account");
            int accountId = account.AccountId;
            var listOfCart = (from a in context.Carts
                              join b in context.Products on a.ProductId equals b.ProductId
                              select new
                              {
                                  a.AccountId,
                                  a.ProductId,
                                  a.Quantity,
                                  b.ProductName,
                                  b.UnitPrice,
                                  b.UnitsInStock
                              }).Where(x => x.AccountId == accountId && x.Quantity > 0).ToList();
            ViewBag.listOfCart = listOfCart;
            decimal? total = 0;
            foreach (var cart in listOfCart)
            {
                total = total + cart.Quantity * cart.UnitPrice;
            }
            ViewBag.total = total;
            return View();
        }
        public IActionResult RemoveInCart(int idProduct)
        {
            Account account = HttpContext.Session.GetJson<Account>("account");
            int accountId = account.AccountId;
            Cart cartSearch = context.Carts.FirstOrDefault(x => x.AccountId == accountId && x.ProductId == idProduct);
            context.Carts.Remove(cartSearch);
            context.SaveChanges();
            return RedirectToAction("ShowCart");
        }
        public IActionResult decreaseInCart(int idProduct)
        {
            Account account = HttpContext.Session.GetJson<Account>("account");
            int accountId = account.AccountId;
            Cart cartSearch = context.Carts.FirstOrDefault(x => x.AccountId == accountId && x.ProductId == idProduct);
            var cartProduct = (from a in context.Carts
                                   join b in context.Products on a.ProductId equals b.ProductId
                                   where b.ProductId == idProduct
                                   select new
                                   {
                                       ProductId = b.ProductId,
                                       b.UnitPrice
                                   });
            decimal unitPrice = Decimal.Parse(cartProduct.Select(x => x.UnitPrice).FirstOrDefault().ToString());
            if (cartSearch.Quantity > 0)
            {
                cartSearch.Quantity--;
                context.SaveChanges();
                decimal? newPrice = cartSearch.Quantity * unitPrice;
                return Ok(new { newPrice = newPrice});
            } else
            {
                return Ok(new { newPrice = 0});
            }
            
        }
        public IActionResult increaseInCart(int idProduct)
        {
            Account account = HttpContext.Session.GetJson<Account>("account");
            int accountId = account.AccountId;
            Cart cartSearch = context.Carts.FirstOrDefault(x => x.AccountId == accountId && x.ProductId == idProduct);
            var cartProduct = (from a in context.Carts
                               join b in context.Products on a.ProductId equals b.ProductId
                               where b.ProductId == idProduct
                               select new
                               {
                                   ProductId = b.ProductId,
                                   b.UnitPrice
                               });
            decimal unitPrice = Decimal.Parse(cartProduct.Select(x => x.UnitPrice).FirstOrDefault().ToString());
            cartSearch.Quantity++;
            context.SaveChanges();
            decimal? newPrice = cartSearch.Quantity * unitPrice;
            return Ok(new { newPrice = newPrice });
        }
        public IActionResult CheckPurchase()
        {
            var cartProduct = (from a in context.Carts
                               join b in context.Products on a.ProductId equals b.ProductId
                               select new
                               {
                                   a.ProductId,
                                   a.Quantity,
                                   b.UnitsInStock
                               }).ToList();
            bool check = false;
            if (cartProduct.Count == 0)
            {
                check = true;
            }
            foreach (var c in cartProduct)
            {
                if (c.Quantity > c.UnitsInStock || c.Quantity <= 0) //neu khong hop le
                {
                    Cart cart = context.Carts.FirstOrDefault(x => x.ProductId == c.ProductId);
                    context.Carts.Remove(cart);
                    context.SaveChanges();
                    check = true;
                }
            }
            if (check) //neu khong hop le
            {
                Account account = HttpContext.Session.GetJson<Account>("account");
                int accountId = account.AccountId;
                var listOfCart = (from a in context.Carts
                                  join b in context.Products on a.ProductId equals b.ProductId
                                  select new
                                  {
                                      a.AccountId,
                                      a.ProductId,
                                      a.Quantity,
                                      b.ProductName,
                                      b.UnitPrice,
                                      b.UnitsInStock
                                  }).Where(x => x.AccountId == accountId && x.Quantity > 0).ToList();
                ViewBag.listOfCart = listOfCart;
                decimal? total = 0;
                foreach (var cart in listOfCart)
                {
                    total = total + cart.Quantity * cart.UnitPrice;
                }
                ViewBag.total = total;
                ViewBag.checkPop = "a";
                return View("ShowCart");
            } else
            {
                return RedirectToAction("Purchase");
            }
        }
        public IActionResult Purchase()
        {
            Account account = HttpContext.Session.GetJson<Account>("account");
            string customerId = account.CustomerId;
            int accountId = account.AccountId;
            var listOfCart = (from a in context.Carts
                              join b in context.Products on a.ProductId equals b.ProductId
                              select new
                              {
                                  a.AccountId,
                                  a.ProductId,
                                  a.Quantity,
                                  b.ProductName,
                                  b.UnitPrice,
                                  b.UnitsInStock
                              }).Where(x => x.AccountId == accountId && x.Quantity > 0).ToList();
            decimal? total = 0;
            foreach (var c in listOfCart)
            {
                total = total + c.Quantity * c.UnitPrice;
            }
            ViewBag.Total = total;
            //Add vao Order
            var listInfoCustomer = (from a in context.Accounts
                                    join b in context.Customers on a.CustomerId equals b.CustomerId
                                    where a.CustomerId.Equals(customerId)
                                    select new
                                    {
                                        b.CompanyName,
                                        b.Address
                                    }).ToList();
            string customerName = listInfoCustomer.Select(x => x.CompanyName).FirstOrDefault();
            string customerAddress = listInfoCustomer.Select(x => x.Address).FirstOrDefault();
            ViewBag.Customer = customerName;
            ViewBag.Address = customerAddress;
            Order order = new Order();
            order.CustomerId = customerId;
            order.ShipName = customerName;
            order.ShipAddress = customerAddress;
            order.Status = 0;
            order.OrderDate = DateTime.Now;
            context.Orders.Add(order);
            context.SaveChanges();
            //Add vao Order Detail
            int orderID = context.Orders.OrderByDescending(x => x.OrderId).Select(x => x.OrderId).FirstOrDefault();
            var cartProduct = (from a in context.Carts
                               join b in context.Products on a.ProductId equals b.ProductId
                               where a.AccountId == accountId
                               select new
                               {
                                   a.AccountId,
                                   a.ProductId,
                                   a.Quantity,
                                   b.UnitPrice
                               }).ToList();
            foreach (var product in cartProduct)
            {
                OrderDetail detail = new OrderDetail();
                detail.OrderId = orderID;
                detail.ProductId = (int)product.ProductId;
                detail.UnitPrice = (decimal)product.UnitPrice;
                detail.Quantity = (short)product.Quantity;
                detail.Discount = 0;
                context.OrderDetails.Add(detail);
                context.SaveChanges();
            }
            //Giam quantity trong bang Product
            List<Cart> cart = context.Carts.ToList();
            foreach (var item in cart)
            {
                Product product = context.Products.FirstOrDefault(x => x.ProductId == item.ProductId);
                product.UnitsInStock -= short.Parse(item.Quantity.ToString());
                context.SaveChanges();
            }
            //Xoa Product trong cart
            List<Cart> listRemove = context.Carts.Where(x => x.AccountId == accountId).ToList();
            context.Carts.RemoveRange(listRemove);
            context.SaveChanges();
            return View();
        }
        public IActionResult OrderTracking()
        {
            Account account = HttpContext.Session.GetJson<Account>("account");
            int accountId = account.AccountId;
            var listOrder = (from a in context.Orders
                             join b in context.Accounts on a.CustomerId equals b.CustomerId
                             where b.AccountId == accountId
                             select new
                             {
                                 a.OrderId,
                                 a.ShipName,
                                 a.ShipAddress,
                                 a.OrderDate,
                                 a.Status
                             }).ToList();
            ViewBag.listOrder = listOrder;
            return View();
        }
        public IActionResult ViewOrderDetail(int orderIdToShow)
        {
            var listOrderDetail = (from a in context.OrderDetails
                                   join b in context.Products on a.ProductId equals b.ProductId
                                   where a.OrderId == orderIdToShow
                                   select new
                                   {
                                       b.ProductName,
                                       a.Quantity,
                                       b.UnitPrice
                                   }).ToList();
            ViewBag.listOrderDetail = listOrderDetail;
            decimal? total = 0;
            foreach (var c in listOrderDetail)
            {
                total = total + c.Quantity * c.UnitPrice;
            }
            ViewBag.Total = total;

            return View();
        }
    }
}
