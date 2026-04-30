using System;
using System.Collections.Generic;
using System.Text;

namespace FlyweightPatternDemo
{
    public enum DisplayType { Block, Inline }
    public enum ClosingType { Paired, SelfClosing }

    // --- 1. ЛЕГКОВАГОВИК (Flyweight) ---
    // Об'єкт, що містить спільний (Intrinsic) стан. Він незмінний.
    public class TagMarkup
    {
        public string TagName { get; }
        public DisplayType DisplayType { get; }
        public ClosingType ClosingType { get; }

        public TagMarkup(string tagName, DisplayType displayType, ClosingType closingType)
        {
            TagName = tagName;
            DisplayType = displayType;
            ClosingType = closingType;
        }
    }

    // --- 2. ФАБРИКА ЛЕГКОВАГОВИКІВ (Flyweight Factory) ---
    // Гарантує, що ідентичні налаштування тегів існують лише в одному екземплярі
    public class MarkupFactory
    {
        private readonly Dictionary<string, TagMarkup> _pool = new Dictionary<string, TagMarkup>();

        // Отримання легковаговика (з пулу, або створення нового)
        public TagMarkup GetMarkup(string tagName, DisplayType displayType, ClosingType closingType)
        {
            string key = $"{tagName}_{displayType}_{closingType}";
            if (!_pool.ContainsKey(key))
            {
                _pool[key] = new TagMarkup(tagName, displayType, closingType);
            }
            return _pool[key];
        }

        // Кількість унікальних об'єктів у пулі
        public int GetPoolSize() => _pool.Count;
    }

    // --- 3. БАЗОВІ ВУЗЛИ (з Лабораторної 5) ---
    public abstract class LightNode
    {
        public abstract string OuterHTML { get; }
    }

    public class LightTextNode : LightNode
    {
        private readonly string _text;
        public LightTextNode(string text) { _text = text; }
        public override string OuterHTML => _text;
    }

    // --- 4. КОНТЕКСТ (Context) ---
    // Містить зовнішній (Extrinsic) стан (дітей) та посилання на Легковаговик
    public class LightElementNode : LightNode
    {
        private readonly TagMarkup _markup; // Посилання на спільний стан
        private readonly List<LightNode> _children;

        public LightElementNode(TagMarkup markup, params LightNode[] children)
        {
            _markup = markup;
            _children = new List<LightNode>(children);
        }

        public void AddChild(LightNode node) => _children.Add(node);

        public override string OuterHTML
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"<{_markup.TagName}>");

                foreach (var child in _children)
                    sb.Append(child.OuterHTML);

                if (_markup.ClosingType == ClosingType.Paired)
                    sb.Append($"</{_markup.TagName}>");

                if (_markup.DisplayType == DisplayType.Block)
                    sb.Append(Environment.NewLine);

                return sb.ToString();
            }
        }
    }

    // --- 5. ГОЛОВНИЙ МЕТОД ТА ТЕСТУВАННЯ ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string bookText = @"The Project Gutenberg eBook of Romeo and Juliet
    
This eBook is for the use of anyone anywhere in the United States and
most other parts of the world at no cost and with almost no restrictions
whatsoever. You may copy it, give it away or re-use it under the terms
of the Project Gutenberg License included with this eBook or online
at www.gutenberg.org. If you are not located in the United States,
you will have to check the laws of the country where you are located
before using this eBook.

Title: Romeo and Juliet

Author: William Shakespeare

Release date: November 1, 1998 [eBook #1513]
                Most recently updated: September 18, 2025

Language: English

Other information and formats: www.gutenberg.org/ebooks/1513

Credits: the PG Shakespeare Team, a team of about twenty Project Gutenberg volunteers

*** START OF THE PROJECT GUTENBERG EBOOK ROMEO AND JULIET ***

THE TRAGEDY OF ROMEO AND JULIET

by William Shakespeare

Contents

THE PROLOGUE.

ACT I
Scene I. A public place.
Scene II. A Street.
Scene III. Room in Capulet’s House.
Scene IV. A Street.";

            // Розбиваємо текст на рядки
            string[] lines = bookText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            Console.WriteLine("=== Перевірка правильності парсингу (Перші 15 рядків) ===\n");
            MarkupFactory testFactory = new MarkupFactory();
            LightElementNode root = ParseTextToHtml(lines, testFactory, 1);

            // Виводимо частину дерева, щоб довести правильність алгоритму
            string[] htmlLines = root.OuterHTML.Split('\n');
            for (int i = 0; i < Math.Min(15, htmlLines.Length); i++)
            {
                Console.WriteLine(htmlLines[i].Trim());
            }

            Console.WriteLine("\n=== ТЕСТУВАННЯ ПАМ'ЯТІ (Симуляція книги на 80 000 рядків) ===\n");

            // Примусово очищаємо пам'ять перед тестами
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // ТЕСТ 1: БЕЗ використання фабрики (Створюємо новий об'єкт розмітки щоразу)
            long memBeforeNoFlyweight = GC.GetTotalMemory(true);
            LightElementNode fatTree = ParseTextToHtml(lines, null, 1000); // 1000 ітерацій
            long memAfterNoFlyweight = GC.GetTotalMemory(true);
            long memoryUsedWithout = memAfterNoFlyweight - memBeforeNoFlyweight;

            Console.WriteLine($"Пам'ять БЕЗ Легковаговика: {memoryUsedWithout / 1024.0 / 1024.0:F2} MB");

            // Очищаємо пам'ять
            fatTree = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // ТЕСТ 2: З використанням фабрики (Легковаговик)
            MarkupFactory mainFactory = new MarkupFactory();
            long memBeforeFlyweight = GC.GetTotalMemory(true);
            LightElementNode smartTree = ParseTextToHtml(lines, mainFactory, 1000);
            long memAfterFlyweight = GC.GetTotalMemory(true);
            long memoryUsedWith = memAfterFlyweight - memBeforeFlyweight;

            Console.WriteLine($"Пам'ять З Легковаговиком:  {memoryUsedWith / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"Унікальних об'єктів розмітки у пам'яті: {mainFactory.GetPoolSize()} шт.");

            if (memoryUsedWithout > 0)
            {
                double saved = (1.0 - ((double)memoryUsedWith / memoryUsedWithout)) * 100;
                Console.WriteLine($"Економія пам'яті: {saved:F1}%");
            }

            Console.ReadLine();
        }

        // Алгоритм парсингу книги
        static LightElementNode ParseTextToHtml(string[] lines, MarkupFactory factory, int iterations)
        {
            // Якщо фабрика не передана (null), ми симулюємо відсутність Легковаговика 
            // і щоразу створюємо об'єкт через new.

            TagMarkup GetMarkup(string tag, DisplayType dt, ClosingType ct)
            {
                if (factory != null) return factory.GetMarkup(tag, dt, ct);
                return new TagMarkup(tag, dt, ct);
            }

            // Кореневий елемент <div>
            TagMarkup divMarkup = GetMarkup("div", DisplayType.Block, ClosingType.Paired);
            LightElementNode root = new LightElementNode(divMarkup);

            bool isFirstLine = true;

            for (int iter = 0; iter < iterations; iter++)
            {
                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    TagMarkup markup;

                    if (isFirstLine)
                    {
                        // a. Перший рядок має бути <h1>
                        markup = GetMarkup("h1", DisplayType.Block, ClosingType.Paired);
                        isFirstLine = false;
                    }
                    else if (line.Length < 20)
                    {
                        // b. Менше 20 символів - <h2>
                        markup = GetMarkup("h2", DisplayType.Block, ClosingType.Paired);
                    }
                    else if (char.IsWhiteSpace(line[0]))
                    {
                        // c. Починається з пробілу - <blockquote>
                        markup = GetMarkup("blockquote", DisplayType.Block, ClosingType.Paired);
                    }
                    else
                    {
                        // d. Будь-який інший випадок - <p>
                        markup = GetMarkup("p", DisplayType.Block, ClosingType.Paired);
                    }

                    // Створюємо елемент і додаємо в нього текстовий вузол (Trim видаляє зайві пробіли для краси)
                    LightElementNode element = new LightElementNode(markup, new LightTextNode(line.Trim()));
                    root.AddChild(element);
                }
            }

            return root;
        }
    }
}