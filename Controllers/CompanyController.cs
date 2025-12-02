using Microsoft.AspNetCore.Mvc;

namespace UspeshnyiTrader.Controllers
{
    public class CompanyController : Controller
    {
        public IActionResult About()
        {
            ViewBag.CompanyInfo = new
            {
                Name = "UspeshnyiTrader",
                Founder = "Ефимов Андрей",
                Established = 2024,
                Description = "Ведущая торговая платформа для профессиональных трейдеров и начинающих инвесторов.",
                Mission = "Сделать торговлю доступной и успешной для каждого.",
                Address = "г. Москва, ул. Торговая, д. 123",
                Phone = "+7 (999) 123-45-67",
                Email = "info@uspeshnyitrader.ru"
            };
            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.ContactInfo = new
            {
                Address = "г. Москва, ул. Торговая, д. 123",
                Phone = "+7 (999) 123-45-67",
                Email = "info@uspeshnyitrader.ru",
                WorkHours = "Пн-Пт: 9:00-18:00, Сб-Вс: 10:00-16:00"
            };
            return View();
        }

        public IActionResult Events()
        {
            var events = new[]
            {
                new { Date = "25.12.2024", Title = "Рождественский турнир трейдеров", Description = "Призы для лучших трейдеров месяца" },
                new { Date = "01.01.2025", Title = "Новогодний бонус", Description = "Специальные условия для новых клиентов" },
                new { Date = "15.01.2025", Title = "Вебинар: Стратегии торговли", Description = "Бесплатный обучающий вебинар" }
            };
            
            ViewBag.Events = events;
            return View();
        }

        [HttpPost]
        public IActionResult SendMessage(string name, string email, string message)
        {
            // Здесь будет логика отправки сообщения
            TempData["SuccessMessage"] = "Ваше сообщение успешно отправлено!";
            return RedirectToAction("Contact");
        }
    }
}