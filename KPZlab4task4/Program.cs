using System;
using System.Text;

namespace StrategyPatternDemo
{
    // --- 1. ІНТЕРФЕЙС СТРАТЕГІЇ (Strategy) ---
    // Визначає загальний контракт для всіх алгоритмів завантаження.
    public interface IImageLoadStrategy
    {
        void Load(string href);
    }

    // --- 2. КОНКРЕТНІ СТРАТЕГІЇ (Concrete Strategies) ---

    // Стратегія 1: Завантаження з мережі
    public class NetworkImageLoader : IImageLoadStrategy
    {
        public void Load(string href)
        {
            Console.WriteLine($"[NetworkStrategy]: Встановлення з'єднання з сервером...");
            Console.WriteLine($"[NetworkStrategy]: Завантаження зображення по URL: {href} - УСПІШНО.\n");
        }
    }

    // Стратегія 2: Завантаження з локальної файлової системи
    public class FileSystemImageLoader : IImageLoadStrategy
    {
        public void Load(string href)
        {
            Console.WriteLine($"[FileSystemStrategy]: Пошук файлу на локальному диску...");
            Console.WriteLine($"[FileSystemStrategy]: Читання файлу: {href} - УСПІШНО.\n");
        }
    }

    // --- 3. БАЗОВІ КЛАСИ LIGHT HTML ---
    public abstract class LightNode
    {
        public abstract string OuterHTML { get; }
        public abstract string InnerHTML { get; }
    }

    // --- 4. КОНТЕКСТ (Context) ---
    // Клас, який використовує стратегію. Він не знає, ЯК саме вантажиться картинка.
    public class LightImageNode : LightNode
    {
        private string _href;
        private IImageLoadStrategy _loadStrategy;

        public LightImageNode(string href)
        {
            _href = href ?? string.Empty;

            // Розумний вибір початкової стратегії на основі шляху
            if (_href.StartsWith("http://") || _href.StartsWith("https://"))
            {
                _loadStrategy = new NetworkImageLoader();
            }
            else
            {
                _loadStrategy = new FileSystemImageLoader();
            }
        }

        // Можливість змінити стратегію динамічно (головна фішка патерна)
        public void SetLoadStrategy(IImageLoadStrategy strategy)
        {
            _loadStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        // Делегування роботи обраній стратегії
        public void LoadImage()
        {
            _loadStrategy.Load(_href);
        }

        // Рендеринг тегу
        public override string OuterHTML => $"<img src=\"{_href}\" />";
        public override string InnerHTML => string.Empty; // У <img> немає внутрішнього HTML
    }

    // --- 5. ГОЛОВНИЙ МЕТОД ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== LightHTML: Завантаження зображень (Шаблон Стратегія) ===\n");

            // 1. Створюємо картинку з мережі (URL)
            Console.WriteLine("--- Сценарій 1: Зображення з мережі ---");
            LightImageNode networkImage = new LightImageNode("https://example.com/images/logo.png");
            Console.WriteLine($"Рендер: {networkImage.OuterHTML}");
            networkImage.LoadImage(); // Відпрацює NetworkImageLoader

            // 2. Створюємо картинку з локального диску
            Console.WriteLine("--- Сценарій 2: Локальне зображення ---");
            LightImageNode localImage = new LightImageNode("C:/Users/Public/Pictures/avatar.jpg");
            Console.WriteLine($"Рендер: {localImage.OuterHTML}");
            localImage.LoadImage(); // Відпрацює FileSystemImageLoader

            Console.ReadLine();
        }
    }
}