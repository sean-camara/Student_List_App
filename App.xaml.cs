using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace MAUIApp7
{
    public partial class App : Application
    {
        public static StudentRepository StudentRepo { get; private set; }

        public App(StudentRepository repo)
        {
            InitializeComponent();

            // Force Light theme for consistent look regardless of system settings
            Application.Current.UserAppTheme = AppTheme.Light;

            StudentRepo = repo;
            MainPage = new AppShell();
        }
    }
}