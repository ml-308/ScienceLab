using System.Collections.Generic;

namespace ScienceLab.Chemistry;

/// <summary>化学元素。</summary>
public sealed class Element
{
    public required int AtomicNumber { get; init; }
    public required string Symbol { get; init; }
    public required string Name { get; init; }
    public required double AtomicMass { get; init; }
    public required string Category { get; init; } // 金属/非金属/稀有气体...
}

public static class PeriodicTable
{
    public static readonly List<Element> Elements = new()
    {
        new Element { AtomicNumber = 1, Symbol = "H", Name = "氢", AtomicMass = 1.008, Category = "非金属" },
        new Element { AtomicNumber = 2, Symbol = "He", Name = "氦", AtomicMass = 4.003, Category = "稀有气体" },
        new Element { AtomicNumber = 3, Symbol = "Li", Name = "锂", AtomicMass = 6.94, Category = "碱金属" },
        new Element { AtomicNumber = 4, Symbol = "Be", Name = "铍", AtomicMass = 9.012, Category = "碱土金属" },
        new Element { AtomicNumber = 5, Symbol = "B", Name = "硼", AtomicMass = 10.81, Category = "非金属" },
        new Element { AtomicNumber = 6, Symbol = "C", Name = "碳", AtomicMass = 12.011, Category = "非金属" },
        new Element { AtomicNumber = 7, Symbol = "N", Name = "氮", AtomicMass = 14.007, Category = "非金属" },
        new Element { AtomicNumber = 8, Symbol = "O", Name = "氧", AtomicMass = 15.999, Category = "非金属" },
        new Element { AtomicNumber = 9, Symbol = "F", Name = "氟", AtomicMass = 18.998, Category = "卤素" },
        new Element { AtomicNumber = 10, Symbol = "Ne", Name = "氖", AtomicMass = 20.180, Category = "稀有气体" },
        new Element { AtomicNumber = 11, Symbol = "Na", Name = "钠", AtomicMass = 22.990, Category = "碱金属" },
        new Element { AtomicNumber = 12, Symbol = "Mg", Name = "镁", AtomicMass = 24.305, Category = "碱土金属" },
        new Element { AtomicNumber = 13, Symbol = "Al", Name = "铝", AtomicMass = 26.982, Category = "金属" },
        new Element { AtomicNumber = 14, Symbol = "Si", Name = "硅", AtomicMass = 28.085, Category = "非金属" },
        new Element { AtomicNumber = 15, Symbol = "P", Name = "磷", AtomicMass = 30.974, Category = "非金属" },
        new Element { AtomicNumber = 16, Symbol = "S", Name = "硫", AtomicMass = 32.06, Category = "非金属" },
        new Element { AtomicNumber = 17, Symbol = "Cl", Name = "氯", AtomicMass = 35.45, Category = "卤素" },
        new Element { AtomicNumber = 18, Symbol = "Ar", Name = "氩", AtomicMass = 39.948, Category = "稀有气体" },
        new Element { AtomicNumber = 19, Symbol = "K", Name = "钾", AtomicMass = 39.098, Category = "碱金属" },
        new Element { AtomicNumber = 20, Symbol = "Ca", Name = "钙", AtomicMass = 40.078, Category = "碱土金属" },
        new Element { AtomicNumber = 26, Symbol = "Fe", Name = "铁", AtomicMass = 55.845, Category = "金属" },
        new Element { AtomicNumber = 29, Symbol = "Cu", Name = "铜", AtomicMass = 63.546, Category = "金属" },
        new Element { AtomicNumber = 30, Symbol = "Zn", Name = "锌", AtomicMass = 65.38, Category = "金属" },
        new Element { AtomicNumber = 47, Symbol = "Ag", Name = "银", AtomicMass = 107.868, Category = "金属" },
        new Element { AtomicNumber = 79, Symbol = "Au", Name = "金", AtomicMass = 196.967, Category = "金属" },
    };
}
