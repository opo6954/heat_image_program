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
using Steema.TeeChart.Styles;

namespace PTW_Load.Chart
{
    /// <summary>
    /// Interaction logic for TeeChartPanel.xaml
    /// </summary>
    public partial class TeeChartPanel : UserControl
    {
        private Dictionary<string, Series> lines = new Dictionary<string, Series>();

        private TeeChart chart;

        public TeeChartPanel()
        {
            InitializeComponent();

            chart = new TeeChart();
            windowsFormsHost.Child = chart;

            Steema.TeeChart.Themes.BlackIsBackTheme theme = new Steema.TeeChart.Themes.BlackIsBackTheme(chart.Chart.Chart);
            theme.Apply();

            chart.Chart.Header.Visible = false;
            chart.Chart.Aspect.View3D = false;
            chart.Chart.Legend.LegendStyle = Steema.TeeChart.LegendStyles.Series;
            chart.Chart.Legend.TextStyle = Steema.TeeChart.LegendTextStyles.Plain;
        }

        public void Clear()
        {
            lines.Clear();
            chart.Chart.Series.Clear();
            chart.Chart.Tools.Clear();
        }

        public void AddCursor()
        {
            Steema.TeeChart.Tools.CursorTool t1 = new Steema.TeeChart.Tools.CursorTool();
            t1.Style = Steema.TeeChart.Tools.CursorToolStyles.Vertical;
            chart.Chart.Tools.Add(t1);

            Steema.TeeChart.Tools.CursorTool t2 = new Steema.TeeChart.Tools.CursorTool();
            t2.Style = Steema.TeeChart.Tools.CursorToolStyles.Vertical;
            chart.Chart.Tools.Add(t2);
        }

        public void GetCursor(out int pos1, out int pos2)
        {
            Steema.TeeChart.Tools.CursorTool cur1 = chart.Chart.Tools[0] as Steema.TeeChart.Tools.CursorTool;
            int tmp1 = (int)cur1.XValue;

            Steema.TeeChart.Tools.CursorTool cur2 = chart.Chart.Tools[1] as Steema.TeeChart.Tools.CursorTool;
            int tmp2 = (int)cur2.XValue;

            if (tmp1 > tmp2)
            {
                pos1 = tmp2;
                pos2 = tmp1;
            }
            else
            {
                pos1 = tmp1;
                pos2 = tmp2;
            }
        }

        public delegate void OnAddItem(List<double> items);
        public void AddItem(List<double> items)
        {
            if (!this.CheckAccess())
            {
                Dispatcher.Invoke(new OnAddItem(AddItem), new object[] { items });
                return;
            }

            chart.Chart.AutoRepaint = false;

            String title = "";
            String key = "";
            double temp = 0.0;

            for (int i = 0; i < 5; i++ )
            {
                title = String.Format("{0} Spot", i);
                key = String.Format("{0}Spot", i);

                if (items[i] != -1)
                    temp = items[i] / 100.0;
                else
                    temp = -1;

                if (lines.ContainsKey(key))
                {
                    Series series = lines[key];
                    series.Add(series.Count, temp);
                    
                    if (temp == -1)
                    {
                        series.SetNull(series.Count - 1);
                    }
                }
                else
                {
                    Series series = new Steema.TeeChart.Styles.Line();

                    series.Title = title;

                    //(series as Steema.TeeChart.Styles.Line).Pointer.Style = PointerStyles.Circle;
                    //(series as Steema.TeeChart.Styles.Line).Pointer.Visible = true;
                    (series as Steema.TeeChart.Styles.Line).LinePen.Width = 1;

                    chart.Chart.Series.Add(series);

                    series.Add(0, temp);

                    if (temp == -1)
                    {
                        series.SetNull(0);
                    }

                    lines.Add(key, series);
                }
            }

            chart.Chart.AutoRepaint = true;
            chart.Chart.Refresh();
        }

        public delegate void OnAddItem2(String key, int value);
        public void AddItem(String key, int value)
        {
            if (!this.CheckAccess())
            {
                Dispatcher.Invoke(new OnAddItem2(AddItem), new object[] { key, value });
                return;
            }

            chart.Chart.AutoRepaint = false;

            if (lines.ContainsKey(key))
            {
                Series series = lines[key];
                series.Add(series.XValues.Last + 1, value);
            }
            else
            {
                Series series = new Steema.TeeChart.Styles.Line();

                series.Title = key;

                (series as Steema.TeeChart.Styles.Line).LinePen.Width = 1;

                chart.Chart.Series.Add(series);

                series.Add(0, value);

                lines.Add(key, series);
            }

            chart.Chart.AutoRepaint = true;
            chart.Chart.Refresh();
        }
    }
}
