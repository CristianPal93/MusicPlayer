using MaterialDesignThemes.Wpf;
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

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        private bool isPlaying = false;
        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (System.Windows.Markup.XamlParseException e) {

                Console.WriteLine(e);
            }
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);

        }

        private void WindowMinimized(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            
        }

        private void WindowMaximized(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            PackIcon pack = (button.FindName("WindowSettings") as PackIcon);
            if (pack.Kind.Equals(PackIconKind.WindowMaximize))
            {
                this.WindowState = WindowState.Maximized;
                (button.FindName("WindowSettings") as PackIcon).Kind = PackIconKind.DockWindow;


            }
            else {
                this.WindowState = WindowState.Normal;
                (button.FindName("WindowSettings") as PackIcon).Kind = PackIconKind.WindowMaximize;


            }
            Console.WriteLine(pack.Kind);
            
        }

        private void Anterior_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Urmator_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            PackIcon pack = (button.FindName("PlayPauseIcon") as PackIcon);
            if (pack.Kind.Equals(PackIconKind.Play))
            {
                this.isPlaying = true;
                (button.FindName("PlayPauseIcon") as PackIcon).Kind = PackIconKind.Pause;

            }
            else {

                this.isPlaying = false;
                (button.FindName("PlayPauseIcon") as PackIcon).Kind = PackIconKind.Play;

            }


        }

    }
}
