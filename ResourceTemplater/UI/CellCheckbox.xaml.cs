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

namespace ResourceTemplater.UI
{
    /// <summary>
    /// Логика взаимодействия для CellCheckbox.xaml
    /// </summary>
    public partial class CellCheckbox : UserControl
    {
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(CellCheckbox),new PropertyMetadata() { DefaultValue = false });

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public CellCheckbox()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if(e.Property == IsCheckedProperty)
            {
                if ((bool)e.NewValue)
                    Background_Frame.Background = (SolidColorBrush)App.Current.Resources["Blue"];
                else
                    Background_Frame.Background = Brushes.Transparent;
            }
            base.OnPropertyChanged(e);
        }

        private void Background_Frame_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                IsChecked = !IsChecked;
        }
    }
}
