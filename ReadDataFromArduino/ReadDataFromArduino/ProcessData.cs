using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ReadDataFromArduino
{
    class ProcessData
    {
        public List<MyPoint3D> m_Templist;
        public MyPoint3D myPoint3D;
        private double vwind = 4.5;                   //风速
        public int Pascal = 1001325;             //大气压强,标准大气压是101.325KPa
        public ProcessData()
        {
            m_Templist = new List<MyPoint3D>();
            myPoint3D = new MyPoint3D();
            myPoint3D.humity = new double[6];
            myPoint3D.timeNow = "";
        }
        public struct MyPoint3D
        {
            public double tempdry;
            public double tempmosit;
            public double humityFromPCB;
            public double[] humity;
            public string timeNow;
        }


        public void CpmtHumity(double tempdryCmpHumity, double tempmositCmpHumity)
        {//计算湿度值
            this.myPoint3D.humity[0] = this.CpmtVaporPressureGoffGratch(tempdryCmpHumity, tempmositCmpHumity);
            this.myPoint3D.humity[1] = this.CpmtVaporPressureHylandWexler(tempdryCmpHumity, tempmositCmpHumity);
            this.myPoint3D.humity[2] = this.CpmtVaporPressureTetens(tempdryCmpHumity, tempmositCmpHumity);
            this.myPoint3D.humity[3] = this.CpmtVaporPressureMagnus(tempdryCmpHumity, tempmositCmpHumity);
            this.myPoint3D.humity[4] = this.CpmtVaporPressureJiLiFormual(tempdryCmpHumity, tempmositCmpHumity);
            this.myPoint3D.humity[5] = this.CpmtVaporPressureBuck(tempdryCmpHumity, tempmositCmpHumity);             
        }

        

        public double CpmtVaporPressureBuck(double tempdry, double tempmosit)//计算饱和水蒸气分压：Buck公式,t>0℃
        {
            double Buck = 0;
            double Pdry = 6.1121 * Math.Pow(Math.E, tempdry*(18.678-(tempdry / 234.5) / (257.14 + tempdry)));
            double Pmosit = 6.1121 * Math.Pow(Math.E, tempmosit * (18.678 - (tempmosit / 234.5) / (257.14 + tempmosit)));

            double A = (72.27 + (6.747 / vwind) + 0.01087 * tempdry * tempdry - 0.5299 * tempdry) * 1e-5;//干球系数A，需要使用干球温度
            Buck = (Pmosit - A * Pascal * (tempmosit - tempdry)) / Pdry;
            Buck *= 100;
            return Buck;
        }

        public double CpmtVaporPressureJiLiFormual(double tempdry, double tempmosit)//计算饱和水蒸气分压：纪利公式
        {
            double JiLiFormual = 0;
            double TKdry = tempdry + 273.15;
            double TKmosit = tempmosit + 273.15;

            double lgBdry = 0.0141966 - 3.142305 * (1000 / TKdry - 1000 / 373.16) + 8.2 * Math.Log10(373.16 / TKdry) - 2.4804e-3 * (373.16 - TKdry);
            double lgBmosit = 0.0141966 - 3.142305 * (1000 / TKmosit - 1000 / 373.16) + 8.2 * Math.Log10(373.16 / TKmosit) - 2.4804e-3 * (373.16 - TKmosit); ;
            lgBdry = Math.Pow(10, lgBdry);
            lgBmosit = Math.Pow(10, lgBmosit);
            double Pdry = 98066 * lgBdry;
            double Pmosit = 98066 * lgBmosit;

            double A = (72.27 + (6.747 / vwind) + 0.01087 * tempdry * tempdry - 0.5299 * tempdry) * 1e-5;//干球系数A，需要使用干球温度
            JiLiFormual = (Pmosit - A * Pascal * (tempmosit - tempdry)) / Pdry;
            JiLiFormual *= 100;
            return JiLiFormual;
        }

        public double CpmtVaporPressureMagnus(double tempdry, double tempmosit)//计算饱和水蒸气分压：马格努斯(Magnus)公式
        {
            double Magnus = 0;
            double Pdry = 6.11 * Math.Pow(10, ((7.45 * tempdry) / (235 + tempdry)));
            double Pmosit = 6.11 * Math.Pow(10, ((7.45 * tempmosit) / (235 + tempmosit)));

            double A = (72.27 + (6.747 / vwind) + 0.01087 * tempdry * tempdry - 0.5299 * tempdry) * 1e-5;//干球系数A，需要使用干球温度
            Magnus = (Pmosit - A * Pascal * (tempmosit - tempdry)) / Pdry;
            Magnus *= 100;
            return Magnus;
        }

        public double CpmtVaporPressureTetens(double tempdry, double tempmosit)//计算饱和水蒸气分压：泰登(Tetens)公式
        {
            double Tetens = 0;           
            double Pdry = 610.6*Math.Pow(Math.E, ((17.269*tempdry)/(237.3+tempdry)));
            double Pmosit = 610.6 * Math.Pow(Math.E, ((17.269 * tempmosit) / (237.3 + tempmosit)));

            double A = (72.27 + (6.747 / vwind) + 0.01087 * tempdry * tempdry - 0.5299 * tempdry) * 1e-5;//干球系数A，需要使用干球温度
            Tetens = (Pmosit - A * Pascal * (tempmosit - tempdry)) / Pdry;
            Tetens *= 100;
            return Tetens;
        }

        public double CpmtVaporPressureHylandWexler(double tempdry, double tempmosit)//计算饱和水蒸气分压：戈夫-格雷奇公式,0-200℃
        {
            double HylandWexler = 0;
            double TKdry = tempdry + 273.15;
            double TKmosit = tempmosit + 273.15;
            double lnPdry = -5674.5359 / TKdry + 6.3925247 - 9.677843e-3 * TKdry + 6.2215701e-7 * TKdry * TKdry 
            + 0.20747825e-18 * Math.Pow(TKdry, 3) - 9.484024e-12 * Math.Pow(TKdry, 4) + 4.1635019 * Math.Log(TKdry);

            double lnPmosit = -5674.5359 / TKmosit + 6.3925247 - 9.677843e-3 * TKmosit + 6.2215701e-7 * TKmosit * TKmosit
            + 0.20747825e-18 * Math.Pow(TKmosit, 3) - 9.484024e-12 * Math.Pow(TKmosit, 4) + 4.1635019 * Math.Log(TKmosit);

            double Pdry = Math.Pow(Math.E, lnPdry);
            double Pmosit = Math.Pow(Math.E, lnPmosit);

            double A = (72.27 + (6.747 / vwind) + 0.01087 * tempdry * tempdry - 0.5299 * tempdry) * 1e-5;//干球系数A，需要使用干球温度
            HylandWexler = (Pmosit - A * Pascal * (tempmosit - tempdry)) / Pdry;
            HylandWexler *= 100;
            return HylandWexler;
        }

        public double CpmtVaporPressureGoffGratch(double tempdry, double tempmosit)//计算饱和水蒸气分压：戈夫-格雷奇公式,因为不涉及到0℃以下，因此只计算T>273.15K
        {
            double GoffGratch = 0;
            double TKdry = tempdry + 273.15;
            double TKmosit = tempmosit + 273.15;
            double lgPdry = -7.90298 * (373.16 / TKdry - 1) + 5.028081 * Math.Log10(373.16 / TKdry) 
                            - 1.3816e-7 * (Math.Pow(10, 11.344 * (1 - TKdry / 373.16)) - 1) 
                            + 8.1328e-3 * (Math.Pow(10, -3.49149 * (373.16 / TKdry - 1)) - 1) + Math.Log10(1013.246);

            double lgPmosit = -7.90298 * (373.16 / TKmosit - 1) + 5.028081 * Math.Log10(373.16 / TKmosit)
                            - 1.3816e-7 * (Math.Pow(10, 11.344 * (1 - TKmosit / 373.16)) - 1)
                            + 8.1328e-3 * (Math.Pow(10, -3.49149 * (373.16 / TKmosit - 1)) - 1) + Math.Log10(1013.246);
            double Pdry = Math.Pow(10, lgPdry);
            double Pmosit = Math.Pow(10, lgPmosit);

            double A = (72.27 + (6.747 / vwind) + 0.01087 * tempdry * tempdry - 0.5299 * tempdry) * 1e-5;//干球系数A，需要使用干球温度
            GoffGratch = (Pmosit - A * Pascal * (tempmosit - tempdry)) / Pdry;
            GoffGratch *= 100;
            return GoffGratch;
        }
    
        public void SaveOneDayData(string filePath)//保存list中的数据，30mins保存一次
        {
            int N = m_Templist.Count;
            String iLine;
            bool exit = File.Exists(filePath);
            StreamWriter sw = new StreamWriter(filePath, true);//如果文件不存在就创建，如果存在就追加数据
            //iLine = String.Format("{0}", N);//为减少内存消耗，取消获取数据的计数

            if (!exit)
            {
                //iLine = "T1,T2,Goff-Gratch,HylandWexler,Tetens,Magnus,JiLiFormual,Buck,time";
                iLine = "T1,T2,RH,time";
                sw.WriteLine(iLine);
            }
            for (int i = 0; i < N; i++)
            {
                double x = m_Templist[i].tempdry;
                double y = m_Templist[i].tempmosit;
                double rh = m_Templist[i].humityFromPCB;
                //double z0 = m_Templist[i].humity[0];//戈夫-格雷奇（Goff-Gratch）公式计算结果
                //double z1 = m_Templist[i].humity[1];//HylandWexler公式
                //double z2 = m_Templist[i].humity[2];//泰登公式(Tetens)
                //double z3 = m_Templist[i].humity[3];//马格努斯(Magnus)公式
                //double z4 = m_Templist[i].humity[4];//纪利公式
                //double z5 = m_Templist[i].humity[5];//Buck
                string timePoint = m_Templist[i].timeNow;//当时的时间

                //iLine = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},'{8}", x, y, z0, z1, z2, z3, z4, z5,timePoint);//在每一个后面添加时间
                iLine = String.Format("{0},{1},{2},'{3}", x, y, rh, timePoint);//在每一个后面添加时间
                if (x == 0 || y == 0) iLine = "";

                sw.WriteLine(iLine);
            }
            sw.Close();
            m_Templist.Clear();
        }

        public bool CheckIfChangeFileName(int Checkpreday, int checknowday)//检查是否需要更换文件名
        {
            if (checknowday > Checkpreday || ((checknowday < Checkpreday) && (checknowday == 1)))
                return true;
            return false;
        }

        public double ConvertData(ushort x)//将温度如2788转换为27.88
        {
            if (x==0)
            {
                return 0;
            }
            double res = x / 100.0;
            return res;
        }
    }
}
