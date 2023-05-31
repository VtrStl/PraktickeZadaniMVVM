using System;

namespace PraktickeZadaniWPF.Model
{
    /// <summary>
    /// Model pro načtení XML do Datagridu a inicializování všech hodnot z XML souboru a zároveň načtení do datagridu
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
