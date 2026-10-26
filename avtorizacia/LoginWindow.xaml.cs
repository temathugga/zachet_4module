using System;
using System.Collections.Generic;
using System.Data.Entity;
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

namespace avtorizacia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            ShowPuzzle();
        }
        private int[] order = { 2, 0, 3, 1 };

        private int firstSlot = -1;

        private BitmapImage GetPiece(int pieceNumber)
        {
            string path = "Images/" + (pieceNumber + 1) + ".png";
            return new BitmapImage(new Uri(path, UriKind.Relative));
        }

        private void ShowPuzzle()
        {
            slot0Image.Source = GetPiece(order[0]);
            slot1Image.Source = GetPiece(order[1]);
            slot2Image.Source = GetPiece(order[2]);
            slot3Image.Source = GetPiece(order[3]);
        }

        private bool IsPuzzleSolved()
        {
            if (order[0] == 0 && order[1] == 1 && order[2] == 2 && order[3] == 3)
            {
                return true;
            }
            return false;
        }

        private void ClickSlot(int slot)
        {
            if (firstSlot == -1)
            {
                firstSlot = slot;
            }
            else
            {
                int temp = order[firstSlot];
                order[firstSlot] = order[slot];
                order[slot] = temp;

                firstSlot = -1;   
                ShowPuzzle();     
            }
        }

        private void Slot0_Click(object sender, MouseButtonEventArgs e) { ClickSlot(0); }
        private void Slot1_Click(object sender, MouseButtonEventArgs e) { ClickSlot(1); }
        private void Slot2_Click(object sender, MouseButtonEventArgs e) { ClickSlot(2); }
        private void Slot3_Click(object sender, MouseButtonEventArgs e) { ClickSlot(3); }


        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = loginTextBox.Text.Trim();
                string password = passwordBox.Password;

                if (login == "" || password == "")
                {
                    MessageBox.Show("Заполните оба поля: Логин и Пароль.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new zachet_bdEntities())
                {
                    Users user = db.Users.FirstOrDefault(u => u.Login == login);

                    if (user == null)
                    {
                        MessageBox.Show("Неправильный логин или пароль", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (user.IsBlocked == 1)
                    {
                        MessageBox.Show("Вы заблокированы, обратитесь к администратору", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    bool passwordOk = (user.Password == password);
                    bool puzzleOk = IsPuzzleSolved();

                    if (puzzleOk && passwordOk)
                    {

                        user.FailedAttempts = 0;
                        db.SaveChanges();
                        MessageBox.Show("Вы успешно авторизовались!", "Успешная авторизация", MessageBoxButton.OK, MessageBoxImage.Information);

                        if (user.Role == "Администратор")
                        {
                            AdminWindow adminWindow = new AdminWindow();
                            adminWindow.Show();
                        }
                        else
                        {
                            UserWindow userWindow = new UserWindow();
                            userWindow.Show();
                        }
                        Close();
                        return;
                    }

                    user.FailedAttempts = user.FailedAttempts + 1;

                    if (user.FailedAttempts >= 3)
                    {
                        user.IsBlocked = 1;
                        db.SaveChanges();
                        MessageBox.Show("Вы заблокированы, обратитесь к администратору", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                    else
                    {

                        if (puzzleOk == false)
                        {
                            MessageBox.Show("Пазл собран неверно. Расставьте фрагменты так, чтобы получилась целая картинка.",
                                "Капча не пройдена", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            MessageBox.Show("Неправильный логин или пароль", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        db.SaveChanges();
                    }
                }
            }




            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к БД: " + ex.Message, "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}