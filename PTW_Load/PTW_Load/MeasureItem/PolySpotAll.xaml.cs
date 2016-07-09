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



namespace PTW_Load.MeasureItem
{
    public partial class  PolySpotAll : UserControl
    {
        public List<PolySpot> polyPt = new List<PolySpot>();
        public List<Line> polyLine = new List<Line>();
        public TextBox polyVal = new TextBox();
        public Label polyUnit = new Label();

        public int maxPt = 6;
        public int minPt = 3;
        public double realWidth;
        public double realHeight;

        public int imageWidth;
        public int imageHeight;

        public int currIdx = 3;



        public void addPt(int numPt)
        {
            //point가 추가되는 경우
            if (currIdx < numPt)
            {
                polyLine[currIdx - 1].X2 = polyPt[currIdx].Margin.Left + 7;
                polyLine[currIdx - 1].Y2 = polyPt[currIdx].Margin.Top + 7;
                

                for (int i = currIdx; i < numPt; i++)
                {
                    polyPt[i].isShown = true;

                    if (i == numPt - 1)
                    {
                        polyLine[i].X2 = polyPt[0].Margin.Left + 7;
                        polyLine[i].Y2 = polyPt[0].Margin.Top + 7;
                    }
                    else
                    {
                        polyLine[i].X2 = polyPt[i + 1].Margin.Left + 7;
                        polyLine[i].Y2 = polyPt[i + 1].Margin.Top + 7;
                    }

                }
                currIdx = numPt;
                
            }

            else if (currIdx > numPt)
            {
                polyLine[numPt - 1].X2 = polyPt[0].Margin.Left + 7;
                polyLine[numPt - 1].Y2 = polyPt[0].Margin.Top + 7;
                

                for (int i = currIdx; i > numPt; i--)
                {
                    polyPt[i - 1].isShown = false;
                }
                currIdx = numPt;
            }
        }

        public void deletePt()
        {
            if (polyPt.Count > minPt) 
            {
                polyPt.RemoveAt(polyPt.Count - 1);
                polyLine.RemoveAt(polyLine.Count - 1);

                polyLine[polyLine.Count - 1].X2 = polyPt[polyPt.Count - 1].X;
                polyLine[polyLine.Count - 1].Y2 = polyPt[polyPt.Count - 1].Y;

            }            
        }
        public void updatePolySpotValue()
        {
            
            MainWindow myWin = (MainWindow)System.Windows.Application.Current.MainWindow;
            if (myWin.StressImage != null)
            {
                double avgStress = 0;

                for (int i = 0; i < currIdx; i++)
                {
                    PolySpot ps = polyPt[i];
                    avgStress += (double)(myWin.StressImage[ps.Y * imageHeight + ps.X]);
                }

                avgStress = avgStress / currIdx;
                setPolySpotValue(avgStress);
            }
        }

        public void setPolySpotValue(double value)
        {
            polyVal.Text = value.ToString();
        }
        
        public void setVisible(bool isVisible)
        {
            Visibility v;
            if (isVisible == true)
                v = Visibility.Visible;
            else
                v = Visibility.Collapsed;

            polyVal.Visibility = v;
            polyUnit.Visibility = v;


            foreach (PolySpot ps in polyPt)
            {
                if(isVisible == false)
                    ps.Visibility = v;
                else if (isVisible == true)
                {
                    if (ps.isShown == true)
                        ps.Visibility = v;
                    else
                        ps.Visibility = Visibility.Collapsed;
                }
            }
            if (isVisible == false)
                foreach (Line l in polyLine)
                    l.Visibility = v;
            else
            {
                for (int i = 0; i < currIdx; i++)
                {
                    polyLine[i].Visibility = v;
                }
                for (int i = currIdx; i < maxPt; i++)
                {
                    polyLine[i].Visibility = Visibility.Collapsed;
                }
                
            }
            
        }

        public void setImageSize(int _imageWidth, int _imageHeight)
        {
            imageWidth = _imageWidth;
            imageHeight = _imageHeight;
        }

        public void initialize(double width, double height, String unitName)
        {
            realWidth = width;
            realHeight = height;

            
            initPt();
            initLine();

            polyVal.Text = (0).ToString();
            polyVal.VerticalContentAlignment = VerticalAlignment.Top;
            polyVal.Padding = new Thickness(0, 0, 0, 0);
            polyVal.FontSize = 15;
            polyVal.Width = 40;
            polyVal.Height = 20;

            

            polyUnit.Content = unitName;



            polyUnit.VerticalContentAlignment = VerticalAlignment.Top;
            polyUnit.Padding = new Thickness(0, 0, 0, 0);
            polyUnit.FontSize = 10;



            polyVal.Background = System.Windows.Media.Brushes.AliceBlue;
            polyUnit.Background = System.Windows.Media.Brushes.AntiqueWhite;
            polyUnit.Foreground = System.Windows.Media.Brushes.Black;



            FormattedText t = new FormattedText(polyUnit.Content.ToString(), System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface(polyUnit.FontFamily, polyUnit.FontStyle, polyUnit.FontWeight, polyUnit.FontStretch), polyUnit.FontSize, Brushes.Black);

            polyUnit.Width = t.Width + 5;
            polyUnit.Height = t.Height;
            
            
                
            drawLabel();
        }
        public void initPt()
        {
            int[] initX = new int[6]{150,200,300,350,300,200};
            int[] initY = new int[6]{150,100,100,150,200,200};
            
            for (int i = 0; i < maxPt; i++)
            {
                PolySpot ps = new PolySpot(initX[i], initY[i], i, this,imageWidth,imageHeight);

                ps.realWidth = realWidth;
                ps.realHeight = realHeight;

                ps.Position();

                if (i >= minPt)
                {
                    ps.isShown = false;
                }

                polyPt.Add(ps);
            }

        }
        public void initLine()
        {
            for (int i = 0; i < maxPt; i++)
            {
                Line l = new Line();

                l.X1 = polyPt[i].Margin.Left+7;
                l.Y1 = polyPt[i].Margin.Top+7;

                l.Stroke = System.Windows.Media.Brushes.Red;
                l.StrokeDashArray = System.Windows.Media.DoubleCollection.Parse("1,4");

                if (i == maxPt - 1)
                {
                    l.X2 = polyPt[0].Margin.Left+7;
                    l.Y2 = polyPt[0].Margin.Top+7;
                }
                else
                {
                    l.X2 = polyPt[i + 1].Margin.Left+7;
                    l.Y2 = polyPt[i + 1].Margin.Top+7;
                }
                
                polyLine.Add(l);
            }

            polyLine[minPt - 1].X2 = polyPt[0].Margin.Left+7;
            polyLine[minPt - 1].Y2 = polyPt[0].Margin.Top+7;
        }


        public void drawLabel()
        {
            
            

            double avgPosX = 0;
            double avgPosY = 0;

            for (int i = 0; i < currIdx; i++)
            {
                avgPosX += polyPt[i].Margin.Left;
                avgPosY += polyPt[i].Margin.Top;
            }


            avgPosX /= (currIdx);
            avgPosY /= (currIdx);


            Thickness th = new Thickness(0,0,0,0);

            th.Left = avgPosX - 250;
            th.Top = avgPosY - 130;
            
            
            polyVal.Margin = th;
            

            
            th.Left = th.Left + 60;
            polyUnit.Margin = th;
            

            
            
            
        }



        public void drawLine(int x, int y, int ptId)
        {
            int nextId = ptId;
            int prevId = ptId - 1;

            if (prevId < 0)
                prevId = currIdx - 1;

            
            polyLine[nextId].X1 = x;
            polyLine[nextId].Y1 = y;
            
            polyLine[prevId].X2 = x;
            polyLine[prevId].Y2 = y;
        }
    }
}

