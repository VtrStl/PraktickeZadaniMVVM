using System;

namespace PraktickeZadaniWPF.Model
{
    /// <summary>
    /// Model pro načtení všech aut v XML do datagridu a inicializování všech hodnot z XML souboru do OutputDefault datagridu.
    /// </summary>
    public class CarModel
    {
        public int Id { get; set; }
        public string? Model { get; set; }
        public DateOnly SaleDate { get; set; }
        public double Price { get; set; }
        public double VAT { get; set; }
    }
}
