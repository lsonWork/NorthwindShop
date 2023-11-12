using Assignment.Models;
using Assignment.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Assignment.Controllers
{
    public class LoginController : Controller
    {
        NorthwindPRN211Context context = new NorthwindPRN211Context();
        public IActionResult Index(int check)
        {
            if (check == 1)
            {
                ViewBag.msgError ="Login failed!";
            }
            return View();
        }
        public IActionResult Login(string username, string password)
        {
            List<Account> listAccount = context.Accounts.Where(x => x.Username == username && x.Password == password).ToList();
            if (listAccount.Count == 0)
            {
                return RedirectToAction("Index", new {check = 1});
            }
            else
            {
                HttpContext.Session.SetJson("account", listAccount[0]);
                if (listAccount[0].Role == 1)
                {
                    return RedirectToAction("ListAccount", "Account");
                }
                else if (listAccount[0].Role == 2)
                {
                    return RedirectToAction("ShowProductPage", "Product");
                } else
                {
                    return RedirectToAction("ShowOrders", "Employee");
                }
            }
        }
    }
}
