using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Liquid
{
    public class Test : Control, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(Test), new PropertyMetadata(NotifyPropertyChanged));

        public static void NotifyPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            Test control = sender as Test;
            if (control.PropertyChanged != null)
            {
                control.PropertyChanged(control, new PropertyChangedEventArgs("Text"));
            }
        }

        public Test()
        {
            DefaultStyleKey = this.GetType();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //CorePart = GetTemplateChild("Core") as FrameworkElement;
            var TextPart = GetTemplateChild("Core") as TextBox;
            //GoToState(false);
            if (TextPart.Text != null)
            {
                TextPart.TextChanged += new TextChangedEventHandler(TextPart_TextChanged);
            }
        }

        void TextPart_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox control = sender as TextBox;
            if (control.Text != null)
            {
                this.Text = control.Text;
            }
        } 
    }
}
