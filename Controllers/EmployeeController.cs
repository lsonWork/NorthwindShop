using Assignment.Models;
using Assignment.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class EmployeeController : Controller
    {
        NorthwindPRN211Context context = new NorthwindPRN211Context();
        public IActionResult ShowOrders()
        {
            var listProcessingOrder = (from a in context.Orders
                                       join b in context.Accounts on a.CustomerId equals b.CustomerId
                                       where a.Status == 0
                                       select new
                                       {
                                           a.OrderId,
                                           a.CustomerId,
                                           b.Username,
                                           a.OrderDate,
                                           a.ShipName,
                                           a.ShipAddress,
                                           a.Status
                                       }).ToList();
            ViewBag.listProcessingOrder = listProcessingOrder;
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
        public IActionResult SetStatus(int orderId)
        {
            Account account = HttpContext.Session.GetJson<Account>("account");
            int? employeeId = account.EmployeeId;
            Order selectedOrder = context.Orders.Where(x => x.OrderId == orderId).FirstOrDefault();
            selectedOrder.Status = 1;
            selectedOrder.EmployeeId = employeeId;
            context.SaveChanges();
            return Ok(new {newStatus = "Approved!"});
        }
        public IActionResult ShowProductList(int check)
        {
            var listProductInSystem = context.Products.Include("Supplier").ToList();
            ViewBag.listProductInSystem = listProductInSystem;
            if (check == 1)
            {
                ViewBag.checker = 1;
            }
            if (check == 2)
            {
                ViewBag.checker = 2;
            }
            if (check == 3)
            {
                ViewBag.checker = 3;
            }
            return View("ProductManagement");
        }
        public IActionResult AddProduct()
        {
            List<Supplier> listSupplier = context.Suppliers.ToList();
            List<Category> listCategory = context.Categories.ToList();
            ViewBag.listSupplier = listSupplier;
            ViewBag.listCategory = listCategory;
            return View();
        }
        public IActionResult DoAdd(string addName, int addCategory, short addQuantity, decimal addPrice, int addSupplier)
        {
            Product obj = context.Products.Where(x => x.ProductName.Equals(addName) && x.SupplierId == addSupplier).FirstOrDefault();
            if (obj == null)
            {
                Product addProduct = new Product();
                addProduct.ProductName = addName;
                addProduct.SupplierId = addSupplier;
                addProduct.CategoryId = addCategory;
                addProduct.UnitPrice = addPrice;
                addProduct.UnitsInStock = addQuantity;
                context.Products.Add(addProduct);
                context.SaveChanges();
                return RedirectToAction("ShowProductList", new {check = 1});
            } else
            {
                List<Supplier> listSupplier = context.Suppliers.ToList();
                List<Category> listCategory = context.Categories.ToList();
                ViewBag.listSupplier = listSupplier;
                ViewBag.listCategory = listCategory;
                ViewBag.addNameR = addName;
                ViewBag.addCategoryR = addCategory;
                ViewBag.addQuantityR = addQuantity;
                ViewBag.addPriceR = addPrice;
                ViewBag.addSupplierR = addSupplier;
                ViewBag.checkAdd = "fail";
                return View("AddProduct");
            }
        }
        public IActionResult DeleteProduct(int productIdToDelete)
        {
            Product product = context.Products.Where(x => x.ProductId == productIdToDelete).FirstOrDefault();
            List<OrderDetail> listOrderDetail = context.OrderDetails.Where(x => x.ProductId == productIdToDelete).ToList();
            List<Cart> listCart = context.Carts.Where(x => x.ProductId == productIdToDelete).ToList();
            context.OrderDetails.RemoveRange(listOrderDetail);
            context.Carts.RemoveRange(listCart);
            context.Products.Remove(product);
            context.SaveChanges();
            return RedirectToAction("ShowProductList", new { check = 2 });
        }
        public IActionResult UpdateProduct(int productIdToUpdate, int check)
        {
            Product productUpdate = context.Products.Include(x => x.Category).Include(x => x.Supplier).Where(x => x.ProductId == productIdToUpdate).FirstOrDefault();
            ViewBag.productUpdate = productUpdate;
            if (check == 1)
            {
                ViewBag.checker = 1;
            }
            return View();
        }
        public IActionResult DoUpdate(int productIdToUpdate, string updateName, decimal updatePrice, short updateQuantity, int updateSupplier)
        {
            Product product = context.Products.Where(x => x.ProductId == productIdToUpdate).FirstOrDefault();
            if (!updateName.Equals(product.ProductName))
            {
                List<Product> listDuplicate = context.Products.Where(x => x.ProductName.Equals(updateName) && x.SupplierId == updateSupplier).ToList();
                if (listDuplicate.Count > 0)
                {
                    return RedirectToAction("UpdateProduct", new { productIdToUpdate = productIdToUpdate, check = 1 });
                }
                else
                {
                    Product updateProduct = context.Products.Where(x => x.ProductId == productIdToUpdate).FirstOrDefault();
                    updateProduct.ProductName = updateName;
                    updateProduct.UnitPrice = updatePrice;
                    updateProduct.UnitsInStock = updateQuantity;
                    context.SaveChanges();
                    return RedirectToAction("ShowProductList", new { check = 3 });
                }
            } else
            {
                Product updateProduct = context.Products.Where(x => x.ProductId == productIdToUpdate).FirstOrDefault();
                updateProduct.ProductName = updateName;
                updateProduct.UnitPrice = updatePrice;
                updateProduct.UnitsInStock = updateQuantity;
                context.SaveChanges();
                return RedirectToAction("ShowProductList", new { check = 3 });
            }
        }
        public IActionResult ViewStatistic()
        {
            List<Order> listOrder = context.Orders.ToList();
            ViewBag.allCreatedOrder = listOrder.Count;
            var listOrderVsDetail = (from a in context.Orders
                                     join b in context.OrderDetails on a.OrderId equals b.OrderId
                                     where a.Status != 0
                                     select new
                                     {
                                         b.UnitPrice,
                                         b.Quantity
                                     }).ToList();
            decimal? total = 0;
            foreach (var order in listOrderVsDetail)
            {
                total += (order.UnitPrice * order.Quantity);
            }
            ViewBag.revenue = total;

            var listStatistic = (from a in context.Orders
                                 join b in context.OrderDetails on a.OrderId equals b.OrderId
                                 join c in context.Products on b.ProductId equals c.ProductId
                                 group b by c.ProductName into grouped
                                 select new
                                 {
                                     ProductName = grouped.Key,
                                     TotalQuantity = grouped.Sum(od => od.Quantity)
                                 }).ToList();
            ViewBag.listStatistic = listStatistic;
            return View();
        }
        public IActionResult ShowOrdersApproved()
        {
            Account account = HttpContext.Session.GetJson<Account>("account");
            int? employeeId = account.EmployeeId;
            var listApproved = (from a in context.Orders
                                       join b in context.Accounts on a.EmployeeId equals b.EmployeeId
                                       where b.EmployeeId == employeeId
                                       select new
                                       {
                                           a.OrderId,
                                           a.CustomerId,
                                           b.Username,
                                           a.OrderDate,
                                           a.ShipName,
                                           a.ShipAddress,
                                           a.Status
                                       }).ToList();
            ViewBag.listApproved = listApproved;
            return View();
        }
    }
}
