using System;

namespace DecoratorPatternDemo
{
    // --- 1. БАЗОВИЙ КОМПОНЕНТ (Component) ---
    // Визначає контракт, якого мають дотримуватися і самі герої, і їхній інвентар.
    public abstract class Hero
    {
        // Використовуємо абстрактні методи для обчислення характеристик.
        // Це гарантує, що при "обгортанні" декораторами характеристики будуть сумуватися.
        public abstract string GetName();
        public abstract int GetAttack();
        public abstract int GetDefense();
        public abstract int GetHealth();
        public abstract string GetInventoryDescription();

        // Метод для красивого виводу профілю (DRY - реалізовано лише в базовому класі)
        public void ShowProfile()
        {
            Console.WriteLine($"=== {GetName()} ===");
            Console.WriteLine($" Атака: {GetAttack()} |  Захист: {GetDefense()} |  Здоров'я: {GetHealth()}");
            Console.WriteLine($" Екіпірування: {GetInventoryDescription()}");
            Console.WriteLine(new string('-', 40));
        }
    }

    // --- 2. КОНКРЕТНІ ГЕРОЇ (Concrete Components) ---
    // Вони задають базові старти без інвентарю.

    public class Warrior : Hero
    {
        private readonly string _name;
        public Warrior(string name) { _name = name ?? "Безіменний"; }

        public override string GetName() => $"Воїн {_name}";
        public override int GetAttack() => 20;
        public override int GetDefense() => 15;
        public override int GetHealth() => 150;
        public override string GetInventoryDescription() => "Базова сорочка";
    }

    public class Mage : Hero
    {
        private readonly string _name;
        public Mage(string name) { _name = name ?? "Безіменний"; }

        public override string GetName() => $"Маг {_name}";
        public override int GetAttack() => 25;
        public override int GetDefense() => 5;
        public override int GetHealth() => 80;
        public override string GetInventoryDescription() => "Стара мантія";
    }

    public class Palladin : Hero
    {
        private readonly string _name;
        public Palladin(string name) { _name = name ?? "Безіменний"; }

        public override string GetName() => $"Паладин {_name}";
        public override int GetAttack() => 15;
        public override int GetDefense() => 20;
        public override int GetHealth() => 120;
        public override string GetInventoryDescription() => "Тренувальні лати";
    }

    // --- 3. БАЗОВИЙ ДЕКОРАТОР (Decorator) ---
    // Обгортка, яка імплементує Hero, але одночасно містить екземпляр Hero всередині себе.
    public abstract class InventoryDecorator : Hero
    {
        protected readonly Hero _hero;

        protected InventoryDecorator(Hero hero)
        {
            _hero = hero ?? throw new ArgumentNullException(nameof(hero));
        }

        // За замовчуванням декоратор просто прокидає виклик далі по ланцюжку.
        public override string GetName() => _hero.GetName();
        public override int GetAttack() => _hero.GetAttack();
        public override int GetDefense() => _hero.GetDefense();
        public override int GetHealth() => _hero.GetHealth();
        public override string GetInventoryDescription() => _hero.GetInventoryDescription();
    }

    // --- 4. КОНКРЕТНІ ДЕКОРАТОРИ (Інвентар) ---
    // Вони модифікують поведінку об'єкта, який обгортають.

    // --- Зброя ---
    public class SwordOfLight : InventoryDecorator
    {
        public SwordOfLight(Hero hero) : base(hero) { }
        public override int GetAttack() => base.GetAttack() + 35; // Додає 35 атаки
        public override string GetInventoryDescription() => base.GetInventoryDescription() + ", Меч Світла (+35 атаки)";
    }

    public class PoisonedDagger : InventoryDecorator
    {
        public PoisonedDagger(Hero hero) : base(hero) { }
        public override int GetAttack() => base.GetAttack() + 15;
        public override string GetInventoryDescription() => base.GetInventoryDescription() + ", Отруйний Кинджал (+15 урон)";
    }

    // --- Одяг/Броня ---
    public class MythrilArmor : InventoryDecorator
    {
        public MythrilArmor(Hero hero) : base(hero) { }
        public override int GetDefense() => base.GetDefense() + 40; // Додає 40 захисту
        public override string GetInventoryDescription() => base.GetInventoryDescription() + ", Міфрилова Броня (+40 БРОНІ)";
    }

    public class ElvenCloak : InventoryDecorator
    {
        public ElvenCloak(Hero hero) : base(hero) { }
        public override int GetDefense() => base.GetDefense() + 10;
        public override int GetHealth() => base.GetHealth() + 20; // Одяг може впливати на здоров'я
        public override string GetInventoryDescription() => base.GetInventoryDescription() + ", Ельфійський Плащ (+10 броні, +20 ХП)";
    }

    // --- Артефакти ---
    public class RingOfVitality : InventoryDecorator
    {
        public RingOfVitality(Hero hero) : base(hero) { }
        public override int GetHealth() => base.GetHealth() + 100; // Величезний бонус до здоров'я
        public override string GetInventoryDescription() => base.GetInventoryDescription() + ", Кільце Життєдайності (+100 )";
    }

    public class CursedAmulet : InventoryDecorator
    {
        public CursedAmulet(Hero hero) : base(hero) { }
        public override int GetAttack() => base.GetAttack() + 50; // Дає величезну силу...
        public override int GetHealth() => base.GetHealth() - 40; // ...але висмоктує життя!
        public override string GetInventoryDescription() => base.GetInventoryDescription() + ", Проклятий Амулет (+50 , -40 )";
    }

    // --- 5. ГОЛОВНИЙ МЕТОД ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== RPG: Генератор персонажів (Шаблон Декоратор) ===\n");

            // 1. Створюємо "голого" воїна і дивимося його старти
            Hero arthur = new Warrior("Артур");
            arthur.ShowProfile();

            // 2. Одягаємо на нього Міфрилову Броню (Тепер arthur - це броня, що містить воїна)
            arthur = new MythrilArmor(arthur);

            // 3. Даємо йому Меч Світла і Кільце
            arthur = new SwordOfLight(arthur);
            arthur = new RingOfVitality(arthur);

            Console.WriteLine("\nПісля екіпірування Артура:");
            arthur.ShowProfile();

            // 4. Демонстрація гнучкості: Маг-вбивця з кинджалами
            Console.WriteLine("\nСтворюємо Мага і вішаємо на нього декілька однакових або різних предметів:");
            Hero shadowMage = new Mage("Мерлін");
            shadowMage = new ElvenCloak(shadowMage);
            shadowMage = new PoisonedDagger(shadowMage); // Перший кинджал
            shadowMage = new PoisonedDagger(shadowMage); // Другий кинджал (можна скільки завгодно!)
            shadowMage = new CursedAmulet(shadowMage);   // Дає атаку, але забирає ХП
            shadowMage.ShowProfile();

            Console.ReadLine();
        }
    }
}