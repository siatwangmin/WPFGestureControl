using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GestureControl
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        double[] a = new double[4], w = new double[4], h = new double[4], Angle = new double[4], Port = new double[4];
        double Temperature, Pressure, Altitude, GroundVelocity, GPSYaw, GPSHeight;
        long Longitude, Latitude;
        private bool bListening = false;
        private bool bClosing = false;

        private SerialPort spSerialPort;



        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer updateTimer = new DispatcherTimer();
            TimeSpan timeSpan = new TimeSpan(0,0,0,0,50);
            updateTimer.Interval = timeSpan;
            updateTimer.Tick += OnUpdateTimer;
            updateTimer.Start();
            
            spSerialPort = new SerialPort("COM22",9600);
            spSerialPort.DataReceived += SerialPort_DataReceived;
            spSerialPort.Open();
           
        }

        private void OnUpdateTimer(object sender, EventArgs e)
        {
            lblRoll.Content = Angle[0].ToString();
            lblPitch.Content = Angle[1].ToString();
            lblYaw.Content = Angle[2].ToString();
            model3DH.Angle = (int) Angle[2];
            model3DP.Angle = 90 -(int) Angle[0];
            model3DR.Angle = (int) Angle[1];
            //throw new NotImplementedException();
        }

        private double[] LastTime = new double[10];
        short sRightPack = 0;
        short[] ChipTime = new short[7];
        private void DecodeData(byte[] byteTemp)
        {
            double[] Data = new double[4];
            //double TimeElapse = (DateTime.Now - TimeStart).TotalMilliseconds / 1000;

            Data[0] = BitConverter.ToInt16(byteTemp, 2);
            Data[1] = BitConverter.ToInt16(byteTemp, 4);
            Data[2] = BitConverter.ToInt16(byteTemp, 6);
            Data[3] = BitConverter.ToInt16(byteTemp, 8);
            sRightPack++;
            switch (byteTemp[1])
            {
                case 0x50:
                    //Data[3] = Data[3] / 32768 * double.Parse(textBox9.Text) + double.Parse(textBox8.Text);
                    ChipTime[0] = (short)(2000 + byteTemp[2]);
                    ChipTime[1] = byteTemp[3];
                    ChipTime[2] = byteTemp[4];
                    ChipTime[3] = byteTemp[5];
                    ChipTime[4] = byteTemp[6];
                    ChipTime[5] = byteTemp[7];
                    ChipTime[6] = BitConverter.ToInt16(byteTemp, 8);


                    break;
                case 0x51:
                    //Data[3] = Data[3] / 32768 * double.Parse(textBox9.Text) + double.Parse(textBox8.Text);
                    Temperature = Data[3] / 100.0;
                    Data[0] = Data[0] / 32768.0 * 16;
                    Data[1] = Data[1] / 32768.0 * 16;
                    Data[2] = Data[2] / 32768.0 * 16;

                    a[0] = Data[0];
                    a[1] = Data[1];
                    a[2] = Data[2];
                    a[3] = Data[3];
                   // if ((TimeElapse - LastTime[1]) < 0.1) return;
                 //   LastTime[1] = TimeElapse;

                    break;
                case 0x52:
                    //Data[3] = Data[3] / 32768 * double.Parse(textBox9.Text) + double.Parse(textBox8.Text);
                    Temperature = Data[3] / 100.0;
                    Data[0] = Data[0] / 32768.0 * 2000;
                    Data[1] = Data[1] / 32768.0 * 2000;
                    Data[2] = Data[2] / 32768.0 * 2000;
                    w[0] = Data[0];
                    w[1] = Data[1];
                    w[2] = Data[2];
                    w[3] = Data[3];

                   // if ((TimeElapse - LastTime[2]) < 0.1) return;
                  //  LastTime[2] = TimeElapse;
                    break;
                case 0x53:
                    //Data[3] = Data[3] / 32768 * double.Parse(textBox9.Text) + double.Parse(textBox8.Text);
                    Temperature = Data[3] / 100.0;
                    Data[0] = Data[0] / 32768.0 * 180;
                    Data[1] = Data[1] / 32768.0 * 180;
                    Data[2] = Data[2] / 32768.0 * 180;
                    Angle[0] = Data[0];
                    Angle[1] = Data[1];
                    Angle[2] = Data[2];
                    Angle[3] = Data[3];
                  //  if ((TimeElapse - LastTime[3]) < 0.1) return;
                  //  LastTime[3] = TimeElapse;
                    break;
                case 0x54:
                    //Data[3] = Data[3] / 32768 * double.Parse(textBox9.Text) + double.Parse(textBox8.Text);
                    Temperature = Data[3] / 100.0;
                    h[0] = Data[0];
                    h[1] = Data[1];
                    h[2] = Data[2];
                    h[3] = Data[3];
                   // if ((TimeElapse - LastTime[4]) < 0.1) return;
                  //  LastTime[4] = TimeElapse;
                    break;
                case 0x55:
                    Port[0] = Data[0];
                    Port[1] = Data[1];
                    Port[2] = Data[2];
                    Port[3] = Data[3];

                    break;

                case 0x56:
                    Pressure = BitConverter.ToInt32(byteTemp, 2);
                    Altitude = (double)BitConverter.ToInt32(byteTemp, 6) / 100.0;

                    break;

                case 0x57:
                    Longitude = BitConverter.ToInt32(byteTemp, 2);
                    Latitude = BitConverter.ToInt32(byteTemp, 6);

                    break;

                case 0x58:
                    GPSHeight = (double)BitConverter.ToInt16(byteTemp, 2) / 10.0;
                    GPSYaw = (double)BitConverter.ToInt16(byteTemp, 4) / 10.0;
                    GroundVelocity = BitConverter.ToInt16(byteTemp, 6) / 1e3;

                    break;
                default:
                    break;
            }
        }
        byte byteLastNo = 0;

        delegate void UpdateData(byte[] byteData);//声明一个委托
        byte[] RxBuffer = new byte[1000];
        UInt16 usRxLength = 0;
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] byteTemp = new byte[1000];

            if (bClosing) return;
            try
            {
                bListening = true;
                UInt16 usLength = 0;
                try
                {
                    usLength = (UInt16)spSerialPort.Read(RxBuffer, usRxLength, 700);
                }
                catch (Exception err)
                {
                    //MessageBox.Show(err.Message);
                    //return;
                }
                usRxLength += usLength;
                while (usRxLength >= 11)
                {
                    UpdateData Update = new UpdateData(DecodeData);
                    RxBuffer.CopyTo(byteTemp, 0);
                    if (!((byteTemp[0] == 0x55) & ((byteTemp[1] & 0x50) == 0x50)))
                    {
                        for (int i = 1; i < usRxLength; i++) RxBuffer[i - 1] = RxBuffer[i];
                        usRxLength--;
                        continue;
                    }
                    if (((byteTemp[0] + byteTemp[1] + byteTemp[2] + byteTemp[3] + byteTemp[4] + byteTemp[5] + byteTemp[6] + byteTemp[7] + byteTemp[8] + byteTemp[9]) & 0xff) == byteTemp[10])
                        Dispatcher.Invoke(Update, byteTemp);
                    for (int i = 11; i < usRxLength; i++) RxBuffer[i - 11] = RxBuffer[i];
                    usRxLength -= 11;
                }

                Thread.Sleep(10);
            }
            finally
            {
                bListening = false;//我用完了，ui可以关闭串口了。   
            }
        }
 
    }
}
