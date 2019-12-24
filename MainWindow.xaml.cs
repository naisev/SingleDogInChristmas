using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Net;

namespace SingleDogInChristmas
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<WeiboData> datas = new ObservableCollection<WeiboData>();
        public static System.Timers.Timer timer = new System.Timers.Timer(10000);
        public MainWindow()
        {
            InitializeComponent();
            DataGrid.AutoGenerateColumns = false;//不自动创建列
            DataGrid.ItemsSource = datas;
        }

        

        private void BtnDebug_Click(object sender, RoutedEventArgs e)
        {
            if (timer.Enabled)
            {
                EndTimer();
                BtnDebug.Content = "开始\n抓取";
            }
            else
            {
                GetWeiBoData();
                BeginTimer();
                BtnDebug.Content = "停止\n抓取";
            }
        }

        private void GetWeiBoData()
        {
            
            //设置url和contentType
            //string url = "https://s.weibo.com/weibo?q=%E5%88%86%E6%89%8B&page=1";
            string url = "https://s.weibo.com/weibo?q=%E5%88%86%E6%89%8B&nodup=1";
            string contentType = "text/html; charset=UTF-8";

            //将用户的cookie放入容器
            CookieCollection cookie;
            try
            {
                cookie = SplitCookie(TxtCookie.Text);
            }
            catch
            {
                MessageBox.Show("cookie输入错误，请重新检查！");
                return;
            }


            //post访问
            byte[] webData = HttpWebClient.Post(url, new byte[0], contentType, "", ref cookie);

            //返回数据编码
            string strData = Encoding.UTF8.GetString(webData);
            List<string> strList = StrongString.BetweenArr(strData, "<p class=\"txt\" node-type=\"feed_list_content\"", "</p>");
            //对数据去重
            Deduplication(strList);

            //放入数据源
            //datas.Clear();
            for (int i = 0; i < strList.Count; i++)
            {
                WeiboData tmp = new WeiboData();
                tmp.Time = DateTime.Now.ToString();
                //格式化内容（防止出现太多的html代码
                tmp.Content = StrongString.GetRight(strList[i], ">");
                tmp.Content = tmp.Content.Replace("<em class=\"s-color-red\">", "");
                tmp.Content = tmp.Content.Replace("</em>", "");
                tmp.Content = tmp.Content.Replace("\n", "");

                //去重检测
                if (datas.Count == 0)
                {
                    datas.Insert(0, tmp);
                }
                else
                {
                    for (int j = 0; j < datas.Count; j++)
                    {
                        if (datas[j].Content == tmp.Content) { break; }
                        else if (j == datas.Count - 1)
                        {
                            datas.Insert(0, tmp);
                            break;
                        }
                    }
                }
            }
        }

        private CookieCollection SplitCookie(string str) 
        {
            str = str.Replace(" ", "");
            str = str.Replace("\n", "");
            str = str.Replace(",", "%2C");
            string[] everyCookie = str.Split(';');
            CookieCollection cookie = new CookieCollection();
            foreach (string tmp in everyCookie)
            {
                try
                {
                    cookie.Add(new Cookie(StrongString.GetLeft(tmp, "="), StrongString.GetRight(tmp, "=")));
                }catch(CookieException e)
                {
                    throw e;
                }
            }
            return cookie;
        }

        private static void Deduplication(List<string> strList)
        {
            for(int i = strList.Count - 1; i >= 0; i--)
            {
                if (strList[i].Contains("展开全文<i class=\"wbicon\">c</i>"))
                {
                    strList.RemoveAt(i);
                }
            }
        }


        private void TxtCookie_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtCookie.Text == "请输入Cookie")
            {
                TxtCookie.Text = "";
            }
        }

        private void TxtCookie_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TxtCookie.Text == "")
            {
                TxtCookie.Text = "请输入Cookie";
            }
        }

        private void Theout(object source, System.Timers.ElapsedEventArgs e)
        {
            TxtCookie.Dispatcher.Invoke(
               new Action(
                    delegate
                    {
                        GetWeiBoData();
                    }
               )
         );
        }

        private void BeginTimer()
        {
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Theout);
            timer.Enabled = true;
            timer.Start();
        }

        private void EndTimer()
        {
            timer.Stop();
        }
    }

    class WeiboData
    {
        public string Time { get; set; }
        public string Content { get; set; }
    }
}
