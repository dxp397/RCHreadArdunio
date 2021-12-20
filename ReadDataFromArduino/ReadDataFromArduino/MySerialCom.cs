using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Collections;
using ReadDataFromArduino;

namespace SerialPortModel
{
    class MySerialCom
    {
        public static SerialPort serialPort = new SerialPort();
        string[] portNamesStr;
        public bool findcomport = false;
        public string Comport = "";
        public ProcessData m_ProcessData;


        public MySerialCom()
        {
            this.setBaudRate(9600);
            this.setParity(Parity.None);
            this.setReadBufferSize(256);
            this.setReadTimeout(5000);
            this.setEncoding(Encoding.GetEncoding("UTF-8"));
            this.setPort("COM7");
            m_ProcessData = new ProcessData();

        }

        public MySerialCom(string Comport)
        {
            this.setBaudRate(9600);
            this.setParity(Parity.None);
            this.setReadBufferSize(256);
            this.setReadTimeout(5000);
            this.setEncoding(Encoding.GetEncoding("UTF-8"));
            this.setPort(Comport);
            m_ProcessData = new ProcessData();
        }

        public MySerialCom(int baudRate, string comPort)
        {
            this.setBaudRate(baudRate);
            this.setParity(Parity.None);
            this.setReadBufferSize(256);
            this.setReadTimeout(5000);
            this.setEncoding(Encoding.GetEncoding("UTF-8"));
            this.setPort(comPort);
            m_ProcessData = new ProcessData();
        }

        public string[] getPortNames() //扫描COM端口名
        {
            portNamesStr = SerialPort.GetPortNames();
            return portNamesStr; //返回string数组
        }

        //设置波特率
        public void setBaudRate(int x)
        {
            if (x > 0)
                serialPort.BaudRate = x;
        }

        //设置端口
        public void setPort(string x)
        {
            this.getPortNames();
            SortedSet<string> portNamesSet = new SortedSet<string>();
            if (portNamesStr.Length > 0)
            {
                foreach (string portName in portNamesStr)
                {
                    portNamesSet.Add(portName); //丢入集合，自动去重
                }

                if (portNamesSet.Contains(x)) //查询集合中是否包含某个元素
                {
                    serialPort.PortName = x; //设置端口
                }
            }
        }

        //设置停止位
        public void setStopBits(StopBits x) //设置每个字节的标准停止位数
        {
            serialPort.StopBits = x; //
        }

        //设置奇偶校验
        public void setParity(Parity x)
        {
            serialPort.Parity = x; //设置奇偶校验检查协议
        }

        //设置编码格式
        public void setEncoding(Encoding x)
        {
            serialPort.Encoding = x;
        }

        //设置超时时间
        public void setReadBufferSize(int x)
        {
            serialPort.ReadBufferSize = x;
        }

        //设置超时时间
        public void setReadTimeout(int x)
        {
            serialPort.ReadTimeout = x;
        }

        //打开串口
        public void openSerialPort()
        {
            if (!isOpen())
              serialPort.Open();
        }

        //关闭串口
        public void closeSerialPort()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        //按行读取
        public string readLine()
        {
            string res = "两温度计的温度分别是：";
            string line1;
            string line2;
            serialPort.ReadTimeout = 5000;//设置读取超时的时间   
            try
            {
                line1 = serialPort.ReadLine();
                line2 = serialPort.ReadLine();
            }
            catch
            {
                line1 = "000";
                line2 = "000";
                res = "端口号不可取，无法读取程序！";
                return res;
            }
            /*保存数据*/
            try
            {
                if (line1 == "\r" || line1 == "") return null;
                short x = Convert.ToInt16(line1);//short类型范围是-32768~32767
                short y = Convert.ToInt16(line2);                
            }
            catch
            {
                res = "";
                return res;
            }
            m_ProcessData.CpmtHumity(m_ProcessData.myPoint3D.tempdry, m_ProcessData.myPoint3D.tempmosit);//计算湿度值
            m_ProcessData.m_Templist.Add(m_ProcessData.myPoint3D);
            
            res += m_ProcessData.myPoint3D.tempdry.ToString();
            res += " 和 ";
            res += m_ProcessData.myPoint3D.tempmosit.ToString();
            res += " 环境湿度是： ";
            res += m_ProcessData.myPoint3D.humity.ToString();            
            return res;
        }

        //状态
        public bool isOpen()
        {
            return serialPort.IsOpen;
        }

        //查询能用的端口
        public string FindPort()
        {
            string res = "";
            this.getPortNames();
            SortedSet<string> portNamesSet = new SortedSet<string>();
            if (portNamesStr.Length > 0)
            {
                foreach (string portName in portNamesStr)
                {
                    portNamesSet.Add(portName); //丢入集合，自动去重
                }
            }
            foreach (string portName in portNamesStr)
            {
                serialPort.PortName = portName;
                serialPort.Open();
                if(serialPort.IsOpen)
                {
                    res = portName;
                    findcomport = true;
                    return res;
                }
            }
            return null;
        }

    }
}