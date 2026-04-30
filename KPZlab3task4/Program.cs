using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ProxyPatternDemo
{
    // --- 1. СПІЛЬНИЙ ІНТЕРФЕЙС (Subject) ---
    // Визначає контракт для читання файлів.
    public interface ITextReader
    {
        char[][] ReadText(string filePath);
    }

    // --- 2. РЕАЛЬНИЙ ОБ'ЄКТ (RealSubject) ---
    // Виконує основну, важку роботу — безпосередньо читає файл.
    public class SmartTextReader : ITextReader
    {
        public char[][] ReadText(string filePath)
        {
            // Метод File.ReadAllLines автоматично відкриває, читає та закриває файл
            string[] lines = File.ReadAllLines(filePath);

            // Створюємо двомірний масив (масив масивів символів)
            char[][] result = new char[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                result[i] = lines[i].ToCharArray(); // Рядок розбивається на символи
            }

            return result;
        }
    }

    // --- 3. ПРОКСІ ДЛЯ ЛОГУВАННЯ (Logging Proxy) ---
    // Перехоплює виклик, логує дані та передає управління реальному об'єкту.
    public class SmartTextChecker : ITextReader
    {
        private readonly ITextReader _realReader;

        public SmartTextChecker(ITextReader realReader)
        {
            _realReader = realReader ?? throw new ArgumentNullException(nameof(realReader));
        }

        public char[][] ReadText(string filePath)
        {
            Console.WriteLine($"\n[Checker] Відкриття файлу: {filePath}...");

            // Передаємо запит реальному об'єкту
            char[][] result = _realReader.ReadText(filePath);

            Console.WriteLine($"[Checker] Файл успішно прочитано.");
            Console.WriteLine($"[Checker] Закриття файлу: {filePath}");

            // Рахуємо статистику
            int totalLines = result.Length;
            int totalChars = 0;
            foreach (var line in result)
            {
                totalChars += line.Length;
            }

            Console.WriteLine($"[Checker] Статистика: рядків - {totalLines}, символів - {totalChars}");

            return result;
        }
    }

    // --- 4. ПРОКСІ ДЛЯ ОБМЕЖЕННЯ ДОСТУПУ (Protection Proxy) ---
    // Контролює, чи має клієнт право читати певний файл.
    public class SmartTextReaderLocker : ITextReader
    {
        private readonly ITextReader _realReader;
        private readonly Regex _restrictedPattern;

        // Конструктор приймає регулярний вираз для блокування певних імен файлів
        public SmartTextReaderLocker(ITextReader realReader, string regexPattern)
        {
            _realReader = realReader ?? throw new ArgumentNullException(nameof(realReader));
            _restrictedPattern = new Regex(regexPattern, RegexOptions.IgnoreCase);
        }

        public char[][] ReadText(string filePath)
        {
            // Перевіряємо ім'я файлу на відповідність забороненому шаблону
            if (_restrictedPattern.IsMatch(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[Locker] Спроба доступу до {filePath}");
                Console.WriteLine("Access denied!");
                Console.ResetColor();

                return Array.Empty<char[]>(); // Повертаємо порожній масив замість null для безпеки (Fail Safe)
            }

            // Якщо доступ дозволено, передаємо запит далі
            return _realReader.ReadText(filePath);
        }
    }

    // --- 5. ГОЛОВНИЙ МЕТОД ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Файлова система (Шаблон Проксі) ===");

            // Створюємо тестові файли для демонстрації роботи
            string publicFile = "public_data.txt";
            string secretFile = "secret_config.txt";
            File.WriteAllText(publicFile, "Привіт!\nЦе звичайний текстовий файл.\nДоступ відкритий.");
            File.WriteAllText(secretFile, "СЕКРЕТНИЙ КЛЮЧ\nQWERTY123456");

            // 1. Створюємо базовий читач
            ITextReader baseReader = new SmartTextReader();

            // 2. Обгортаємо його в Логер (Проксі №1)
            ITextReader loggingReader = new SmartTextChecker(baseReader);

            // 3. Обгортаємо Логер в Охоронця (Проксі №2). 
            // Забороняємо доступ до файлів, які починаються на "secret"
            ITextReader secureReader = new SmartTextReaderLocker(loggingReader, @"^secret.*\.txt$");

            // --- ТЕСТУВАННЯ ---

            // Тест 1: Читання дозволеного файлу
            Console.WriteLine("\n--- Тест 1: Читання публічного файлу ---");
            char[][] publicContent = secureReader.ReadText(publicFile);

            // Виводимо прочитаний вміст для наочності
            Console.WriteLine("\nПрочитаний текст:");
            foreach (var line in publicContent)
            {
                Console.WriteLine(new string(line));
            }

            // Тест 2: Спроба читання забороненого файлу
            Console.WriteLine("\n--- Тест 2: Читання захищеного файлу ---");
            char[][] secretContent = secureReader.ReadText(secretFile);

            // Прибираємо за собою тестові файли
            File.Delete(publicFile);
            File.Delete(secretFile);

            Console.ReadLine();
        }
    }
}