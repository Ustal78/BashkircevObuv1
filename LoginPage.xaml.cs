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
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentUserClass.Logout();

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            mainWindow.UserText.Text = "Гость";
            mainWindow.LogoutButton.Visibility = Visibility.Visible;

            Manager.MainFrame.Navigate(new ProductPage());
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();

            if (string.IsNullOrEmpty(login))
            {
                ErrorText.Text = "Введите логин";
                return;
            }

            var user = BashkircevObuvEntities.GetContext().Users.FirstOrDefault(u => u.Login == login);

            if (user == null)
            {
                ErrorText.Text = "Пользователь с таким логином не найден";
                return;
            }

            CurrentUserClass.user = user;

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            mainWindow.UserText.Text = user.LastName + " " + user.FirstName;
            mainWindow.LogoutButton.Visibility = Visibility.Visible;

            Manager.MainFrame.Navigate(new ProductPage());
        }
    }
}