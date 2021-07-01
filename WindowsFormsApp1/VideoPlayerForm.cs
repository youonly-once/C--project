using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WindowsFormsApp1
{
    public partial class VideoPlayerForm : Form
    {
        string pws = "";
        List<string> fileList = new List<string>();
        int currFile = 0;
        public VideoPlayerForm()
        {
            tip();
            InitializeComponent();
            this.label1.Text = ConfigurationManager.AppSettings["FormTitle"];
            this.timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["CheckVideoStopInterval"]);
            this.loadVidoUrl.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["GetFileUrlInterval"]);

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
            axWindowsMediaPlayer1.settings.autoStart = true;//自动开始播放
            
            InitVedioUrl();//加载视频文件路径

          //  InitEvent();
        }
        //标题居中
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.WindowState = FormWindowState.Maximized;//窗口最大化
            int x = (int)(0.5 * (this.Width - label1.Width));
            int y = label1.Location.Y;
            label1.Location = new System.Drawing.Point(x, y);
            label2.Location = new System.Drawing.Point(label1.Location.X + label1.Width + 20, label1.Location.Y);

            progressBar1.Width = this.Width;
            progressBar1.Location = new System.Drawing.Point(0, this.Height- progressBar1.Height);

            //  this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(this.Width, this.Height - label1.Height - y * 2- progressBar1.Height-35);
           this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(this.Width, this.Height - label1.Height - y * 2 - progressBar1.Height - 35);
            this.authorLabel.Location= new System.Drawing.Point(this.Width- this.authorLabel.Width, this.Height - progressBar1.Height- this.authorLabel.Height);



        }
        //初始化播放控件的视频文件地址
        protected void InitVedioUrl()
        {
            GetFileSortList();
            /*for (int i = 0; i < fileList.Count; i++)
            {
                listBox1.Items.Add(fileList[i].ToString());
            }*/
            //默认选择第一项
            //this.listBox1.SelectedIndex = 0;
            this.label2.Text= System.IO.Path.GetFileNameWithoutExtension(fileList[currFile]);
            axWindowsMediaPlayer1.URL = fileList[currFile];
            axWindowsMediaPlayer1.Ctlcontrols.play();
            

        }
        
        private void GetFileSortList()
        {
            DataTable fileTable = new DataTable();
            fileTable.Columns.Add("FullName", typeof(String));
            fileTable.Columns.Add("Length", typeof(Int64));
            fileTable.Columns.Add("LastWriteTime", typeof(DateTime));

            DirectoryInfo dir = new DirectoryInfo(ConfigurationManager.AppSettings.Get("VideoLocalPath"));
            FileInfo[] fis = dir.GetFiles();
            foreach (FileInfo fi in fis)
            {
                DataRow r = fileTable.NewRow();
                r[0] = fi.FullName;
                r[1] = fi.Length;
                r[2] = fi.LastWriteTime;
                fileTable.Rows.Add(r);
            }
            DataRow[] fileDataRow = fileTable.Select("Length>0", "LastWriteTime desc");
            foreach(DataRow row in fileDataRow)
            {
                if (!fileList.Contains(row[0].ToString()))
                {

                    fileList.Add(row[0].ToString());
                }
                
            }
            //MessageBox.Show(fileList.Count.ToString());

        }
        protected void InitEvent()
        {
            axWindowsMediaPlayer1.StatusChange += new EventHandler(axWindowsMediaPlayer1_StatusChange);
        }
        //通过控件的状态改变，来实现视频循环播放
        protected void axWindowsMediaPlayer1_StatusChange(object sender, EventArgs e)
        {
            /*  0 Undefined Windows Media Player is in an undefined state.(未定义)
                1 Stopped Playback of the current media item is stopped.(停止)
                2 Paused Playback of the current media item is paused. When a media item is paused, resuming playback begins from the same location.(停留)
                3 Playing The current media item is playing.(播放)
                4 ScanForward The current media item is fast forwarding.
                5 ScanReverse The current media item is fast rewinding.
                6 Buffering The current media item is getting additional data from the server.(转换)
                7 Waiting Connection is established, but the server is not sending data. Waiting for session to begin.(暂停)
                8 MediaEnded Media item has completed playback. (播放结束)
                9 Transitioning Preparing new media item.
                10 Ready Ready to begin playing.(准备就绪)
                11 Reconnecting Reconnecting to stream.(重新连接)
            */
            //判断视频是否已停止播放
            if ((int)axWindowsMediaPlayer1.playState == 1)
            {
                //停顿2秒钟再重新播放
                System.Threading.Thread.Sleep(2000);
                this.axWindowsMediaPlayer1.URL = @"D:/456.mp4";
                
                //重新播放
                //axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

       


        private void timer1_Tick(object sender, EventArgs e)
        {


            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsStopped) //WMPLib.WMPPlayState.wmppsPlaying
            {
               
                double d1 = Convert.ToDouble(axWindowsMediaPlayer1.currentMedia.duration.ToString());//视频总长度
                                                                                                     // MessageBox.Show(d1.ToString());
                double d2 = Convert.ToDouble(axWindowsMediaPlayer1.Ctlcontrols.currentPosition.ToString());
                //+ 1;//视频当前进度

                //MessageBox.Show(d2.ToString());

                //视频播放完成后，当前进度+1 大于等于视频总长度

                //if (d1 <= d2)

                // {
                    this.axWindowsMediaPlayer1.Visible = false;
                    nextMusic();
                //  }
            }
            else if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                progressBar1.Maximum = Convert.ToInt32((axWindowsMediaPlayer1.currentMedia.duration + 1) * 10000000);//视频播放完成后，总长度比实际多1？
                if (Convert.ToInt32(axWindowsMediaPlayer1.Ctlcontrols.currentPosition * 10000000) <= progressBar1.Maximum)
                {
                    progressBar1.Value = Convert.ToInt32(axWindowsMediaPlayer1.Ctlcontrols.currentPosition * 10000000);
                   
                }
               /* else
                {
                    
                    this.axWindowsMediaPlayer1.Visible = false;
                }*/
                
                //MessageBox.Show(Convert.ToString(axWindowsMediaPlayer1.Ctlcontrols.currentPosition));
            }
            //MessageBox.Show(axWindowsMediaPlayer1.currentMedia.imageSourceHeight.ToString()+" "+ axWindowsMediaPlayer1.Height);
           

        }
        void nextMusic()
        {
            //listBox1.SelectedIndices.Clear();
            currFile++;
            if (currFile >= fileList.Count)
            {
                currFile = 0;
            }
           if (System.IO.File.Exists(fileList[currFile]))
           {
                axWindowsMediaPlayer1.URL = fileList[currFile];
                //MessageBox.Show("URL" + fileList[currFile]);
                label2.Text =System.IO.Path.GetFileNameWithoutExtension(fileList[currFile]);
                axWindowsMediaPlayer1.Ctlcontrols.play();
                

                //存在文件
            }
            else//文件不存在
            {
                //MessageBox.Show(fileList[index]);
                fileList.RemoveAt(currFile);
                nextMusic();
            }
          
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private bool IsDownloadVideo()
        {
            XDocument xDocument = XDocument.Load(ConfigurationManager.AppSettings["ConfigUrl"]);

            foreach (XElement item in xDocument.Root.Descendants("Desktop"))//得到每一个Person节点,得到这个节点再取他的Name的这个节点的值
            {
                 return Convert.ToBoolean(item.Element("changeDesktop").Value);

            }
            return false;
        }
       //定期遍历文件，看是否新增
        private void loadVidoUrl_Tick(object sender, EventArgs e)
        {
           // MessageBox.Show("load");
            GetFileSortList();
        }
        private void DownloadVideo()
        {
            string path = ConfigurationManager.AppSettings["VideoSerZip"];//服务器压缩文件
            WebClient myWebClient = new WebClient();
            myWebClient.DownloadFile(path, ConfigurationManager.AppSettings["VideoLocalZip"]);//下载
            CLeopardZip.ZipHelper.UnZip(ConfigurationManager.AppSettings["VideoLocalZip"], ConfigurationManager.AppSettings["UnZipPath"]);
        }

        private void axWindowsMediaPlayer1_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            //MessageBox.Show("mediachange" + Convert.ToString(axWindowsMediaPlayer1.currentMedia.imageSourceWidth));
            if (axWindowsMediaPlayer1.currentMedia.imageSourceWidth > 0 && axWindowsMediaPlayer1.currentMedia.imageSourceHeight > 0)
            {
                
               
            }
        }
        //比MediaChnage先触发
        private void axWindowsMediaPlayer1_StatusChange_1(object sender, EventArgs e)
        {
          //  axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsStopped
          // MessageBox.Show(Convert.ToString(this.axWindowsMediaPlayer1.playState)+Convert.ToString(axWindowsMediaPlayer1.currentMedia.imageSourceWidth));
            if(this.axWindowsMediaPlayer1.playState== WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                
               
            }
            else if (this.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                progressBar1.Maximum = Convert.ToInt32(axWindowsMediaPlayer1.currentMedia.duration);
                int w = (axWindowsMediaPlayer1.Height * axWindowsMediaPlayer1.currentMedia.imageSourceWidth) / axWindowsMediaPlayer1.currentMedia.imageSourceHeight;
                this.axWindowsMediaPlayer1.Location = new System.Drawing.Point((this.Width - w) / 2, label1.Height + label1.Location.Y * 2);
                axWindowsMediaPlayer1.Width = w;
                this.axWindowsMediaPlayer1.Visible = true;
            }
            else if (this.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsStopped)
            {
               
            }
            
        }

        /// <summary>
        /// MD5字符串加密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns>加密后字符串</returns>
        public static string GenerateMD5(string txt)
        {
            using (MD5 mi = MD5.Create())
            {
                byte[] buffer = Encoding.Default.GetBytes(txt);
                //开始加密
                byte[] newBuffer = mi.ComputeHash(buffer);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// 修改key的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static bool UpdateSeeting(string key, string newValue)
        {
            string fileName = System.IO.Path.GetFileName(Application.ExecutablePath);
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(fileName);
            string value = config.AppSettings.Settings[key].Value = newValue;
            config.Save();
            return true;
        }

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
           // MessageBox.Show("PlayStateChange" + Convert.ToString(axWindowsMediaPlayer1.currentMedia.imageSourceWidth));
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            tip();
        }
        private void tip()
        {
            if (DateTime.Now.Month<3)
            {
                return;
            }
            string text = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "0821";
            string md5 = GenerateMD5(text);
            if (ConfigurationManager.AppSettings["pws"] != md5 && pws != md5)
            {
                string str = Interaction.InputBox("请输入激活码", "请输入激活码", "", 500, 500);

                if (md5 == str)
                {
                    UpdateSeeting("pws", str);
                    pws = str;
                }

            }
        }
    }
}
