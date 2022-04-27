using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace SerialAssistant
{
    public partial class Form1 : Form
    {
        private long receive_count = 0; //接收字节计数
        private long send_count = 0;    //发送字节计数
        private StringBuilder sb = new StringBuilder();    //为了避免在接收处理函数中反复调用，依然声明为一个全局变量
        private DateTime current_time = new DateTime();    //为了避免在接收处理函数中反复调用，依然声明为一个全局变量
        private string send_type;   //发送大类

        private StringBuilder builder = new StringBuilder();    //避免在事件处理方法中反复创建，定义为全局
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            int i;
            //单个添加
            for (i = 300; i <= 38400; i = i * 2)
            {
                comboBox2.Items.Add(i.ToString());  //添加波特率列表
            }

            //批量添加波特率列表
            string[] baud = { "43000", "56000", "57600", "115200", "128000", "230400", "256000", "460800" };
            comboBox2.Items.AddRange(baud);

            //获取电脑当前可用串口并添加到选项列表中
            comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            //设置选项默认值
            comboBox2.Text = "115200";
            comboBox3.Text = "8";
            comboBox4.Text = "None";
            comboBox5.Text = "1";


        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //将可能产生异常的代码放置在try块中
                //根据当前串口属性来判断是否打开
                if (serialPort1.IsOpen)
                {
                    //串口已经处于打开状态
                    serialPort1.Close();    //关闭串口
                    button1.Text = "打开串口";
                    button1.BackColor = Color.ForestGreen;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    label6.Text = "串口已关闭";
                    label6.ForeColor = Color.Red;
                    button2.Enabled = false;        //失能发送按钮
                }
                else
                {
                    //串口已经处于关闭状态，则设置好串口属性后打开
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.DataBits = Convert.ToInt16(comboBox3.Text);

                    if (comboBox4.Text.Equals("None"))
                        serialPort1.Parity = System.IO.Ports.Parity.None;
                    else if (comboBox4.Text.Equals("Odd"))
                        serialPort1.Parity = System.IO.Ports.Parity.Odd;
                    else if (comboBox4.Text.Equals("Even"))
                        serialPort1.Parity = System.IO.Ports.Parity.Even;
                    else if (comboBox4.Text.Equals("Mark"))
                        serialPort1.Parity = System.IO.Ports.Parity.Mark;
                    else if (comboBox4.Text.Equals("Space"))
                        serialPort1.Parity = System.IO.Ports.Parity.Space;

                    if (comboBox5.Text.Equals("1"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.One;
                    else if (comboBox5.Text.Equals("1.5"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    else if (comboBox5.Text.Equals("2"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.Two;

                    serialPort1.Open();     //打开串口
                    button1.Text = "关闭串口";
                    button1.BackColor = Color.Firebrick;
                    label6.Text = "串口已打开";
                    label6.ForeColor = Color.Green;
                    button2.Enabled = true;        //使能发送按钮


                }
            }
            catch (Exception ex)
            {
                //捕获可能发生的异常并进行处理

                //捕获到异常，创建一个新的对象，之前的不可以再用
                serialPort1 = new System.IO.Ports.SerialPort();
                //刷新COM口选项
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;

            }
        }



        //串口接收事件处理
        private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int num = serialPort1.BytesToRead;      //获取接收缓冲区中的字节数
            byte[] received_buf = new byte[num];    //声明一个大小为num的字节数据用于存放读出的byte型数据


            receive_count += num;                   //接收字节计数变量增加nun
            serialPort1.Read(received_buf, 0, num);   //读取接收缓冲区中num个字节到byte数组中

            sb.Clear();     //防止出错,首先清空字符串构造器

            if (radioButton2.Checked)
            {
                //选中HEX模式显示
                foreach (byte b in received_buf)
                {
                    sb.Append(b.ToString("X2") + ' ');    //将byte型数据转化为2位16进制文本显示，并用空格隔开
                }

            }
            else
            {
                //选中ASCII模式显示
                sb.Append(Encoding.ASCII.GetString(received_buf));  //将整个数组解码为ASCII数组
            }
            try
            {
                //因为要访问UI资源，所以需要使用invoke方式同步ui
                Invoke((EventHandler)(delegate
                {
                    if (checkBox1.Checked)
                    {
                        //显示时间
                        current_time = System.DateTime.Now;     //获取当前时间
                        textBox_receive.AppendText(current_time.ToString("HH:mm:ss") + "  " + sb.ToString());

                    }
                    else
                    {
                        //不显示时间 
                        textBox_receive.AppendText(sb.ToString());
                    }
                    label7.Text = "Rx:" + receive_count.ToString() + "Bytes";
                    label19.Text = "接收完成";
                    string[] arr = Regex.Split(textBox_receive.Text, "\r\n", RegexOptions.IgnoreCase);
                    switch (send_type)
                    {
                        //电流
                        case "CURRent_LIMit": textBox_CURRent_LIMit.Text = arr[0]; break;
                        case "CURRent_DELay": textBox2.Text = arr[0]; break;
                        case "CURRent_INRush_STARt": textBox3.Text = arr[0]; break;
                        case "CURRent_INRush_INTerval": textBox4.Text = arr[0]; break;
                        //电压
                        case "VOLTag_AMPLitude_AC": textBox5.Text = arr[0]; break;
                        case "VOLTag_AMPLitude_DC": textBox6.Text = arr[0]; break;
                        case "VOLTage_LIMit_AC": textBox7.Text = arr[0]; break;
                        case "VOLTage_LIMit_DC_PLUS": textBox8.Text = arr[0]; break;
                        case "VOLTage_LIMit_DC_MINus": textBox9.Text = arr[0]; break;
                        case "VOLTage_RANGe": comboBox6.Text = arr[0]; break;
                        //频率
                        case "SOURce_FREQuency_{CW|IMMediate}": textBox73.Text = arr[0]; break;
                        //相位内容
                        case "PHASe_ON": textBox10.Text = arr[0]; break;
                        case "PHASe_OFF": textBox11.Text = arr[0]; break;
                        //测量内容
                        case "CURRent_AC": textBox12.Text = arr[0]; break;
                        case "CURRent_AMPLitude_MAXimum": textBox13.Text = arr[0]; break;
                        case "CURRent_CREStfactor": textBox14.Text = arr[0]; break;
                        case "CURRent_INRush": textBox15.Text = arr[0]; break;
                        case "CURRent_DC": textBox17.Text = arr[0]; break;
                        case "VOLTage_DC": textBox16.Text = arr[0]; break;
                        case "VOLTage_ACDC": textBox18.Text = arr[0]; break;
                        case "FREQuency": textBox21.Text = arr[0]; break;
                        case "POWer_AC_REAL": textBox22.Text = arr[0]; break;
                        case "POWer_AC_APParent": textBox23.Text = arr[0]; break;
                        case "POWer_AC_REACtive": textBox24.Text = arr[0]; break;
                        case "POWer_AC_PFACtor": textBox25.Text = arr[0]; break;
                        //转换率
                        case "OUTPut_SLEW_VOLTage_AC": textBox26.Text = arr[0]; break;
                        case "OUTPut_SLEW_VOLTage_DC": textBox27.Text = arr[0]; break;
                        case "OUTPut_SLEW_FREQuency": textBox28.Text = arr[0]; break;
                        //输出控制
                        case "OUTPut_MODE": label49.Text = "输出模式：" + arr[0]; break;
                        //配置
                        case "CONFigure_EXTernal": label58.Text = "模拟(仿真)信号输入：" + arr[0]; break;
                        case "CONFigure_INHibit": label57.Text = "远程控制状态：" + arr[0]; break;
                        case "CONFigure_COUPling": label59.Text = "耦合模式交流信号源输出表示：" + arr[0]; break;
                        //高级模式LIST
                        case "SOURce_LIST_POINts": label42.Text = "列表模式的序列号码：" + arr[0]; break;
                        case "SOURce_LIST_COUNt": textBox29.Text =  arr[0]; break; 
                        case "SOURce_LIST_DWELl": textBox1.Text =  arr[0]; break; 
                        case "SOURce_LIST_BASE": textBox31.Text =  arr[0]; break; 
                        case "SOURce_LIST_SHAPe": textBox72.Text = arr[0]; break;
                        case "SOURce_LIST_VOLTage_AC_STARt": textBox32.Text = arr[0]; break; 
                        case "SOURce_LIST_VOLTage_AC_END": textBox33.Text = arr[0]; break; 
                        case "SOURce_LIST_VOLTage_DC_STARt": textBox34.Text = arr[0]; break; 
                        case "SOURce_LIST_VOLTage_DC_END": textBox35.Text = arr[0]; break; 
                        case "SOURce_LIST_FREQuency_STARt": textBox36.Text = arr[0]; break; 
                        case "SOURce_LIST_FREQuency_END": textBox37.Text = arr[0]; break; 
                        case "SOURce_LIST_DEGRee": textBox38.Text = arr[0]; break; 
                        //PULSE模式
                        case "SOURce_PULSE_VOLTage_AC": textBox39.Text = arr[0]; break; 
                        case "SOURce_PULSE_VOLTage_DC": textBox40.Text = arr[0]; break; 
                        case "SOURce_PULSE_FREQuency": textBox41.Text = arr[0]; break; 
                        case "SOURce_PULSE_SHAPe": textBox42.Text = arr[0]; break; 
                        case "SOURce_PULSE_SPHase": textBox43.Text = arr[0]; break; 
                        case "SOURce_PULSE_COUNt": textBox44.Text = arr[0]; break; 
                        case "SOURce_PULSE_DCYCle": textBox45.Text = arr[0]; break; 
                        case "SOURce_PULSE_PERiod": textBox46.Text = arr[0]; break; 
                        case "TRIG": label81.Text = "模式变换拨片:" + arr[0]; break; 
                        //STEP模式
                        case "SOURce_STEP_VOLTage_AC": textBox47.Text = arr[0]; break; 
                        case "SOURce_STEP_VOLTage_DC": textBox48.Text = arr[0]; break; 
                        case "SOURce_STEP_FREQuency": textBox49.Text = arr[0]; break; 
                        case "SOURce_STEP_SHAPe": textBox50.Text = arr[0]; break; 
                        case "SOURce_STEP_SPHase": textBox51.Text = arr[0]; break; 
                        case "SOURce_STEP_DVOLtage_AC": textBox52.Text = arr[0]; break;
                        case "SOURce_STEP_DVOLtage_DC": textBox53.Text = arr[0]; break; 
                        case "SOURce_STEP_DFRequency": textBox54.Text = arr[0]; break; 
                        case "SOURce_STEP_DWELl": textBox55.Text = arr[0]; break; 
                        case "SOURce_STEP_COUNt": textBox56.Text = arr[0]; break;
                        //HAR模式
                        case "SENSe_HARMonic": label112.Text = "谐波测量开关:"+ arr[0]; break;
                        case "SOURce_CONFigure_HARMonic_SOURce": textBox57.Text = arr[0]; break; 
                        case "SOURce_CONFigure_HARMonic_TIMes": textBox58.Text = arr[0]; break; 
                        case "SOURce_CONFigure_HARMonic_PARameter": textBox59.Text = arr[0]; break; 
                        case "SOURce_CONFigure_HARMonic_FREQuency": textBox60.Text = arr[0]; break;
                        case "FETCh_HARMonic_THD": textBox74.Text = arr[0]; break; 
                        case "FETCh_HARMonic_FUNDamental": textBox75.Text = arr[0]; break; 
                        case "FETCh_HARMonic_ARRay": textBox76.Text = arr[0]; break;
                        //SYN模式
                        case "SOURce_SYNThesis_COMPose": textBox61.Text = arr[0]; break; 
                        case "SOURce_SYNThesis_AMPLitude": textBox62.Text = arr[0]; break; 
                        case "SOURce_SYNThesis_PHASe": textBox63.Text = arr[0]; break; 
                        case "SOURce_SYNThesis_FUNDamental": textBox64.Text = arr[0]; break; 
                        case "SOURce_SYNThesis_DC": textBox65.Text = arr[0]; break; 
                        case "SOURce_SYNThesis_FREQuency": textBox66.Text = arr[0]; break; 
                        case "SOURce_SYNThesis_SPHase": textBox67.Text = arr[0]; break; 
                        //INTERHARMONICS子系统
                        case "SOURce_INTerharmonics_FREQuency_STARt": textBox68.Text = arr[0]; break; 
                        case "SOURce_INTerharmonics_FREQuency_END": textBox69.Text = arr[0]; break; 
                        case "SOURce_INTerharmonics_LEVEl": textBox70.Text = arr[0]; break; 
                        case "SOURce_INTerharmonics_DWELl": textBox71.Text = arr[0]; break;
                        default: break;
                    }
                    label19.Text = "处理完成";

                }
                  )
                );
            }
            catch (Exception ex)
            {
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show(ex.Message);

            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox_receive.Text = "";  //清空接收文本框
            textBox_send.Text = "";     //清空发送文本框
            receive_count = 0;          //接收计数清零
            send_count = 0;             //发送计数
            label7.Text = "Rx:" + receive_count.ToString() + "Bytes";   //刷新界面
            label8.Text = "Tx:" + send_count.ToString() + "Bytes";      //刷新界面
        }




        //向定时器传递信号变量
        string timer1_work = "";
        int i = 0; //定时器计数
        //SCPI指令
        string[] SCPI_VOLTage_word = new string[] {
            "SOURce:VOLTage:LEVel:IMMediate:AMPLitude:AC?"+ "\r\n",
            "SOURce:VOLTage:LEVel:IMMediate:AMPLitude:DC?" + "\r\n",
            "SOURce:VOLTage:LIMit:AC?" + "\r\n",
            "SOURce:VOLTage:LIMit:DC:PLUS?" + "\r\n",
            "SOURce:VOLTage:LIMit:DC:MINus?" +  "\r\n" ,
            "SOURce:VOLTage:RANGe?" + "\r\n"
         };
        string[] SCPI_CURRent_word = new string[] {
            "SOURce:CURRent:LIMit?" + "\r\n" ,//交流电源供应器的均方根值电流限度供软件保护
            "SOURce:CURRent:DELay?" + "\r\n" ,//触发过电流保护的延迟时间
            "SOURce:CURRent:INRush:STARt?" + "\r\n" , //突波电流测量的启动时间
            "SOURce:CURRent:INRush:INTerval?" + "\r\n"  //突波电流测量的量测间隔  
        };
        string[] SCPI_PHASe_word = new string[] {
            "SOURce:PHASe:ON?"  + "\r\n",
            "SOURce:PHASe:OFF?"  + "\r\n"
        };
        string[] SCPI_SLEW_word = new string[] {
            "OUTPut:SLEW:VOLTage:AC?" +"\r\n",
            "OUTPut:SLEW:VOLTage:DC?" +"\r\n",
            "OUTPut:SLEW:FREQuency?" +"\r\n"
        };
        //自定义发送信号标志
        string[] VOLTage_sign = new string[]
        {   "VOLTag_AMPLitude_AC",
            "VOLTag_AMPLitude_DC",
            "VOLTage_LIMit_AC",
            "VOLTage_LIMit_DC_PLUS",
            "VOLTage_LIMit_DC_MINus",
            "VOLTage_RANGe"
        };
        string[] CURRent_sign = new string[]
        {
            "CURRent_LIMit",
            "CURRent_DELay",
            "CURRent_INRush_STARt",
            "CURRent_INRush_INTerval"
         };
        string[] PHASe_sign = new string[]
        {
            "PHASe_ON",
            "PHASe_OFF"
        };
        string[] SLEW_sign = new string[]
        {
             "OUTPut_SLEW_VOLTage_AC",
             "OUTPut_SLEW_VOLTage_DC",
             "OUTPut_SLEW_FREQuency"
        };
        //定时器发送
        private void timer1_Tick(object sender, EventArgs e)
        {
            switch(timer1_work)
            {
                case "VOLTage":
                    if (i > 5){
                        i = 0;
                        timer1.Stop();
                    }
                    send_type = VOLTage_sign[i];
                    timer_send_text = SCPI_VOLTage_word[i];                                      
                    break;
                case "CURRent":
                    if (i > 3)
                    {
                        i = 0;
                        timer1.Stop();
                    }
                    send_type = CURRent_sign[i];
                    timer_send_text = SCPI_CURRent_word[i];
                    break;
                case "PHASe":
                    if (i > 1)
                    {
                        i = 0;
                        timer1.Stop();
                    }
                    send_type = PHASe_sign[i];
                    timer_send_text = SCPI_PHASe_word[i];
                    break;
                case "SLEW":
                    if (i > 2)
                    {
                        i = 0;
                        timer1.Stop();
                    }
                    send_type = SLEW_sign[i];
                    timer_send_text = SCPI_SLEW_word[i];
                    break;
                default: break;
            }
            textBox_receive.Text = "";
            Send();
            i++;
        }





        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }
       
        //利用文本框发送
        public void SendTextBox()
        {
            byte[] temp = new byte[1];
            try
            {
                //首先判断串口是否开启
                if (serialPort1.IsOpen)
                {
                    int num = 0;   //获取本次发送字节数
                    //串口处于开启状态，将发送区文本发送

                    //判断发送模式
                    if (radioButton4.Checked)
                    {
                        //以HEX模式发送
                        //首先需要用正则表达式将用户输入字符中的十六进制字符匹配出来
                        string buf = textBox_send.Text;
                        string pattern = @"\s";
                        string replacement = "";
                        Regex rgx = new Regex(pattern);
                        string send_data = rgx.Replace(buf, replacement);

                        //不发送新行
                        num = (send_data.Length - send_data.Length % 2) / 2;
                        for (int i = 0; i < num; i++)
                        {
                            temp[0] = Convert.ToByte(send_data.Substring(i * 2, 2), 16);
                            serialPort1.Write(temp, 0, 1);  //循环发送
                        }
                        //如果用户输入的字符是奇数，则单独处理
                        if (send_data.Length % 2 != 0)
                        {
                            temp[0] = Convert.ToByte(send_data.Substring(textBox_send.Text.Length - 1, 1), 16);
                            serialPort1.Write(temp, 0, 1);
                            num++;
                        }
                        //判断是否需要发送新行
                        if (checkBox3.Checked)
                        {
                            //自动发送新行
                            serialPort1.WriteLine("");
                        }
                    }
                    else
                    {
                        //以ASCII模式发送
                        //判断是否需要发送新行
                        if (checkBox3.Checked)
                        {
                            //自动发送新行
                            serialPort1.WriteLine(textBox_send.Text);
                            num = textBox_send.Text.Length + 2; //回车占两个字节
                        }
                        else
                        {
                            //不发送新行
                            serialPort1.Write(textBox_send.Text);
                            num = textBox_send.Text.Length;
                        }
                    }

                    send_count += num;      //计数变量累加
                    label8.Text = "Tx:" + send_count.ToString() + "Bytes";   //刷新界面
                }
            }
            catch (Exception ex)
            {
                serialPort1.Close();
                //捕获到异常，创建一个新的对象，之前的不可以再用
                serialPort1 = new System.IO.Ports.SerialPort();
                //刷新COM口选项
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;

            }
        }
        //不利用文本框发送
        string timer_send_text = "null";
        public void Send()
        {
            byte[] temp = new byte[1];
            try
            {
                //首先判断串口是否开启
                if (serialPort1.IsOpen)
                {
                    //串口处于开启状态，将发送区文本发送
                    //以ASCII模式发送
                    //判断是否需要发送新行
                    //不发送新行
                    serialPort1.Write(timer_send_text);
                }
            }
            catch (Exception ex)
            {
                serialPort1.Close();
                //捕获到异常，创建一个新的对象，之前的不可以再用
                serialPort1 = new System.IO.Ports.SerialPort();
                //刷新COM口选项
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;

            }
        }

        //手动发送命令
        private void button2_Click(object sender, EventArgs e)
        {
            SendTextBox();
        }
       
        
        
        
        /*****************************************电流********************************************/
        //获取电流设置
        private void button6_Click(object sender, EventArgs e)
        {   
            timer1.Start();
            timer1_work = "CURRent";
        }
        //设定均方根值电流限度
        private void button9_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CURRent:LIMit " + textBox_CURRent_LIMit.Text + "\r\n"; //交流电源供应器的均方根值电流限度供软件保护;
            send_type = "CURRent_LIMit";
            SendTextBox();
        }
        //电流保护延时时间
        private void button16_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CURRent:DELay " + textBox2.Text + "\r\n"; //触发过电流保护的延迟时间
            send_type = "CURRent_DELay";
            SendTextBox();
        }
        //电流测量启动时间
        private void button17_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CURRent:INRush:STARt " + textBox3.Text + "\r\n"; //突波电流测量的启动时间
            send_type = "CURRent_INRush_STARt";
            SendTextBox();
        }
        ////突波电流测量的量测间隔
        private void button18_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CURRent:INRush:INTerval " + textBox4.Text + "\r\n";  //突波电流测量的量测间隔
            send_type = "CURRent_INRush_INTerval";
            SendTextBox();
        }



        /****************************************************************************************频率*/
        //输出波形频率
        private void button51_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:FREQuency:{CW|IMMediate} " + textBox73.Text + "\r\n";  //突波电流测量的量测间隔
            send_type = "SOURce_FREQuency_{CW|IMMediate}";
            SendTextBox();
        }







        /*******************************************电压*********************************************/
        //获取电压设定
        private void button8_Click(object sender, EventArgs e)
        {
            timer1.Start();
            timer1_work = "VOLTage";
        } 
        //设定交流输出电压
        private void button10_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:VOLTage:LEVel:IMMediate:AMPLitude:AC " + textBox5.Text + "\r\n";   //交流输出电压
            send_type = "VOLTag_AMPLitude_AC";
            SendTextBox();
        }
        //设定直流输出电压
        private void button11_Click_1(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:VOLTage:LEVel:IMMediate:AMPLitude:DC " + textBox6.Text + "\r\n";     //直流输出电压
            send_type = "VOLTag_AMPLitude_DC";
            SendTextBox();
        }
        //设定 Vac LIMIT 值
        private void button12_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:VOLTage:LIMit:AC " + textBox7.Text + "\r\n";     
            send_type = "VOLTage_LIMit_AC";
            SendTextBox();
        }
        //设定 Vdc LIMIT(+)值
        private void button13_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:VOLTage:LIMit:DC:PLUS " + textBox8.Text + "\r\n";
            send_type = "VOLTage_LIMit_DC_PLUS";
            SendTextBox();
        }
        //设定 Vdc LIMIT(-)值
        private void button14_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:VOLTage:LIMit:DC:MINus " + textBox9.Text + "\r\n";
            send_type = "VOLTage_LIMit_DC_MINus";
            SendTextBox();
        }
        //设定输出电压文件位
        private void button15_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:VOLTage:RANGe " + comboBox6.Text + "\r\n";
            send_type = "VOLTage_RANGe";
            SendTextBox();
        }



        /*****************************************相位************************************************/
        //获取相位参数
        private void button21_Click(object sender, EventArgs e)
        {
            timer1.Start();
            timer1_work = "PHASe";
        }
        //开始时设定波形的转变角度
        private void button19_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:PHASe:ON " + textBox10.Text + "\r\n";
            send_type = "PHASe_ON";
            SendTextBox();
        }
        //离开时设定波形的转变角度
        private void button20_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:PHASe:OFF " + textBox11.Text + "\r\n";
            send_type = "PHASe_OFF";
            SendTextBox();
        }






       















        /********************************************测量*********************************************/
        //查询均方根值电流
        private void button22_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:CURRent:AC?" +"\r\n";
            send_type = "CURRent_AC";
            SendTextBox();
        }
        //查询峰值电流
        private void button23_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:CURRent:AMPLitude:MAXimum?" + "\r\n";
            send_type = "CURRent_AMPLitude_MAXimum";
            SendTextBox();
        }
        ////查询电流峰值因数
        private void button24_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:SCALar:CURRent:CREStfactor?" + "\r\n";
            send_type = "CURRent_CREStfactor";
            SendTextBox();
        }
        //查询突波电流
        private void button26_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:SCALar:CURRent:INRush?" + "\r\n";
            send_type = "CURRent_INRush";
            SendTextBox();
        }
        //查询直流电流位准
        private void button27_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:SCALar:CURRent:DC?" + "\r\n";
            send_type = "CURRent_DC";
            SendTextBox();
        }
        //直流电压
        private void button25_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:SCALar:VOLTage:DC?" + "\r\n";
            send_type = "VOLTage_DC";
            SendTextBox();
        }
        //输出端输出的均方根值电压
        private void button28_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:SCALar:VOLTage:ACDC?" + "\r\n";
            send_type = "VOLTage_ACDC";
            SendTextBox();
        }
        //测量输出频率
        private void button37_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:SCALar:FREQuency?" + "\r\n";
            send_type = "FREQuency";
            SendTextBox();
        }
        //输出端输出的实功率
        private void button38_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:SCALar:POWer:AC:REAL?" + "\r\n";
            send_type = "POWer_AC_REAL";
            SendTextBox();
        }
        //输出的视在功率
        private void button39_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:SCALar:POWer:AC:APParent?" + "\r\n";
            send_type = "POWer_AC_APParent";
            SendTextBox();
        }
        //输出端输出的虚功率
        private void button40_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:SCALar:POWer:AC:REACtive?" + "\r\n";
            send_type = "POWer_AC_REACtive";
            SendTextBox();
        }
        //输出的功率因数
        private void button41_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:SCALar:POWer:AC:PFACtor?" + "\r\n";
            send_type = "POWer_AC_PFACtor";
            SendTextBox();
        }

        //记录数据
        private void button5_Click(object sender, EventArgs e)
        {
            ListViewItem i_item = listView1.Items.Add((listView1.Items.Count + 1) + "");
            i_item.SubItems.Add(textBox12.Text);
            i_item.SubItems.Add(textBox18.Text);
            i_item.SubItems.Add(textBox22.Text);
            i_item.SubItems.Add(textBox30.Text);
            i_item.EnsureVisible();
        }
        //清除选中行
        private void button7_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.SelectedItems[0].Remove();
            }
        }
        //清除所有
        private void button75_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }







        /******************************************调试***************************************/
        //重设交流电源供应器为初始的状态。最好等待约 7 秒传送下个指令。
        private void button4_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "*RST" + "\r\n";
            SendTextBox();
        }
        //清除缓存器
        private void button29_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "*CLS" + "\r\n";
            SendTextBox();
        }
        //电源状态*********************************************************************
        private void button32_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "*IDN?"+ "\r\n";
            SendTextBox();
        }
        //电源状态********************************************************************
        
        //查询服务请求启动缓存器
        private void button30_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "*SRE?"+ "\r\n";
            SendTextBox();
        }
        //查询status Byte寄存器
        private void button33_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "*STB?"+ "\r\n";
            SendTextBox();
        }
        //査询交流电源供应器的自我测试结果
        private void button34_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "*TST?"+ "\r\n";
            SendTextBox();
        }
        //数据***************************************************************************
        //储存数值于指定的组别内存中
        private void button35_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "*SAV " + textBox19.Text+ "\r\n";
            SendTextBox();
        }
        //还原之前储存于内存中指定组别的数值
        private void button36_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "*RCL " + textBox20.Text + "\r\n";
            SendTextBox();
        }
        /***********************输出控制*************************************************************************************/
        //输出控制开
        private void button42_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:STATe ON" + "\r\n";
            SendTextBox();
        }
        //输出控制关
        private void button43_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:STATe OFF" + "\r\n";
            SendTextBox();
        }
        //输出继电器开
        private void button44_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:RELay ON" + "\r\n";
            SendTextBox();
        }
        //输出继电器关
        private void button45_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:RELay OFF" + "\r\n";
            SendTextBox();
        }
        //输出信号耦合设计
        private void button46_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:COUPling AC" + "\r\n";
            SendTextBox();
        }

        private void button47_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:COUPling DC" + "\r\n";
            SendTextBox();
        }

        private void button48_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:COUPling ACDC" + "\r\n";
            SendTextBox();
        }
        //输出模式
        //输出模式
        private void button117_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:MODE " + comboBox7.Text+ "\r\n";
            send_type = "OUTPut_MODE";
            SendTextBox();
        }
        //输出AC转换率
        private void button55_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:SLEW:VOLTage:AC " + textBox26.Text + "\r\n";
            send_type = "OUTPut_SLEW_VOLTage_AC";
            SendTextBox();
        }
        //输出DC转换率
        private void button56_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:SLEW:VOLTage:DC " + textBox27.Text + "\r\n";
            send_type = "OUTPut_SLEW_VOLTage_DC";
            SendTextBox();
        }

        private void button57_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:SLEW:FREQuency " + textBox28.Text + "\r\n";
            send_type = "OUTPut_SLEW_FREQuency";
            SendTextBox();
        }
        //获取转换率设定
        private void button58_Click(object sender, EventArgs e)
        {
            timer1.Start();
            timer1_work = "SLEW";
        }
        //除无法输出的锁存
        private void button59_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "OUTPut:PROTection:CLEar" + "\r\n";
            SendTextBox();
        }
        /**************************************配置*************************************/
        //远程控制
        private void button60_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:INHibit OFF" + "\r\n";
            send_type = "CONFigure_INHibit";
            SendTextBox();
        }

        private void button61_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:INHibit LIVE" + "\r\n";
            send_type = "CONFigure_INHibit";
            SendTextBox();
        }

        private void button62_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:INHibit TRIG" + "\r\n";
            send_type = "CONFigure_INHibit";
            SendTextBox();
        }
        //模拟信号输入
        private void button63_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:EXTernal ON" + "\r\n";
            send_type = "CONFigure_EXTernal";
            SendTextBox();
        }

        private void button64_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:EXTernal OFF" + "\r\n";
            send_type = "CONFigure_EXTernal";
            SendTextBox();
        }
        //耦合模式交流信号源输出
        private void button65_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:COUPling AC" + "\r\n";
            send_type = "CONFigure_COUPling";
            SendTextBox();
        }

        private void button66_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:COUPling DC" + "\r\n";
            send_type = "CONFigure_COUPling";
            SendTextBox();
        }
        //前面板
        private void button67_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SYSTem:LOCal" + "\r\n";
            SendTextBox();
        }

        private void button68_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SYSTem:REMote" + "\r\n";
            SendTextBox();
        }


        /*********************************高级模式**************************/
        //模式变换拨片
        private void button90_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "TRIG ON" + "\r\n";
            send_type = "TRIG";
            SendTextBox();
        }

        private void button91_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "TRIG OFF" + "\r\n";
            send_type = "TRIG";
            SendTextBox();
        }
        private void button122_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "TRIG PAUSE" + "\r\n";
            send_type = "TRIG";
            SendTextBox();
        }
        private void button123_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "TRIG CONTINUE" + "\r\n";
            send_type = "TRIG";
            SendTextBox();
        }
        //LIST模式
        private void button70_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:POINts?" + "\r\n";
            send_type = "SOURce_LIST_POINts";
            SendTextBox();
        }
        //设定次数
        private void button71_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:COUNt " + textBox29.Text+ "\r\n";
            send_type = "SOURce_LIST_COUNt";
            SendTextBox();
        }
        //波形缓冲器列表点的序列
        private void button50_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:SHAPe " + textBox72.Text + "\r\n";
            send_type = "SOURce_LIST_SHAPe";
            SendTextBox();
        }

        //设定时间序列
        private void button31_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:DWELl " + textBox1.Text+ "\r\n";
            send_type = "SOURce_LIST_DWELl";
            SendTextBox();
        }
        //设定列表的时间基点。
        private void button69_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:BASE " + textBox31.Text + "\r\n";
            send_type = "SOURce_LIST_BASE";
            SendTextBox();
        }
        //本指令设定交流开始电压列表点的序列
        private void button72_Click_1(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:VOLTage:AC:STARt" + textBox32.Text + "\r\n";
            send_type = "SOURce_LIST_VOLTage_AC_STARt";
            SendTextBox();
        }
        //交流结束电压列表点的序列
        private void button76_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:VOLTage:AC:END " + textBox33.Text + "\r\n";
            send_type = "SOURce_LIST_VOLTage_AC_END";
            SendTextBox();
        }
        //直流开始电压列表点的序列
        private void button77_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:VOLTage:DC:STARt " + textBox34.Text + "\r\n";
            send_type = "SOURce_LIST_VOLTage_DC_STARt";
            SendTextBox();
        }
        //直流结束电压列表点的序列
        private void button78_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:VOLTage:DC:END " + textBox35.Text + "\r\n";
            send_type = "SOURce_LIST_VOLTage_DC_END";
            SendTextBox();
        }
        //开始频率列表点的序列
        private void button79_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:FREQuency:STARt " + textBox36.Text + "\r\n";
            send_type = "SOURce_LIST_FREQuency_STARt";
            SendTextBox();
        }
        //结束频率列表点的序列
        private void button80_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:FREQuency:END " + textBox37.Text + "\r\n";
            send_type = "SOURce_LIST_FREQuency_END";
            SendTextBox();
        }
        //相位角列表点的序列
        private void button81_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:LIST:DEGRee " + textBox38.Text + "\r\n";
            send_type = "SOURce_LIST_DEGRee";
            SendTextBox();
        }
        //PULSE模式////////////////////////////////////////////////////////////////////////
        //工作循环中设定交流电压
        private void button82_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:PULSE:VOLTage:AC " + textBox39.Text + "\r\n";
            send_type = "SOURce_PULSE_VOLTage_AC";
            SendTextBox();
        }
        //工作循环中设定直流电压
        private void button83_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:PULSE:VOLTage:DC " + textBox40.Text + "\r\n";
            send_type = "SOURce_PULSE_VOLTage_DC";
            SendTextBox();
        }
        //工作循环期间设定频率
        private void button84_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:PULSE:FREQuency " + textBox41.Text + "\r\n";
            send_type = "SOURce_PULSE_FREQuency";
            SendTextBox();
        }
        //波形缓冲器
        private void button85_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:PULSE:SHAPe " + textBox42.Text + "\r\n";
            send_type = "SOURce_PULSE_SHAPe";
            SendTextBox();
        }
        //工作循环的开始相位角
        private void button86_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:PULSE:SPHase " + textBox43.Text + "\r\n";
            send_type = "SOURce_PULSE_SPHase";
            SendTextBox();
        }
        //完成之前脉冲执行的次数
        private void button87_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:PULSE:COUNt " + textBox44.Text + "\r\n";
            send_type = "SOURce_PULSE_COUNt";
            SendTextBox();
        }
        //工作循环
        private void button88_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:PULSE:DCYCle " + textBox45.Text + "\r\n";
            send_type = "SOURce_PULSE_DCYCle";
            SendTextBox();
        }
        //周期
        private void button89_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:PULSE:PERiod " + textBox46.Text + "\r\n";
            send_type = "SOURce_PULSE_PERiod";
            SendTextBox();
        }
        //STEP模式
        //STEP模式的初始交流电压
        private void button92_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:STEP:VOLTage:AC " + textBox47.Text + "\r\n";
            send_type = "SOURce_STEP_VOLTage_AC";
            SendTextBox();
        }
        //STEP 模式的初始直流电压
        private void button93_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:STEP:VOLTage:DC " + textBox48.Text + "\r\n";
            send_type = "SOURce_STEP_VOLTage_DC";
            SendTextBox();
        }
        //STEP 模式的初始频率
        private void button94_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:STEP:FREQuency " + textBox49.Text + "\r\n";
            send_type = "SOURce_STEP_FREQuency";
            SendTextBox();
        }
        //STEP 模式选择波形缓冲器
        private void button95_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:STEP:SHAPe " + textBox50.Text + "\r\n";
            send_type = "SOURce_STEP_SHAPe";
            SendTextBox();
        }
        //STEP 模式的开始相位角
        private void button96_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:STEP:SPHase " + textBox51.Text + "\r\n";
            send_type = "SOURce_STEP_SPHase";
            SendTextBox();
        }
        //每个步骤中的角接交流电压
        private void button97_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:STEP:DVOLtage:AC " + textBox52.Text + "\r\n";
            send_type = "SOURce_STEP_DVOLtage_AC";
            SendTextBox();
        }
        //每个步骤中的角接直流电压
        private void button98_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:STEP:DVOLtage:DC " + textBox53.Text + "\r\n";
            send_type = "SOURce_STEP_DVOLtage_DC";
            SendTextBox();
        }
        //每个步骤中的角接频率
        private void button99_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:STEP:DFRequency " + textBox54.Text + "\r\n";
            send_type = "SOURce_STEP_DFRequency";
            SendTextBox();
        }
        //每个步骤中的停留时间
        private void button100_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:STEP:DWELl " + textBox55.Text + "\r\n";
            send_type = "SOURce_STEP_DWELl";
            SendTextBox();
        }
        //完成之前执行步骤的次数
        private void button101_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:STEP:COUNt " + textBox56.Text + "\r\n";
            send_type = "SOURce_STEP_COUNt";
            SendTextBox();
        }
        //HAR模式
        //谐波分析模式的测量电源
        //谐波测量开关
        private void button52_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SENSe:HARMonic ON" +  "\r\n";
            send_type = "SENSe_HARMonic";
            SendTextBox();
        }

        private void button53_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SENSe:HARMonic OFF" + "\r\n";
            send_type = "SENSe_HARMonic";
            SendTextBox();
        }
        private void button102_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:HARMonic:SOURce " + textBox57.Text + "\r\n";
            send_type = "SOURce_CONFigure_HARMonic_SOURce";
            SendTextBox();
        }
        // //测量结果显示于LCD的方式
        private void button103_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:HARMonic:TIMes " + textBox58.Text + "\r\n";
            send_type = "SOURce_CONFigure_HARMonic_TIMes";
            SendTextBox();
        }
        //每个谐波阶的数据格式
        private void button104_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:HARMonic:PARameter " + textBox59.Text + "\r\n";
            send_type = "SOURce_CONFigure_HARMonic_PARameter";
            SendTextBox();
        }
        //原始波形的基频
        private void button105_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:CONFigure:HARMonic:FREQuency " + textBox60.Text + "\r\n";
            send_type = "SOURce_CONFigure_HARMonic_FREQuency";
            SendTextBox();
        }
        //总和谐失真的%
        private void button54_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:HARMonic:THD?"  + "\r\n";
            send_type = "FETCh_HARMonic_THD";
            SendTextBox();
        }
        //输出电流或电压的基频
        private void button118_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:HARMonic:FUNDamental?" + "\r\n";
            send_type = "FETCh_HARMonic_FUNDamental";
            SendTextBox();
        }
        //所有谐波阶的振幅
        private void button119_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "FETCh:HARMonic:ARRay?" + "\r\n";
            send_type = "FETCh_HARMonic_ARRay";
            SendTextBox();
        }


        //SYN模式
        //每个谐波阶的数据格式
        private void button106_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:SYNThesis:COMPose " + textBox61.Text + "\r\n";
            send_type = "SOURce_SYNThesis_COMPose";
            SendTextBox();
        }
        //每个谐波阶的振幅
        private void button107_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:SYNThesis:AMPLitude " + textBox62.Text + "\r\n";
            send_type = "SOURce_SYNThesis_AMPLitude";
            SendTextBox();
        }
        //每个谐波阶的相位角
        private void button108_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:SYNThesis:PHASe " + textBox63.Text + "\r\n";
            send_type = "SOURce_SYNThesis_PHASe";
            SendTextBox();
        }
        //基本交流电压
        private void button109_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:SYNThesis:FUNDamental " + textBox64.Text + "\r\n";
            send_type = "SOURce_SYNThesis_FUNDamental";
            SendTextBox();
        }
        //直流电压使 SYNTHESIS 模式的电压波形增加
        private void button110_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:SYNThesis:DC " + textBox65.Text + "\r\n";
            send_type = "SOURce_SYNThesis_DC";
            SendTextBox();
        }
        //SYNTHESIS 模式的基频
        private void button111_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:SYNThesis:FREQuency " + textBox66.Text + "\r\n";
            send_type = "SOURce_SYNThesis_FREQuency";
            SendTextBox();
        }
        //SYNTHESIS 模式的起始相位角
        private void button112_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:SYNThesis:SPHase " + textBox67.Text + "\r\n";
            send_type = "SOURce_SYNThesis_SPHase";
            SendTextBox();
        }
        //
        //扫描波启动频率
        private void button113_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:INTerharmonics:FREQuency:STARt " + textBox68.Text + "\r\n";
            send_type = "SOURce_INTerharmonics_FREQuency_STARt";
            SendTextBox();
        }
        //扫描波结束频率
        private void button114_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:INTerharmonics:FREQuency:END " + textBox69.Text + "\r\n";
            send_type = "SOURce_INTerharmonics_FREQuency_END";
            SendTextBox();
        }
        //本指令设定扫描波的均方根值大小为基频的多少百分率。
        private void button115_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:INTerharmonics:LEVEl " + textBox70.Text + "\r\n";
            send_type = "SOURce_INTerharmonics_LEVEl";
            SendTextBox();
        }
        //扫描波的停留时间
        private void button116_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "SOURce:INTerharmonics:DWELl " + textBox71.Text + "\r\n";
            send_type = "SOURce_INTerharmonics_DWELl";
            SendTextBox();
        }
      
        








        //tabcontrol的标签皮肤改造

        private void tabcontrol_item(object sender, DrawItemEventArgs e)
        {
            //标签背景填充颜色
            SolidBrush color1 = new SolidBrush(Color.LightGoldenrodYellow);
            SolidBrush color2 = new SolidBrush(Color.LightSeaGreen);
            Rectangle rec1 = tabControl5.GetTabRect(0);
            Rectangle rec2 = tabControl5.GetTabRect(1);
            Rectangle rec3 = tabControl5.GetTabRect(2);
            e.Graphics.FillRectangle(color1,rec1);
            e.Graphics.FillRectangle(color1, rec2);
            e.Graphics.FillRectangle(color2, rec3);
            //标签文字填充颜色
            SolidBrush FrontBrush = new SolidBrush(Color.Black);
            StringFormat StringF = new StringFormat();
            //设置文字对齐方式
            StringF.Alignment = StringAlignment.Center;
            //StringF.LineAlignment = StringAlignment.Center;
            
            for (int i = 0; i < tabControl5.TabPages.Count; i++)
            {
                 //获取标签头工作区域
                 Rectangle Rec = tabControl5.GetTabRect(i);
                 //绘制标签头背景颜色
                // e.Graphics.FillRectangle(BackBrush, Rec);
                //绘制标签头文字
                e.Graphics.DrawString(tabControl5.TabPages[i].Text, new Font("宋体", 9), FrontBrush, Rec, StringF);
            }
        }
       
        #region **测试数据**
        public List<float> x1 = new List<float>();
        public List<float> y1 = new List<float>();
        public List<float> x2 = new List<float>();
        public List<float> y2 = new List<float>();
        public List<float> x3 = new List<float>();
        public List<float> y3 = new List<float>();
        public List<float> x4 = new List<float>();
        public List<float> y4 = new List<float>();
        #endregion
        //波形输出控制/////////////////////////////////////////////////////////////////////////////////
        private void button73_Click(object sender, EventArgs e)
        {      
            x1.Clear();
            y1.Clear();
            x2.Clear();
            y2.Clear();
            x3.Clear();
            y3.Clear();
            x4.Clear();
            y4.Clear();
            zGraph1.f_ClearAllPix();
            zGraph1.f_reXY();
            zGraph1.f_LoadOnePix(ref x1, ref y1, Color.Red, 2);
            zGraph1.f_AddPix(ref x2, ref y2, Color.Blue, 3);
           // zGraph1.f_AddPix(ref x3, ref y3, Color.FromArgb(0, 128, 192), 2);
           // zGraph1.f_AddPix(ref x4, ref y4, Color.Yellow, 3); 
            timer2波形.Start();
            //初始化
            textBox12.Text = "12";
            textBox18.Text = "18";
        }
        //波形关闭控制
        private void button74_Click(object sender, EventArgs e)
        {
            timer2波形.Stop();
            button73.Enabled = true;
        }
        int j = 0;//开关变量
        private int timerDrawI = 0;//横坐标
        private void timedraw_Tick(object sender, EventArgs e)
        {
            ///模拟串口采样显示[周期k]
            button73.Enabled = false;
            switch (j)
            {
                case 0:
                    if (checkBox4.Checked)
                    {
                        timer_send_text = "FETCh:CURRent:AC?" + "\r\n";
                        send_type = "CURRent_AC";
                        textBox_receive.Text = "";
                        Send();
                        x1.Add(timerDrawI);
                        y1.Add(Convert.ToInt32(textBox12.Text));
                    }
                    break;
                case 1:
                    if (checkBox2.Checked)
                    {   //电流有效值输出
                        timer_send_text = "FETCh:SCALar:VOLTage:ACDC?" + "\r\n";
                        send_type = "VOLTage_ACDC";
                        textBox_receive.Text = "";
                        Send();
                        x2.Add(timerDrawI);
                        y2.Add(Convert.ToInt32(textBox18.Text));
                    }
                    break;
            }
            timerDrawI++;
            if (j > 1) j = 0;
            else j++;
            zGraph1.f_Refresh();
        }
        //初始化
        private void button4_Click_1(object sender, EventArgs e)
        {

        }
        //显示指令帮助窗体
        private void button49_Click(object sender, EventArgs e)
        {
            指令帮助 x = new 指令帮助();
            x.Show();
        }

        
    }
}

