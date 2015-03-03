using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RehabilitationScoringSystemFinal
{
    /// <summary>
    /// skeleton.xaml 的互動邏輯
    /// </summary>
    public partial class skeleton : Window
    {
        public skeleton()
        {
            InitializeComponent();
            /*if (rb_up.IsChecked == true)   {
                checkBox1.IsChecked = checkBox1.Click();

            }
            else if (rb_down.IsChecked == true){}
            else if (rb_all.IsChecked == true) { }*/
                   
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Kinect Kinectwindow = new Kinect(this);
            this.Close();
            Kinectwindow.Show();
        }
    }
}
