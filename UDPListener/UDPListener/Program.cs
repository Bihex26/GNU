using System;
using System.Net;
using System.Net.Sockets;

namespace UDPListener
{
    class Program
    {
        private const int ListenPort = 5000;
        private static double[] _peakHold;

        private static readonly double SampleRate = 1e6;
        private static readonly double BasebandFreq = 1e5;

        [STAThread]
        public static void Main()
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
                            Console.WriteLine(i + "||" + amplitudeArr[i, 0] + "|||" + amplitudeArr[i, 1]);
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

