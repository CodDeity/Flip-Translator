using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Flip.Controls
{
    /// <summary>
    /// Interaction logic for FlipBox.xaml
    /// </summary>
    public partial class FlipBox : UserControl
    {
        public FlipBox()
        {
            InitializeComponent();
            this.textbox.DataContext = this;
        }
        public static readonly DependencyProperty IsReadonlyProperty =
       DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(FlipBox), new PropertyMetadata(false));
        public static readonly DependencyProperty IsNumericalInputProperty =
       DependencyProperty.Register("IsNumericalInput", typeof(bool), typeof(FlipBox), new PropertyMetadata(false));
        public static readonly DependencyProperty MinimumProperty =
       DependencyProperty.Register("Minimum", typeof(int), typeof(FlipBox), new PropertyMetadata(0));
        public static readonly DependencyProperty MaximumProperty =
       DependencyProperty.Register("Maximum", typeof(int), typeof(FlipBox), new PropertyMetadata(int.MaxValue));
        public static readonly DependencyProperty HelperContentProperty =
       DependencyProperty.Register("HelperContent", typeof(object), typeof(FlipBox), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty TextProperty =
      DependencyProperty.Register("Text", typeof(string), typeof(FlipBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty FontSizeProperty =
       DependencyProperty.Register("FontSize", typeof(double), typeof(FlipBox), new PropertyMetadata((double)12));
        public static readonly DependencyProperty FontFamilyProperty =
      DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(FlipBox), new FrameworkPropertyMetadata(new FontFamily("Arial")));

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object HelperContent
        {
            get { return (object)GetValue(HelperContentProperty); }
            set { SetValue(HelperContentProperty, value); }
        }
        public bool IsNumericalInput
        {
            get { return (bool)GetValue(IsNumericalInputProperty); }
            set { SetValue(IsNumericalInputProperty, value); }
        }
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(IsNumericalInputProperty, value); }
        }
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(IsNumericalInputProperty, value); }
        }
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadonlyProperty); }
            set { SetValue(IsReadonlyProperty, value); }
        }
        public string Text
        {
            get { return textbox.Text; }
            set { SetValue(TextProperty, value); OnPropertyChanged(); }
        }
        public double FontSize
        {
            get { return textbox.FontSize; }
            set { SetValue(FontSizeProperty, value); textbox.FontSize = value; OnPropertyChanged(); }
        }
        public FontFamily FontFamily
        {
            get { return textbox.FontFamily; }
            set { SetValue(FontFamilyProperty, value); textbox.FontFamily = value; OnPropertyChanged(); }
        }
        public bool Focus()
        {
            if (this.IsInitialized)
                return textbox.Focus();
            else return false;
        }
        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsNumericalInput)
            {
                if (textbox.Text.Length != 0 && !char.IsDigit(textbox.Text.Last()))
                {
                    textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1);
                }
                int num;
                if (Int32.TryParse(textbox.Text, out num))
                {
                    if (num > Maximum)
                    {
                        textbox.Text = Maximum.ToString();
                    }
                    else if (num < Minimum)
                    {
                        textbox.Text = Minimum.ToString();
                    }
                }
                else
                {
                    textbox.Text = "";
                }
            }
        }
    }
}
