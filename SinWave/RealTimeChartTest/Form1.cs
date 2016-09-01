using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LineChartTEST
{
    public partial class Form1 : Form
    {
        class Point
        {
            public double X { get; set; }
            public double Y { get; set; }

            public Point()
            {
                X = 0;
                Y = 0;
            }

            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        private delegate void CanIJust();

        //private List<int> _valueList;
        private List<Point> _pointsList;
        private Thread _thread;
        private CanIJust _doIt;
        private Random _ran;
        private int _interval;
        private List<double> _timeList;
        private List<int> _customValueList;
        private const int ListenPort = 5000;
        private static readonly int XMaxSize = 256;

        public Form1()
        {
            InitializeComponent();

            chart1.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
            chart1.Series[0].XValueType = ChartValueType.Double;
            chart1.Series[0].YValueType = ChartValueType.Double;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "{##.##}Hz";
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{##.##}dB";
            //chart1.ChartAreas[0].AxisX.ScaleView.SizeType = IntervalType.Number;
            //chart1.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.FixedCount;
            //chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Auto;
            //chart1.ChartAreas[0].AxisX.Interval = 0;

            //_valueList = new List<int>();
            _pointsList = new List<Point>();
            _ran = new Random();
            _interval = 1;
            //tbUpdateInterval.Text = "500";

            //Begin to listen
            //UDPListener();
            Task task = Task.Factory.StartNew(UDPListener);

            //Draw
            GoBoy();

            _timeList = new List<double>();
            _customValueList = new List<int>();
        }

        private void GoBoy()
        {
            _doIt += new CanIJust(AddData);

            DateTime now = DateTime.Now;
            chart1.ChartAreas[0].AxisX.Minimum = -50;
            chart1.ChartAreas[0].AxisX.Maximum = 50;


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

        private void AddData()
        {
            chart1.ResetAutoValues();
            chart1.Series[0].Points.Clear();

            if (_pointsList.Count >= XMaxSize)
            {
                for (int i = 0; i < XMaxSize; i++)
                {
                    chart1.Series[0].Points.AddXY(_pointsList[i].X , _pointsList[i].Y);
                }
                _pointsList.RemoveRange(0, XMaxSize);
            }

            chart1.Invalidate();
        }

        ////Clear list
        //_pointsList.Clear();

        #region Unused

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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_thread != null)
                _thread.Abort();
        }

        private void btn2D_Click(object sender, EventArgs e)
        {
            btn2D.Enabled = false;
            btn3D.Enabled = true;

            chart1.ChartAreas[0].Area3DStyle.Enable3D = false;
        }

        private void btn3D_Click(object sender, EventArgs e)
        {
            btn2D.Enabled = true;
            btn3D.Enabled = false;

            chart1.ChartAreas[0].Area3DStyle.Enable3D = true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            _customValueList.Add(_ran.Next(0, 100));
            _timeList.Add(DateTime.Now.ToOADate());
            UpdateSecondChart();
        }

        private void UpdateSecondChart()
        {
            chart2.Series[0].Points.AddXY(_timeList[_timeList.Count - 1], _customValueList[_customValueList.Count - 1]);
            chart2.Invalidate();
        }

        private void btnSerialize_Click(object sender, EventArgs e)
        {
            string filePath = Application.StartupPath + "\\ChartData_Stream.xml";
            if (File.Exists(filePath))
            {
                File.Copy(filePath, Application.StartupPath + "\\ChartData_Stream.bak", true);
                File.Delete(filePath);
            }

            FileStream stream = new FileStream(filePath, FileMode.Create);

            chart2.Serializer.Content = SerializationContents.Default;
            chart2.Serializer.Format = System.Windows.Forms.DataVisualization.Charting.SerializationFormat.Xml;
            chart2.Serializer.Save(stream);

            stream.Close();

            //FileStream stream = new FileStream(filePath, FileMode.Create);
            //StreamWriter writer = new StreamWriter(stream);
        }

        private void btnDeserialize_Click(object sender, EventArgs e)
        {
            string filePath = Application.StartupPath + "\\ChartData_Stream.xml";
            FileStream stream = new FileStream(filePath, FileMode.Open);
            chart2.Serializer.IsResetWhenLoading = true;
            chart2.Serializer.Load(stream);

            stream.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            chart2.Series[0].Points.Clear();
        }

        private void btnFilePathSe_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath = Application.StartupPath + "\\ChartData_FilePath.xml";
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, Application.StartupPath + "\\ChartData_FilePath.bak", true);
                    File.Delete(filePath);
                }

                //FileStream stream = new FileStream(filePath, FileMode.Create);

                chart2.Serializer.Content = SerializationContents.Default;
                chart2.Serializer.Format = System.Windows.Forms.DataVisualization.Charting.SerializationFormat.Xml;
                chart2.Serializer.Save(filePath);

                //stream.Close();
                //FileStream stream = new FileStream(filePath, FileMode.Create);
                //StreamWriter writer = new StreamWriter(stream);
            }
            catch (Exception exc)
            {
                MessageBox.Show("An exception occurred.\nPlease try again.");
            }
        }

        private void btnFilePathDe_Click(object sender, EventArgs e)
        {
            string filePath = Application.StartupPath + "\\ChartData_FilePath.xml";
            chart2.Serializer.Reset();
            chart2.Serializer.Load(filePath);

        }

        #endregion

        public void UDPListener()
        {
            UdpClient listener = new UdpClient(ListenPort);
            IPEndPoint groupEp = new IPEndPoint(IPAddress.Any, ListenPort);

            while (true)
            {
                //if (_pointsList.Count < XMaxSize)
                //{
                    var receiveByteArray = listener.Receive(ref groupEp);

                    if (receiveByteArray.Length != 0)
                    {
                        float[] fArray = new float[receiveByteArray.Length/4];

                        for (int i = 0; i < receiveByteArray.Length/4; i++)
                            fArray[i] = BitConverter.ToSingle(receiveByteArray, i*4);

                        for (var i = 0; i < fArray.Length; i++)
                        {
                            _pointsList.Add(new Point(fArray[i], fArray[i + 1]));
                            i++;
                        }
                    }
                //}
                //else
                //    Thread.Sleep(10);
            }
            //listener.Close();
        }

    }
}
