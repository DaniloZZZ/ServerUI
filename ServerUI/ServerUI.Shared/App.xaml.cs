using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Networking.Sockets;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.Foundation;


// Шаблон пустого приложения задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234227

namespace ServerUI
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif
        public MainPage mp; 
        bool Connected;
        public int[] got = new int[200];
        private int[] sent = new int[200];
        public int[] got_filterd = new int[200];

        /// <summary>
        /// Инициализирует одноэлементный объект приложения. Это первая выполняемая строка разрабатываемого
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
        //    OnLaunched();

           // Application.Current.Resources.
            this.InitializeComponent();
            Frame f =Window.Current.Content as Frame;
           // mp = (MainPage)f.Content;
            this.Suspending += this.OnSuspending;
            //MainPage Page = new MainPage();
            int val = Data.Inital.slider_val;
            //Page.print((string)val);
        }
     
        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
        /// если приложение запускается для открытия конкретного файла, отображения
        /// результатов поиска и т. д.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;
            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (rootFrame == null)
            {
                // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                rootFrame = new Frame();
                
                // TODO: Измените это значение на размер кэша, подходящий для вашего приложения
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Загрузить состояние из ранее приостановленного приложения
                }

                // Размещение фрейма в текущем окне
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Удаляет турникетную навигацию для запуска.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // Если стек навигации не восстанавливается для перехода к первой странице,
                // настройка новой страницы путем передачи необходимой информации в качестве параметра
                // навигации
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            mp = (MainPage)rootFrame.Content;
            mp.Loaded += Mp_Loaded;
            // Обеспечение активности текущего окна
            Window.Current.Activate();
        }

        private void Mp_Loaded(object sender, RoutedEventArgs e)
        {
            

        }

        public async Task<bool> CreateListener()
        {
            bool alright = false;
            //Create a StreamSocketListener to start listening for TCP connections. 
            try
            {

                try
                {
                    string myIP = Networking.GetLocalIp();
                    mp.myIP(myIP);
                    

                    await Networking.sendIP(myIP);
                    mp.status("Sent my IP.", Colors.LightGray);
                    mp.print("Sent my ip " + myIP + " on web server for client to read");
                    alright = true;
                }
                catch (Exception ex)
                {
                    mp.print("Error while sending my IP: " + ex.Message);
                    mp.status("Couldn't send my IP", Colors.DarkRed);

                }
                try
                {
                    StreamSocketListener socketListener = new
                    StreamSocketListener();
                    await socketListener.BindServiceNameAsync(mp.Port);
                    socketListener.ConnectionReceived += SocketListener_ConnectionReceived;
                    if (Connected) Connected = false;
                    else Connected = true;
                }
                catch (Exception ex) { mp.print("Error while creaitng listener: " + ex.Message); alright = false; }
            }
            catch (Exception ex)
            {
                mp.print(ex.Message);
                alright = false;
            }
                return alright;
        }
        public async void SocketListener_ConnectionReceived(
                                       StreamSocketListener sender,
                                       StreamSocketListenerConnectionReceivedEventArgs args)
        {
            mp.ConnectedHandler();
            mp.status("Client connected. IP: " + args.Socket.Information.RemoteAddress, Colors.Green);
            mp.print("Got connection from " + args.Socket.Information.RemoteAddress);
            Stopwatch stw = new Stopwatch();

            stw.Start();
            int[] times = new int[200];
            int i = 0;
            double p = 0.4;
            int w = 3;
            while (Connected)
            {

                string Recieved = null;
                i++;
                stw.Restart();
                //await Task.Delay(1000);
                if (!await Send_Values(sender, args)) {
                    sender.Dispose(); 
                    await CreateListener(); return;
                }
               // mp.print("recieving");
                Recieved = await Recieve(args);
               // mp.print("recieved");
                stw.Stop();
                int tim = (int)stw.ElapsedMilliseconds;
                push(times, tim);
                string gs = "not int";
                try
                {
                    int g = parse_rec(Recieved);
                    if (g > 0)
                    {
                        g = g - 300;
                        int gf = (int)(sum_n(got_filterd,w )*(1-p)/w+g*p);
                        push(got, g);
                        push(got_filterd, gf );
                        gs = g.ToString();
                    }
                    else gs = "Error while parsing";
                }
                catch { mp.print("Data was not an int"); }

                mp.print("Got: " + Recieved + "\n" + "Elapsed: " + tim + "ms." + " Average ping: " + avg_no_null(times) + "ms.");
                await mp.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                 {
                     
                     mp.tgraph.Draw(times);
                     mp.sgraph.Draw(sent);
                     mp.gfgraph.Draw(got_filterd);
                     mp.ggraph.Draw(got);
                 });               
            }
        }


        public async  Task<string> Recieve(StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Stream inStream = args.Socket.InputStream.AsStreamForRead();
            StreamReader reader = new StreamReader(inStream);
            TimeSpan ts = new TimeSpan(0, 0, 3);
            //  var cts = new CancellationTokenSource(3000);
            //char[] result = new char[2];

            try
            {
                // await Dispatcher.RunAsync(norm, () => { disp.Start(); });
                Task<string> readTask = reader.ReadLineAsync();
                return await readTask;
                //  await Dispatcher.RunAsync(norm, () => { disp.Stop(); });
            }
            catch(Exception ex) { mp.print("Recieving error :" + ex.Message); return null; }

        }
        public async Task<bool>  Send_Values(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                //Send the line back to the remote client. 
                Stream outStream = args.Socket.OutputStream.AsStreamForWrite();
                StreamWriter writer = new StreamWriter(outStream);
                int to_send = await mp.control_values();
                await writer.WriteLineAsync("$" + to_send + "#");
                await writer.FlushAsync();
                push(sent, to_send);
                return true;
            }
            catch (Exception ex)
            {
                 mp.print("Sending error: "+ex.Message);
                if(ex.HResult== -2147014842) //    _______________ If lost connection

                {
                  //  args.Socket.Dispose();
                   // sender.Dispose();
                    
                }
                return false;
            }
            
        }

        private int sum_n(int[] got_filterd, int v)
        {
            int s = 0;
            for (int i = 0; i < v; i++)
            {
                s += got_filterd[i];
            }
            return s;
        }

        private int parse_rec(string recieved)
        {
            int a = recieved.IndexOf('$');
            if (a != -1)
            {
                string s = recieved.Substring(a + 1);
                int b = s.IndexOf(';');
                if (b != -1)
                {
                    s = s.Substring(0, b);
                    return Convert.ToInt32(s);
                }
                else return -1;
            }
            else return -1;
        }


#if WINDOWS_PHONE_APP
        /// <summary>
        /// Восстанавливает переходы содержимого после запуска приложения.
        /// </summary>
        /// <param name="sender">Объект, где присоединен обработчик.</param>
        /// <param name="e">Сведения о событии перехода.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }
       static public int avg_no_null(int[] inp)
        {
            int l = inp.Length;
            int n = 0;
            int sum = 0;
            for (int i = 0; i < l; i++)
            {
                if (inp[i] != 0) { sum += inp[i]; n++; }
            }
            if (n == 0) n = 1;
            return (int)(sum / n);
        }

        static public int[] push(int[] inp, int n)
        {
            int l = inp.Length;
            for (int i = l - 1; i > 0; i--)
            {
                { inp[i] = inp[i - 1]; }
            }
            inp[0] = n;
            return inp;
        }
    }

}