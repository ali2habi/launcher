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

using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using FireSharp.Interfaces;
using FireSharp;
using FireSharp.Config;
using FireSharp.Response;
using Firebase.Storage;
using System.Diagnostics.Eventing.Reader;

using SCREW.Auth.System4TT;
using System.Net;
using System.Xml.Linq;

namespace launcher
{
    public partial class MainWindow : Window
    {
        InitConfigsSys LauncherSys = new InitConfigsSys();
        public MainWindow()
        {
            InitializeComponent();
            ShowLoginScreen();
        }
        public void ShowLoginScreen()
        {
            WorkSpace.Visibility = Visibility.Hidden;
            RegisterScreen.Visibility = Visibility.Hidden;
            LoginScreen.Visibility = Visibility.Visible;
        }
        public async void ShowWorkspace()
        {
            WorkSpace.Visibility = Visibility.Visible;
            LoginScreen.Visibility = Visibility.Hidden;
            RegisterScreen.Visibility = Visibility.Hidden;

            var _number = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/PhoneNumber");
            var _age = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Age");
            var _username = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Name");
            var _address = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Address");
            //var _avatar = await LauncherSys.GetFirebaseClient().GetAsync($"screw-launcher.appspot.com/user_avatars/sigma.jpg");

            UserAge.Content = _age.ResultAs<string>();
            UserID.Content = LauncherSys.GetUserCredential().User.Uid;
            Usermail.Content = LauncherSys.GetUserCredential().User.Info.Email;
            UserNumber.Content = _number.ResultAs<string>();
            profile_name.Text = _username.ResultAs<string>();
            UserAddress.Content = _address.ResultAs<string>();
            //profile_avatar.Source = _avatar.ResultAs<ImageSource>();
        }
        private void OnExitClicked(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(0);
        }
        private async void TryToLogin(object sender, RoutedEventArgs e)
        {
            if (username.Text.Length > 0 && password.Password.Length > 0)
            {
                bool isLoggedIn = await LauncherSys.Login(username.Text, password.Password);
                if (isLoggedIn)
                {
                    ShowWorkspace();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!");
                }
            }
            else
            {
                MessageBox.Show("Неверное заполены поля!");
            }
        }
        
        private void OnProfileClicked(object sender, RoutedEventArgs e)
        {

        }

        private async void TryToCreate(object sender, RoutedEventArgs e)
        {
            if (username_.Text.Length > 0 && password_.Text.Length > 0)
            {
                bool isRegisterIn = await LauncherSys.Create(username.Text, password.Password);
                if (isRegisterIn)
                {
                    ShowWorkspace();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!");
                }
            }
            else
            {
                MessageBox.Show("Неверное заполены поля!");
            }
        }
        private void ShowRegistrationScreen(object sender, RoutedEventArgs e)
        {
            WorkSpace.Visibility = Visibility.Hidden;
            RegisterScreen.Visibility = Visibility.Visible;
            LoginScreen.Visibility = Visibility.Hidden;
            reg2screen.Visibility = Visibility.Hidden;
        }
        private void ShowReg_2_Screen(object sender, RoutedEventArgs e)
        {
            WorkSpace.Visibility = Visibility.Hidden;
            reg1screen.Visibility = Visibility.Hidden;
            LoginScreen.Visibility = Visibility.Hidden;
            reg2screen.Visibility = Visibility.Visible;
        }

        //private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        //{
        //    if (launcher.Properties.Settings.Default.login != null && launcher.Properties.Settings.Default.password != null)
        //    {
        //        bool isAuthed = await LauncherSys.Login(launcher.Properties.Settings.Default.login, launcher.Properties.Settings.Default.password);
        //    if (isAuthed)
        //        {
        //            ShowWorkspace();
        //        }
        //    }
        //}
    }
    public class InitConfigsSys
    {
        static FirebaseAuthClient client;
        static FireSharp.FirebaseClient firebaseClient;
        static UserCredential userCredential;
        static FirebaseStorage storage;
        public InitConfigsSys()
        {
            InitConfigs();
        }

        public FireSharp.FirebaseClient GetFirebaseClient()
        {
            return firebaseClient;
        }
        public UserCredential GetUserCredential()
        {
            return userCredential;
        }
        private void InitConfigs()
        {
            FirebaseAuthConfig config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyAEF_kcSK3w1Bg11_cqZrtxWj7UPoP_Iio",
                AuthDomain = "screw-launcher.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
            {
                    new EmailProvider()
            },
                UserRepository = new FileUserRepository("test")
            };

            client = new FirebaseAuthClient(config);
        }
        private void InitConfigFirebase()
        {
            string _authSecret = null;

            if (userCredential != null && userCredential.User != null && userCredential.User.Credential != null)
            {
                _authSecret = userCredential.User.Credential.IdToken;
            }

            IFirebaseConfig firebaseConfig = new FireSharp.Config.FirebaseConfig
            {
                RequestTimeout = TimeSpan.FromDays(1),
                BasePath = "https://screw-launcher-default-rtdb.firebaseio.com/",
                AuthSecret = _authSecret
            };
            firebaseClient = new FirebaseClient(firebaseConfig);
        }
        private void InitConfigFirebaseStorage()
        {
            storage = new FirebaseStorage("screw-launcher.appspot.com", new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(userCredential.User.Credential.IdToken)
            });
        }
        public async Task<bool> Login(string username, string password)
        {
            try
            {
                userCredential = await client.SignInWithEmailAndPasswordAsync(username, password);
                InitConfigFirebaseStorage();
                InitConfigFirebase();
                launcher.Properties.Settings.Default.login = username;
                launcher.Properties.Settings.Default.password = password;
                launcher.Properties.Settings.Default.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> Create(string username, string password)
        {
            try
            {
                userCredential = await client.CreateUserWithEmailAndPasswordAsync(username, password);
                InitConfigFirebaseStorage();
                InitConfigFirebase();
                launcher.Properties.Settings.Default.login = username;
                launcher.Properties.Settings.Default.password = password;


                //var userInfo = new
                //{
                //    UserId = userCredential.User.Uid,
                //    Name = _name.Content,
                //    Address = address,
                //    Age = age,
                //    PhoneNumber = phoneNumber
                //};
                //await firebaseClient.SetAsync($"Information/{userCredential.User.Uid}", userInfo);


                launcher.Properties.Settings.Default.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
