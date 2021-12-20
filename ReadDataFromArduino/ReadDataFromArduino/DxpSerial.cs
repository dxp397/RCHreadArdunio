using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadDataFromArduino
{
    class DxpSerial         
    {
        //发送的获取数据：08 03 00 01 00 03 54 92
        
        
        public System.IO.Ports.SerialPort serialPort1;
        Modbus.Device.IModbusSerialMaster master;

        public ProcessData m_ProcessData;//数据处理类
        List<string> nameused;   //用于存放所有使用过的portname

        public DxpSerial(string portName)
        {
            m_ProcessData = new ProcessData();
            serialPort1 = new System.IO.Ports.SerialPort(portName);//声明串口
            master = Modbus.Device.ModbusSerialMaster.CreateRtu(serialPort1);
            serialPort1.BaudRate = 9600;//波特率
            serialPort1.DataBits = 8;//数据位
            serialPort1.Parity = Parity.None;
            serialPort1.StopBits = StopBits.One;            
            serialPort1.ReadTimeout = 2000;//设置读的超时时间
            serialPort1.WriteTimeout = 5000;//设置写的超时时间
            nameused = new List<string>();
        }

        public void OpenComPort()
        {
            bool hasname = this.searchName(serialPort1.PortName);
            nameused.Add(serialPort1.PortName);
            if (serialPort1.IsOpen)
            {
                Console.WriteLine("串口已打开");
            }
            else
            {
                serialPort1.Open();
            }
            //if (!hasname)
           // {
             //   serialPort1.Open();
            //Console.WriteLine(serialPort1.IsOpen);
            //}            
        }

        public string StartToGetData()
        {
            if (!serialPort1.IsOpen) this.OpenComPort();

            string TempHumityRes = "干球温度：";
            byte slaveId = 8;          //这边的值都是ushort 10 进制的数，一般仪器接收的都是16进制的，所以得自己换算
            ushort startAddress = 1;
            ushort[] dataModbus = new ushort[3];
            try
            {
              dataModbus = master.ReadHoldingRegisters(slaveId, startAddress, 3);
            }
            catch (System.TimeoutException)
            {
                //MessageBox.Show("连接超时，即将关闭软件设备！", "提示", MessageBoxButtons.OK);
                
                
                Console.WriteLine("退出程序了哈");
                //Environment.Exit(0);

            }

            

            m_ProcessData.myPoint3D.tempdry = m_ProcessData.ConvertData(dataModbus[0]);
            m_ProcessData.myPoint3D.tempmosit = m_ProcessData.ConvertData(dataModbus[1]);
            m_ProcessData.myPoint3D.humityFromPCB = m_ProcessData.ConvertData(dataModbus[2]);     //直接从PCB板子获取到湿度值
            if (dataModbus[0] == 0 || dataModbus[1]==0|| dataModbus[2]==0)
            {
                TempHumityRes = "连接失败，无法获取到数据，请检查线路后重启软件";
                return TempHumityRes;
            }

            //m_ProcessData.CpmtHumity(m_ProcessData.myPoint3D.tempdry, m_ProcessData.myPoint3D.tempmosit);//计算湿度值
            m_ProcessData.myPoint3D.timeNow = DateTime.Now.ToLocalTime().ToString();
            m_ProcessData.m_Templist.Add(m_ProcessData.myPoint3D);

            TempHumityRes += m_ProcessData.myPoint3D.tempdry.ToString();
            TempHumityRes += " 和 ";
            TempHumityRes += "湿球温度：";
            TempHumityRes += m_ProcessData.myPoint3D.tempmosit.ToString();
            TempHumityRes += " 环境湿度是： ";
            TempHumityRes += m_ProcessData.myPoint3D.humityFromPCB.ToString();

            return TempHumityRes;
        }

        public void closeSerialPort()
        {
            serialPort1.Close();
        }

        public bool IsComportOpen()
        {
            return serialPort1.IsOpen;
        }

        public void SearchAvailablaPort()
        {
            string[] item = new string[20];
            for (int i = 1; i < 21; i++){
                item[i - 1] = "COM" + i.ToString();
            }
            for (int i = 0; i < 20; i++) {
                //serialPort1.PortName = item[i];
               // if (serialPort1.Open()){
               //     Console.WriteLine("这个是端口{0}打不开",item[i]);
               // }
            }

        }

        public bool searchName(string name)   //寻找nameusd中是否有与之前重名的portname
        {
            bool res = false;
            int len = nameused.Count;
            if (len==0)
            {
                return false;
            }
            
            for (int i = 0; i < len; i++)
            {
                if (name == nameused[i])
                {
                    return true;
                }
            }
            return res;
        }
    }
}
