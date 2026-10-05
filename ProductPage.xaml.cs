using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BashkircevObuv
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();

            UpdateProducts();

            var currentServices = BashkircevObuvEntities.GetContext().Products.ToList();

            ProductListView.ItemsSource = currentServices;

            if (CurrentUserClass.user == null)
            {
                OrderButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                OrderButton.Visibility = Visibility.Visible;
            }

            if (CurrentUserClass.user == null)
            {
                SearchBox.IsEnabled = false;
                SortCombo.IsEnabled = false;
                OrderButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchBox.IsEnabled = true;
                SortCombo.IsEnabled = true;
                OrderButton.Visibility = Visibility.Visible;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void UpdateProducts()
        {
            var products = BashkircevObuvEntities.GetContext()
                .Products.ToList();

            // Поиск
            string search = SearchBox.Text.Trim().ToLower();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(search) || p.Manufacturers.ManufacturerName.ToLower().Contains(search) || p.Category.CategoryName.ToLower().Contains(search) ||
                    (p.ProductDescription != null &&
                     p.ProductDescription.ToLower().Contains(search))
                ).ToList();
            }

            // Сортировка
            switch (SortCombo.SelectedIndex)
            {
                case 0:
                    products = products.OrderBy(p => p.ProductName).ToList();
                    break;

                case 1:
                    products = products.OrderByDescending(p => p.ProductName).ToList();
                    break;

                case 2:
                    products = products.OrderBy(p => p.ProductCost).ToList();
                    break;

                case 3:
                    products = products.OrderByDescending(p => p.ProductCost).ToList();
                    break;
            }

            ProductListView.ItemsSource = products;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserClass.user == null)
            {
                MessageBox.Show("Для оформления заказа необходимо войти в систему.");
                return;
            }

        }
    }
}
