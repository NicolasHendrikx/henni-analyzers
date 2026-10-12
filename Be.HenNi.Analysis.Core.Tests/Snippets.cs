namespace Be.HenNi.Analysis.Core.Tests;

/// <summary>
/// Source code shared by model and metric tests.
/// </summary>
internal static class Snippets
{
    public const string Person = """
        public class Person
        {
            private const int Max = 3;
            private static int _count;
            private readonly string _id = "";
            public int Age;
            public string Name { get; set; } = "";
            public string Upper => Name.ToUpper() + _id + _count + Max;
        }
        """;

    public const string Point = "public record Point(int X, int Y);";

    public const string Greeter = """
        public class Greeter(string name, int unused)
        {
            private readonly int _n = unused;
            public string Greet() => "Hello " + name + _n;
        }
        """;

    public const string Account = """
        public class Account
        {
            private decimal _balance;
            private string _owner = "";

            public decimal Balance { get => _balance; set => _balance = value; }
            public string GetOwner() { return _owner; }
            public void SetOwner(string owner) { this._owner = owner; }
            public string Label { get; private set; } = "";
            public void Deposit(decimal amount) { if (amount > 0) _balance += amount; }
            public decimal Doubled => _balance * 2;
        }
        """;

    public const string Order = """
        namespace Shop
        {
            public class Base { protected int Inherited; }

            public class Customer
            {
                public const int Max = 3;
                public string Name = "";
                private string _email = "";
                public int Points { get; set; }
                public string GetEmail() => _email;
            }

            public enum Level { Low, High }

            public class Order : Base
            {
                private Customer _customer = new Customer();

                public string Describe()
                    => _customer.Name + _customer.Points + _customer.GetEmail() + _customer.GetEmail()
                       + Customer.Max + Level.High + Inherited + "x".Length + _customer.ToString();

                public int Score() => _customer.Points;
            }
        }
        """;

    public const string Flow = """
        public class Flow
        {
            public int Compute(int[] values, string? text)
            {
                int total = 0;
                foreach (var v in values)
                {
                    if (v > 0 && v < 10) total += v;
                    else if (v == 0 || v == -1) total--;
                }
                while (total > 100) total /= 2;
                try { total += text?.Length ?? 0; }
                catch (System.Exception) { total = -1; }
                switch (total)
                {
                    case 1:
                    case 2:
                        total = 0;
                        break;
                    default:
                        break;
                }
                return total > 0 ? total : 0;
            }

            public string Name(int x) => x switch { 1 => "one", 2 => "two", _ => "many" };
        }
        """;

    public const string Rectangle = """
        public class Rectangle
        {
            private double _width;
            private double _height;
            private string _label = "";

            public double Area() => _width * _height;
            public double Perimeter() => 2 * (_width + _height);
            public string Describe() => _label + Formatted;
            private string Formatted => Area().ToString();
            public void Rename(string label) { _label = label; }
        }
        """;
}
