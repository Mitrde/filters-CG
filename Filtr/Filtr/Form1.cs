using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Filtr
{
    public partial class Form1 : Form
    {
        Bitmap image1;
        Bitmap image2;
        protected bool[,] mas=new bool[3,3];
        
        public Form1()
        {
            InitializeComponent();
            string path = Environment.CurrentDirectory + "\\Null.jpg";

            image2 = new Bitmap(path);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }


        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //cоздаем диалог для открытия файла
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files| *.png;*.jpg;*.bmp| All Files (*.*)|*.*";
            if (dialog.ShowDialog()== DialogResult.OK)
            {
                image1 = new Bitmap(dialog.FileName);
                pictureBox1.Image = image1;
                pictureBox1.Refresh();
            }
        }
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image file|*.jpg";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = sfd.FileName;
                BinaryWriter bw = new BinaryWriter(File.Create(path));
                bw.Write("Example jpg file");
                bw.Dispose();
            }
        }
        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image1, backgroundWorker1);  //////////////////////////////////////////////////////////////////
            if (backgroundWorker1.CancellationPending != true)
                image2 = image1;
                image1 = newImage;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image1;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void размытиеПоГаусуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void черноеБелоеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlackWhite();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SepiaFiltr();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void повышениеЯркостиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BrightnesUp();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void фильтрСобеляToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sharpness();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void эффектСтеклаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GlassFilter();
            backgroundWorker1.RunWorkerAsync(filter);
           
        }

        private void поворот30ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Turn();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void выделениеГраницToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BoarderFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

      
        private void button2_Click_1(object sender, EventArgs e)
        {
            pictureBox1.Image = image2;
            Bitmap tmp;
            tmp=image1;
            image1=image2;
            image2=tmp;
            pictureBox1.Refresh();

        }

        private void медианныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MedianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void автоконтрастToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new AutoContrast();
            filter.calculateDelay(image1);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GreyWorld();
            filter.calculateAvg(image1);
            backgroundWorker1.RunWorkerAsync(filter);

        }

      

       private void dilationToolStripMenuItem_Click(object sender, EventArgs e)
       {
           Form2 f = new Form2(new MyDelegate(mask));
           f.ShowDialog();
           Filters filter = new Dilation(mas);
           backgroundWorker1.RunWorkerAsync(filter);
       }
       void mask(bool[,] param)
       {
           mas = param;  
       }

       private void erosionToolStripMenuItem_Click(object sender, EventArgs e)
       {
           Form2 f = new Form2(new MyDelegate(mask));
           f.ShowDialog();
           Filters filter = new Erosion(mas);
           backgroundWorker1.RunWorkerAsync(filter);
       }
       

       private void openingToolStripMenuItem_Click(object sender, EventArgs e)
       {
           Form2 f = new Form2(new MyDelegate(mask));
           f.ShowDialog();
           Filters filter = new Opening(mas);
           backgroundWorker1.RunWorkerAsync(filter);
            
       }

       private void closingToolStripMenuItem_Click(object sender, EventArgs e)
       {
           Form2 f = new Form2(new MyDelegate(mask));
           f.ShowDialog();
           Filters filter = new Closing(mas);
           backgroundWorker1.RunWorkerAsync(filter);
       }

       private void topHatToolStripMenuItem_Click(object sender, EventArgs e)
       {
           Form2 f = new Form2(new MyDelegate(mask));
           f.ShowDialog();
           Filters filter = new grad(mas);
           backgroundWorker1.RunWorkerAsync(filter);
       }
      
       

       
       
        
    }
}
