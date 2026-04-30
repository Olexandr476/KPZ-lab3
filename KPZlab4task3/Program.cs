using System;
using System.Collections.Generic;
using System.Text;

namespace ObserverPatternDemo
{
    public enum DisplayType { Block, Inline }
    public enum ClosingType { Paired, SelfClosing }

    // --- 1. СУБ'ЄКТ (Subject / Publisher) ---
    // Базовий клас тепер вміє керувати підписниками (Спостерігачами)
    public abstract class LightNode
    {
        // Словник для зберігання підписок: Назва івенту -> Список колбеків
        private readonly Dictionary<string, List<Action>> _eventListeners = new Dictionary<string, List<Action>>();

        // Метод підписки (Attach)
        public void AddEventListener(string eventType, Action listener)
        {
            if (!_eventListeners.ContainsKey(eventType))
            {
                _eventListeners[eventType] = new List<Action>();
            }
            _eventListeners[eventType].Add(listener);
        }

        // Метод відписки (Detach)
        public void RemoveEventListener(string eventType, Action listener)
        {
            if (_eventListeners.ContainsKey(eventType))
            {
                _eventListeners[eventType].Remove(listener);
            }
        }

        // Метод сповіщення (Notify) - симулює настання події
        public void TriggerEvent(string eventType)
        {
            if (_eventListeners.ContainsKey(eventType))
            {
                Console.WriteLine($"\n[Система]: Спрацював івент '{eventType}'. Сповіщення підписників...");
                foreach (var listener in _eventListeners[eventType])
                {
                    listener.Invoke(); // Виклик функції-спостерігача
                }
            }
            else
            {
                Console.WriteLine($"\n[Система]: Івент '{eventType}' відбувся, але підписників немає.");
            }
        }

        public abstract string OuterHTML { get; }
        public abstract string InnerHTML { get; }
    }

    // --- 2. ЕЛЕМЕНТИ ДЕРЕВА ---
    public class LightTextNode : LightNode
    {
        private readonly string _text;
        public LightTextNode(string text) { _text = text ?? string.Empty; }

        public override string OuterHTML => _text;
        public override string InnerHTML => _text;
    }

    public class LightElementNode : LightNode
    {
        public string TagName { get; }
        public DisplayType DisplayType { get; }
        public ClosingType ClosingType { get; }

        private readonly List<LightNode> _children;

        public LightElementNode(string tagName, DisplayType displayType, ClosingType closingType, params LightNode[] children)
        {
            TagName = tagName;
            DisplayType = displayType;
            ClosingType = closingType;
            _children = new List<LightNode>(children);
        }

        public override string InnerHTML
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var child in _children) sb.Append(child.OuterHTML);
                return sb.ToString();
            }
        }

        public override string OuterHTML
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"<{TagName}");

                if (ClosingType == ClosingType.SelfClosing)
                    sb.Append(" />");
                else
                    sb.Append($">{InnerHTML}</{TagName}>");

                if (DisplayType == DisplayType.Block)
                    sb.Append(Environment.NewLine);

                return sb.ToString();
            }
        }
    }

    // --- 3. ГОЛОВНИЙ МЕТОД ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== LightHTML Events (Шаблон Спостерігач) ===\n");

            // Створюємо кнопку
            LightElementNode button = new LightElementNode("button", DisplayType.Inline, ClosingType.Paired,
                new LightTextNode("Натисни мене"));

            // Створюємо картинку
            LightElementNode image = new LightElementNode("img", DisplayType.Inline, ClosingType.SelfClosing);

            // --- ДОДАЄМО СПОСТЕРІГАЧІВ (Підписка на події) ---

            // Підписуємось на клік по кнопці (1-й спостерігач)
            button.AddEventListener("click", () =>
            {
                Console.WriteLine("Спостерігач 1: Кнопку було натиснуто! Зберігаю дані в базу...");
            });

            // На одну подію може бути підписано кілька спостерігачів (2-й спостерігач)
            button.AddEventListener("click", () =>
            {
                Console.WriteLine("Спостерігач 2: Змінюю колір кнопки на синій.");
            });

            // Підписуємось на наведення миші на картинку
            image.AddEventListener("mouseover", () =>
            {
                Console.WriteLine("Спостерігач 3: Мишка наведена на картинку. Показую tooltip.");
            });

            // --- СИМУЛЯЦІЯ ДІЙ КОРИСТУВАЧА ---

            Console.WriteLine("Рендеринг елементів:");
            Console.WriteLine(button.OuterHTML);
            Console.WriteLine(image.OuterHTML);

            Console.WriteLine("\n--- Користувач клікає на кнопку ---");
            button.TriggerEvent("click");

            Console.WriteLine("\n--- Користувач наводить мишу на картинку ---");
            image.TriggerEvent("mouseover");

            Console.WriteLine("\n--- Користувач клікає на картинку ---");
            image.TriggerEvent("click"); // Ніхто не підписаний на клік по картинці

            Console.ReadLine();
        }
    }
}