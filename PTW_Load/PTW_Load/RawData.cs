using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace IRNDT_System.LockIn
{
    class RawData
    {
        private short[][] value;
        private int index = 0;
        private int fCount = 0;
        private int repeat = 0;
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }

        public delegate void ResultImage(short[] image, int w, int h, short min, short max);
        public event ResultImage PhaseImage;
        public event ResultImage AmplitudeImage;

        public void MakeBuffer(int count)
        {
            index = 0;
            repeat = count;
            fCount = count * 4;
            value = new short[fCount][];
        }

        public void AddFrame(int w, int h, short[] f)
        {
            Debug.WriteLine(String.Format("[ADD FRAME] {0} :: {1}", index, fCount));
            if (index >= fCount)
                return;

            FrameWidth = w;
            FrameHeight = h;

            value[index] = (short[])f.Clone();
            index++;
        }

        public void Phase()
        {
            short[] result = new short[FrameWidth * FrameHeight];
            short[][] tResult;
            short retMin = short.MaxValue;
            short retMax = short.MinValue;

            if (repeat == 1)
            {
                for (int i = 0; i < (FrameWidth * FrameHeight); i++)
                {
                    double val;
                    if ((value[1][i] - value[3][i]) == 0)
                    {
                        val = Math.Atan((double)(value[0][i] - value[2][i]));
                    }
                    else
                    {
                        val = Math.Atan((double)(value[0][i] - value[2][i]) / (double)(value[1][i] - value[3][i]));
                    }

                    val = (val + Math.PI / 2.0) * 360.0 / Math.PI;
                    result[i] = (short)val;

                    if (result[i] > retMax)
                        retMax = result[i];

                    if (result[i] < retMin)
                        retMin = result[i];
                }
            }
            else
            {
                tResult = new short[repeat][];
                for (int r = 0; r < repeat; r++)
                {
                    tResult[r] = new short[FrameWidth * FrameHeight];

                    for (int i = 0; i < (FrameWidth * FrameHeight); i++)
                    {
                        double val;
                        int offset = r * 4;
                        if ((value[offset + 1][i] - value[offset + 3][i]) == 0)
                        {
                            val = Math.Atan((double)(value[offset + 0][i] - value[offset + 2][i]));
                        }
                        else
                        {
                            val = Math.Atan((double)(value[offset + 0][i] - value[offset + 2][i]) / (double)(value[offset + 1][i] - value[offset + 3][i]));
                        }

                        val = (val + Math.PI / 2.0) * 360.0 / Math.PI;
                        tResult[r][i] = (short)val;
                    }
                }

                for (int i = 0; i < (FrameWidth * FrameHeight); i++)
                {
                    long avg = 0;
                    for (int r = 0; r < repeat; r++)
                    {
                        avg += tResult[r][i];
                    }

                    result[i] = (short)(avg / repeat);

                    if (result[i] > retMax)
                        retMax = result[i];

                    if (result[i] < retMin)
                        retMin = result[i];
                }
            }
            

            PhaseImage(result, FrameWidth, FrameHeight, retMin, retMax);
        }

        public void Amplitude()
        {
            short[] result = new short[FrameWidth * FrameHeight];
            short[][] tResult;
            short retMin = short.MaxValue;
            short retMax = short.MinValue;

            if(repeat == 1)
            {
                for (int i = 0; i < (FrameWidth * FrameHeight); i++)
                {
                    double val;

                    val = Math.Sqrt(Math.Pow(value[0][i] - value[2][i], 2) + Math.Pow(value[1][i] - value[3][i], 2));

                    //val = val * 10;

                    result[i] = (short)val;

                    if (result[i] > retMax)
                        retMax = result[i];

                    if (result[i] < retMin)
                        retMin = result[i];
                }
            }
            else
            {
                tResult = new short[repeat][];
                for (int r = 0; r < repeat; r++)
                {
                    tResult[r] = new short[FrameWidth * FrameHeight];

                    for (int i = 0; i < (FrameWidth * FrameHeight); i++)
                    {
                        double val;
                        int offset = r * 4;
                        val = Math.Sqrt(Math.Pow(value[offset + 0][i] - value[offset + 2][i], 2) + Math.Pow(value[offset + 1][i] - value[offset + 3][i], 2));

                        //val = val * 10;

                        tResult[r][i] = (short)val;
                    }
                }

                for (int i = 0; i < (FrameWidth * FrameHeight); i++)
                {
                    long avg = 0;
                    for (int r = 0; r < repeat; r++)
                    {
                        avg += tResult[r][i];
                    }

                    result[i] = (short)(avg / repeat);

                    if (result[i] > retMax)
                        retMax = result[i];

                    if (result[i] < retMin)
                        retMin = result[i];
                }
            }

            AmplitudeImage(result, FrameWidth, FrameHeight, retMin, retMax);
        }
    }
}
