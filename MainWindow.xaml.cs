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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using Microsoft.Win32;
using iTextSharp.text.html;

namespace WPFDynamicTab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<TabItem> _tabItems;
        private TabItem _tabAdd;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                // initialize tabItem array
                _tabItems = new List<TabItem>();

                //// add a tabItem with + in header 
                //_tabAdd = new TabItem();
                //_tabAdd.Header = "+";
                //// tabAdd.MouseLeftButtonUp += new MouseButtonEventHandler(tabAdd_MouseLeftButtonUp);

                //_tabItems.Add(_tabAdd);

                // add first tab
                this.AddTabItem();

                // bind tab control
                tabDynamic.DataContext = _tabItems;

                tabDynamic.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private TabItem AddTabItem()
        {
            int count = _tabItems.Count;

            // create new tab item
            TabItem tab = new TabItem();
            
            tab.Header ="Start";
            tab.Name = "Start";
            tab.HeaderTemplate = tabDynamic.FindResource("TabHeader") as DataTemplate;

            tab.MouseDoubleClick += new MouseButtonEventHandler(tab_MouseDoubleClick);

            // add controls to tab item, this case I added just a textbox
            TilesExample tiles = new TilesExample();
            tab.Content = tiles;

            // insert tab item right before the last (+) tab item
            _tabItems.Add(tab);

            return tab;
        }

        private void tabAdd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // clear tab control binding
            tabDynamic.DataContext = null;

            TabItem tab = this.AddTabItem();

            // bind tab control
            tabDynamic.DataContext = _tabItems;

            // select newly added tab item
            tabDynamic.SelectedItem = tab;
        }

        private void tab_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TabItem tab = sender as TabItem;

            TabProperty dlg = new TabProperty();

            // get existing header text
            dlg.txtTitle.Text = tab.Header.ToString();

            if (dlg.ShowDialog() == true)
            {
                // change header text
                tab.Header = dlg.txtTitle.Text.Trim();
            }
        }

        private void tabDynamic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tab = tabDynamic.SelectedItem as TabItem;
            if (tab == null) return;

            if (tab.Equals(_tabAdd))
            {
                // clear tab control binding
                tabDynamic.DataContext = null;

                TabItem newTab = this.AddTabItem();

                // bind tab control
                tabDynamic.DataContext = _tabItems;

                // select newly added tab item
                tabDynamic.SelectedItem = newTab;
            }
            else
            {
                // your code here...
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string tabName = (sender as Button).CommandParameter.ToString();

            var item = tabDynamic.Items.Cast<TabItem>().Where(i => i.Name.Equals(tabName)).SingleOrDefault();

            TabItem tab = item as TabItem;

            if (tab != null)
            {
                if (_tabItems.Count < 2)
                {
                    MessageBox.Show("Cannot remove last tab.");
                }
                else if (MessageBox.Show(string.Format("Are you sure you want to remove the tab '{0}'?", tab.Header.ToString()),
                    "Remove Tab", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // get selected tab
                    TabItem selectedTab = tabDynamic.SelectedItem as TabItem;

                    // clear tab control binding
                    tabDynamic.DataContext = null;

                    _tabItems.Remove(tab);

                    // bind tab control
                    tabDynamic.DataContext = _tabItems;

                    //// select previously selected tab. if that is removed then select first tab
                    //if (selectedTab == null || selectedTab.Equals(tab))
                    //{
                    //    selectedTab = _tabItems[0];
                    //}
                    //tabDynamic.SelectedItem = selectedTab;
                }
            }
        }

        private void btnAddItem_Click_1(object sender, RoutedEventArgs e)
        {
            TabItem tab = new TabItem();

            tab.Header = string.Format("Tab {0}", _tabItems.Count);
            tab.Name = string.Format("tab{0}", _tabItems.Count);
            tab.HeaderTemplate = tabDynamic.FindResource("TabHeader") as DataTemplate;

            tab.MouseDoubleClick += new MouseButtonEventHandler(tab_MouseDoubleClick);

            // add controls to tab item, this case I added just a textbox
            Page2 page = new Page2(string.Format("Tab {0}", _tabItems.Count));

            tab.Content = page;

         
            tabDynamic.DataContext = null;

            // insert tab item right before the last (+) tab item
            _tabItems.Add(tab);

            // bind tab control
            tabDynamic.DataContext = _tabItems;

            tabDynamic.SelectedIndex = tabDynamic.Items.Count - 1;

        }

        private void btnGetPDF_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.ShowDialog();
            if (dlg.FileName != null)
            {
                
                MemoryStream ms = createPDF(File.ReadAllText(dlg.FileName));
                if (ms != null)
                {
                    var testFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.pdf");
                    if (File.Exists(testFile))
                        File.Delete(testFile);

                    System.IO.File.WriteAllBytes(testFile, ms.ToArray());
                }
            }
        }

        private MemoryStream createPDF(string html)
        {
            MemoryStream msOutput = new MemoryStream();
            TextReader reader = new StringReader(html);

            // step 1: creation of a document-object
            Document document = new Document(PageSize.A4, 30, 30, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(document, msOutput);

            // step 3: we create a worker parse the document
            HTMLWorker worker = new HTMLWorker(document);


            StyleSheet styles = new StyleSheet();

            styles.LoadTagStyle(HtmlTags.H1, HtmlTags.FONTSIZE, "16");
            styles.LoadTagStyle(HtmlTags.H1, HtmlTags.FONTSTYLE, "italic");
            styles.LoadTagStyle(HtmlTags.H1, HtmlTags.COLOR, "#ff0000");
            styles.LoadTagStyle(HtmlTags.UL, HtmlTags.INDENT, "10");
            styles.LoadTagStyle(HtmlTags.LI, HtmlTags.LEADING, "16");
                List<IElement> objects = HTMLWorker.ParseToList(
                  reader, styles
                );

            document.Open();
            worker.StartDocument();

            foreach (IElement element in objects)
                {
                    document.Add(element);
                }

            worker.EndDocument();
            worker.Close();
            document.Close();

            return msOutput;
        }
    }

    

    public class CloseButtonVisibility:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TabControl tc = parameter as TabControl;
            if (tc.Items.Count == 1)

                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
