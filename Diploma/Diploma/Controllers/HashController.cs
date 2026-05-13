using BCrypt;
using System.Web.Mvc;

namespace Diploma.Controllers
{
    public class HashController : Controller
    {
        public ActionResult Generate()
        {
            string password = "admin123";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            return Content($"<h2>Hash for password '{password}'</h2>" +
                          $"<p><strong>Hash:</strong> {hash}</p>" +
                          $"<p><strong>SQL to update:</strong></p>" +
                          $"<pre>UPDATE \"Users\" SET \"PasswordHash\" = '{hash}' WHERE \"UserName\" = 'admin';</pre>");
        }
    }
}