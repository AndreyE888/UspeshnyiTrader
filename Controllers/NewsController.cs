using Microsoft.AspNetCore.Mvc;
using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Controllers
{
    public class NewsController : Controller
    {
        // Временные данные для новостей (потом заменим на БД)
        private readonly List<NewsItem> _news = new()
        {
            new NewsItem 
            { 
                Id = 1, 
                Title = "Запуск новой торговой платформы", 
                Content = "Представляем обновленный интерфейс с улучшенной производительностью и новыми функциями для трейдеров.",
                PublishDate = new DateTime(2024, 12, 15),
                ImageUrl = "/images/news1.jpg"
            },
            new NewsItem 
            { 
                Id = 2, 
                Title = "Добавлены новые торговые инструменты", 
                Content = "Расширяем список доступных активов для диверсификации портфеля. Теперь доступно более 50 инструментов.",
                PublishDate = new DateTime(2024, 12, 10),
                ImageUrl = "/images/news2.jpg"
            },
            new NewsItem 
            { 
                Id = 3, 
                Title = "Обновление мобильного приложения", 
                Content = "Новая версия приложения с улучшенным UX/UI дизайном и повышенной стабильностью работы.",
                PublishDate = new DateTime(2024, 12, 5),
                ImageUrl = "/images/news3.jpg"
            }
        };

        public IActionResult Index()
        {
            ViewBag.News = _news;
            return View();
        }

        public IActionResult Details(int id)
        {
            var newsItem = _news.FirstOrDefault(n => n.Id == id);
            if (newsItem == null)
            {
                return NotFound();
            }
            return View(newsItem);
        }
    }

    // Временная модель для новостей
    public class NewsItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public string ImageUrl { get; set; }
    }
}