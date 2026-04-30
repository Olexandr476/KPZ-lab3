using System;
using System.IO;

namespace AdapterPatternDemo
{
    // --- 1. Цільовий інтерфейс (Target) ---
    // Усі логери в нашій системі повинні дотримуватися цього контракту.
    public interface ILogger
    {
        void Log(string message);
        void Error(string message);
        void Warn(string message);
    }

    // --- 2. Стандартна реалізація (Console Logger) ---
    public class Logger : ILogger
    {
        public void Log(string message)
        {
            WriteWithColor($"[LOG]: {message}", ConsoleColor.Green);
        }

        public void Error(string message)
        {
            WriteWithColor($"[ERROR]: {message}", ConsoleColor.Red);
        }

        public void Warn(string message)
        {
            // У консолі немає ідеально оранжевого, тому використовуємо DarkYellow
            WriteWithColor($"[WARN]: {message}", ConsoleColor.DarkYellow);
        }

        // Інкапсулюємо логіку зміни кольору, щоб уникнути дублювання коду (DRY)
        private void WriteWithColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }

    // --- 3. Клас, який потрібно адаптувати (Adaptee) ---
    // Він нічого не знає про логування, він просто вміє писати у файл.
    public class FileWriter
    {
        private readonly string _filePath;

        public FileWriter(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Шлях до файлу не може бути порожнім.");

            _filePath = filePath;
        }

        public void Write(string text)
        {
            File.AppendAllText(_filePath, text);
        }

        public void WriteLine(string text)
        {
            File.AppendAllText(_filePath, text + Environment.NewLine);
        }
    }

    // --- 4. Адаптер (Adapter) ---
    // Робить так, щоб FileWriter виглядав для системи як ILogger.
    public class FileLoggerAdapter : ILogger
    {
        private readonly FileWriter _fileWriter;

        // Використовуємо ін'єкцію залежностей через конструктор (Dependency Injection)
        public FileLoggerAdapter(FileWriter fileWriter)
        {
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        }

        public void Log(string message)
        {
            _fileWriter.WriteLine($"[LOG - {DateTime.Now}]: {message}");
        }

        public void Error(string message)
        {
            _fileWriter.WriteLine($"[ERROR - {DateTime.Now}]: {message}");
        }

        public void Warn(string message)
        {
            _fileWriter.WriteLine($"[WARN - {DateTime.Now}]: {message}");
        }
    }

    // --- 5. Головний метод програми ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("--- Демонстрація шаблону Адаптер ---\n");

            // 1. Використовуємо звичайний консольний логер
            Console.WriteLine("Робота консольного логера:");
            ILogger consoleLogger = new Logger();
            consoleLogger.Log("Система успішно запущена.");
            consoleLogger.Warn("Пам'ять заповнена на 80%.");
            consoleLogger.Error("Критична помилка з'єднання з базою даних!\n");

            // 2. Використовуємо файловий логер через адаптер
            Console.WriteLine("Робота файлового логера (перевірте файл log.txt у папці проєкту)...");

            FileWriter writer = new FileWriter("log.txt");
            ILogger fileLogger = new FileLoggerAdapter(writer); // Адаптуємо FileWriter

            // Клієнтський код працює з fileLogger так само, як і з consoleLogger
            fileLogger.Log("Запис у файл: система ініціалізована.");
            fileLogger.Warn("Запис у файл: виявлено підозрілу активність.");
            fileLogger.Error("Запис у файл: не вдалося зберегти налаштування користувача.");

            Console.WriteLine("Логи успішно записані у файл log.txt!");
            Console.ReadLine();
        }
    }
}