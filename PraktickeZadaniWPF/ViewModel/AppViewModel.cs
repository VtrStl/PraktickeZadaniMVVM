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
        /// Načte XML soubor a ověří, jestli je XML soubor ve správném formatu
        /// </summary>
        private void LoadXml()
        {            
            try
            {
                OpenFileDialog dialog = new() { DefaultExt = "xml", Filter = "XML Files (*.xml)|*.xml" };
                if (dialog.ShowDialog() == true)
                {
                    XmlDocument xmlDoc = new();
                    xmlDoc.Load(dialog.FileName);
                    XmlNodeList autoNodes = xmlDoc.GetElementsByTagName("auto");
                    LoadAndCalculateXmlToDg(autoNodes);
                }
            }
            
            catch (FormatException formatEx)
            {
                AppModel.ExceptionMessageBox(formatEx);
            }
                                    
            catch (Exception ex)
            {
                AppModel.ExceptionMessageBox(ex);
            }
        }

        /// <summary>
        /// Po načtení XML souboru se provedou potřebné výpočty, které probíhají v AppModel.cs a načte hodnoty do obou datagridu.
        /// V případě, že XML tabulka neodpovídá stromové struktůře s kterou se počítá, informuje uživatele, jaký stromový XML formát je podporovaný. 
        /// </summary>
        private void LoadAndCalculateXmlToDg(XmlNodeList autoNodes)
        {
            try
            {                
                ObservableCollection<CarModel> carmodels = new();
                ObservableCollection<CarWeekendSalesModel> carWeekendSalesModels = new();
                AppModel appModel = new(carmodels, carWeekendSalesModels);
                OutputDefault = appModel.LoadData(autoNodes);
                OutputCalculated = appModel.CalculateWeekendSales();
            }
            catch (ArgumentNullException nullEx)
            {
                AppModel.ExceptionMessageBox(nullEx);
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