namespace PraktickeZadaniWPF.Model
{
    /// <summary>
    /// Model pro načtení hodnot z XML které se prodali za víkend, inicializování těchto hodnot, následném vypočítání a zobrazení do OutputCalculate datagridu.
    /// </summary>
    public class CarWeekendSalesModel
    {
        public int Id { get; set; }
        public string? ModelPrice { get; set; }
        public double PriceWithVAT { get; set; }
    }
}
