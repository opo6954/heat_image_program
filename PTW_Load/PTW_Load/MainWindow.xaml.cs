
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Threading;
using IRNDT_System.LockIn;
using Microsoft.Win32;
using PTW_Load.MeasureItem;

namespace PTW_Load
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    
    
    public partial class MainWindow : Window
    {
        
        [DllImport("CalcLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern public static unsafe void CalcurateMinMax(IntPtr img, IntPtr sum, IntPtr pmax, IntPtr pmin, int size, out int min, out int max, int bit);

        [DllImport("CalcLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern public static unsafe void DrawImage(IntPtr hBit, IntPtr img, int width, int height, int min, int max, int span, int bit);

        [DllImport("CalcLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern public static unsafe void DrawImageRGB(IntPtr hBit, IntPtr img, int width, int height, int min, int max, int span, int bit);

        [DllImport("CalcLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern public static unsafe void GetAvgData(IntPtr sum, IntPtr avg, int count, int size);

        [DllImport("CalcLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern public static unsafe void GetDeltaData(IntPtr delta, IntPtr max, IntPtr min, int size);

        [DllImport("CalcLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern public static unsafe void CalcurateMinMax2(IntPtr img, int size, out int min, out int max);

        [DllImport("CalcLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern public static unsafe void DrawImage2(IntPtr hBit, IntPtr img, int width, int height, int min, int max, int span, int bit);

        [DllImport("CalcLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern public static unsafe void DrawImage2RGB(IntPtr hBit, IntPtr img, int width, int height, int min, int max, int span, int bit);

        MemoryMappedFile mmf;

        int MainHeaderSize;
        int FrameHeaderSize;
        int FrameBodySize;
        int FrameFullSize;
        int FrameCount;
        long StartPosition;

        private List<Spot> SpotItems = new List<Spot>();
        private Spot PressedSpot = null;


        private PolySpotAll polySpotAll;
        private PolySpot PressedPoly = null;

        private List<Material> MaterialItems = new List<Material>();

        int start;
        int end;

        short[] AvgImage;
        short[] DeltaImage;
        public short[] StressImage;


        short[] AmplitudeImage;

        double mpa;

        //initialize
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void initialize()
        {
            setGrayColorBar();
            setMaterials();


            polySpotAll = new PolySpotAll();

            
            
            polySpotAll.initialize( grid_edit.ActualWidth, grid_edit.ActualHeight);

            polySpotAll.setVisible(false);
            
            grid_edit.Children.Add(polySpotAll.polyPt[0]);

            grid_edit.Children.Add(polySpotAll.polyPt[1]);
            grid_edit.Children.Add(polySpotAll.polyPt[2]);
            
            grid_edit.Children.Add(polySpotAll.polyLine[0]);
            grid_edit.Children.Add(polySpotAll.polyLine[1]);
            grid_edit.Children.Add(polySpotAll.polyLine[2]);

            
            

            /*
            Spot spot = new Spot();

            spot.Visibility = Visibility.Visible;

            spot.X = 0;
            spot.Y = 250;
            spot.RealWidth = grid_edit.ActualWidth;
            spot.RealHeight = grid_edit.ActualHeight;
            spot.ID = "po";
            spot.Position();
            SpotItems.Add(spot);
            grid_edit.Children.Add(spot);
            */

            

        }

        private double findValueWithPt(string valStr)
        {
            string[] index = valStr.Split(',');

            double valueUp = findValueWithoutPt(index[0]);
            double valueD=0;


            //index[1]에만 소수점 자리 포함됨
            //1 0
            for (int i = index[1].Length - 1; i >= 0; i--)
            {
                valueD = valueD + char.GetNumericValue(index[1][i]) * Math.Pow(10, -1 * (i+1));
            }

            return valueUp + valueD;
        }
        private double findValueWithoutPt(string valueStr)
        {
            double valueD=0;


            for (int i = valueStr.Length - 1; i >= 0; i--)
            {
                valueD = valueD + char.GetNumericValue(valueStr[i]) * Math.Pow(10, valueStr.Length - (i + 1));
            }

            return valueD;

        }
        private double findIndex(string val)
        {
            string[] value = val.Split('E');

            double valueUp=-1;

            if (value[0].Contains(','))
                valueUp = findValueWithPt(value[0]);
            else
                valueUp = findValueWithoutPt(value[0]);
            

            double index = 0;
            double isMinus = 1;

            if (value[1][0] == '-')//음수일 경우
                isMinus = -1;
            else
                isMinus = 1;

            for (int i = value[1].Length - 1; i > 0; i--)
            {
                index = index + char.GetNumericValue(value[1][i]) * Math.Pow(10, value[1].Length - (i + 1));
            }

            valueUp = valueUp * Math.Pow(10, isMinus * index);

            return valueUp;

}

        private void setMaterials()
        {
            try
            {
                using (StreamReader sr = new StreamReader("Materials.ini"))
                {
                    string currentLine = sr.ReadLine();

                    int infoCnt = 0;
                    string _name="";
                    double _alpha=-1;
                    double _Cp=-1;
                    double _Km=-1;
                    double _Rho=-1;

                    
                    while (currentLine != null)
                    {
                        
                        if (currentLine.Contains('['))
                        {
                            _name = currentLine.TrimStart('[');
                            _name = _name.TrimEnd(']');
                            
                            infoCnt++;
                        }
                        else if (currentLine.Contains("="))
                        {
                            string[] num = currentLine.Split('=');//0번째: 이름. 1번째: 값

                            double valueD=0;


                            if (num[1].Contains('E'))//if exponential is contained
                            {
                                valueD = findIndex(num[1]);
                             }
                            else//그냥 숫자인 경우
                            {
                                
                                if(num[1].Contains(','))//소수점 포함될 시 0,0001
                                {
                                    valueD = findValueWithPt(num[1]);
                                }
                                else//소수점 없이 걍 정수일시
                                {
                                    valueD = findValueWithoutPt(num[1]);
                                }

                            }

                            if (num[0] == "Alpha")
                            {
                                _alpha = valueD;
                            }
                            else if (num[0] == "Cp")
                            {
                                _Cp = valueD;
                            }
                            else if (num[0] == "Km")
                            {
                                _Km = valueD;
                            }
                            else if (num[0] == "Rho")
                            {
                                _Rho = valueD;
                            }

                            infoCnt++;
                        }
                        

                        if (infoCnt == 5)
                        {
                            MaterialItems.Add(new Material() { name=_name, alpha=_alpha,Cp=_Cp,Km=_Km,Rho=_Rho,dispNameKm=_name + "  " + _Km.ToString()});
                            infoCnt = 0;
                        }

                        currentLine = sr.ReadLine();
                    }
                    
                }
            }
            catch (Exception e)
            {
                
            }

            //combobox에 집어넣자

            comboBox_Km.Items.Add(new Material() { Km = -1, dispNameKm = "직접 입력" });

            foreach (Material mat in MaterialItems)
            {
                comboBox_Km.Items.Add(mat);
                
            }
        }

        private void setGrayColorBar()
        {
            System.Drawing.Bitmap grayImgRrc = PTW_Load.Properties.Resources.grayGradient;
            System.Drawing.Bitmap colorImgRrc = PTW_Load.Properties.Resources.colorGradient;

            MemoryStream imgStreamGray = new MemoryStream();
            MemoryStream imgStreamColor = new MemoryStream();
            grayImgRrc.Save(imgStreamGray, System.Drawing.Imaging.ImageFormat.Png);
            colorImgRrc.Save(imgStreamColor, System.Drawing.Imaging.ImageFormat.Png);

            imgStreamGray.Seek(0, SeekOrigin.Begin);
            imgStreamColor.Seek(0, SeekOrigin.Begin);


            BitmapFrame imgGray = BitmapFrame.Create(imgStreamGray);
            BitmapFrame imgColor = BitmapFrame.Create(imgStreamColor);

            imageGrayBarGrid.Source = imgGray;
            imageColorBarGrid.Source = imgColor;
        }


        private void button_load_Click(object sender, RoutedEventArgs e)
        {

            //DebugInfo.Content = "power"; 이런 식으로 DebugInfo 내용을 변경하면 됩니다.

            Thread load = new Thread(Load);
            load.Start();
            
        }

        private void button_play_Click(object sender, RoutedEventArgs e)
        {
            teeChartPanel_h.GetCursor(out start, out end);
            teeChartPanel.Clear();
              
            //km값 설정하는 곳, combobox랑 매치시키자, play thread 내부에서 말이지
            //mpa = double.Parse(textBox_mpa.Text);

            

            if (textBox_mpa.Text != null)
            {
                mpa = double.Parse(textBox_mpa.Text);
                if (start != end)
                {
                    Thread play = new Thread(Play);
                    play.Start();
                }
            }
        }

        delegate void OnChangeLabelBar(float min, float max);
        private void ChangeLabelBar(float min, float max)
        {
            float divider = 5;
            max = max / 100;
            min = min / 100;

            float diff = (max - min) / divider;

            colorValue0.Content = min.ToString();
            colorValue1.Content = (min + diff).ToString();
            colorValue2.Content = (min + 2*diff).ToString();
            colorValue3.Content = (min + 3*diff).ToString();

            colorValue4.Content = max.ToString();
        }

        delegate void OnChangeSpotValue(Spot spot, double value);
        private void ChangeSpotValue(Spot spot, double value)
        {
            spot.setSpotValue(value);
        }
        


        delegate void OnChangeUI(bool state);
        private void ChangeUI(bool state)
        {
            if (state == false)
            {
                teeChartPanel_h.Clear();
                teeChartPanel.Clear();

                image_display_gray.Source = null;
                image_display_RGB.Source = null;                
                image_avg_gray.Source = null;
                image_avg_RGB.Source = null;
                image_delta_gray.Source = null;
                image_delta_RGB.Source = null;
                image_stress_gray.Source = null;
                image_stress_RGB.Source = null;
                image_rev_gray.Source = null;
                image_rev_RGB.Source = null;
            }

            comboBox_Km.IsEnabled = state;            
            button_play.IsEnabled = state;
            textBox_repeat.IsEnabled = state;
            button_analysis.IsEnabled = state;
            checkBox0.IsEnabled = state;
            checkBox1.IsEnabled = state;
            checkBox2.IsEnabled = state;
            checkBox3.IsEnabled = state;
            checkBox4.IsEnabled = state;
            checkColorPallete.IsEnabled = state;
            
            button_export.IsEnabled = state;
        }

        delegate void OnData(List<double> items);
        private void Data(List<double> items)
        {
            teeChartPanel.AddItem(items);
        }
        

        private void Play()
        {
            
            IntPtr pSum = Marshal.AllocHGlobal(327680 * 4);
            IntPtr pAvg = Marshal.AllocHGlobal(327680 * 2);
            IntPtr pMax = Marshal.AllocHGlobal(327680 * 2);
            IntPtr pMin = Marshal.AllocHGlobal(327680 * 2);
            IntPtr pDelta = Marshal.AllocHGlobal(327680 * 2);

            //comboBox_Km.sel

            

            //모든 frame을 그리는듯
            for (int i = start; i < end; i++)
            {
                StartPosition = MainHeaderSize + FrameFullSize * i;

                ReadFrameHeader(i);
                short[] body = GetFrame(i);

                int Min;
                int Max;
                IntPtr p = Marshal.AllocHGlobal(327680 * 2);
                Marshal.Copy(body, 0, p, 327680);

                List<double> spotData = new List<double>();

                //spot을 spot 온도에 추가하는 부분임
                foreach (Spot spot in SpotItems)
                {
                    if (spot.Visibility == Visibility.Visible)
                    {
                        spotData.Add(ConvertTemp(body[spot.Y * 640 + spot.X]));
                        
                        //LHW DEBUG
                        
                    }
                    else
                        spotData.Add(-1);
                }

                this.Dispatcher.Invoke(new OnData(Data), new object[] { spotData });
                 

                CalcurateMinMax(p, pSum, pMax, pMin, 327680 * 2, out Min, out Max, 16);

                Bitmap bmp = new Bitmap(640, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 640, 512);
                BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //LHW DEBUG

                DrawImage(bmpData.Scan0, p, 640, 512, Min, Max, Max - Min, 16);
                

                bmp.UnlockBits(bmpData);

                //dispatcher 소환해서 xml의 element에 접근 가능함
                
                this.Dispatcher.Invoke(new OnDraw(Draw), new Object[] { bmp, image_display_gray });


                Bitmap bmpRGB = new Bitmap(640, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                
                BitmapData bmpDataRGB = bmpRGB.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //LHW DEBUG
                DrawImageRGB(bmpDataRGB.Scan0, p, 640, 512, Min, Max, Max - Min, 16);

                bmpRGB.UnlockBits(bmpDataRGB);



                this.Dispatcher.Invoke(new OnDraw(Draw), new Object[] { bmpRGB, image_display_RGB });



                Marshal.FreeHGlobal(p);


            }
            
            GetAvgData(pSum, pAvg, end - start, 327680);
            GetDeltaData(pDelta, pMax, pMin, 327680);

            DrawImageFromData(pAvg, image_avg_gray);
            DrawImageFromDataRGB(pAvg, image_avg_RGB); 

            DrawImageFromData(pDelta, image_delta_gray);
            DrawImageFromDataRGB(pDelta, image_delta_RGB);

            short[] firstFrame = GetFrame(start);
            AvgImage = new short[327680];
            DeltaImage = new short[327680];
            StressImage = new short[327680];

            Marshal.Copy(pAvg, AvgImage, 0, 327680);
            Marshal.Copy(pDelta, DeltaImage, 0, 327680);

            IntPtr pRet = Marshal.AllocHGlobal(327680 * 2);

            for (int i = 0; i < 327680; i++)
            {
                StressImage[i] = (short)(DeltaImage[i] / (mpa * ConvertTemp(firstFrame[i])) * 100000);
            }


            foreach (Spot spot in SpotItems)
            {
                if (spot.Visibility == Visibility.Visible)
                {
                    this.Dispatcher.Invoke(new OnChangeSpotValue(ChangeSpotValue), new Object[] {spot, (double)(StressImage[spot.Y*640 + spot.X])/100.0 });
                }
            }
            
            Marshal.Copy(StressImage, 0, pRet, 327680);

            DrawImageFromData(pRet, image_stress_gray);
            DrawImageFromDataRGB(pRet, image_stress_RGB);

            
            Marshal.FreeHGlobal(pRet);
            
            Marshal.FreeHGlobal(pSum);
            Marshal.FreeHGlobal(pAvg);
            Marshal.FreeHGlobal(pMax);
            Marshal.FreeHGlobal(pMin);
            Marshal.FreeHGlobal(pDelta);
             
        }

        public int ConvertTemp(int pixel)
        {
            int[] DL = {8667, 8878, 9095, 9318, 9548,
		        9784, 10027, 10276, 10533, 10797,
		        11068, 11346, 11632, 11926, 12227,
		        12537, 12854, 13180, 13514, 13857,
		        14208, 14569, 14938, 15317, 15705,
		        16103};
            int[] val = { 205, 211, 217, 223, 230, 
		        236, 243, 249, 257, 264, 
		        271, 278, 286, 294, 301, 
		        310, 317, 326, 334, 343, 
		        351, 361, 369, 379, 388, 
		        398 };

            for (int i = 0; i < 26; i++)
            {
                if (pixel < DL[i])
                {
                    int sub = 0;

                    if (i == 0)
                        sub = pixel - 8462;
                    else
                        sub = pixel - DL[i - 1];

                    return (int)((i + 18 + sub / (double)val[i]) * 100);
                }
            }

            return 0;
        }

        private void ReadFrameHeader(int index)
        {
            int StartPosition = MainHeaderSize + FrameFullSize * index;

            using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(StartPosition, FrameHeaderSize))
            {
                FrameHeader fheader;

                accessor.Read(0, out fheader);

                byte[] time = new byte[4];
                byte[] word = new byte[2];
                byte[] dword = new byte[4];

                unsafe
                {
                    Marshal.Copy((IntPtr)fheader.FrameTime, time, 0, 4);

                    byte minute = time[0];
                    byte hour = time[1];
                    byte cent = time[2];
                    byte second = time[3];

                    byte ft = fheader.FrameThousands;

                    Marshal.Copy((IntPtr)fheader.FrameMillions, word, 0, 2);
                    short fm = BitConverter.ToInt16(word, 0);

                    Marshal.Copy((IntPtr)fheader.LockinPeriod, dword, 0, 4);
                    int lockinPeriod = BitConverter.ToInt32(dword, 0);

                    Marshal.Copy((IntPtr)fheader.LockinPhase, dword, 0, 4);
                    int lockinPhase = BitConverter.ToInt32(dword, 0);
                }
            }
        }

        private short [] GetFrame(int index)
        {
            short[] body = new short[327680];
            int StartPosition = MainHeaderSize + FrameFullSize * index;

            using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(StartPosition + FrameHeaderSize, FrameBodySize))
            {
                FrameBody fbody;

                for (int h = 0; h < 512; h++)
                {
                    accessor.Read(640 * h * 2, out fbody);
                    unsafe
                    {
                        Marshal.Copy((IntPtr)fbody.Frame, body, 640 * h, 640);
                    }
                }
            }

            return body;
        }

        private void DrawImageFromData(IntPtr data, System.Windows.Controls.Image image)
        {
            int max;
            int min;
            CalcurateMinMax2(data, 327680, out min, out max);

            Bitmap deltabmp = new Bitmap(640, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Rectangle deltarect = new System.Drawing.Rectangle(0, 0, 640, 512);
            BitmapData deltaData = deltabmp.LockBits(deltarect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //LHW DEBUG
            DrawImage2(deltaData.Scan0, data, 640, 512, min, max, max - min, 16);
            //DrawImage2RGB(deltaData.Scan0, data, 640, 512, min, max, max - min, 16);

            deltabmp.UnlockBits(deltaData);

            this.Dispatcher.Invoke(new OnDraw(Draw), new Object[] { deltabmp, image });        

        }

        private void DrawImageFromDataRGB(IntPtr data, System.Windows.Controls.Image image)
        {
            int max;
            int min;
            CalcurateMinMax2(data, 327680, out min, out max);

            Bitmap deltabmp = new Bitmap(640, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Rectangle deltarect = new System.Drawing.Rectangle(0, 0, 640, 512);
            BitmapData deltaData = deltabmp.LockBits(deltarect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //LHW DEBUG

            DrawImage2RGB(deltaData.Scan0, data, 640, 512, min, max, max - min, 16);

            deltabmp.UnlockBits(deltaData);

            this.Dispatcher.Invoke(new OnDraw(Draw), new Object[] { deltabmp, image });        
        }

        private void Load()
        {
            this.Dispatcher.Invoke(new OnChangeUI(ChangeUI), new object[] { false });
            if (mmf != null)
                mmf.Dispose();

            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog().Value)
            {
                mmf = MemoryMappedFile.CreateFromFile(dialog.FileName);

                this.Dispatcher.Invoke(new OnChangeUI(ChangeUI), new object [] {true});
            }
            else
            {
                return;
            }


            using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(0, 1016))
            {
                MainHeader header;

                accessor.Read(0, out header);

                byte[] tmp = new byte[5];
                byte[] dword = new byte[4];
                byte[] word = new byte[2];

                unsafe
                {
                    Marshal.Copy((IntPtr)header.Signature, tmp, 0, 5);
                    String Signature = System.Text.Encoding.Default.GetString(tmp);

                    Marshal.Copy((IntPtr)header.MainHeaderSize, dword, 0, 4);
                    MainHeaderSize = BitConverter.ToInt32(dword, 0);

                    Marshal.Copy((IntPtr)header.FrameHeaderSize, dword, 0, 4);
                    FrameHeaderSize = BitConverter.ToInt32(dword, 0);

                    Marshal.Copy((IntPtr)header.FrameFullSize, dword, 0, 4);
                    FrameFullSize = BitConverter.ToInt32(dword, 0) * 2;          //word Size

                    Marshal.Copy((IntPtr)header.FrameBodySize, dword, 0, 4);
                    FrameBodySize = BitConverter.ToInt32(dword, 0) * 2;          //word Size

                    Marshal.Copy((IntPtr)header.FrameCount, dword, 0, 4);
                    FrameCount = BitConverter.ToInt32(dword, 0);

                    Marshal.Copy((IntPtr)header.Emissivity, dword, 0, 4);
                    float e = (float)BitConverter.ToSingle(dword, 0);

                    Marshal.Copy((IntPtr)header.BgTemperture, dword, 0, 4);
                    float bgt = (float)BitConverter.ToSingle(dword, 0);

                    Marshal.Copy((IntPtr)header.AtmTransmission, dword, 0, 4);
                    float atmt = (float)BitConverter.ToSingle(dword, 0);

                    Marshal.Copy((IntPtr)header.AtsTemperature, dword, 0, 4);
                    float atst = (float)BitConverter.ToSingle(dword, 0);

                    short ppl = header.PPL;

                    short lpf = header.LPF;

                    short add = header.ADDynamic;
                }
            }

            IntPtr pSum = Marshal.AllocHGlobal(327680 * 4);
            IntPtr pAvg = Marshal.AllocHGlobal(327680 * 2);
            IntPtr pMax = Marshal.AllocHGlobal(327680 * 2);
            IntPtr pMin = Marshal.AllocHGlobal(327680 * 2);
            IntPtr pDelta = Marshal.AllocHGlobal(327680 * 2);

            

            for (int i = 0; i < FrameCount; i++)
            {
                StartPosition = MainHeaderSize + FrameFullSize * i;
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(StartPosition, FrameHeaderSize))
                {
                    FrameHeader fheader;

                    accessor.Read(0, out fheader);

                    byte[] time = new byte[4];
                    byte[] word = new byte[2];
                    byte[] dword = new byte[4];

                    unsafe
                    {
                        Marshal.Copy((IntPtr)fheader.FrameTime, time, 0, 4);

                        byte minute = time[0];
                        byte hour = time[1];
                        byte cent = time[2];
                        byte second = time[3];

                        byte ft = fheader.FrameThousands;

                        Marshal.Copy((IntPtr)fheader.FrameMillions, word, 0, 2);
                        short fm = BitConverter.ToInt16(word, 0);

                        Marshal.Copy((IntPtr)fheader.LockinPeriod, dword, 0, 4);
                        int lockinPeriod = BitConverter.ToInt32(dword, 0);

                        Marshal.Copy((IntPtr)fheader.LockinPhase, dword, 0, 4);
                        int lockinPhase = BitConverter.ToInt32(dword, 0);

                        Marshal.Copy((IntPtr)fheader.Signal1, word, 0, 2);
                        short sig1 = BitConverter.ToInt16(word, 0);

                        Marshal.Copy((IntPtr)fheader.Signal2, word, 0, 2);
                        short sig2 = BitConverter.ToInt16(word, 0);

                        Marshal.Copy((IntPtr)fheader.Signal3, word, 0, 2);
                        short sig3 = BitConverter.ToInt16(word, 0);

                        teeChartPanel_h.AddItem("Signal 1", sig1);
                        teeChartPanel_h.AddItem("Signal 2", sig2);
                        teeChartPanel_h.AddItem("Signal 3", sig3);
                    }
                }
            }

            teeChartPanel_h.AddCursor();
            
            //첫 Frame을 그린다.
            //첫 frame만 그리는듯
            using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(MainHeaderSize + FrameHeaderSize, FrameBodySize))
            {
                FrameBody fbody;
                short[] body = new short[327680];

                for (int h = 0; h < 512; h++)
                {
                    accessor.Read(640 * h * 2, out fbody);
                    unsafe
                    {
                        Marshal.Copy((IntPtr)fbody.Frame, body, 640 * h, 640);
                    }
                }

                int Min;
                int Max;
                IntPtr p = Marshal.AllocHGlobal(327680 * 2);
                Marshal.Copy(body, 0, p, 327680);

                CalcurateMinMax(p, pSum, pMax, pMin, 327680 * 2, out Min, out Max, 16);




                Bitmap bmp = new Bitmap(640, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 640, 512);
                BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                //LHW DEBUG
                DrawImage(bmpData.Scan0, p, 640, 512, Min, Max, Max - Min, 16);
                
                bmp.UnlockBits(bmpData);

                this.Dispatcher.Invoke(new OnDraw(Draw), new Object[] { bmp, image_display_gray });


                Bitmap bmpRGB = new Bitmap(640, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                
                BitmapData bmpDataRGB = bmpRGB.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


                DrawImageRGB(bmpDataRGB.Scan0, p, 640, 512, Min, Max, Max - Min, 16);
                bmpRGB.UnlockBits(bmpDataRGB);

                this.Dispatcher.Invoke(new OnDraw(Draw), new Object[] { bmpRGB, image_display_RGB });

                this.Dispatcher.Invoke(new OnChangeLabelBar(ChangeLabelBar), new Object[] { Min, Max });

                Marshal.FreeHGlobal(p);
            }


            

            Marshal.FreeHGlobal(pSum);
            Marshal.FreeHGlobal(pAvg);
            Marshal.FreeHGlobal(pMax);
            Marshal.FreeHGlobal(pMin);
            Marshal.FreeHGlobal(pDelta);
        }

        delegate void OnDraw(Bitmap bit, System.Windows.Controls.Image image);
        private void Draw(Bitmap bit, System.Windows.Controls.Image image)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bit.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                image.Source = bitmapImage;
            }

            bit.Dispose();
        }

        private int AnalysisMin;
        private int AnalysisMax;
        private int AnalysisRepeat;

        private void button_analysis_Click(object sender, RoutedEventArgs e)
        {
            teeChartPanel_h.GetCursor(out start, out end);
            AnalysisMax = end;
            AnalysisMin = start;
            AnalysisRepeat = int.Parse(textBox_repeat.Text);
            Thread load = new Thread(Aanlysis);
            load.Start();
        }

        private void Aanlysis()
        {
            int MainHeaderSize;
            int FrameHeaderSize;
            int FrameBodySize;
            int FrameFullSize;
            int FrameCount;
            long StartPosition;

            int repeat = AnalysisRepeat;

            int period = (int)((AnalysisMax - AnalysisMin) / (4 * repeat));
            int[] index = new int[4 * repeat];

            for(int i = 0; i < 4 * repeat; i++)
            {
                index[i] =  AnalysisMin + period * i;
            }

            RawData raw = new RawData();
            raw.MakeBuffer(repeat);
            raw.PhaseImage += new RawData.ResultImage(raw_PhaseImage);
            raw.AmplitudeImage += new RawData.ResultImage(raw_AmplitudeImage);

            using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(0, 1016))
            {
                MainHeader header;

                accessor.Read(0, out header);

                byte[] tmp = new byte[5];
                byte[] dword = new byte[4];
                byte[] word = new byte[2];

                unsafe
                {
                    Marshal.Copy((IntPtr)header.Signature, tmp, 0, 5);
                    String Signature = System.Text.Encoding.Default.GetString(tmp);

                    Marshal.Copy((IntPtr)header.MainHeaderSize, dword, 0, 4);
                    MainHeaderSize = BitConverter.ToInt32(dword, 0);

                    Marshal.Copy((IntPtr)header.FrameHeaderSize, dword, 0, 4);
                    FrameHeaderSize = BitConverter.ToInt32(dword, 0);

                    Marshal.Copy((IntPtr)header.FrameFullSize, dword, 0, 4);
                    FrameFullSize = BitConverter.ToInt32(dword, 0) * 2;          //word Size

                    Marshal.Copy((IntPtr)header.FrameBodySize, dword, 0, 4);
                    FrameBodySize = BitConverter.ToInt32(dword, 0) * 2;          //word Size

                    Marshal.Copy((IntPtr)header.FrameCount, dword, 0, 4);
                    FrameCount = BitConverter.ToInt32(dword, 0);
                }
            }

            for (int i = 0; i < 4 * repeat; i++)
            {
                StartPosition = MainHeaderSize + FrameFullSize * index[i];

                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(StartPosition + FrameHeaderSize, FrameBodySize))
                {
                    FrameBody fbody;
                    short[] body = new short[327680];

                    for (int h = 0; h < 512; h++)
                    {
                        accessor.Read(640 * h * 2, out fbody);
                        unsafe
                        {
                            Marshal.Copy((IntPtr)fbody.Frame, body, 640 * h, 640);
                        }
                    }

                    raw.AddFrame(640, 512, body);
                }
            }

            //raw.Phase();
            raw.Amplitude();
        }

        void raw_AmplitudeImage(short[] image, int w, int h, short min, short max)
        {
            int Min;
            int Max;

            AmplitudeImage = image;

            IntPtr p = Marshal.AllocHGlobal(327680 * 2);
            Marshal.Copy(image, 0, p, 327680);

            CalcurateMinMax2(p, 327680, out Min, out Max);

            Max = 35;

            Bitmap bmp = new Bitmap(640, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 640, 512);
            BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //LHW DEBUG
            DrawImage(bmpData.Scan0, p, 640, 512, Min, Max, Max - Min, 16);
            bmp.UnlockBits(bmpData);

            this.Dispatcher.Invoke(new OnDraw(Draw), new Object[] { bmp, image_rev_gray});


            Bitmap bmpRGB = new Bitmap(640, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            
            BitmapData bmpDataRGB = bmpRGB.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //LHW DEBUG
            DrawImageRGB(bmpDataRGB.Scan0, p, 640, 512, Min, Max, Max - Min, 16);
            bmpRGB.UnlockBits(bmpDataRGB);

            this.Dispatcher.Invoke(new OnDraw(Draw), new Object[] { bmpRGB, image_rev_RGB });





            Marshal.FreeHGlobal(p);
        }

        void raw_PhaseImage(short[] image, int w, int h, short min, short max)
        {
            int Min;
            int Max;

            IntPtr p = Marshal.AllocHGlobal(327680 * 2);
            Marshal.Copy(image, 0, p, 327680);

            CalcurateMinMax2(p, 327680, out Min, out Max);

            Bitmap bmp = new Bitmap(640, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 640, 512);
            BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            DrawImage(bmpData.Scan0, p, 640, 512, Min, Max, Max - Min, 16);
            bmp.UnlockBits(bmpData);

            this.Dispatcher.Invoke(new OnDraw(Draw), new Object[] { bmp, image_delta_gray});


            Bitmap bmpRGB = new Bitmap(640, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            
            BitmapData bmpDataRGB = bmpRGB.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            DrawImageRGB(bmpDataRGB.Scan0, p, 640, 512, Min, Max, Max - Min, 16);
            bmpRGB.UnlockBits(bmpDataRGB);

            this.Dispatcher.Invoke(new OnDraw(Draw), new Object[] { bmpRGB, image_delta_RGB });



            Marshal.FreeHGlobal(p);
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(grid_edit);

            foreach (Spot spot in SpotItems)
            { 
                if (spot.MouseDownEvent(p.X, p.Y))
                {
                    PressedSpot = spot;
                    return;
                }
            }

            foreach (PolySpot spot in polySpotAll.polyPt)
            {
                if (spot.MouseDownEvent(p.X, p.Y))
                {
                    PressedPoly = spot;
                    return;
                }
            }



            Mouse.OverrideCursor = null;
        }

        private void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(grid_edit);

            if (PressedSpot != null)
            {
                PressedSpot.MouseMoveEvent(p.X, p.Y);
                return;
            }

            if (PressedPoly != null)
            {
                PressedPoly.MouseMoveEvent(p.X, p.Y);
                return;
            }

            foreach (Spot spot in SpotItems)
            {
                if (spot.MouseMoveEvent(p.X, p.Y))
                    return;
            }

            foreach (PolySpot spot in polySpotAll.polyPt)
            {
                if (spot.MouseMoveEvent(p.X, p.Y))
                    return;
            }

            Mouse.OverrideCursor = null;
        }

        private void Grid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PressedSpot = null;

            System.Windows.Point p = e.GetPosition(grid_edit);

            foreach (Spot spot in SpotItems)
            {
                
                spot.MouseUpEvent(p.X, p.Y);
            }

            foreach (PolySpot spot in polySpotAll.polyPt)
            {
                spot.MouseUpEvent(p.X, p.Y);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                Spot spot = new Spot();

                spot.Visibility = Visibility.Collapsed;
                
                spot.X = 320;
                spot.Y = 250;
                spot.RealWidth = grid_edit.ActualWidth;
                spot.RealHeight = grid_edit.ActualHeight;
                spot.ID = String.Format("{0}", i);
                spot.Position();
                SpotItems.Add(spot);
                grid_edit.Children.Add(spot);
            }

            initialize();







        }

        private void grid_edit_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (Spot spot in SpotItems)
            {
                spot.RealWidth = grid_edit.ActualWidth;
                spot.RealHeight = grid_edit.ActualHeight;
                spot.Position();
            }
            if (polySpotAll != null)
            {
                foreach (PolySpot spot in polySpotAll.polyPt)
                {
                    spot.realWidth = grid_edit.ActualWidth;
                    spot.realHeight = grid_edit.ActualHeight;
                    spot.Position();
                }
            }

        }

        private void comboBox_Select(object sender,  SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            if ((double)(cb.SelectedValue) == -1)//for 직접 입력
            {
                textBox_mpa.IsEnabled = true;
            }
            else
            {
                textBox_mpa.IsEnabled = false;
                textBox_mpa.Text = ((double)cb.SelectedValue).ToString();
            }
        }

        

        private void combo_Polygon(object sender, SelectionChangedEventArgs e)
        {
            
            ComboBox cb = sender as ComboBox;

            ComboBoxItem cbitem = (ComboBoxItem)cb.SelectedItem;

            String val = cbitem.Content.ToString();

            int p = int.Parse(val);

            if (polySpotAll != null)
                polySpotAll.addPt(int.Parse(val));    
            

//            this.Dispatcher.Invoke(new OnData(Data), new object[] { spotData });
                        
            
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.Name.Equals("checkBox0"))
            {
                SpotItems[0].Visibility = Visibility.Visible;
            }
            else if (cb.Name.Equals("checkBox1"))
            {
                SpotItems[1].Visibility = Visibility.Visible;
            }
            else if (cb.Name.Equals("checkBox2"))
            {
                SpotItems[2].Visibility = Visibility.Visible;
            }
            else if (cb.Name.Equals("checkBox3"))
            {
                SpotItems[3].Visibility = Visibility.Visible;
            }
            else if (cb.Name.Equals("checkBox4"))
            {
                SpotItems[4].Visibility = Visibility.Visible;
            }
            else if (cb.Name.Equals("checkColorPallete"))
            {
                image_display_gray.Visibility = Visibility.Hidden;
                image_display_RGB.Visibility = Visibility.Visible;

                image_avg_gray.Visibility = Visibility.Hidden;
                image_avg_RGB.Visibility = Visibility.Visible;

                image_delta_gray.Visibility = Visibility.Hidden;
                image_delta_RGB.Visibility = Visibility.Visible;

                image_stress_gray.Visibility = Visibility.Hidden;
                image_stress_RGB.Visibility = Visibility.Visible;

                image_rev_gray.Visibility = Visibility.Hidden;
                image_rev_RGB.Visibility = Visibility.Visible;

                imageGrayBarGrid.Visibility = Visibility.Hidden;
                imageColorBarGrid.Visibility = Visibility.Visible;
            }
            else if(cb.Name.Equals("checkPoly"))
            {
                polySpotAll.setVisible(true);
            }
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.Name.Equals("checkBox0"))
            {
                SpotItems[0].Visibility = Visibility.Collapsed;
            }
            else if (cb.Name.Equals("checkBox1"))
            {
                SpotItems[1].Visibility = Visibility.Collapsed;
            }
            else if (cb.Name.Equals("checkBox2"))
            {
                SpotItems[2].Visibility = Visibility.Collapsed;
            }
            else if (cb.Name.Equals("checkBox3"))
            {
                SpotItems[3].Visibility = Visibility.Collapsed;
            }
            else if (cb.Name.Equals("checkBox4"))
            {
                SpotItems[4].Visibility = Visibility.Collapsed;
            }
            else if (cb.Name.Equals("checkColorPallete"))
            {
                image_display_gray.Visibility = Visibility.Visible;
                image_display_RGB.Visibility = Visibility.Hidden;

                image_avg_gray.Visibility = Visibility.Visible;
                image_avg_RGB.Visibility = Visibility.Hidden;

                image_delta_gray.Visibility = Visibility.Visible;
                image_delta_RGB.Visibility = Visibility.Hidden;

                image_stress_gray.Visibility = Visibility.Visible;
                image_stress_RGB.Visibility = Visibility.Hidden;

                image_rev_gray.Visibility = Visibility.Visible;
                image_rev_RGB.Visibility = Visibility.Hidden;

                imageGrayBarGrid.Visibility = Visibility.Visible;
                imageColorBarGrid.Visibility = Visibility.Hidden;
            }
            else if (cb.Name.Equals("checkPoly"))
            {
                polySpotAll.setVisible(false);
            }
        }

        private void button_export_Click(object sender, RoutedEventArgs e)
        {
            if (AvgImage != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Title = "AverageFile";
                if (dialog.ShowDialog().Value)
                {
                    Save(AvgImage, dialog.FileName, 100.0);
                }
            }

            if (DeltaImage != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Title = "DeltaFile";
                if (dialog.ShowDialog().Value)
                {
                    Save(DeltaImage, dialog.FileName, 100.0);
                }
            }

            if (StressImage != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Title = "StressFile";
                if (dialog.ShowDialog().Value)
                {
                    Save(StressImage, dialog.FileName, 100.0);
                }
            }

            if (AmplitudeImage != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Title = "AmplitudeFile";
                if (dialog.ShowDialog().Value)
                {
                    Save(AmplitudeImage, dialog.FileName, 100000);
                }
            }
        }


        private void Save(short[] data, string fileName, double divide = 1.0)
        {
            StringBuilder csv = new StringBuilder();

            for (int h = 0; h < 512; h++)
            {
                for (int w = 0; w < 640; w++)    
                {
                    csv.Append(String.Format("{0},", data[h * 640 + w] / divide));
                }
                csv.Remove(csv.Length - 1, 1);
                csv.Append(Environment.NewLine);
            }
//csv.Append(String.Format("{0},{1},{2},{3},{4},{5},{6}{7}", data.WriteTime.ToString("yyyyMMddHHmmss"), data.Max_0, data.Avg_0, data.Min_0, data.Max_1, data.Avg_1, data.Min_1, Environment.NewLine));

            File.WriteAllText(fileName, csv.ToString());
        }
        
    }

    public unsafe struct MainHeader
    {
        public fixed byte Signature[5];
        public fixed byte Version[5];
        public byte FinDeFichier;
        public fixed byte MainHeaderSize[4];
        public fixed byte FrameHeaderSize[4];
        public fixed byte FrameFullSize[4];
        public fixed byte FrameBodySize[4];
        public fixed byte FrameCount[4];
        public fixed byte CurrentFrame[4];
        public fixed byte SaveDate[4];
        public fixed byte SaveTime[4];
        public byte SaveMilli;
        public fixed byte CameraName[20];
        public fixed byte LensName[20];
        public fixed byte FilterName[20];
        public fixed byte ApertureName[20];
        public fixed byte BilletSpeed[4];
        public fixed byte BilletDiameter[4];
        public fixed byte BilletShape[2];
        public fixed byte Reserved0[7];
        public fixed byte Emissivity[4];
        public fixed byte BgTemperture[4];
        public fixed byte Distance[4];
        public fixed byte IRUS[17];
        public fixed byte AtmTransmission[4];
        public fixed byte Ext0[10];
        public fixed byte AtsTemperature[4];
        public fixed byte Ext1[189];
        public short PPL;
        public short LPF;
        public short ADDynamic;
    }

    public unsafe struct FrameHeader
    {
        public fixed byte Reserved0[80];
        public fixed byte FrameTime[4];
        public fixed byte Reserved1[76];
        public byte FrameThousands;
        public fixed byte FrameMillions[2];
        public fixed byte Ext0[85];
        public fixed byte LockinVersion[2];
        public fixed byte LockinPeriod[4];
        public fixed byte LockinPhase[4];
        public byte LockinMin;
        public byte LockinMax;
        public byte NumberOfSignal;
        public fixed byte Signal1[2];
        public fixed byte Signal2[2];
        public fixed byte Signal3[2];
        public fixed byte Signal4[2];
    }

    public unsafe struct FrameBody
    {
        public fixed short Frame[640];
    }
}
