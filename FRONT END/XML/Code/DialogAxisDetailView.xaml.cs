using Flyhouse.Globals;
using Flyhouse.UI.Dialogs.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Flyhouse.UI.Dialogs.View
{
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class DialogAxisDetailView : UserControl
    {
        public DialogAxisDetailView()
        {
            InitializeComponent();
            App.IsDisableKeyboard = true;
        }
        private bool _hasValidURI;

        public bool HasValidURI
        {
            get { return _hasValidURI; }
            set { _hasValidURI = value; OnPropertyChanged("HasValidURI"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {

            //var texbox = (TextBox)sender;
            //if (texbox != null && !string.IsNullOrEmpty(texbox.Text))
            //{
            //    texbox.Text = GlobalsUtility.convertMMToFt(Convert.ToDouble(texbox.Text));
            //}
            DialogAxisSetupGlobalViewModel model = this.DataContext as DialogAxisSetupGlobalViewModel;
            if (model != null)
            {
                //model.KeyboardMode = Enum.KeyboardModes.None;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DialogAxisSetupGlobalViewModel model = this.DataContext as DialogAxisSetupGlobalViewModel;
            if (model != null)
            {
                model.CurrentKeyboardMode = Enum.KeyboardModes.Numeric;
            }
        }

        private void AlphaTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DialogAxisSetupGlobalViewModel model = this.DataContext as DialogAxisSetupGlobalViewModel;
            if (model != null)
            {
                model.CurrentKeyboardMode = Enum.KeyboardModes.Alphanumeric;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            App.IsDisableKeyboard = false;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Uri uri;
            HasValidURI = Uri.TryCreate((sender as TextBox).Text, UriKind.Absolute, out uri);
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Uri uri;
            string text = (sender as TextBox).Text;
            if (string.IsNullOrWhiteSpace(text) == false)
            {
                if (Uri.TryCreate(text, UriKind.Absolute, out uri))
                {
                    Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
                }
                else
                {
                    if (text.ToLowerInvariant().StartsWith("http://") || text.ToLowerInvariant().StartsWith("https://"))
                    {
                        Process.Start(new ProcessStartInfo(text));
                    }
                    else
                    {
                        using (Process process = new Process())
                        {
                            string link = $"https://{text}/Tc3PlcHmiWeb/Port_851/Visu/kid.htm";
                            process.StartInfo.UseShellExecute = true;
                            process.StartInfo.FileName = link;
                            process.Start();
                        }
                    }
                }
            }
        }


        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Regex regex = new Regex("[^0-9.]+");
            //e.Handled = regex.IsMatch(e.Text);
        }
    }
}
