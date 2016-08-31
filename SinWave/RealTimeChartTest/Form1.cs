using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LineChartTEST
{
    public partial class Form1 : Form
    {
        private delegate void CanIJust();
        private List<int> _valueList;
        private Thread _thread;
        private CanIJust _doIt;
        private Random _ran;
        private int _interval;
        private List<double> _timeList;
        private List<int> _customValueList;
        private const int ListenPort = 5000;
        private static double[] _peakHold;
        private static readonly double SampleRate = 1e6;
        private static readonly double BasebandFreq = 1e5;

        public Form1()
        {
            InitializeComponent();

            chart1.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
            chart1.Series[0].XValueType = ChartValueType.Time;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss.fff";
            chart1.ChartAreas[0].AxisX.ScaleView.SizeType = DateTimeIntervalType.Milliseconds;
            chart1.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.FixedCount;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Milliseconds;
            chart1.ChartAreas[0].AxisX.Interval = 0;

            _valueList = new List<int>();
            _ran = new Random();
            _interval = 500;
            tbUpdateInterval.Text = "500";
            GoBoy();


            _timeList = new List<double>();
            _customValueList = new List<int>();
        }

        private void GoBoy()
        {
            _doIt += new CanIJust(UDPListener);
            DateTime now = DateTime.Now;
            chart1.ChartAreas[0].AxisX.Minimum = now.ToOADate();
            chart1.ChartAreas[0].AxisX.Maximum = now.AddSeconds(10).ToOADate();


            _thread = new Thread(new ThreadStart(ComeOnYouThread));
            _thread.Start();
        }

        private void ComeOnYouThread()
        {
            while (true)
            {
                try
                {
                    chart1.Invoke(_doIt);

                    Thread.Sleep(_interval);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception : " + e.ToString());
                }
            }
        }

        private void AddData(double x)
        {
            DateTime now = DateTime.Now;
            //Insert a number into the list.
            _valueList.Add(_ran.Next(0, 100));


            chart1.ResetAutoValues();

            //Remove old datas from the chart.
            if (chart1.Series[0].Points.Count > 0)
            {
                while (chart1.Series[0].Points[0].XValue < now.AddSeconds(-5).ToOADate())
                {
                    chart1.Series[0].Points.RemoveAt(0);

                    chart1.ChartAreas[0].AxisX.Minimum = chart1.Series[0].Points[0].XValue;
                    chart1.ChartAreas[0].AxisX.Maximum = now.AddSeconds(5).ToOADate();
                }
            }

            //Insert a data into the chart.

            chart1.Series[0].Points.AddXY(now.ToOADate(), _valueList[_valueList.Count - 1]);

            chart1.Invalidate();
        }

        private void btnUpdateInterval_Click(object sender, EventArgs e)
        {
            int interval = 0;
            if (int.TryParse(tbUpdateInterval.Text, out interval))
            {
                if (interval > 0)
                    _interval = interval;
                else
                    MessageBox.Show("The data should be more than 0");
            }
            else
            {
                MessageBox.Show("Inappropriate data.");
            }
        }

        //protected override void OnClosed(EventArgs e)
        //{
        //    base.OnClosed(e);
        //    if (_thread != null)
        //        _thread.Abort();
        //}

        //private void btn2D_Click(object sender, EventArgs e)
        //{
        //    btn2D.Enabled = false;
        //    btn3D.Enabled = true;

        //    chart1.ChartAreas[0].Area3DStyle.Enable3D = false;
        //}

        //private void btn3D_Click(object sender, EventArgs e)
        //{
        //    btn2D.Enabled = true;
        //    btn3D.Enabled = false;

        //    chart1.ChartAreas[0].Area3DStyle.Enable3D = true;
        //}

        //private void btnAdd_Click(object sender, EventArgs e)
        //{            
        //    _customValueList.Add(_ran.Next(0,100));
        //    _timeList.Add(DateTime.Now.ToOADate());
        //    UpdateSecondChart();
        //}

        //private void UpdateSecondChart()
        //{
        //    chart2.Series[0].Points.AddXY(_timeList[_timeList.Count - 1], _customValueList[_customValueList.Count - 1]);
        //    chart2.Invalidate();
        //}

        //private void btnSerialize_Click(object sender, EventArgs e)
        //{
        //    string filePath = Application.StartupPath + "\\ChartData_Stream.xml";
        //    if (File.Exists(filePath))
        //    {
        //        File.Copy(filePath, Application.StartupPath + "\\ChartData_Stream.bak", true);
        //        File.Delete(filePath);
        //    }

        //    FileStream stream = new FileStream(filePath, FileMode.Create);

        //    chart2.Serializer.Content = SerializationContents.Default;
        //    chart2.Serializer.Format = System.Windows.Forms.DataVisualization.Charting.SerializationFormat.Xml;
        //    chart2.Serializer.Save(stream);

        //    stream.Close();

        //    //FileStream stream = new FileStream(filePath, FileMode.Create);
        //    //StreamWriter writer = new StreamWriter(stream);
        //}

        //private void btnDeserialize_Click(object sender, EventArgs e)
        //{
        //    string filePath = Application.StartupPath + "\\ChartData_Stream.xml";
        //    FileStream stream = new FileStream(filePath, FileMode.Open);
        //    chart2.Serializer.IsResetWhenLoading = true;
        //    chart2.Serializer.Load(stream);

        //    stream.Close();
        //}

        //private void btnClear_Click(object sender, EventArgs e)
        //{
        //    chart2.Series[0].Points.Clear();
        //}

        //private void btnFilePathSe_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        string filePath = Application.StartupPath + "\\ChartData_FilePath.xml";
        //        if (File.Exists(filePath))
        //        {
        //            File.Copy(filePath, Application.StartupPath + "\\ChartData_FilePath.bak", true);
        //            File.Delete(filePath);
        //        }

        //        //FileStream stream = new FileStream(filePath, FileMode.Create);

        //        chart2.Serializer.Content = SerializationContents.Default;
        //        chart2.Serializer.Format = System.Windows.Forms.DataVisualization.Charting.SerializationFormat.Xml;
        //        chart2.Serializer.Save(filePath);

        //        //stream.Close();
        //        //FileStream stream = new FileStream(filePath, FileMode.Create);
        //        //StreamWriter writer = new StreamWriter(stream);
        //    }
        //    catch (Exception exc)
        //    {
        //        MessageBox.Show("An exception occurred.\nPlease try again.");
        //    }
        //}

        //private void btnFilePathDe_Click(object sender, EventArgs e)
        //{
        //    string filePath = Application.StartupPath + "\\ChartData_FilePath.xml";
        //    chart2.Serializer.Reset();
        //    chart2.Serializer.Load(filePath);

        //}

        public void UDPListener()
        {
            UdpClient listener = new UdpClient(ListenPort);
            IPEndPoint groupEp = new IPEndPoint(IPAddress.Any, ListenPort);

            try
            {
                while (true)
                {
                    Console.WriteLine(@"Waiting for broadcast");
                    var receiveByteArray = listener.Receive(ref groupEp);
                    Console.WriteLine($"Received a broadcast from {groupEp}");

                    if (receiveByteArray.Length != 0)
                    {
                        float[] fArray = new float[receiveByteArray.Length / 4];

                        for (int i = 0; i < receiveByteArray.Length / 4; i++)
                            fArray[i] = BitConverter.ToSingle(receiveByteArray, i * 4);

                        var scaleFactor = SetScale(SampleRate, BasebandFreq);

                        var amplitudeArr = TransformData(fArray, SampleRate, BasebandFreq, scaleFactor);

                        for (var i = 0; i < amplitudeArr.GetLength(0); i++)
                        {
                            AddData(amplitudeArr[i,0]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                Console.ReadLine();
            }
            listener.Close();
        }

        public static double SetScale(double sampleRate, double basebandFreq)
        {
            double res;
            var x = Math.Max(Math.Abs(sampleRate), Math.Abs(basebandFreq));
            if (x >= 10e9)
                res = 1e-9;
            else if (x >= 10e6)
                res = 1e-6;
            else
                res = 1e-3;
            return res;
        }

        public static double[,] TransformData(float[] arr, double sampleRate, double basebandFreq, double scaleFactor)
        {
            var length = arr.Length;
            double[,] pointArr = new double[length, 2];

            //Peak hold
            if (_peakHold == null)
            {
                _peakHold = new double[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                    _peakHold[i] = arr[i];
            }
            else
            {
                for (int i = 0; i < arr.Length; i++)
                    if (arr[i] < _peakHold[i])
                        arr[i] = (float)_peakHold[i];
            }

            //Reverse
            for (var i = 0; i < arr.Length / 2; i++)
            {
                var temp = arr[i];
                arr[i] = arr[arr.Length / 2 + i - 1];
                arr[arr.Length / 2 + i - 1] = temp;
            }

            for (var i = -length / 2; i < length / 2 - 1; i++)
            {
                //Hz
                pointArr[length / 2 + i, 0] = i * sampleRate * scaleFactor / length + basebandFreq * scaleFactor;
                //dB
                pointArr[length / 2 + i, 1] = arr[length / 2 + i];
            }

            return pointArr;
        }
    }
}