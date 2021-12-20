using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Timers;
using System.IO;
using System.Threading;
using System.Web;
using System.Security.Cryptography;


namespace ReadDataFromArduino
{

    public partial class Form1 : Form
    {
        public static int GetDataInvalTime = 1000; //获取数据的时间间隙
        public static int SaveDataInvalTime = 1800000;  //保存数据的时间间隙
        public static int ShowDataInvalTime = 1000*60;   //显示数据的时间间隙
        bool HaveSaveNowData = false;
        int indexGetDataTimes = 0;//每半小时内获取数据的次数，兼顾着在关闭主程序时是否提示已保存数据的重任
        int indexAltNotSaveTimes = 180;//提示没有保存数据的次数，在获取180次数据没保存后就弹出窗口提示需要保存后退出
        int preday = DateTime.Now.Day;
        bool ChangeSaveFileName = false;
        string timefile = "";//
        int AddTimeToData = 0;
        bool IsDeterminedComport = false;     //是否确定了串口
        System.Timers.Timer timerToGetData;//每1s中获取一次数据，每天获取数据8640个
        System.Timers.Timer timerToSaveData;//每30mins中保存一次数据，每天换一个文件保存，每天保存48次
                                            //可能还需要一个在combox中自动显示端口号的功能
        System.Timers.Timer timerToShowData;  //每隔一段时间就显示一次数据
        DxpSerial m_dxpSerial;
        closeSave clsForm;
        private int TempHumityResGetFail = 0;

        public Form1()
        {
            InitializeComponent();
            this.getdatabtn.Click += new System.EventHandler(this.getdatabtn_Click);
            this.stopGetdata.Click += new System.EventHandler(this.stopGetdata_Click);
            this.clearbtn.Click += new System.EventHandler(this.clearbtn_Click);
            this.AddTimeToDatabtn.Click+= new System.EventHandler(this.AddTimeToDatabtn_Click);  //添加时间戳
            this.comportSurebtn.Click += ComportSurebtn_Click;
            this.determiandPabtn.Click += new System.EventHandler(this.DetermiandPabtn_Click);    //获取大气压
            m_dxpSerial = new DxpSerial("COM9999");
            this.searchPort();
            


            this.SaveNowData.Click+= new System.EventHandler(this.SaveNowData_Click);
            this.FormClosing += Window_Closing;

            

            timerToGetData = new System.Timers.Timer();
            timerToGetData.Interval = GetDataInvalTime;
            timerToGetData.Enabled = false;
            timerToGetData.SynchronizingObject = this;
            timerToGetData.Elapsed += new System.Timers.ElapsedEventHandler(this.TimerToGetData_Elapsed);

            timerToSaveData = new System.Timers.Timer();
            timerToSaveData.Interval = SaveDataInvalTime;//30mins
            timerToSaveData.Enabled = false;
            timerToSaveData.SynchronizingObject = this;
            timerToSaveData.Elapsed += new System.Timers.ElapsedEventHandler(this.TimerToSaveData_Elapsed);
            this.searchPort();    //查找可用com端口

            timerToShowData = new System.Timers.Timer();//开始获取数据后才运行
            timerToShowData.Interval = ShowDataInvalTime;
            timerToShowData.Enabled = false;
            timerToShowData.SynchronizingObject = this;
            timerToShowData.Elapsed += new System.Timers.ElapsedEventHandler(this.TimerToShowData_Elapsed);
        }

        private void TimerToShowData_Elapsed(object sender, ElapsedEventArgs e)//一分钟一次在label显示时间
        {
            int indexTime = 60;//多少秒显示一次时间，根据ShowDataInvalTime设置            
            if ((this.label4.Text== "00.00" || this.label5.Text == "00.00")&& m_dxpSerial.m_ProcessData.m_Templist.Count > 0 )
            {
                this.label4.Text = m_dxpSerial.m_ProcessData.m_Templist[0].tempdry.ToString();
                this.label5.Text = m_dxpSerial.m_ProcessData.m_Templist[0].humityFromPCB.ToString();
            }
            else if(m_dxpSerial.m_ProcessData.m_Templist.Count > indexTime)
            {                
                int len = m_dxpSerial.m_ProcessData.m_Templist.Count - 1;
                List<double> templist = new List<double>();
                List<double> humlist = new List<double>();
                for (int i = 0; i < indexTime; i++)
                {
                    templist.Add(m_dxpSerial.m_ProcessData.m_Templist[len].tempdry);
                    humlist.Add(m_dxpSerial.m_ProcessData.m_Templist[len].humityFromPCB);
                    len--;
                }
                double dbindxtime = Convert.ToDouble(indexTime);//必须将indextime转换为double，因为这样tempaverage才能得到double值而不是int值
                double tempaverage = templist.Sum() / dbindxtime;
                double humaverage = humlist.Sum() / dbindxtime;

                tempaverage = Math.Round(tempaverage, 2);//转换为两位小数
                humaverage = Math.Round(humaverage, 2);
                this.label4.Text = tempaverage.ToString();
                this.label5.Text = humaverage.ToString();


            }




        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult dialog = new DialogResult();

            //closeSave closeSaveForm = new closeSave();
            if (indexGetDataTimes > indexAltNotSaveTimes)
            {
                clsForm = new closeSave();
                clsForm.Activate();
                clsForm.Show();
                clsForm.Update();
                e.Cancel = true;    //必须要有 取消 这个键
                Console.WriteLine("WindowsColising");
            }
        }

        private void SaveNowData_Click(object sender, EventArgs e)
        {
            this.SaveNowDataFuc();
        }

        public void SaveNowDataFuc()//为了手动保存而写，后面的时间函数中保存是自动保存
        {
            HaveSaveNowData = true;
            indexGetDataTimes = 0;
            string filepath = "C:\\Users\\dxp\\Desktop\\";
            if (ChangeSaveFileName == true || timefile == "")
            {
                timefile = DateTime.Now.ToString("yyyy-MM-dd");
                ChangeSaveFileName = false;
            }
            filepath += timefile;
            filepath += ".csv";
            m_dxpSerial.m_ProcessData.SaveOneDayData(filepath);
            m_dxpSerial.m_ProcessData.m_Templist.Clear();
            this.SaveNowData.Text = "已保存到桌面";
            Console.WriteLine("保存数据了哈");
        }

        private void DetermiandPabtn_Click(object sender, EventArgs e)
        {
            string str = this.GetPatxtbox.Text;
            int PaConvertFromTxtBox = 0;
            try
            {
                PaConvertFromTxtBox = Convert.ToInt32(str);                
            }
            catch
            {
                MessageBox.Show("请输入正确的压强格式，如1001325，不要添加单位", "大气压格式错误");
            }
            if (PaConvertFromTxtBox < 900000 || PaConvertFromTxtBox > 1002325)
                MessageBox.Show("请查询当地大气压后输入正确的压强大小，您也可以选择不输入！", "大气压数值不正确");
            else if(PaConvertFromTxtBox !=0 )
            {
                m_dxpSerial.m_ProcessData.Pascal = PaConvertFromTxtBox;
                this.NowPaLabel.Text = "当前使用大气压为：";
                this.NowPaLabel.Text += m_dxpSerial.m_ProcessData.Pascal.ToString();
                this.NowPaLabel.Text += " Pa ，标准大气压为10001325 Pa";
            } 
        }

        private void ComportSurebtn_Click(object sender, EventArgs e)//串口确定按钮
        {
            this.ComPortcomboBox.IsAccessible = true;
           
            if (!m_dxpSerial.IsComportOpen()) 
            {
                m_dxpSerial.serialPort1.PortName = this.ComPortcomboBox.Text;
                m_dxpSerial.OpenComPort();
                if (!m_dxpSerial.IsComportOpen()) MessageBox.Show("无法打开此串口，请检查串口是否正确！","提示", MessageBoxButtons.OKCancel);
                if (m_dxpSerial.IsComportOpen())
                {
                    IsDeterminedComport = true;
                    timerToGetData.Enabled = true;
                    timerToGetData.Interval = GetDataInvalTime;
                    timerToSaveData.Enabled = true;
                    timerToSaveData.Interval = SaveDataInvalTime;//1800000ms是30mins
                    timerToShowData.Enabled = true;
                    timerToShowData.Interval = ShowDataInvalTime;

                }
            }
        }        

        private void TimerToGetData_Elapsed(object sender, ElapsedEventArgs e)    //获取数据的计时器
        {
            indexGetDataTimes++;
            if (indexGetDataTimes >= indexAltNotSaveTimes) HaveSaveNowData = false;//大于indexAltNotSaveTimes次数据未保存才会在关闭主程序时提示保存数据
            this.richTextBox1.AppendText("\n");         
            if (AddTimeToData != 0)
            {
                string time = DateTime.Now.ToLocalTime().ToString();
                this.richTextBox1.AppendText(time);
                this.richTextBox1.AppendText(" 温度:");
            }
            string TempHumityRes = m_dxpSerial.StartToGetData();  //开始获取数据
            if (TempHumityRes == "连接失败，无法获取到数据，请检查线路后重启软件")
            {
                TempHumityResGetFail++;
                if (TempHumityResGetFail == 1000)
                {
                    this.richTextBox1.AppendText("已尝试5次重新连接且失败，即将关闭软件！");
                    this.richTextBox1.AppendText("\n");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                }
            }          
            this.richTextBox1.AppendText(TempHumityRes);
            this.richTextBox1.AppendText("\n");
            //this.richTextBox1.SelectionStart = TempHumityRes.Length;
            int nowday = DateTime.Now.Day;
            ChangeSaveFileName = m_dxpSerial.m_ProcessData.CheckIfChangeFileName(preday,nowday);
            if (ChangeSaveFileName == true) preday = nowday;
            if (!m_dxpSerial.IsComportOpen()) MessageBox.Show("串口未打开或线路断开！");
            if ((this.label4.Text == "00.00" || this.label5.Text == "00.00") && m_dxpSerial.m_ProcessData.m_Templist.Count > 0)
            {
                this.label4.Text = m_dxpSerial.m_ProcessData.m_Templist[0].tempdry.ToString();
                this.label5.Text = m_dxpSerial.m_ProcessData.m_Templist[0].humityFromPCB.ToString();
            }
            if (indexGetDataTimes%2==0)//2秒两秒后更改button的显示文字
            {
                this.SaveNowData.Text = "保存数据";
            }
        }
        
        private void TimerToSaveData_Elapsed(object sender, ElapsedEventArgs e)//30mins保存一次数据,一天一个文件
        {//首先创建一个今天的文件用于存放数据，
            indexGetDataTimes = 0;
            string filepath = "C:\\Users\\dxp\\Desktop\\";
            if (ChangeSaveFileName==true||timefile==""){
                timefile = DateTime.Now.ToString("yyyy-MM-dd");
                ChangeSaveFileName = false;
            }
            filepath += timefile;
            filepath += ".csv";
            m_dxpSerial.m_ProcessData.SaveOneDayData(filepath);
        }
        private void stopGetdata_Click(object sender, EventArgs e)
        {
            m_dxpSerial.closeSerialPort();
            timerToGetData.Stop();
            timerToSaveData.Stop();
            timerToShowData.Stop();
            this.richTextBox1.AppendText("已停止获取数据，串口已关闭！\n");
            this.label4.Text = "00.00";
            this.label5.Text = "00.00";
        }

        private void getdatabtn_Click(object sender, EventArgs e)
        {//添加如果先点击获取数据就提示没有打开串口
            //IsDeterminedComport = true;
            this.richTextBox1.AppendText("串口打开，开始获取数据，可滑动窗口查看！\n");
            if (IsDeterminedComport == true) {
                timerToGetData.Enabled = true;
                timerToGetData.Interval = GetDataInvalTime;
                timerToSaveData.Enabled = true;
                timerToSaveData.Interval = SaveDataInvalTime;//1800000ms是30mins
                timerToShowData.Enabled = true;
                timerToShowData.Interval = ShowDataInvalTime;

            }
            else
            {
                MessageBox.Show("请先确定端口号！");
            }

        }


        private void clearbtn_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
        }
        
        private void AddTimeToDatabtn_Click(object sender, EventArgs e) //添加时间戳
        {
            if (AddTimeToData == 0)
            {
                AddTimeToData++;
            }
            else
            {
                AddTimeToData = 0;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void searchPort()
        {
            string[] portname = SerialPort.GetPortNames();//获取能用的串口
            this.ComPortcomboBox.Items.Clear();
            this.ComPortcomboBox.Items.AddRange(portname);
            if (portname.Length >0)
            {
                this.ComPortcomboBox.Text = portname[0];
            } 
        }
        

        /*public void addStringToTextBox(object sender, SerialDataReceivedEventArgs e) //事件处理方法
        {
            //使用Lambda表达式匿名委托设置文本
            this.Invoke(new Action(() =>
            {
                if (AddTimeToData == true)
                {
                    string time = DateTime.Now.ToLocalTime().ToString();
                    this.textBox1.AppendText(time);
                    this.textBox1.AppendText(": ");
                }
                this.textBox1.AppendText(mySerialCom.readLine());

                this.textBox1.AppendText("\r\n");
            }));
        }*/
    }
}
