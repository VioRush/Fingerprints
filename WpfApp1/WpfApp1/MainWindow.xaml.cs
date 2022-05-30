using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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

namespace WpfApp1
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        //filtry
        private void Medianowy_Click(object sender, RoutedEventArgs e)
        {
            int size;
            size = int.Parse(Wymiar.Text);
            BitmapCopy();
            /*Bitmap copy;
            using (MemoryStream ms = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(loadedImage));
                enc.Save(ms);
                Bitmap loaded = new Bitmap(ms);
                copy = new Bitmap(loaded);
            }*/
            Bitmap temp = MedianFilter(copy, size);
            ObrazBinaryzowany.Source = BitmapToImage(temp);
        }

        private Bitmap MedianFilter(Bitmap bitmap, int matrixSize)
        {
            Bitmap source = bitmap;
            var data = source.LockBits(
               new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
               System.Drawing.Imaging.ImageLockMode.ReadWrite,
               System.Drawing.Imaging.PixelFormat.Format32bppRgb
           );

            byte[] pixelBuffer = new byte[data.Stride * data.Height];
            byte[] resultBuffer = new byte[data.Stride * data.Height];

            Marshal.Copy(data.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            source.UnlockBits(data);


            int filterOffset = (matrixSize - 1) / 2;
            int calcOffset = 0;
            int byteOffset = 0;

            List<int> neighbourPixels = new List<int>();
            byte[] middlePixel;

            for (int offsetY = filterOffset; offsetY <
                source.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    source.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY * data.Stride + offsetX * 4;

                    neighbourPixels.Clear();

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * data.Stride);

                            neighbourPixels.Add(BitConverter.ToInt32(pixelBuffer, calcOffset));
                        }
                    }

                    neighbourPixels.Sort();

                    middlePixel = BitConverter.GetBytes(neighbourPixels[neighbourPixels.Count / 2]);

                    resultBuffer[byteOffset] = middlePixel[0];
                    resultBuffer[byteOffset + 1] = middlePixel[1];
                    resultBuffer[byteOffset + 2] = middlePixel[2];
                    resultBuffer[byteOffset + 3] = middlePixel[3];
                }
            }

            Bitmap resultBitmap = new Bitmap(source.Width, source.Height);

            var resultData = resultBitmap.LockBits(new System.Drawing.Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadWrite,
                        System.Drawing.Imaging.PixelFormat.Format32bppRgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

        private void Kuwahara_Click(object sender, RoutedEventArgs e)
        {
            int size;
            size = int.Parse(WymiarKuwahara.Text);
            BitmapCopy();
            Bitmap temp = KuwaharaFilter(copy, size);
            ObrazBinaryzowany.Source = BitmapToImage(temp);
        }

        public static Bitmap KuwaharaFilter(Bitmap source, int size)
        {
            Bitmap temp = new Bitmap(source.Width, source.Height);

            int[] ApetureMinX = { -(size / 2), 0, -(size / 2), 0 };
            int[] ApetureMaxX = { 0, (size / 2), 0, (size / 2) };
            int[] ApetureMinY = { -(size / 2), -(size / 2), 0, 0 };
            int[] ApetureMaxY = { 0, 0, (size / 2), (size / 2) };

            for (int x = 0; x < temp.Width; ++x)
            {
                for (int y = 0; y < temp.Height; ++y)
                {
                    int[] RValues = { 0, 0, 0, 0 };
                    int[] GValues = { 0, 0, 0, 0 };
                    int[] BValues = { 0, 0, 0, 0 };
                    int[] NumPixels = { 0, 0, 0, 0 };
                    int[] MaxRValue = { 0, 0, 0, 0 };
                    int[] MaxGValue = { 0, 0, 0, 0 };
                    int[] MaxBValue = { 0, 0, 0, 0 };
                    int[] MinRValue = { 255, 255, 255, 255 };
                    int[] MinGValue = { 255, 255, 255, 255 };
                    int[] MinBValue = { 255, 255, 255, 255 };

                    for (int i = 0; i < 4; ++i)    //cztery regiony
                    {
                        for (int x2 = ApetureMinX[i]; x2 < ApetureMaxX[i]; ++x2)
                        {
                            int TempX = x + x2;
                            if (TempX >= 0 && TempX < temp.Width)
                            {
                                for (int y2 = ApetureMinY[i]; y2 < ApetureMaxY[i]; ++y2)
                                {
                                    int TempY = y + y2;
                                    if (TempY >= 0 && TempY < temp.Height)
                                    {
                                        System.Drawing.Color TempColor = source.GetPixel(TempX, TempY);
                                        RValues[i] += TempColor.R;
                                        GValues[i] += TempColor.G;
                                        BValues[i] += TempColor.B;

                                        if (TempColor.R > MaxRValue[i])
                                        {
                                            MaxRValue[i] = TempColor.R;
                                        }
                                        else if (TempColor.R < MinRValue[i])
                                        {
                                            MinRValue[i] = TempColor.R;
                                        }

                                        if (TempColor.G > MaxGValue[i])
                                        {
                                            MaxGValue[i] = TempColor.G;
                                        }
                                        else if (TempColor.G < MinGValue[i])
                                        {
                                            MinGValue[i] = TempColor.G;
                                        }

                                        if (TempColor.B > MaxBValue[i])
                                        {
                                            MaxBValue[i] = TempColor.B;
                                        }
                                        else if (TempColor.B < MinBValue[i])
                                        {
                                            MinBValue[i] = TempColor.B;
                                        }
                                        ++NumPixels[i];
                                    }
                                }
                            }
                        }
                    }
                    int j = 0;
                    int MinDifference = 10000;
                    for (int i = 0; i < 4; ++i)
                    {
                        int CurrentDifference = (MaxRValue[i] - MinRValue[i]) + (MaxGValue[i] - MinGValue[i]) + (MaxBValue[i] - MinBValue[i]);
                        if (CurrentDifference < MinDifference && NumPixels[i] > 0)
                        {
                            j = i;
                            MinDifference = CurrentDifference;
                        }
                    }

                    System.Drawing.Color MeanPixel = System.Drawing.Color.FromArgb(RValues[j] / NumPixels[j],
                        GValues[j] / NumPixels[j],
                        BValues[j] / NumPixels[j]);
                    temp.SetPixel(x, y, MeanPixel);
                }
            }
            return temp;
        }

        private void Otsu_Click(object sender, RoutedEventArgs e)
        {
            byte[] LUT = new byte[256];

            int a = OtsuThreshold();

            Bitmap copy;
            using (MemoryStream ms = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(loadedImage));
                enc.Save(ms);
                Bitmap loaded = new Bitmap(ms);
                copy = new Bitmap(loaded);
            }

            var data = copy.LockBits(
                new System.Drawing.Rectangle(0, 0, copy.Width, copy.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb
            );
            var copyData = new byte[data.Stride * data.Height];

            Marshal.Copy(data.Scan0, copyData, 0, copyData.Length);

            for (int i = 0; i < a; i++) LUT[i] = 0;
            for (int i = a; i < 256; i++) LUT[i] = 255;

            for (int i = 0; i < copyData.Length; i++)
            {
                copyData[i] = LUT[copyData[i]];
            }
            Marshal.Copy(copyData, 0, data.Scan0, copyData.Length);
            copy.UnlockBits(data);
            BitmapImage im = BitmapToImage(copy);
            ObrazBinaryzowany.Source = im;

        }

        public int OtsuThreshold()
        {
            Bitmap copy;
            using (MemoryStream ms = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(loadedImage));
                enc.Save(ms);
                Bitmap loaded = new Bitmap(ms);
                copy = new Bitmap(loaded);
            }

            int[] histogram = new int[256];
            for (int i = 0; i < histogram.Length; i++) histogram[i] = 0;

            for (int i = 0; i < copy.Width; i++)
            {
                for (int j = 0; j < copy.Height; j++)
                {
                    System.Drawing.Color c = copy.GetPixel(i, j);
                    int value = (c.R + c.G + c.B) / 3;
                    histogram[value]++;
                }
            }
            int total = copy.Height * copy.Width;

            float sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += i * histogram[i];
            }

            float sumB = 0;
            int wB = 0;
            int wF = 0;

            float varMax = 0;
            int threshold = 0;

            for (int i = 0; i < 256; i++)
            {
                wB += histogram[i];
                if (wB == 0) continue;
                wF = total - wB;

                if (wF == 0) break;

                sumB += (float)(i * histogram[i]);
                float mB = sumB / wB;
                float mF = (sum - sumB) / wF;

                float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = i;
                }
            }
            return threshold;
        }
    }
}
