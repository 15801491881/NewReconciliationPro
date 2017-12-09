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
using System.Windows.Forms;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;

namespace ReconciliatlnPro
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
       
        MerageFilesHelper helper = null;

        public MainWindow()
        {
            InitializeComponent();
            // to do
        }

        private void button1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("click");
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = dialog.SelectedPath;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            setPath(textBox1);
        }

        
        private void setPath(System.Windows.Controls.TextBox textBox)
        {
            Console.WriteLine("setPaths");
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox.Text = dialog.SelectedPath;
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Console.Write("关闭窗口");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            setPath(textBox2);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            setPath(textBox3);
        }
        private delegate void MerageFilesHandle(string templateDir, string dataDir, string targetDir);
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
           
            Console.WriteLine("开始合并......."+Thread.CurrentThread);
            // MessageBox.Show("开始合并.......", "提示");
            //  string beginDateStr = beginDate.Text.ToString();
            string beginDateStr = Convert.ToDateTime(beginDate.Text).ToString("yyyy-MM-dd");
            Console.WriteLine("begin date:" + beginDateStr);
            Console.WriteLine("开始合并......." + Thread.CurrentThread);
            // MerageFilesHandle handle = new MerageFilesHandle(helper.MerageFiles);
            button3.IsEnabled = false;
            helper = new MerageFilesHelper();
           // beginImport();
            dataGrid.DataContext = await helper.MerageFiles(textBox1.Text, textBox2.Text, textBox3.Text);
           
            System.Windows.Forms.MessageBox.Show("完成合并", "提示", MessageBoxButtons.YesNo);
            Console.WriteLine("完成");
            button3.IsEnabled = true;

        }
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateLeableDelegate(string value);
        

        private void beginImport()
        {
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar1.SetValue);
            for (int i = 0; i < 100; i++)
            {
                Dispatcher.BeginInvoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(i + 1) });
                label1.Content = i;
                Thread.Sleep(1000);
            }
        }

    }
}
