using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace Filtr
{
    public delegate void MyDelegate(bool[,] data);

    abstract class Filters
    {
        protected virtual Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return Color.Black;
        }

        protected int pix;//количество пикселей ы
        protected int min;//min на гистограме
        protected int max;
        protected float avg;
        protected int sR;//сумма Красных
        protected int sG;
        protected int sB;
        public void calculateDelay(Bitmap sourceImage)//вычисляет края гист
        {
            pix = (sourceImage.Width - 1) * (sourceImage.Height - 1);

            min = 0;
            max = 255;
            int count = 0;
            int[] gistogram = new int[256];

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color pixel = sourceImage.GetPixel(i, j);

                    gistogram[(pixel.R + pixel.G + pixel.B) / 3]++;
                }
            }

            int k = 0;
            while (count < (pix * 0.05))
            {
                count += gistogram[k];
                k++;
            }
            min = k;

            k = 255;
            count = 0;
            while (count < (pix * 0.05))
            {
                count += gistogram[k];
                k--;
            }
            max = k;
        }
        public void calculateAvg(Bitmap sourceImage)//вычисляет Avg,sR,sG,sB
        {
            pix = (sourceImage.Width - 1) * (sourceImage.Height - 1);
            sR = 0;
            sG = 0;
            sB = 0;
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color pixel = sourceImage.GetPixel(i, j);
                    sR += pixel.R;
                    sG += pixel.G;
                    sB += pixel.B;

                }
            }
            avg = ((sR + sG + sB) / (3 * pix));//средний цвет

        }
        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)  //process IMAGE
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }


            return resultImage;
        }
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R,
                                                255 - sourceColor.G,
                                                255 - sourceColor.B);
            return resultColor;
        }
    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }
    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }
    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;

        }
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }
    class BlackWhite : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            double Intensity = 0;
            Color sourceColor = sourceImage.GetPixel(x, y);
            Intensity = sourceColor.R * 0.36 + 0.53 * sourceColor.G + 0.11 * sourceColor.B;
            Color resultColor = Color.FromArgb((int)Intensity,
                                               (int)Intensity,
                                               (int)Intensity);
            return resultColor;

        }
    }
    class SepiaFiltr : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = 25;
            double Intensity = 0;
            double resultR = 0;
            double resultG = 0;
            double resultB = 0;
            Color sourceColor = sourceImage.GetPixel(x, y);

            Intensity = sourceColor.R * 0.36 + 0.53 * sourceColor.G + 0.11 * sourceColor.B;
            resultR = Intensity + 2 * k;
            resultG = Intensity + 0.5 * k;
            resultB = Intensity - k;

            return Color.FromArgb(
              Clamp((int)resultR, 0, 255),
              Clamp((int)resultG, 0, 255),
              Clamp((int)resultB, 0, 255)
              );
        }

    }
    class BrightnesUp : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = 25;

            double resultR = 0;
            double resultG = 0;
            double resultB = 0;
            Color sourceColor = sourceImage.GetPixel(x, y);


            resultR = sourceColor.R + k;
            resultG = sourceColor.G + k;
            resultB = sourceColor.B + k;

            return Color.FromArgb(
              Clamp((int)resultR, 0, 255),
              Clamp((int)resultG, 0, 255),
              Clamp((int)resultB, 0, 255)
              );
        }

    }
    class SobelFilter : MatrixFilter
    {
        public SobelFilter()
        {
            kernel = new float[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        }
    }
    class Sharpness : MatrixFilter
    {
        public Sharpness()
        {
            kernel = new float[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
        }
    }
    class GlassFilter : Filters
    {
        Random rnd = new Random();
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
           
            int rx = rnd.Next(10) - 5;
            int ry = rnd.Next(10) - 5;
            Color resultColor = sourceImage.GetPixel(Clamp(x + rx, 0, sourceImage.Width - 1), Clamp(y + ry, 0, sourceImage.Height - 1));
            return resultColor;
        }

    }
    class Turn : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            float cos = 0.866025f;
            float sin = 0.5f;
            float x0 = sourceImage.Width / 2;
            float y0 = sourceImage.Height / 2;
            float xr = (x - x0) * cos - (y - y0) * sin + x0;
            float yr = (x - x0) * sin + (y - y0) * cos + y0;
            Color resultColor = sourceImage.GetPixel(Clamp((int)xr, 0, sourceImage.Width - 1), Clamp((int)yr, 0, sourceImage.Height - 1));
            return resultColor;
        }

    }
    class BoarderFilter : MatrixFilter
    {
        public BoarderFilter()
        {
            kernel = new float[,] { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };
        }
    }
    class MedianFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radius = 1;
            int size = 2 * radius + 1;
            int num = 0;
            int n = (radius * 2 + 1) * (radius * 2 + 1);

            int[] cR = new int[n];
            int[] cB = new int[n];
            int[] cG = new int[n];

            for (int l = -radius; l <= radius; l++)
                for (int k = -radius; k <= radius; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    cR[num] = neighborColor.R;
                    cG[num] = neighborColor.G;
                    cB[num] = neighborColor.B;
                    num++;
                }
            Array.Sort(cR, 0, n - 1);   // сортировка массивов
            Array.Sort(cG, 0, n - 1);
            Array.Sort(cB, 0, n - 1);

            int n_ = (int)(n / 2); //mediana tipa

            return Color.FromArgb(
              Clamp((int)cR[n_], 0, 255),
              Clamp((int)cG[n_], 0, 255),
              Clamp((int)cB[n_], 0, 255)
              );

        }
    }
    class AutoContrast : Filters
    {

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color p = sourceImage.GetPixel(x, y);


            return Color.FromArgb(
              Clamp((int)(p.R - min) * 255 / (max - min), 0, 255),
              Clamp((int)(p.G - min) * 255 / (max - min), 0, 255),
              Clamp((int)(p.B - min) * 255 / (max - min), 0, 255)
              );
        }
    }
    class GreyWorld : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color p = sourceImage.GetPixel(x, y);


            return Color.FromArgb(
              Clamp((int)(p.R * avg * pix / sR), 0, 255),
              Clamp((int)(p.G * avg * pix / sG), 0, 255),
              Clamp((int)(p.B * avg * pix / sB), 0, 255)
              );
        }

    }
    /*
    class Dilation:Filters
    {
        protected bool[,] kernel = null;
        protected Dilation() { }
        protected bool[,] elem = new bool[3, 3];
        public Dilation(bool[,] kernel)
        {
            this.kernel = kernel;
            
            elem[0, 0] = kernel[2, 2];
            elem[0, 1] = kernel[2, 1];
            elem[0, 2] = kernel[2, 0];
            elem[1, 0] = kernel[1, 2];
            elem[1, 1] = kernel[1, 1];
            elem[1, 2] = kernel[1, 0];
            elem[2, 0] = kernel[0, 2];
            elem[2, 1] = kernel[0, 1];
            elem[2, 2] = kernel[0, 0];
        }

        
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = elem.GetLength(0) / 2;
            int radiusY = elem.GetLength(1) / 2;
            for (int l= -radiusY; l<=radiusY;l++)
                for(int k=-radiusX;k<=radiusX;k++)
                {
                    if (elem[k + radiusX, l + radiusY] == true)
                    {
                        int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                        int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                        Color neighborColor = sourceImage.GetPixel(idX, idY);
                        if (neighborColor == Color.FromArgb(0, 0, 0))
                        {
                            return Color.FromArgb(0, 0, 0);
                        }
                    }
                    
                }
            return Color.FromArgb(255,255,255);
        }

    }
    class Erosing : Filters
    {
        protected bool[,] kernel = null;
        protected Erosing() { }
        public Erosing(bool[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    if (kernel[k + radiusX, l + radiusY] == true)
                    {
                        int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                        int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                        Color neighborColor = sourceImage.GetPixel(idX, idY);
                        if (neighborColor != Color.FromArgb(0, 0, 0))
                        {
                            return Color.FromArgb(255, 255, 255);
                        }
                    }
                    

                }
            return Color.FromArgb(0,0,0);
        }

    }
    class Opening : Filters
    {
        protected bool[,] kernel = null;
        protected Opening() { }
        protected bool[,] elem = new bool[3, 3];
        public Opening(bool[,] kernel)
        {
            this.kernel = kernel;
            elem[0, 0] = kernel[2, 2];
            elem[0, 1] = kernel[2, 1];
            elem[0, 2] = kernel[2, 0];
            elem[1, 0] = kernel[1, 2];
            elem[1, 1] = kernel[1, 1];
            elem[1, 2] = kernel[1, 0];
            elem[2, 0] = kernel[0, 2];
            elem[2, 1] = kernel[0, 1];
            elem[2, 2] = kernel[0, 0];
        }
       

    }
}
*/

    abstract class Matmorf : Filters
    {
        protected bool[,] maska = null;

        public Matmorf(bool[,] m)
        {
            maska = m;
        }
    }
    class Dilation : Matmorf
    {

        public Dilation(bool[,] m) : base(m) { }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            int rx = maska.GetLength(0) / 2;
            int ry = maska.GetLength(1) / 2;
            int w = sourceImage.Width;
            int h = sourceImage.Height;

            for (int y = ry; y < h - ry; y++)
            {
                worker.ReportProgress((int)((float)y / sourceImage.Width * 100));
                if (worker.CancellationPending) return null;
                for (int x = rx; x < w - rx; x++)
                {
                    int mr = sourceImage.GetPixel(x, y).R;
                    int mg = sourceImage.GetPixel(x, y).G;
                    int mb = sourceImage.GetPixel(x, y).B;
                    int k = 0;
                    for (int j = -ry; j <= ry; j++)
                    {
                        int l = 0;
                        for (int i = -rx; i <= rx; i++)
                        {
                            if (maska[l, k])
                            {
                                if (sourceImage.GetPixel(x + i, y + j).R > mr) mr = sourceImage.GetPixel(x + i, y + j).R;
                                if (sourceImage.GetPixel(x + i, y + j).G > mg) mg = sourceImage.GetPixel(x + i, y + j).G;
                                if (sourceImage.GetPixel(x + i, y + j).B > mb) mb = sourceImage.GetPixel(x + i, y + j).B;
                            }
                            l++;
                        }
                        k++;
                    }
                    resultImage.SetPixel(x, y, Color.FromArgb(mr, mg, mb));
                }
            }
            return resultImage;
        }
    }
    class Erosion : Matmorf
    {
        public Erosion(bool[,] m) : base(m) { }


        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            int rx = maska.GetLength(0) / 2;
            int ry = maska.GetLength(1) / 2;
            int w = sourceImage.Width;
            int h = sourceImage.Height;

            for (int y = ry; y < h - ry; y++)
            {
                worker.ReportProgress((int)((float)y / sourceImage.Width * 100));
                if (worker.CancellationPending) return null;
                for (int x = rx; x < w - rx; x++)
                {
                    int mr = sourceImage.GetPixel(x, y).R;
                    int mg = sourceImage.GetPixel(x, y).G;
                    int mb = sourceImage.GetPixel(x, y).B;
                    int k = 0;
                    for (int j = -ry; j <= ry; j++)
                    {
                        int l = 0;
                        for (int i = -rx; i <= rx; i++)
                        {
                            if (maska[l, k])
                            {
                                if (sourceImage.GetPixel(x + i, y + j).R < mr) mr = sourceImage.GetPixel(x + i, y + j).R;
                                if (sourceImage.GetPixel(x + i, y + j).G < mg) mg = sourceImage.GetPixel(x + i, y + j).G;
                                if (sourceImage.GetPixel(x + i, y + j).B < mb) mb = sourceImage.GetPixel(x + i, y + j).B;
                            }
                            l++;
                        }
                        k++;
                    }
                    resultImage.SetPixel(x, y, Color.FromArgb(mr, mg, mb));
                }
            }
            return resultImage;
        }
    }
    class Opening : Matmorf
    {

        public Opening(bool[,] m) : base(m) { }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap r = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap rr = new Bitmap(sourceImage.Width, sourceImage.Height);
            int rx = maska.GetLength(0) / 2;
            int ry = maska.GetLength(1) / 2;
            int w = sourceImage.Width;
            int h = sourceImage.Height;

            for (int y = ry; y < h - ry; y++)
            {
                worker.ReportProgress((int)((float)y / sourceImage.Width * 50));
                if (worker.CancellationPending) return null;
                for (int x = rx; x < w - rx; x++)
                {
                    int mr = sourceImage.GetPixel(x, y).R;
                    int mg = sourceImage.GetPixel(x, y).G;
                    int mb = sourceImage.GetPixel(x, y).B;
                    int k = 0;
                    for (int j = -ry; j <= ry; j++)
                    {
                        int l = 0;
                        for (int i = -rx; i <= rx; i++)
                        {
                            if (maska[l, k])
                            {
                                if (sourceImage.GetPixel(x + i, y + j).R < mr) mr = sourceImage.GetPixel(x + i, y + j).R;
                                if (sourceImage.GetPixel(x + i, y + j).G < mg) mg = sourceImage.GetPixel(x + i, y + j).G;
                                if (sourceImage.GetPixel(x + i, y + j).B < mb) mb = sourceImage.GetPixel(x + i, y + j).B;
                            }
                            l++;
                        }
                        k++;
                    }
                    r.SetPixel(x, y, Color.FromArgb(mr, mg, mb));
                }
            }

            for (int y = ry; y < h - ry; y++)
            {
                worker.ReportProgress(50 + (int)((float)y / sourceImage.Width * 50));
                if (worker.CancellationPending) return null;
                for (int x = rx; x < w - rx; x++)
                {
                    int mr = r.GetPixel(x, y).R;
                    int mg = r.GetPixel(x, y).G;
                    int mb = r.GetPixel(x, y).B;
                    int k = 0;
                    for (int j = -ry; j <= ry; j++)
                    {
                        int l = 0;
                        for (int i = -rx; i <= rx; i++)
                        {
                            if (maska[l, k])
                            {
                                if (r.GetPixel(x + i, y + j).R > mr) mr = r.GetPixel(x + i, y + j).R;
                                if (r.GetPixel(x + i, y + j).G > mg) mg = r.GetPixel(x + i, y + j).G;
                                if (r.GetPixel(x + i, y + j).B > mb) mb = r.GetPixel(x + i, y + j).B;
                            }
                            l++;
                        }
                        k++;
                    }
                    rr.SetPixel(x, y, Color.FromArgb(mr, mg, mb));
                }
            }
            return rr;
        }

    }
    class Closing : Matmorf
    {
        public Closing(bool[,] m) : base(m) { }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap r = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap rr = new Bitmap(sourceImage.Width, sourceImage.Height);
            int rx = maska.GetLength(0) / 2;
            int ry = maska.GetLength(1) / 2;
            int w = sourceImage.Width;
            int h = sourceImage.Height;

            for (int y = ry; y < h - ry; y++)
            {
                worker.ReportProgress((int)((float)y / sourceImage.Width * 50));
                if (worker.CancellationPending) return null;
                for (int x = rx; x < w - rx; x++)
                {
                    int mr = sourceImage.GetPixel(x, y).R;
                    int mg = sourceImage.GetPixel(x, y).G;
                    int mb = sourceImage.GetPixel(x, y).B;
                    int k = 0;
                    for (int j = -ry; j <= ry; j++)
                    {
                        int l = 0;
                        for (int i = -rx; i <= rx; i++)
                        {
                            if (maska[l, k])
                            {
                                if (sourceImage.GetPixel(x + i, y + j).R > mr) mr = sourceImage.GetPixel(x + i, y + j).R;
                                if (sourceImage.GetPixel(x + i, y + j).G > mg) mg = sourceImage.GetPixel(x + i, y + j).G;
                                if (sourceImage.GetPixel(x + i, y + j).B > mb) mb = sourceImage.GetPixel(x + i, y + j).B;
                            }
                            l++;
                        }
                        k++;
                    }
                    r.SetPixel(x, y, Color.FromArgb(mr, mg, mb));
                }
            }

            for (int y = ry; y < h - ry; y++)
            {
                worker.ReportProgress(50 + (int)((float)y / sourceImage.Width * 50));
                if (worker.CancellationPending) return null;
                for (int x = rx; x < w - rx; x++)
                {
                    int mr = r.GetPixel(x, y).R;
                    int mg = r.GetPixel(x, y).G;
                    int mb = r.GetPixel(x, y).B;
                    int k = 0;
                    for (int j = -ry; j <= ry; j++)
                    {
                        int l = 0;
                        for (int i = -rx; i <= rx; i++)
                        {
                            if (maska[l, k])
                            {
                                if (r.GetPixel(x + i, y + j).R < mr) mr = r.GetPixel(x + i, y + j).R;
                                if (r.GetPixel(x + i, y + j).G < mg) mg = r.GetPixel(x + i, y + j).G;
                                if (r.GetPixel(x + i, y + j).B < mb) mb = r.GetPixel(x + i, y + j).B;
                            }
                            l++;
                        }
                        k++;
                    }
                    rr.SetPixel(x, y, Color.FromArgb(mr, mg, mb));
                }
            }
            return rr;
        }
    }
    class grad : Matmorf
    {
        public grad(bool[,] m) : base(m) { }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap r = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap rr = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap rrr = new Bitmap(sourceImage.Width, sourceImage.Height);
            int rx = maska.GetLength(0) / 2;
            int ry = maska.GetLength(1) / 2;
            int w = sourceImage.Width;
            int h = sourceImage.Height;

            for (int y = ry; y < h - ry; y++)
            {
                worker.ReportProgress((int)((float)y / sourceImage.Width * 33));
                if (worker.CancellationPending) return null;
                for (int x = rx; x < w - rx; x++)
                {
                    int mr = sourceImage.GetPixel(x, y).R;
                    int mg = sourceImage.GetPixel(x, y).G;
                    int mb = sourceImage.GetPixel(x, y).B;
                    int k = 0;
                    for (int j = -ry; j <= ry; j++)
                    {
                        int l = 0;
                        for (int i = -rx; i <= rx; i++)
                        {
                            if (maska[l, k])
                            {
                                if (sourceImage.GetPixel(x + i, y + j).R > mr) mr = sourceImage.GetPixel(x + i, y + j).R;
                                if (sourceImage.GetPixel(x + i, y + j).G > mg) mg = sourceImage.GetPixel(x + i, y + j).G;
                                if (sourceImage.GetPixel(x + i, y + j).B > mb) mb = sourceImage.GetPixel(x + i, y + j).B;
                            }
                            l++;
                        }
                        k++;
                    }
                    r.SetPixel(x, y, Color.FromArgb(mr, mg, mb));
                }
            }
            for (int y = ry; y < h - ry; y++)
            {
                worker.ReportProgress(33 + (int)((float)y / sourceImage.Width * 33));
                if (worker.CancellationPending) return null;
                for (int x = rx; x < w - rx; x++)
                {
                    int mr = sourceImage.GetPixel(x, y).R;
                    int mg = sourceImage.GetPixel(x, y).G;
                    int mb = sourceImage.GetPixel(x, y).B;
                    int k = 0;
                    for (int j = -ry; j <= ry; j++)
                    {
                        int l = 0;
                        for (int i = -rx; i <= rx; i++)
                        {
                            if (maska[l, k])
                            {
                                if (sourceImage.GetPixel(x + i, y + j).R < mr) mr = sourceImage.GetPixel(x + i, y + j).R;
                                if (sourceImage.GetPixel(x + i, y + j).G < mg) mg = sourceImage.GetPixel(x + i, y + j).G;
                                if (sourceImage.GetPixel(x + i, y + j).B < mb) mb = sourceImage.GetPixel(x + i, y + j).B;
                            }
                            l++;
                        }
                        k++;
                    }
                    rr.SetPixel(x, y, Color.FromArgb(mr, mg, mb));
                }
            }
            for (int i = 0; i < w; i++)
            {
                worker.ReportProgress(66 + (int)((float)i / sourceImage.Width * 34));
                if (worker.CancellationPending) return null;
                for (int j = 0; j < h; j++)
                    rrr.SetPixel(i, j, Color.FromArgb(Clamp((r.GetPixel(i, j).R - rr.GetPixel(i, j).R), 0, 255), Clamp((r.GetPixel(i, j).G - rr.GetPixel(i, j).G), 0, 255), Clamp((r.GetPixel(i, j).B - rr.GetPixel(i, j).B), 0, 255)));
            }
            return rrr;
        }
    }
}
