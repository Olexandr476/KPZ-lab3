using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompositePatternDemo
{
    // Допоміжні перелічення для чіткої типізації
    public enum DisplayType { Block, Inline }
    public enum ClosingType { Paired, SelfClosing }

    // --- 1. БАЗОВИЙ КОМПОНЕНТ (Component) ---
    // Спільний контракт для всіх елементів дерева
    public abstract class LightNode
    {
        // Властивості для отримання HTML-коду
        public abstract string OuterHTML { get; }
        public abstract string InnerHTML { get; }
    }

    // --- 2. ЛИСТОК (Leaf) ---
    // Кінцевий елемент, який не може мати нащадків
    public class LightTextNode : LightNode
    {
        private readonly string _text;

        public LightTextNode(string text)
        {
            _text = text ?? string.Empty;
        }

        // Для текстового вузла Inner і Outer HTML — це просто сам текст
        public override string OuterHTML => _text;
        public override string InnerHTML => _text;
    }

    // --- 3. КОМПОЗИТ (Composite) ---
    // Складний елемент, який містить інші вузли (як текстові, так і інші елементи)
    public class LightElementNode : LightNode
    {
        public string TagName { get; }
        public DisplayType DisplayType { get; }
        public ClosingType ClosingType { get; }

        // Інкапсулюємо колекції
        private readonly List<string> _cssClasses;
        private readonly List<LightNode> _children;

        // Властивості доступу
        public IReadOnlyList<string> CssClasses => _cssClasses.AsReadOnly();
        public IReadOnlyList<LightNode> Children => _children.AsReadOnly();
        public int ChildrenCount => _children.Count;

        // Конструктор з використанням params для зручного деревоподібного створення
        public LightElementNode(string tagName, DisplayType displayType, ClosingType closingType,
                                List<string> cssClasses = null, params LightNode[] children)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                throw new ArgumentException("Назва тега не може бути порожньою.");

            TagName = tagName;
            DisplayType = displayType;
            ClosingType = closingType;
            _cssClasses = cssClasses ?? new List<string>();
            _children = new List<LightNode>(children);
        }

        public void AddChild(LightNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            _children.Add(node);
        }

        public void AddClass(string className)
        {
            if (!string.IsNullOrWhiteSpace(className) && !_cssClasses.Contains(className))
                _cssClasses.Add(className);
        }

        // Рендеринг внутрішнього контенту (рекурсивний виклик дочірніх OuterHTML)
        public override string InnerHTML
        {
            get
            {
                // Використовуємо StringBuilder для оптимізації роботи з пам'яттю (уникаємо запаху String Concatenation)
                StringBuilder sb = new StringBuilder();
                foreach (var child in _children)
                {
                    sb.Append(child.OuterHTML);
                }
                return sb.ToString();
            }
        }

        // Рендеринг повного тега з атрибутами та контентом
        public override string OuterHTML
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                // Формування відкритого тега
                sb.Append($"<{TagName}");
                if (_cssClasses.Any())
                {
                    sb.Append($" class=\"{string.Join(" ", _cssClasses)}\"");
                }

                if (ClosingType == ClosingType.SelfClosing)
                {
                    sb.Append(" />"); // Одиничний тег (наприклад, <img />)
                }
                else
                {
                    sb.Append(">");
                    sb.Append(InnerHTML); // Додаємо внутрішній контент
                    sb.Append($"</{TagName}>"); // Закриваючий тег
                }

                // Додаємо перенесення рядка для блочних елементів, щоб вивід у консолі був красивим
                if (DisplayType == DisplayType.Block)
                {
                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }
        }
    }

    // --- 4. ГОЛОВНИЙ МЕТОД ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Власна мова розмітки LightHTML (Шаблон Компонувальник) ===\n");

            // Будуємо HTML структуру: таблиця з користувачами
            // Завдяки передачі children в конструктор, код візуально нагадує HTML-дерево
            LightElementNode table = new LightElementNode("table", DisplayType.Block, ClosingType.Paired, new List<string> { "table", "table-bordered" },
                new LightElementNode("thead", DisplayType.Block, ClosingType.Paired, null,
                    new LightElementNode("tr", DisplayType.Block, ClosingType.Paired, null,
                        new LightElementNode("th", DisplayType.Inline, ClosingType.Paired, null, new LightTextNode("Ім'я")),
                        new LightElementNode("th", DisplayType.Inline, ClosingType.Paired, null, new LightTextNode("Вік")),
                        new LightElementNode("th", DisplayType.Inline, ClosingType.Paired, null, new LightTextNode("Статус"))
                    )
                ),
                new LightElementNode("tbody", DisplayType.Block, ClosingType.Paired, null,
                    // Перший рядок
                    new LightElementNode("tr", DisplayType.Block, ClosingType.Paired, null,
                        new LightElementNode("td", DisplayType.Inline, ClosingType.Paired, null, new LightTextNode("Олександр")),
                        new LightElementNode("td", DisplayType.Inline, ClosingType.Paired, null, new LightTextNode("20")),
                        new LightElementNode("td", DisplayType.Inline, ClosingType.Paired, null,
                            new LightElementNode("span", DisplayType.Inline, ClosingType.Paired, new List<string> { "badge", "bg-success" }, new LightTextNode("Студент"))
                        )
                    ),
                    // Другий рядок
                    new LightElementNode("tr", DisplayType.Block, ClosingType.Paired, null,
                        new LightElementNode("td", DisplayType.Inline, ClosingType.Paired, null, new LightTextNode("Марія")),
                        new LightElementNode("td", DisplayType.Inline, ClosingType.Paired, null, new LightTextNode("19")),
                        new LightElementNode("td", DisplayType.Inline, ClosingType.Paired, null,
                            // Демонстрація одиничного тегу
                            new LightElementNode("img", DisplayType.Inline, ClosingType.SelfClosing, new List<string> { "icon" })
                        )
                    )
                )
            );

            Console.WriteLine("--- Аналіз кореневого елемента ---");
            Console.WriteLine($"Тег: {table.TagName}");
            Console.WriteLine($"Кількість дочірніх елементів: {table.ChildrenCount}");
            Console.WriteLine($"Класи: {string.Join(", ", table.CssClasses)}");

            Console.WriteLine("\n--- Рендеринг (OuterHTML) ---");
            Console.WriteLine(table.OuterHTML);

            Console.ReadLine();
        }
    }
}