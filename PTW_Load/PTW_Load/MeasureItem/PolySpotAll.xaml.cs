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

        private const int maxPt = 6;
        private const int minPt = 3;
        public double realWidth;
        public double realHeight;



        public void addPt(int numPt)
        {
            if (polyPt.Count < maxPt)
            {
                PolySpot newPt = new PolySpot();
                newPt.polyLayer = this;

                newPt.realHeight = realHeight;
                newPt.realWidth = realWidth;

                newPt.Position();

                polyPt.Add(newPt);

                Line l = new Line();
                l.X1 = polyPt[polyPt.Count - 1].X;
                l.Y1 = polyPt[polyPt.Count - 1].Y;

                l.X2 = polyPt[0].X;
                l.Y2 = polyPt[0].Y;

                polyLine.Add(l);
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

        public void setVisible(bool isVisible)
        {
            Visibility v;
            if (isVisible == true)
                v = Visibility.Visible;
            else
                v = Visibility.Collapsed;

            foreach (PolySpot ps in polyPt)
            {
                ps.Visibility = v;
            }
            foreach (Line l in polyLine)
            {
                l.Visibility = v;
            }
        }

        public void initialize(double width, double height)
        {
            realWidth = width;
            realHeight = height;
            initPt();
            initLine();
        }
        public void initPt()
        {
            int[] initX = new int[3]{100,200,300};
            int[] initY = new int[3]{100,150,100};
            
            for (int i = 0; i < minPt; i++)
            {
                PolySpot ps = new PolySpot(initX[i], initY[i], i, this);

                ps.realWidth = realWidth;
                ps.realHeight = realHeight;

                ps.Position();

                polyPt.Add(ps);
            }

        }
        public void initLine()
        {
            for (int i = 0; i < polyPt.Count; i++)
            {
                Line l = new Line();

                l.X1 = polyPt[i].Margin.Left+7;
                l.Y1 = polyPt[i].Margin.Top+7;

                l.Stroke = System.Windows.Media.Brushes.Red;
                l.StrokeDashArray = System.Windows.Media.DoubleCollection.Parse("1,4");

                if (i == polyPt.Count - 1)
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
        }

        public void drawLine(int x, int y, int ptId)
        {
            int nextId = ptId;
            int prevId = ptId - 1;

            if (prevId < 0)
                prevId = polyLine.Count - 1;

            
            polyLine[nextId].X1 = x;
            polyLine[nextId].Y1 = y;
            
            polyLine[prevId].X2 = x;
            polyLine[prevId].Y2 = y;
        }
    }
}

