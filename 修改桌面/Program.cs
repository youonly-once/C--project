using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Configuration;
using System.Web;
using System.Threading;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
//using WindowsUser32API;
using System.Xml.Linq;
using InternetCheck;
using Microsoft.Win32;

//using WindowsAPI;
namespace 修改桌面
{
    class Program
    {
        private Process programProcess=null;//保存程序的句柄
        private static Program p = new Program();
        
        private bool   isChangeDesktop;//是否显示桌面
        private bool   isDownloadImg;//是否下载图片
        private string firstRunDateTime;
       
        private bool firstRun=true;
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        /// <summary>    
        /// 该函数将创建指定窗口的线程设置到前台，并且激活该窗口。
        /// 键盘输入转向该窗口，并为用户改各种可视的记号。系统给创建前台窗口的
        /// 线程分配的权限稍高于其他线程。  
        /// </summary>  
        static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
        /// <summary>    
        /// 该函数返回桌面窗口的句柄。桌面窗口覆盖整个屏幕。 
        /// </summary>   
        static public extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow", SetLastError = true)]
        static public extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(
        int uAction,
        int uParam,
        string lpvParam,
        int fuWinIni
        );

        static void Main(string[] args)
        {

            if (Internet.IsConnectInternet() && Internet.PingIpOrDomainName(ConfigurationManager.AppSettings["ServerIP"]))
            {
               
                //网络正常、读取配置文件
                p.LoadSerXMLConfig();
                //获取第一次运行时间
                p.FirstRunTimer();
            }
            else
            {
                p.CheckInterNetTimer();
            }
  
            p.ClearConsoleTimer();//定期清理控制台
            
            
            Console.ReadKey();

        }
        /*
         *检测网络通畅
         */
        private void CheckInterNetTimer()
        {
            
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["CheckNetInterval"]); //执行间隔时间,单位为毫秒; 这里实际间隔为10分钟  
            timer.Start();
            timer.Elapsed += new ElapsedEventHandler((source, e) =>
            {
                string ser = ConfigurationManager.AppSettings["ServerIP"];
                if (Internet.IsConnectInternet() && Internet.PingIpOrDomainName(ser))
                {
                    timer.Stop();
                    //网络正常、读取配置文件
                    LoadSerXMLConfig();
                    //获取第一次运行时间
                    FirstRunTimer();
                    
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToLocalTime().ToString() + "：网络不通");
                }

            });

            
        }
        
       /*
        * 为让所有屏幕统一执行，约定第一次执行时间
        */
        private void FirstRunTimer()
        { 
            System.Timers.Timer firstRunTimer = new System.Timers.Timer();
        TimeSpan timeSpan= Convert.ToDateTime(firstRunDateTime) - DateTime.Now;
            Console.WriteLine(DateTime.Now.ToLocalTime().ToString() + "：第一次执行：" +firstRunDateTime+","+ timeSpan.ToString()+"毫秒");
            firstRunTimer.Enabled = true;
            if (timeSpan.TotalMilliseconds < 0){ firstRunTimer.Interval = 1000;}
            else{ firstRunTimer.Interval = timeSpan.TotalMilliseconds;}
            firstRunTimer.Start();
            firstRunTimer.AutoReset = false;//执行一次
            firstRunTimer.Elapsed += new System.Timers.ElapsedEventHandler(Task);
           
        }
        private void TaskTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]); //执行间隔时间,单位为毫秒; 这里实际间隔为10分钟  
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Task);
            Console.WriteLine(Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]) + "毫秒执行一次");
        }
        private void ClearConsoleTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["ClearConsoleInterval"]); //执行间隔时间,单位为毫秒; 这里实际间隔为10分钟  
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler((source, e) =>{Console.Clear(); });
            Console.WriteLine(DateTime.Now.ToLocalTime().ToString() + "：开始控制台清理Timer");
        }
        
        private void OpenProgram()
        {
            string programPath = ConfigurationManager.AppSettings["ProgramPath"];
             programProcess = Process.Start(programPath, ConfigurationManager.AppSettings["ProgramConfig"]);
            //此时programProcess.MainWindowHandle可能为0,因为窗口还没显示
            Console.WriteLine("打开程序：" + programProcess.ProcessName +" = "+ programProcess.MainWindowHandle);
            ThreadStart threadStart = new ThreadStart(CheckProgramRun);
            Thread thread = new Thread(threadStart);
            thread.Start();
           

        }
        private void CheckProgramRun()
        {
            Console.WriteLine("开始检测程序是否退出");
            programProcess.WaitForExit();
            Console.WriteLine("程序已经退出");
            programProcess = null;
            if (ConfigurationManager.AppSettings["ProgramName"].Equals("chrome"))
            {
                OpenProgram();
            }
            
        }
        private  void Task(object source, ElapsedEventArgs e)
        {
            
            Console.WriteLine(DateTime.Now.ToLocalTime().ToString()+"======任务开始 第一次执行："+firstRun+"======");
            LoadSerXMLConfig();

            Console.WriteLine("显示桌面：" + isChangeDesktop);
            if (isChangeDesktop)//显示桌面
            {
                Console.WriteLine("下载图片：" + isDownloadImg);
                if (isDownloadImg)
                {
                    p.DownloadDesktopImg();
                    //拉伸
                    RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel",false);
                    registryKey = registryKey.OpenSubKey("Desktop", true);
                    //Console.WriteLine(registryKey.ValueCount);
                   registryKey.SetValue("TileWallpaper", "0");
                    registryKey.SetValue("WallpaperStyle", "2");
                    registryKey.Close();

                    Console.WriteLine("设置桌面背景：" + SystemParametersInfo(20, 0, ConfigurationManager.AppSettings["DesktopImagepath"], 0x2));
                }
                Console.WriteLine("当前是否为桌面：" + IsOnDesktop());
                if ( !IsOnDesktop())
                {
                    Console.WriteLine("显示桌面");
                    Type oleType = Type.GetTypeFromProgID("Shell.Application");
                    object oleObject = System.Activator.CreateInstance(oleType);
                    oleType.InvokeMember("ToggleDesktop", BindingFlags.InvokeMethod, null, oleObject, null);
                }
                //考虑到javaw程序切换到桌面后回不去，显示桌面直接kill，下次再OPEN
                if (programProcess != null && !ConfigurationManager.AppSettings["ProgramName"].Equals("chrome"))
                {
                    programProcess.Kill();
                    programProcess = null;
                }
                
                
            }
            else//显示程序
            {
              if(programProcess == null)
                {
                   /* String programname = ConfigurationManager.AppSettings["ProgramName"];
                    Console.WriteLine("查找程序窗口句柄：" + programname);
                    foreach (Process thisProc in Process.GetProcessesByName(programname))
                    {
                        if (thisProc.MainWindowHandle != IntPtr.Zero)
                        {
                            programProcess = thisProc;
                           
                            ShowProgram();
                        }
                    }
                    if (programProcess == null)
                    {*/
                        OpenProgram();
                   // }
                }
                else
                {
                    ShowProgram();
                }
            }
            if (firstRun)
            {
                firstRun = false;
                TaskTimer();
               
            }

            
        }
         private bool IsOnDesktop()
        {
             // 获取所有进程。
             Process[] proc = Process.GetProcesses();

                // 获取当前的窗口句柄。
              IntPtr cWin = GetForegroundWindow();

               foreach(Process x in proc)
               {
                //ShowWindow(x.MainWindowHandle, 7);//最小化所有窗口，7表示最小化，激活窗口任然激活
                  if(x.MainWindowHandle == cWin)
                  {
                      return false;
                  }
               }
            return true ;
        }
        private void ShowProgram()
        {

            if (GetForegroundWindow().Equals(programProcess.MainWindowHandle))
            {
                Console.WriteLine("窗口已显示：" + GetForegroundWindow());
            }
            else
            {
                Console.WriteLine("显示程序窗口：" + programProcess.ProcessName + " = " + programProcess.MainWindowHandle);
                //ShowWindow(intptr, 0);
                ShowWindow(programProcess.MainWindowHandle, 3);//显示
                SetForegroundWindow(programProcess.MainWindowHandle);//置前
               
            }
           
        }
       
        private string IsChange()
        {
            String urlstr = "http://10.139.114.219:8001/change_desktop/change.txt";
            WebClient myWebClient = new WebClient();
            Stream inputStream = myWebClient.OpenRead(urlstr);
            byte[] bytes = new byte[1024];
            inputStream.Read(bytes, 0, 1024);
            Console.WriteLine("是否显示桌面：" + System.Text.Encoding.GetEncoding("GB2312").GetString(bytes).TrimEnd('\0'));
            return System.Text.Encoding.GetEncoding("GB2312").GetString(bytes).TrimEnd('\0');
        }

        //流方式下载
        protected void DownloadDesktopImg()
        {
            String urlstr = ConfigurationManager.AppSettings["PictureUrl"];
            WebClient myWebClient = new WebClient();
            myWebClient.DownloadFile(urlstr, ConfigurationManager.AppSettings["DesktopImagepath"]);
            Console.WriteLine("下载图片完成：" + urlstr);

        }
        
        private void LoadSerXMLConfig()
        {
            Console.WriteLine(DateTime.Now.ToLocalTime().ToString() + "：读取配置文件" );
            XDocument xDocument = XDocument.Load(ConfigurationManager.AppSettings["ConfigUrl"]);
           
            foreach (XElement item in xDocument.Root.Descendants("Desktop"))//得到每一个Person节点,得到这个节点再取他的Name的这个节点的值
            {
                isChangeDesktop = Convert.ToBoolean(item.Element("changeDesktop").Value);
                isDownloadImg= Convert.ToBoolean(item.Element("downloadImg").Value);//Person的节点的下得节点为Name的
                firstRunDateTime = item.Element("firstRunDateTime").Value;
              
            }
        }
    }
}
