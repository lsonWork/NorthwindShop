using Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class AccountController : Controller
    {
        NorthwindPRN211Context context = new NorthwindPRN211Context();
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ListAccount()
        {
            List<Account> listAccount = context.Accounts.ToList();
            ViewBag.listAccount = listAccount;
            return View();
        }
        public IActionResult DoAdd()
        {
            List<string?> listAssign = context.Accounts.Select(x => x.CustomerId).ToList();
            List<string> listCustomerID = context.Customers.Where(x => !listAssign.Contains(x.CustomerId)).Select(x => x.CustomerId).ToList();

            List<int?> listAssignEm = context.Accounts.Select(x => x.EmployeeId).ToList();
            List<int> listEmployeeID = context.Employees.Where(x => !listAssignEm.Contains(x.EmployeeId)).Select(x => x.EmployeeId).ToList();
            //List<string> listCustomerID = context.Customers.Where(x => !listAssign.Contains(x.CustomerId)).Select(x => x.CustomerId).ToList();
            ViewBag.listCustomerID = listCustomerID;
            ViewBag.listEmployeeID = listEmployeeID;
            string msgUsernameC = TempData["msgUsernameC"] as string;
            if (msgUsernameC != null)
            {
                ViewBag.msgUsernameC = msgUsernameC;
                TempData.Remove("msgUsernameC");
            }
            string usernameEnterC = TempData["usernameEnterC"] as string;
            if (usernameEnterC != null)
            {
                ViewBag.usernameEnterC = usernameEnterC;
                TempData.Remove("usernameEnterC");
            }
            string passwordEnterC = TempData["passwordEnterC"] as string;
            if (passwordEnterC != null)
            {
                ViewBag.passwordEnterC = passwordEnterC;
                TempData.Remove("passwordEnterC");
            }
            string msgUsernameE = TempData["msgUsernameE"] as string;
            if (msgUsernameE != null)
            {
                ViewBag.msgUsernameE = msgUsernameE;
                TempData.Remove("msgUsernameE");
            }
            string usernameEnterE = TempData["usernameEnterE"] as string;
            if (usernameEnterE != null)
            {
                ViewBag.usernameEnterE = usernameEnterE;
                TempData.Remove("usernameEnterE");
            }
            string passwordEnterE = TempData["passwordEnterE"] as string;
            if (passwordEnterE != null)
            {
                ViewBag.passwordEnterE = passwordEnterE;
                TempData.Remove("passwordEnterE");
            }
            return View();
        }
        public Boolean checkExistUsername(string username)
        {
            List<Account> list = context.Accounts.Where(x => x.Username == username).ToList();
            return list.Count > 0; //neu co roi thi true
        }
        public IActionResult AddCustomer(string usernameCustomer, string passwordCustomer, string customerID)
        {
            if (checkExistUsername(usernameCustomer))
            {
                TempData["msgUsernameC"] = "Username was existed";
                TempData["usernameEnterC"] = usernameCustomer;
                TempData["passwordEnterC"] = passwordCustomer;
                return RedirectToAction("DoAdd");
            }
            else
            {
                Account account = new Account();
                account.CustomerId = customerID;
                account.Username = usernameCustomer;
                account.Password = passwordCustomer;
                account.Role = 2;
                context.Accounts.Add(account);
                context.SaveChanges();
                return RedirectToAction("ListAccount");
            }
        }
        public IActionResult AddEmployee(string usernameEmployee, string passwordEmployee, int employeeID)
        {
            if (checkExistUsername(usernameEmployee))
            {
                TempData["msgUsernameE"] = "Username was existed";
                TempData["usernameEnterE"] = usernameEmployee;
                TempData["passwordEnterE"] = passwordEmployee;
                return RedirectToAction("DoAdd");
            }
            else
            {
                Account account = new Account();
                account.EmployeeId = employeeID;
                account.Username = usernameEmployee;
                account.Password = passwordEmployee;
                account.Role = 3;
                context.Accounts.Add(account);
                context.SaveChanges();
                return RedirectToAction("ListAccount");
            }
        }
        [HttpPost]
        public IActionResult Delete(string idDelete)
        {
            bool success = int.TryParse(idDelete, out int number);
            if (success)
            {
                context.Database.ExecuteSqlRaw($"DELETE FROM Account WHERE EmployeeID = {number}");
            } else
            {
                context.Database.ExecuteSqlRaw($"DELETE FROM Account WHERE CustomerID = '{idDelete}'");
            }
            return RedirectToAction("ListAccount");
        }
        [HttpPost]
        public IActionResult DoUpdate(string idUpdate)
        {
            bool idParse = int.TryParse(idUpdate, out int number);
            Account account;
            if (idParse)
            {
                account = context.Accounts.FirstOrDefault(x => x.EmployeeId == number);
            }
            else
            {
                account = context.Accounts.FirstOrDefault(x => x.CustomerId.Equals(idUpdate));
            }
            ViewBag.oldInfo = account;
            return View();
        }
        [HttpPost]
        public IActionResult Update(string idUpdate, string newPass)
        {
            bool idParse = int.TryParse(idUpdate, out int number);
            Account account;
            if (idParse)
            {
                account = context.Accounts.FirstOrDefault(x => x.EmployeeId == number);
            }
            else
            {
                account = context.Accounts.FirstOrDefault(x => x.CustomerId.Equals(idUpdate));
            }
            account.Password = newPass;
            context.SaveChanges();
            return RedirectToAction("ListAccount");
        }
    }
}
