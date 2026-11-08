
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BashkircevObuv
{
    public partial class OrderPage : Page
    {
        DataTable cart = new DataTable();

        public OrderPage()
        {
            InitializeComponent();

            cart.Columns.Add("StockItemID", typeof(int));
            cart.Columns.Add("ProductName", typeof(string));
            cart.Columns.Add("Size", typeof(string));
            cart.Columns.Add("Quantity", typeof(int));
            cart.Columns.Add("Price", typeof(decimal));
            cart.Columns.Add("Total", typeof(decimal));

            CartGrid.ItemsSource = cart.DefaultView;

            ProductCombo.ItemsSource = BashkircevObuvEntities
                .GetContext().Products.ToList();
        }

        private void ProductCombo_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            Products product = ProductCombo.SelectedItem as Products;

            if (product == null)
                return;

            SizeCombo.ItemsSource = BashkircevObuvEntities
                .GetContext().StockItems
                .Where(x => x.ProductID == product.ProductID)
                .ToList()
                .Where(x => int.TryParse(x.QuantityAvailable, out int count)
                            && count > 0)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Products product = ProductCombo.SelectedItem as Products;
            StockItems stock = SizeCombo.SelectedItem as StockItems;

            if (product == null || stock == null)
            {
                MessageBox.Show("Выберите модель и размер.");
                return;
            }

            if (!int.TryParse(QuantityBox.Text, out int quantity)
                || quantity <= 0)
            {
                MessageBox.Show("Введите правильное количество пар.");
                return;
            }

            if (!int.TryParse(stock.QuantityAvailable, out int available))
            {
                MessageBox.Show("Не удалось определить остаток товара.");
                return;
            }

            int alreadyAdded = 0;

            foreach (DataRow row in cart.Rows)
            {
                if ((int)row["StockItemID"] == stock.StockItemID)
                    alreadyAdded += (int)row["Quantity"];
            }

            if (quantity + alreadyAdded > available)
            {
                MessageBox.Show("На складе недостаточно пар этого размера.");
                return;
            }

            cart.Rows.Add(
                stock.StockItemID,
                product.ProductName,
                stock.Sizes.SizeValue,
                quantity,
                product.ProductCost,
                product.ProductCost * quantity
            );

            UpdateTotal();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CartGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите позицию для удаления.");
                return;
            }

            DataRowView row = CartGrid.SelectedItem as DataRowView;

            if (row != null)
                cart.Rows.Remove(row.Row);

            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = 0;

            foreach (DataRow row in cart.Rows)
                total += (decimal)row["Total"];

            TotalText.Text = "Итого: " + total + " руб.";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserClass.user == null)
            {
                MessageBox.Show("Для оформления заказа необходимо войти.");
                return;
            }

            if (cart.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте обувь в заказ.");
                return;
            }

            var context = BashkircevObuvEntities.GetContext();

            decimal total = 0;

            foreach (DataRow row in cart.Rows)
                total += (decimal)row["Total"];

            Orders order = new Orders
            {
                OrderDate = DateTime.Today,
                TotalAmount = total.ToString(),
                UserID = CurrentUserClass.user.UserID
            };

            context.Orders.Add(order);
            context.SaveChanges();

            foreach (DataRow row in cart.Rows)
            {
                int stockItemID = (int)row["StockItemID"];
                int quantity = (int)row["Quantity"];

                StockItems stock = context.StockItems
                    .FirstOrDefault(x => x.StockItemID == stockItemID);

                if (stock == null
                    || !int.TryParse(stock.QuantityAvailable, out int available)
                    || available < quantity)
                {
                    MessageBox.Show(
                        "Остатки товара изменились. Проверьте наличие и оформите заказ заново.");
                    return;
                }

                OrderItems item = new OrderItems
                {
                    OrderID = order.OrderID,
                    StockItemID = stockItemID,
                    Quantity = quantity.ToString(),
                    PricePerUnit = ((decimal)row["Price"]).ToString()
                };

                context.OrderItems.Add(item);

                stock.QuantityAvailable = (available - quantity).ToString();
            }

            context.SaveChanges();

            MessageBox.Show("Заказ успешно оформлен!");

            Manager.MainFrame.Navigate(new ProductPage());
        }
    }
}
