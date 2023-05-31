using System;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using PraktickeZadaniWPF.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using System.Xml;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace PraktickeZadaniWPF.ViewModel
{
    /// <summary>
    /// ViewModel který komunikuje mezi Model a View 
    /// </summary>
    public class AppViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<CarModel>? outputDefault;
        public ObservableCollection<CarModel>? OutputDefault
        {
            get { return outputDefault; }
            private set
            {
                outputDefault = value;
                OnPropertyChanged(nameof(OutputDefault));
            }
        }
        private ObservableCollection<CarWeekendSalesModel>? outputCalculated;
        public ObservableCollection<CarWeekendSalesModel>? OutputCalculated
        {
            get { return outputCalculated; }
            private set
            {
                outputCalculated = value; 
                OnPropertyChanged(nameof(OutputCalculated));
            }
        }
        public ICommand LoadXmlButton { get; }

        public AppViewModel()
        {
            LoadXmlButton = new RelayCommand(LoadXml);
        }

        /// <summary>
        /// Načte XML soubor a načte hodnoty do OutputDefault datagrid tabulky.
        /// Zároveň zjistí, jestli daný datum prodeje byl o víkendu, ty vybere a dá do OutputCalculation datagrid tabulky a zároveň vypočítá DPH.
        /// Když najde hodnotu, která už v OutputCalculation tabulky už je, tak přičte cenu a zároveň vypočítá DPH.
        /// </summary>
        private void LoadXml()
        {            
            try
            {
                OpenFileDialog dialog = new() { DefaultExt = "xml", Filter = "XML Files (*.xml)|*.xml" };
                if (dialog.ShowDialog() == true)
                {                    
                    int outputDefaultID = 1;
                    int outputCalculatedID = 1;
                    XmlDocument xmlDoc = new();
                    xmlDoc.Load(dialog.FileName);
                    XmlNodeList autoNodes = xmlDoc.GetElementsByTagName("auto");
                    LoadAndCalculateXmlToDg(autoNodes, outputDefaultID, outputCalculatedID);
                }
            }
            
            catch (FormatException formatEx)
            {
                MessageBox.Show($"Nastala chyba při načítání souboru. Zkontrolujte, zda je XML ve správném formátu." +
                    $"\r\n\r\nZpráva chyby:\r\n{formatEx.Message}\r\n\r\nŘádek výskytu:\r\n{formatEx.StackTrace} ", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
                                    
            catch (Exception ex)
            {
                MessageBox.Show($"Nastala chyba: {ex.Message}\r\n\r\nŘádek výskytu:\r\n{ex.StackTrace}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAndCalculateXmlToDg(XmlNodeList autoNodes, int outputDefaultID, int outputCalculatedID)
        {
            try
            {
                OutputDefault?.Clear();
                OutputCalculated?.Clear();
                OutputDefault = new ObservableCollection<CarModel>();
                OutputCalculated = new ObservableCollection<CarWeekendSalesModel>();
                foreach (XmlNode autoNode in autoNodes)
                {
                    string? model = autoNode.Attributes?["model"]?.Value;
                    string? saleDateStr = autoNode.SelectSingleNode("datum_prodeje")?.InnerText;
                    DateOnly saleDate = DateOnly.ParseExact(saleDateStr!, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    double price = Convert.ToDouble(autoNode.SelectSingleNode("cena")?.InnerText, CultureInfo.InvariantCulture);
                    double vat = Convert.ToDouble(autoNode.SelectSingleNode("dph")?.InnerText, CultureInfo.InvariantCulture);

                    // Přidá model auta se všemi informaci do OutputDefault datagridu
                    OutputDefault?.Add(new CarModel { Id = outputDefaultID++, Model = model, SaleDate = saleDate, Price = price, VAT = vat });
                    if (saleDate.DayOfWeek == DayOfWeek.Saturday || saleDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        CarWeekendSalesModel? weekendSalesModel = OutputCalculated.FirstOrDefault(m => m.ModelPrice!.StartsWith(model!));
                        if (weekendSalesModel != null)
                        {
                            weekendSalesModel.PriceWithVAT += price * (vat / 100) + price;

                            // Rozdělí hodnotu ModelPrice, aby se získala pouze cena, aktualizuje ji novou hodnotou a pak ji spojí zpět.
                            string[] parts = weekendSalesModel.ModelPrice!.Split("\r\n");
                            double existingPrice = Convert.ToDouble(Regex.Replace(parts[1], "[^0-9.,]+", ""));
                            double newPrice = existingPrice + price;
                            weekendSalesModel.ModelPrice = $"{model}\r\n{newPrice:C}";
                        }
                        else
                        {
                            // Pokud model auta prodaný o víkendu není v OutputCalculated, tak přidá model do OutputCalculated
                            OutputCalculated.Add(new CarWeekendSalesModel { Id = outputCalculatedID++, ModelPrice = $"{model}\r\n{price:C}", PriceWithVAT = price * (vat / 100) + price });
                        }
                    }
                }
            }
            catch (ArgumentNullException nullEx)
            {
                MessageBox.Show("XML nelze přečíst.\r\nPravděpodobně máte XML elementy ve špatném formátu, je podporovaná XML struktůra:" +
                    "\r\n<auta>" +
                    "\r\n  <auto model='Škoda Felicia>'" +
                    "\r\n    <datum_prodeje></datum_prodeje>" +
                    "\r\n    <cena></cena>" +
                    "\r\n    <dph></dph>" +
                    "\r\n  </auto>" +
                    "\r\n</auta>" +
                    $"\r\n\r\nZpráva chyby:\r\n{nullEx.Message}\r\n\r\nŘádek výskytu:\r\n{nullEx.StackTrace}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Převede cenu modelu na formát měny v OutputDefault datagridu
        /// </summary>
        public class CurrencyConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null) { return null!; }
                double price = (double)value;
                return price.ToString("C", culture);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}