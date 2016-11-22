using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Networking.Sockets;
using System.Threading;
using System.Text;
using System;
using System.IO;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using System.Net;
using Windows.Networking.Connectivity;
using System.Linq;
using Windows.UI.ViewManagement;
using ServerUI.Data;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace ServerUI
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        bool conn;
        public Graph sgraph;
        public Graph tgraph;
        public Graph ggraph;
        public Graph gfgraph;
        public Stack ConsStack = new Stack(20);
        int[] vals = new int[200];
        public Data.Inital start;
        public Grid GraphGrid { get { return SentGraph; } }
        public string Port { get { return port.Text; } }
        public string BindingSample { get { return "If you see mee, congrats!"; } }
        
        int mode = 0;
        int t = 0;
        DispatcherTimer disp = new DispatcherTimer();
        DispatcherTimer anim = new DispatcherTimer();

        Windows.UI.Core.CoreDispatcherPriority norm = Windows.UI.Core.CoreDispatcherPriority.Normal;
        private bool sideopn;
        public int cons_items { get { try { return Convert.ToInt32(ConsIttxt.Text); } catch { return 0; } } set { ConsIttxt.Text = value.ToString(); } }

        public async Task<int> control_values()
        {
            int a=0;
            await Dispatcher.RunAsync(norm, () => { a= (int) slider.Value; });
            return a;
        }
        public MainPage()
        {

            this.InitializeComponent();
            
            sgraph = new Graph(SentGraph);
            tgraph = new Graph(TimesGraph);
            ggraph = new Graph(GetGraph);
            gfgraph = new Graph(GetGraph);
            gfgraph.LineColor = Colors.DarkCyan;
            ggraph.Title = "Recieved Data";
            tgraph.Title = "Ping";
            sgraph.Title = "Sent Data";

           
            slider.Value = Data.Inital.slider_val;
            disp.Interval = new TimeSpan(0, 0, 1);
            disp.Tick += (s, e) =>
            {
                Status.Text = "Connection lost! Waiting...";
                Status.Foreground = new SolidColorBrush(Color.FromArgb(100, 250, 80, 80));
                port.IsEnabled = true;
            };
            anim.Interval = new TimeSpan(1000);
            anim.Tick += Anim_Tick;
            anim.Start();
        }

        private void Anim_Tick(object sender, object e)
        {
            t++;
            double ph = 2* t / 31.8;
            AnimCSize();
            byte v =(byte) (240 - 35 * Math.Pow(Math.Sin(ph/2), 24));
            Circle.Fill = new SolidColorBrush(Color.FromArgb(255,240,v,240));
        }


        public void slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            try
            {//slider.Header = "Value: " + slider.Value;
            }
            catch { };
        }


        public async void print(string data)
        {
            await Dispatcher.RunAsync(norm, () => { ConsStack.Push(data); cons.Text = ConsStack.ToString(); cons_items++; });
           
        }

        public async void status(string data)
        {
            await Dispatcher.RunAsync(norm, () => { Status.Text = data; });
        }
        public async void status(string data, Color color)
        {
            await Dispatcher.RunAsync(norm, () =>
            {
                Status.Text = data;
                Status.Foreground = new SolidColorBrush(color);
            });            
        }
        public async void ConnectedHandler()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Connected(); });
            
        }
        async private void Connected()
        {                              //TODO: add trigger to prevent playing animations simulateniously.
            anim.Stop(); int i = 0;
            int s = 8; double k =  2*(100 / s) / 31.8;
            while (i++ <s)
            {
                Circle.Height += 30 * ( Math.Sin(k * i) );
                Circle.Width += 30 * (Math.Sin(k * i));
                byte v = (byte)(240 - 175 * Math.Pow(Math.Sin(k*i/4 + 1),4 ));
                Circle.Fill = new SolidColorBrush(Color.FromArgb(255, v, 240, v));
                await Task.Delay(50);
            }
            anim.Start();
        }
        private async void Start()
        {
            App ap = (App)App.Current;
            port.IsEnabled = false;
            mode++;
            if (await ap.CreateListener())
            {
                await Task.Delay(100);// If successfully connected, play green animation
                anim.Stop();
                int i = 0;
                while (i < 20)
                {
                    i++; t++;
                    byte v = (byte)(240 - 105 * Math.Pow(Math.Sin(2 * t / 31.8 + 1), 24));
                    Circle.Fill = new SolidColorBrush(Color.FromArgb(255, v, 240, v));
                    AnimCSize();
                    await Task.Delay(20);
                }
                anim.Start();
            }
            else
            {
                anim.Stop();//  If not connected, play red animation
                int i = 0;
                while (i < 20)
                {
                    i++; t++;
                    byte v = (byte)(240 - 105 * Math.Pow(Math.Sin(2 * i / 31.8 + 1), 24));
                    Circle.Fill = new SolidColorBrush(Color.FromArgb(255, 240, v, v));
                    AnimCSize();
                    await Task.Delay(20);
                }
                anim.Start();
            }
        }

        internal async void myIP(string myIP)
        {

            await Dispatcher.RunAsync(norm, () => { ipadress.Text = ("my IP: " + myIP); });
        }

        private void AnimCSize()
        {
            double ph = 2 * t / 31.8;
            Circle.Height = 100 * (1 - Math.Cos(ph) / 10);
            Circle.Width = 100 * (1 - Math.Cos(ph) / 10);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Connected();
        }

        private void OpnSide_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sideopn)
            {
                sideopn = false;
                Side.Margin = new Thickness(0, 0, -240, 0);
                OpnSide.Fill = new SolidColorBrush(Colors.DarkBlue);
            }
            else
            {
                sideopn = true;
                Side.Margin = new Thickness(0, 0, 0, 0);
                OpnSide.Fill = new SolidColorBrush(Colors.Crimson);
            }
        }

        private void ConsDrop_Click(object sender, RoutedEventArgs e)
        {
            cons.Text = "Cleared console.";
            cons_items = 0;
        }

        private void run_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }
        private void Circle_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Start();
        }
    }
}
