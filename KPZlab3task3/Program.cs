using System;

namespace BridgePatternDemo
{
    // --- 1. ІНТЕРФЕЙС РЕАЛІЗАЦІЇ (Implementor) ---
    // Визначає, ЯК саме буде малюватися фігура.
    public interface IRenderer
    {
        void RenderShape(string shapeName);
    }

    // --- 2. КОНКРЕТНІ РЕАЛІЗАЦІЇ (Concrete Implementors) ---
    // Конкретні "движці" рендерингу.
    public class VectorRenderer : IRenderer
    {
        public void RenderShape(string shapeName)
        {
            Console.WriteLine($"Drawing {shapeName} as lines/vectors (Векторна графіка)");
        }
    }

    public class RasterRenderer : IRenderer
    {
        public void RenderShape(string shapeName)
        {
            Console.WriteLine($"Drawing {shapeName} as pixels (Растрова графіка)");
        }
    }

    // --- 3. АБСТРАКЦІЯ (Abstraction) ---
    // Визначає, ЩО саме ми малюємо. Зберігає посилання на движок рендерингу.
    public abstract class Shape
    {
        // Це і є той самий "Міст". Замість жорсткої прив'язки до рендерера,
        // ми працюємо з ним через абстрактний інтерфейс.
        // Модифікатор protected дозволяє дочірнім класам користуватися цим полем.
        protected readonly IRenderer _renderer;

        // Впровадження залежності (Dependency Injection) через конструктор
        protected Shape(IRenderer renderer)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        // Абстрактний метод, який повинні реалізувати всі фігури
        public abstract void Draw();
    }

    // --- 4. РОЗШИРЕНІ АБСТРАКЦІЇ (Refined Abstractions) ---
    // Самі фігури не знають, ЯК вони будуть намальовані. Вони лише кажуть:
    // "Рендерер, намалюй мене, я - Коло".

    public class Circle : Shape
    {
        public Circle(IRenderer renderer) : base(renderer) { }

        public override void Draw()
        {
            _renderer.RenderShape("Circle");
        }
    }

    public class Square : Shape
    {
        public Square(IRenderer renderer) : base(renderer) { }

        public override void Draw()
        {
            _renderer.RenderShape("Square");
        }
    }

    public class Triangle : Shape
    {
        public Triangle(IRenderer renderer) : base(renderer) { }

        public override void Draw()
        {
            _renderer.RenderShape("Triangle");
        }
    }

    // --- 5. ГОЛОВНИЙ МЕТОД ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Графічний редактор (Шаблон Міст) ===\n");

            // 1. Створюємо рушії рендерингу
            IRenderer vectorEngine = new VectorRenderer();
            IRenderer rasterEngine = new RasterRenderer();

            // 2. Створюємо фігури, динамічно передаючи їм потрібний тип рендерингу
            // (наводимо мости)
            Shape vectorCircle = new Circle(vectorEngine);
            Shape rasterCircle = new Circle(rasterEngine);

            Shape vectorSquare = new Square(vectorEngine);
            Shape rasterTriangle = new Triangle(rasterEngine);

            // 3. Тестуємо малювання
            Console.WriteLine("--- Рендеринг Кола ---");
            vectorCircle.Draw();
            rasterCircle.Draw();

            Console.WriteLine("\n--- Рендеринг інших фігур ---");
            vectorSquare.Draw();
            rasterTriangle.Draw();

            Console.ReadLine();
        }
    }
}