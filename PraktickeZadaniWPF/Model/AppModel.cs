using System;
using System.Linq;
using System.Xml;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;

namespace PraktickeZadaniWPF.Model
{
    public class AppModel
    {
        private ObservableCollection<CarModel> CarModels { get; set; }
        private ObservableCollection<CarWeekendSalesModel> CarWeekendSalesModels { get; set; }
        
        /// <summary>
        /// Inicializuje potřebné atributy do proměnných v AppModel třídě.
        /// </summary>
        /// <param name="carModels"></param>
        /// <param name="carWeekendSalesModels"></param>
        public AppModel(ObservableCollection<CarModel> carModels, ObservableCollection<CarWeekendSalesModel> carWeekendSalesModels)
        {
            CarModels = carModels;
            CarWeekendSalesModels = carWeekendSalesModels;
        }
        
        /// <summary>
        /// Metoda pro načtení všech modelů aut do DefaultOutput datagridu z XML souboru
        /// </summary>
        /// <param name="autoNodes"></param>
        /// <returns></returns>
        public ObservableCollection<CarModel> LoadData(XmlNodeList autoNodes)
        {
            CarModels.Clear();
            int outputDefaultID = 1;
            foreach (XmlNode autoNode in autoNodes)
            {
                string? model = autoNode.Attributes?["model"]?.Value;
                string? saleDateStr = autoNode.SelectSingleNode("datum_prodeje")?.InnerText;
                DateOnly saleDate = DateOnly.ParseExact(saleDateStr!, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                double price = Convert.ToDouble(autoNode.SelectSingleNode("cena")?.InnerText, CultureInfo.InvariantCulture);
                double vat = Convert.ToDouble(autoNode.SelectSingleNode("dph")?.InnerText, CultureInfo.InvariantCulture);

                CarModels.Add(new CarModel
                {
                    Id = outputDefaultID++,
                    Model = model,
                    SaleDate = saleDate,
                    Price = price,
                    VAT = vat
                });
            }
            return CarModels;
        }

        /// <summary>
        /// Metoda pro načtení a výpočet modelů aut které se prodali o víkendu s výpočtem ceny s DPH.
        /// Když metado najde hodnotu, která už v datagridu OutputCalculation je, tak přičte do řádku modelu cenu s DPH.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<CarWeekendSalesModel> CalculateWeekendSales()
        {
            int outputCalculatedID = 1;
            foreach (var carModel in CarModels)
            {
                if (carModel.SaleDate.DayOfWeek == DayOfWeek.Saturday || carModel.SaleDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    CarWeekendSalesModel? weekendSalesModel = CarWeekendSalesModels.FirstOrDefault(
                        m => m.ModelPrice!.StartsWith(carModel.Model!));

                    if (weekendSalesModel != null)
                    {
                        weekendSalesModel.PriceWithVAT += carModel.Price * (carModel.VAT / 100) + carModel.Price;

                        string[] parts = weekendSalesModel.ModelPrice!.Split("\r\n");
                        double existingPrice = Convert.ToDouble(Regex.Replace(parts[1], "[^0-9.,]+", ""));
                        double newPrice = existingPrice + carModel.Price;
                        weekendSalesModel.ModelPrice = $"{carModel.Model}\r\n{newPrice:C0}";
                    }
                    else
                    {
                        CarWeekendSalesModels.Add(new CarWeekendSalesModel
                        {
                            Id = outputCalculatedID++,
                            ModelPrice = $"{carModel.Model}\r\n{carModel.Price:C0}",
                            PriceWithVAT = carModel.Price * (carModel.VAT / 100) + carModel.Price
                        });
                    }
                }
            }
            return CarWeekendSalesModels;
        }
        
        /// <summary>
        /// Statická metoda, která slouží k zobrazování chybového okénka k různým v výjimkám
        /// Tahle metoda je v modelu, aby ve ViewModel nezabírala zbytečně moc místa a skutečně ViewModel sloužil jako prostředník mezi Modelem a View
        /// </summary>
        /// <param name="ex"></param>
        public static void ExceptionMessageBox(Exception ex)
        {
            switch (ex)
            {
                case FormatException:
                    MessageBox.Show($"Nastala chyba při načítání souboru. Zkontrolujte, zda je XML ve správném formátu." +
                    $"\r\n\r\nZpráva chyby:\r\n{ex.Message}\r\n\r\nŘádek výskytu:\r\n{ex.StackTrace} ",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                
                case ArgumentNullException:
                    MessageBox.Show("XML nelze přečíst.\r\nPravděpodobně máte XML elementy ve špatném formátu, " +
                    "je podporovaná XML struktůra:" +
                    "\r\n<auta>" +
                    "\r\n  <auto model='Škoda Felicia>'" +
                    "\r\n    <datum_prodeje></datum_prodeje>" +
                    "\r\n    <cena></cena>" +
                    "\r\n    <dph></dph>" +
                    "\r\n  </auto>" +
                    "\r\n</auta>" +
                    $"\r\n\r\nZpráva chyby:\r\n{ex.Message}\r\n\r\nŘádek výskytu:\r\n{ex.StackTrace}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                
                case Exception:
                    MessageBox.Show($"Nastala chyba: {ex.Message}\r\n\r\nŘádek výskytu:\r\n{ex.StackTrace}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
    }
}