using avtorizacia;
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
using System.Windows.Shapes;

namespace avtorizacia
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            roleComboBox.ItemsSource = new[] { "Администратор", "Пользователь" };
            roleComboBox.SelectedIndex = 1;
            LoadUsers();
        }

        private void LoadUsers()
        {
            using (var db = new zachet_bdEntities())
                usersDataGrid.ItemsSource = db.Users.ToList();
        }

        private bool InputOk()
        {
            if (string.IsNullOrWhiteSpace(loginTextBox.Text) || string.IsNullOrWhiteSpace(passwordBox.Password) || roleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Заполните логин, пароль и роль",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void usersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(usersDataGrid.SelectedItem is Users user)) return;

            loginTextBox.Text = user.Login;
            passwordBox.Password = user.Password;
            roleComboBox.SelectedItem = user.Role;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InputOk()) return;
            string login = loginTextBox.Text.Trim();

            using (var db = new zachet_bdEntities())
            {
                if (db.Users.Any(u => u.Login == login))
                {
                    MessageBox.Show("Такой логин уже существует",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                db.Users.Add(new Users
                {
                    Login = login,
                    Password = passwordBox.Password,
                    Role = roleComboBox.SelectedItem.ToString(),
                    IsBlocked = 0,
                    FailedAttempts = 0
                });
                db.SaveChanges();
            }
            LoadUsers();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(usersDataGrid.SelectedItem is Users selected))
            {
                MessageBox.Show("Выберите пользователя",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!InputOk()) return;
            string login = loginTextBox.Text.Trim();

            using (var db = new zachet_bdEntities())
            {
                var user = db.Users.Find(selected.Id);
                if (user == null) return;

                if (db.Users.Any(u => u.Login == login && u.Id != user.Id))
                {
                    MessageBox.Show("Логин занят другим пользователем",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                user.Login = login;
                user.Password = passwordBox.Password;
                user.Role = roleComboBox.SelectedItem.ToString();
                db.SaveChanges();
            }
            LoadUsers();
        }

        private void unblockButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(usersDataGrid.SelectedItem is Users selected))
            {
                MessageBox.Show("Выберите пользователя",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new zachet_bdEntities())
            {
                var user = db.Users.Find(selected.Id);
                if (user == null) return;

                if (user.IsBlocked == 0)
                {
                    MessageBox.Show("Пользователь не заблокирован",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                user.IsBlocked = 0;
                user.FailedAttempts = 0;
                db.SaveChanges();
            }
            LoadUsers();
            MessageBox.Show("Пользователь разблокирован",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

