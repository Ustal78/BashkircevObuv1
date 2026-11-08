
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


namespace BashkircevObuv
{
    public partial class AnalyticsPage : Page
    {
        private List<ReportRow> topProducts = new List<ReportRow>();

        public AnalyticsPage()
        {
            InitializeComponent();

            if (!CurrentUserClass.IsAdmin)
            {
                MessageBox.Show("Доступ разрешён только администратору.");
                Manager.MainFrame.Navigate(new ProductPage());
                return;
            }

            DateTime today = DateTime.Today;

            StartDatePicker.SelectedDate =
                new DateTime(today.Year, today.Month, 1);

            EndDatePicker.SelectedDate =
                new DateTime(today.Year, today.Month,
                    DateTime.DaysInMonth(today.Year, today.Month));

            UpdateReport();
        }

        private void DatePicker_SelectedDateChanged(
            object sender, SelectionChangedEventArgs e)
        {
            UpdateReport();
        }

        private void UpdateReport()
        {
            if (StartDatePicker.SelectedDate == null ||
                EndDatePicker.SelectedDate == null)
                return;

            DateTime start = StartDatePicker.SelectedDate.Value.Date;
            DateTime end = EndDatePicker.SelectedDate.Value.Date;

            if (start > end)
            {
                ErrorText.Text =
                    "Ошибка: дата начала позже даты окончания.";

                OrdersCountText.Text = "Количество заказов: —";
                SalesTotalText.Text = "Сумма продаж: —";
                TopGrid.ItemsSource = null;
                topProducts.Clear();
                return;
            }

            ErrorText.Text = "";

            var context = BashkircevObuvEntities.GetContext();

            var orders = context.Orders
                .Where(o => o.OrderDate >= start &&
                            o.OrderDate <= end)
                .ToList();

            OrdersCountText.Text =
                "Количество заказов: " + orders.Count;

            var orderIds = orders.Select(o => o.OrderID).ToList();

            var items = context.OrderItems
                .Where(i => orderIds.Contains(i.OrderID))
                .ToList();

            decimal salesTotal = 0;

            foreach (var item in items)
            {
                int quantity;
                decimal price;

                if (int.TryParse(item.Quantity, out quantity) &&
                    decimal.TryParse(item.PricePerUnit, out price))
                {
                    salesTotal += quantity * price;
                }
            }

            SalesTotalText.Text =
                "Сумма продаж: " + salesTotal.ToString("N2") + " руб.";

            var stockItems = context.StockItems.ToList();
            var products = context.Products.ToList();

            var rows = new List<ReportRow>();

            foreach (var group in items.GroupBy(i => i.StockItemID))
            {
                var stock = stockItems.FirstOrDefault(
                    s => s.StockItemID == group.Key);

                if (stock == null)
                    continue;

                var product = products.FirstOrDefault(
                    p => p.ProductID == stock.ProductID);

                if (product == null)
                    continue;

                int sold = 0;

                foreach (var item in group)
                {
                    int quantity;

                    if (int.TryParse(item.Quantity, out quantity))
                        sold += quantity;
                }

                rows.Add(new ReportRow
                {
                    ProductName = product.ProductName,
                    Manufacturer = product.Manufacturers.ManufacturerName,
                    Quantity = sold
                });
            }

            topProducts = rows
                .GroupBy(p => new
                {
                    p.ProductName,
                    p.Manufacturer
                })
                .Select(g => new ReportRow
                {
                    ProductName = g.Key.ProductName,
                    Manufacturer = g.Key.Manufacturer,
                    Quantity = g.Sum(p => p.Quantity)
                })
                .OrderByDescending(p => p.Quantity)
                .Take(5)
                .ToList();

            TopGrid.ItemsSource = topProducts;
        }

        private class ReportRow
        {
            public string ProductName { get; set; }
            public string Manufacturer { get; set; }
            public int Quantity { get; set; }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null ||
                EndDatePicker.SelectedDate == null)
                return;

            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.");
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PDF файл (*.pdf)|*.pdf";
            dialog.FileName = "Отчет_по_продажам.pdf";

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                Document document = new Document();
                PdfWriter.GetInstance(
                    document, new FileStream(dialog.FileName, FileMode.Create));

                document.Open();

                string fontPath = @"C:\Windows\Fonts\arial.ttf";

                BaseFont baseFont = BaseFont.CreateFont(
                    fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                Font titleFont = new Font(baseFont, 16);
                Font normalFont = new Font(baseFont, 12);

                document.Add(new Paragraph("Отчет по продажам", titleFont));
                document.Add(new Paragraph(
                    "Период: " +
                    StartDatePicker.SelectedDate.Value.ToString("dd.MM.yyyy") +
                    " — " +
                    EndDatePicker.SelectedDate.Value.ToString("dd.MM.yyyy"),
                    normalFont));

                document.Add(new Paragraph(
                    OrdersCountText.Text, normalFont));

                document.Add(new Paragraph(
                    SalesTotalText.Text, normalFont));

                document.Add(new Paragraph("Топ-5 моделей обуви:", normalFont));

                foreach (var product in topProducts)
                {
                    document.Add(new Paragraph(
                        product.ProductName + " — " +
                        product.Manufacturer + " — " +
                        product.Quantity + " пар",
                        normalFont));
                }

                document.Close();

                MessageBox.Show("Отчёт сохранён в PDF.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта: " + ex.Message);
            }
        }
    }
}
