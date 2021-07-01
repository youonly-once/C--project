using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
namespace UPH_趋势
{
    public partial class Form1 : Form
    {
        string pws = "";
        private int row = 14;
        private int column = 11;
        private List<List<Label>> labelArray = new List<List<Label>>();
        public Form1()
        {
            tip();
            InitializeComponent();

            createLabel();
          
            //loadData();
        }
        protected override void OnResize(EventArgs e)
        {

            this.WindowState = FormWindowState.Maximized;//窗口最大化
            int x = (int)(0.5 * (this.Width - title.Width));
            int y = title.Location.Y;
            title.Location = new System.Drawing.Point(x, y);

            this.authorLabel.Location = new System.Drawing.Point(this.Width - this.authorLabel.Width, this.Height -  this.authorLabel.Height);

            this.tableLayoutPanel1.Size = new System.Drawing.Size(this.Width, this.Height - this.title.Height -2*y- this.authorLabel.Height);
            this.tableLayoutPanel1.Location= new System.Drawing.Point(0, this.title.Height +2*y);

           

        }
        private void createLabel()
        {
            for(int i = 0; i < row; i++)
            {
                List<Label> labelList = new List<Label>();
                for(int j = 0; j < column; j++)
                {
                    Label label = new System.Windows.Forms.Label();
                    labelList.Add(label);
                    //label.Size=new System.Drawing.Size(91, 30);
                    //label.AutoSize = true;
                    label.Dock = System.Windows.Forms.DockStyle.Fill;
                    label.Font = new System.Drawing.Font("宋体", 40F);
                    label.ForeColor = System.Drawing.Color.White;
                   // label.Location = new System.Drawing.Point(4, 488);
                    label.Name = "label"+i+""+j;
                    //label.Size = new System.Drawing.Size(101, 38);
                    //label.TabIndex = 39;
                    label.Text = i+""+j;
                    label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    tableLayoutPanel1.Controls.Add(label, j, i+2);
                }
                labelArray.Add(labelList);
            }
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {
           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            tip(); loadData();
          
        }
        private void tip()
        {
            if (DateTime.Now.Month < 3)
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
        private void loadData()
        {
            DataTable dataTable = SqlHepler.StoredProcedure("dbo.T_DayUPH", null);
            for (int i = 0; i < row; i++)
            {
                List<Label> labelList = labelArray[i];
                for (int j = 0; j < column; j++)
                {
                    labelList[j].Text = dataTable.Rows[i][j].ToString();
                }
                ;
            }
        }
            private void Form1_Load(object sender, EventArgs e)
        {

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
    }
}
