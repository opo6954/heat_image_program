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
        

        public int maxPt = 6;
        public int minPt = 3;
        public double realWidth;
        public double realHeight;

        public int imageWidth;
        public int imageHeight;

        public int currIdx = 3;

        delegate void OnMoveComponent(Grid before, Grid curr);

        private void MoveComponent(Grid before, Grid curr)
        {
            foreach (PolySpot ps in polyPt)
            {
                before.Children.Remove(ps);
                curr.Children.Add(ps);
            }
            foreach (Line l in polyLine)
            {
                before.Children.Remove(l);
                curr.Children.Add(l);
            }
            before.Children.Remove(polyVal);
            curr.Children.Add(polyVal);
        }



        public void moveAllObject(Grid g)
        {
            this.Dispatcher.Invoke(new OnMoveComponent(MoveComponent),new object[]{polyPt[0].Parent,g});
        }

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

            short[] image = null;
            double[] imageD = null;

            double spotValue=0;

            switch (myWin.currAnalysisRegion)
            {
                case 0:
                    if (myWin.FirstFrameImage != null)
                        image = myWin.FirstFrameImage;
                    break;
                case 1:
                    if (myWin.AvgImage != null)
                        image = myWin.AvgImage;
                    break;
                case 2:
                    if (myWin.DeltaImage != null)
                        image = myWin.DeltaImage;
                    break;
                case 3:
                    if (myWin.StressImage != null)
                        imageD = myWin.StressImage;
                    break;
                case 4:
                    if (myWin.LossImage != null)
                        image = myWin.LossImage;
                    break;
                case 5:
                    if (myWin.AmplitudeImage != null)
                        image = myWin.AmplitudeImage;
                    break;
            }

            if (image == null && imageD == null)
                return;
            else
            {
                if (myWin.currAnalysisRegion == 0)
                {
                    

                    for (int i = 0; i < currIdx; i++)
                    {
                        PolySpot ps = polyPt[i];
                        spotValue += (double)(myWin.ConvertTemp(image[ps.Y * imageWidth + ps.X]));
                    }
                    spotValue = spotValue / currIdx;
                }
                else if (myWin.currAnalysisRegion == 3)
                {
                    for (int i = 0; i < currIdx; i++)
                    {
                        PolySpot ps = polyPt[i];
                        spotValue += (double)(imageD[ps.Y * imageWidth + ps.X]);
                    }
                    spotValue = spotValue / currIdx;
                }
                else
                {
                    for (int i = 0; i < currIdx; i++)
                    {
                        PolySpot ps = polyPt[i];
                        spotValue += (double)(image[ps.Y * imageWidth + ps.Y]);
                    }
                    spotValue = spotValue / currIdx;
                }
            }

            setPolySpotValue(spotValue);
            
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

            





            polyVal.Background = System.Windows.Media.Brushes.AliceBlue;
           

            
                
            drawLabel();
        }
        public void initPt()
        {
            int[] initX = new int[6]{100,150,250,300,250,150};
            int[] initY = new int[6]{100,50,50,100,150,150};
            
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

