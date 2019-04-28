
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections;
using System.Reflection;

// api eklemek için [DllImport("user32.dll")]
using System.Runtime.InteropServices;

// aforge görüntü işleme dll
using AForge.Video;
using AForge.Video.DirectShow;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Imaging.Textures;
using AForge.Math.Geometry;


namespace Kapak1
{
    public partial class Form1 : Form
    {
        
        string RxString;

        // Aforge görüntü işleme kodları 
        // Video'dan Resim Yakalama Aygıtları
        private FilterInfoCollection CaptureDevices;
        private VideoCaptureDevice FinalFrame;

        // İşlem bitmapleri
        private System.Drawing.Bitmap image;
        private System.Drawing.Bitmap sourceImage;
        private System.Drawing.Bitmap filteredImage;
        
        private String kapakRengi = "yok";
        private Boolean kapakSaglam = false;

        sc_islem  can_islem = new sc_islem();   

        public Form1()
        {
            InitializeComponent();
        }

        

        // Formu yüklerken
        private void Form1_Load(object sender, EventArgs e)
        {
            // Aforge görüntü işleme kodları - Video giriş kontrolleri
            CaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            foreach (FilterInfo Device in CaptureDevices)
            {
                comboBox1.Items.Add(Device.Name);
            }
            try
            {
                comboBox1.SelectedIndex = 0;
                button1.Enabled = true;
            }
            catch
            {
                MessageBox.Show("There is no Camera!");
            }
            
        }



        // Aforge görüntü işleme kodları - Yeni çerçeve
        void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FinalFrame = new VideoCaptureDevice(CaptureDevices[comboBox1.SelectedIndex].MonikerString);
            FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
            FinalFrame.Start();
            button2.Enabled = true;
            button4.Enabled = true;
        }


        private void ApplyFilter(IFilter filter)
        {
            // apply filter
            filteredImage = filter.Apply(sourceImage);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(button1.Enabled == true) if (FinalFrame.IsRunning) FinalFrame.Stop();           

            sourceImage = (Bitmap)pictureBox1.Image.Clone();
            ApplyFilter(new ResizeBicubic(256,192));
            pictureBox2.Image = filteredImage;

            sourceImage = filteredImage;
            ApplyFilter(new Median());
            pictureBox3.Image = filteredImage;
            
            sourceImage = filteredImage;
            // create filter
            HSLFiltering HSLfilter = new HSLFiltering();
            // set color ranges to keep
            HSLfilter.Hue = new IntRange(0, 400);
            HSLfilter.Saturation = new Range(0.2f, 1);
            HSLfilter.Luminance = new Range(0.1f, 1);
            // apply the filter
            ApplyFilter(HSLfilter);

            sourceImage = filteredImage;
            // and to picture box
            pictureBox4.Image = sourceImage;

             // UpdatePictureBoxPosition();

            if (button1.Enabled == true) FinalFrame.Start();
                          
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (button1.Enabled == true) if (FinalFrame.IsRunning) FinalFrame.Stop();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            openFileDialog1.Filter = "Tüm Resim Dosyaları|" + "*.bmp;*.jpg;*.gif;*.wmf;*.tif;*.png";
            openFileDialog1.DefaultExt = ".jpg";
            openFileDialog1.Title = "Resim Açma Ekranı";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                sourceImage = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);
                // check pixel format
                if ((sourceImage.PixelFormat == PixelFormat.Format16bppGrayScale) ||
                     (Bitmap.GetPixelFormatSize(sourceImage.PixelFormat) > 32))
                {
                    MessageBox.Show("The demo application supports only color images.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // free image
                    sourceImage.Dispose();
                    sourceImage = null;
                }
                else
                {
                    // make sure the image has 24 bpp format
                    if (sourceImage.PixelFormat != PixelFormat.Format24bppRgb)
                    {
                        Bitmap temp = AForge.Imaging.Image.Clone(sourceImage, PixelFormat.Format24bppRgb);
                        sourceImage.Dispose();
                        sourceImage = temp;
                    }
                }

                pictureBox1.Image = null;

                // display image
                pictureBox1.Image = sourceImage;
                button2.Enabled = true;
                button4.Enabled = true;
            }
        }


        // Conver list of AForge.NET's points to array of .NET points
        private AForge.Point[] ToPointsArray(List<IntPoint> points)
        {
            AForge.Point[] array = new AForge.Point[points.Count];

            for (int i = 0, n = points.Count; i < n; i++)
            {
                array[i] = new AForge.Point(points[i].X, points[i].Y);
            }

            return array;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            kapakRengi = "Yok";
            kapakSaglam = false;

            if (DaireMi((Bitmap)pictureBox4.Image.Clone()))
            {
                textBox2.Text = "Circle";
                int Hue=Convert.ToInt32(textBox1.Text);
                textBox1.ForeColor = System.Drawing.Color.White; 
                if ((Hue >= 330 && Hue < 361) || Hue < 30 ) {textBox1.Text = "RED"; textBox1.BackColor = System.Drawing.Color.Red;}
                else if (Hue >= 30 && Hue < 90) { textBox1.Text = "YELLOW"; textBox1.BackColor = System.Drawing.Color.Yellow; textBox1.ForeColor = System.Drawing.Color.Black;  }
                else if (Hue >= 90 && Hue < 150) { textBox1.Text = "GREEN"; textBox1.BackColor = System.Drawing.Color.Green; }
                else if (Hue >= 150 && Hue < 270) { textBox1.Text = "BLUE"; textBox1.BackColor = System.Drawing.Color.Blue; }
                else { textBox1.Text = Hue.ToString(); textBox1.BackColor = System.Drawing.Color.Black; }

            }

            else
            {
                textBox1.ForeColor = System.Drawing.Color.Black;
                textBox1.BackColor = System.Drawing.Color.WhiteSmoke;
                textBox1.Text = "-----";
                textBox2.Text = "NOT Circle";
            }



        }

        private Boolean DaireMi(Bitmap bitmap)
        {
            Boolean Daire = false;
            Bitmap bitmap1 = bitmap;

            // lock image
            BitmapData bitmapData = bitmap1.LockBits(
                new Rectangle(0, 0, bitmap1.Width, bitmap1.Height),
                ImageLockMode.ReadWrite, bitmap1.PixelFormat);

            // step 2 - locating objects
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 20;
            blobCounter.MinWidth = 20;

            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            bitmap1.UnlockBits(bitmapData);

            // step 3 - check objects' type and highlight
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            Graphics g = Graphics.FromImage(bitmap1);
            Pen yellowPen = new Pen(Color.Yellow, 2); // circles

            if (blobs.Length.ToString() != "0")
            {
                for (int i = 0, n = blobs.Length; i < n; i++)
                {
                    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                    AForge.Point center;
                    float radius;
                    // is circle ?
                    if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                    {
                        g.DrawEllipse(yellowPen,
                            (float)(center.X - radius), (float)(center.Y - radius),
                            (float)(radius * 2), (float)(radius * 2));
                        // MessageBox.Show("x:" + center.X.ToString() + "y:" + center.Y.ToString() + "r:" + radius.ToString());

                        

                        byte red = blobs[i].ColorMean.R ;
                        byte green = blobs[i].ColorMean.G;
                        byte blue = blobs[i].ColorMean.B;
                        int Hue = 0;
                        int max;
                        int min;
                        int ort;

                        ort = (red + green + blue) / 3;

                        max = Math.Max(red, Math.Max(green, blue));
                        min = Math.Min(red, Math.Min(green, blue));
                        if (max != min)
                        {
                            if (max == red && green >= blue) Hue = 60 * (green - blue) / (max - min) + 0;
                            if (max == red && green < blue) Hue = 60 * (green - blue) / (max - min) + 360;
                            if (max == green) Hue = 60 * (blue - red) / (max - min) + 120;
                            if (max == blue) Hue = 60 * (red - green) / (max - min) + 240;
                        }

                        textBox1.Text = Hue.ToString();

                        Daire = true;
                    }
                }
            }

            yellowPen.Dispose();
            g.Dispose();

            // put new image to clipboard
            //Clipboard.SetDataObject(bitmap);

            // and to picture box
            pictureBox5.Image = bitmap1;
            // UpdatePictureBoxPosition();

            return Daire;
        }

        private void SariKapakAra(Bitmap bitmap)
        {

            Bitmap bitmap1 = bitmap;

            // lock image
            BitmapData bitmapData = bitmap1.LockBits(
                new Rectangle(0, 0, bitmap1.Width, bitmap1.Height),
                ImageLockMode.ReadWrite, bitmap1.PixelFormat);

            // create filter
            HSLFiltering HSLfilter = new HSLFiltering();
            // set color ranges to keep
            HSLfilter.Hue = new IntRange(0, 120);
            HSLfilter.Saturation = new Range(0.2f, 1);
            HSLfilter.Luminance = new Range(0.1f, 1);
            // apply the filter
            HSLfilter.ApplyInPlace(bitmapData);

            // step 2 - locating objects
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 20;
            blobCounter.MinWidth = 20;

            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            bitmap1.UnlockBits(bitmapData);

            // step 3 - check objects' type and highlight
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            Graphics g = Graphics.FromImage(bitmap1);
            Pen yellowPen = new Pen(Color.Yellow, 2); // circles

            if (blobs.Length.ToString() != "0")
            {
                for (int i = 0, n = blobs.Length; i < n; i++)
                {
                    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);

                    AForge.Point center;
                    float radius;
                    kapakRengi = "Sarı";
                    // is circle ?
                    if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                    {
                        g.DrawEllipse(yellowPen,
                            (float)(center.X - radius), (float)(center.Y - radius),
                            (float)(radius * 2), (float)(radius * 2));
                        // MessageBox.Show("x:" + center.X.ToString() + "y:" + center.Y.ToString() + "r:" + radius.ToString());

                        kapakSaglam = true;
                    }
                    else
                    {
                        kapakSaglam = false;
                    }
                }
            }

            yellowPen.Dispose();
            g.Dispose();

            // put new image to clipboard
            //Clipboard.SetDataObject(bitmap);

            // and to picture box
            pictureBox4.Image = bitmap1;
            // UpdatePictureBoxPosition();
        }

        private void KirmiziKapakAra(Bitmap bitmap)
        {

            Bitmap bitmap1 = bitmap;

            // lock image
            BitmapData bitmapData = bitmap1.LockBits(
                new Rectangle(0, 0, bitmap1.Width, bitmap1.Height),
                ImageLockMode.ReadWrite, bitmap1.PixelFormat);

            // create filter
            HSLFiltering HSLfilter = new HSLFiltering();
            // set color ranges to keep
            HSLfilter.Hue = new IntRange(300, 30);
            HSLfilter.Saturation = new Range(0.2f, 1);
            HSLfilter.Luminance = new Range(0.1f, 1);
            // apply the filter
            HSLfilter.ApplyInPlace(bitmapData);

            // step 2 - locating objects
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 20;
            blobCounter.MinWidth = 20;

            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            bitmap1.UnlockBits(bitmapData);

            // step 3 - check objects' type and highlight
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            Graphics g = Graphics.FromImage(bitmap1);
            Pen yellowPen = new Pen(Color.Yellow, 2); // circles

            if (blobs.Length.ToString() != "0")
            {
                for (int i = 0, n = blobs.Length; i < n; i++)
                {
                    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);

                    AForge.Point center;
                    float radius;
                    kapakRengi = "Kırmızı";
                    // is circle ?
                    if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                    {
                        g.DrawEllipse(yellowPen,
                            (float)(center.X - radius), (float)(center.Y - radius),
                            (float)(radius * 2), (float)(radius * 2));
                        // MessageBox.Show("x:" + center.X.ToString() + "y:" + center.Y.ToString() + "r:" + radius.ToString());

                        kapakSaglam = true;
                    }
                    else
                    {
                        kapakSaglam = false;
                    }
                }
            }

            yellowPen.Dispose();
            g.Dispose();

            // put new image to clipboard
            //Clipboard.SetDataObject(bitmap);

            // and to picture box
            pictureBox4.Image = bitmap1;
            // UpdatePictureBoxPosition();
        }

        private void MaviKapakAra(Bitmap bitmap)
        {

            Bitmap bitmap1 = bitmap;

            // lock image
            BitmapData bitmapData = bitmap1.LockBits(
                new Rectangle(0, 0, bitmap1.Width, bitmap1.Height),
                ImageLockMode.ReadWrite, bitmap1.PixelFormat);           
            
            // create filter
            HSLFiltering HSLfilter = new HSLFiltering();
            // set color ranges to keep
            HSLfilter.Hue = new IntRange(0,400);
            HSLfilter.Saturation = new Range(0.2f, 1);
            HSLfilter.Luminance = new Range(0.1f, 1);
            // apply the filter
            HSLfilter.ApplyInPlace(bitmapData);
            
            // step 2 - locating objects
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 20;
            blobCounter.MinWidth = 20;

            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            bitmap1.UnlockBits(bitmapData);

            // step 3 - check objects' type and highlight
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            Graphics g = Graphics.FromImage(bitmap1);
            Pen yellowPen = new Pen(Color.Yellow, 2); // circles

            if (blobs.Length.ToString()!="0")
            {
                for (int i = 0, n = blobs.Length; i < n; i++)
                {
                    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);

                    AForge.Point center;
                    float radius;
                    kapakRengi = "Mavi";

                    // is circle ?
                    if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                    {
                        g.DrawEllipse(yellowPen,
                            (float)(center.X - radius), (float)(center.Y - radius),
                            (float)(radius * 2), (float)(radius * 2));
                        // MessageBox.Show("x:" + center.X.ToString() + "y:" + center.Y.ToString() + "r:" + radius.ToString());

                        kapakSaglam = true;
                    }
                    else
                    {
                        kapakSaglam = false;
                    }
                }
            }

            yellowPen.Dispose();
            g.Dispose();            

            // put new image to clipboard
            //Clipboard.SetDataObject(bitmap);
            
            // and to picture box
            pictureBox4.Image = bitmap1;
           // UpdatePictureBoxPosition();
        }

        

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            try
            {
                if (serialPort1.IsOpen)
                {
                    MessageBox.Show("Serial port  is already opened!");

                }
                else
                {
                    serialPort1.PortName = textBox3.Text;
                    serialPort1.BaudRate = 9600;
                    serialPort1.Open();                    
                }
                button5.Enabled = false;
                button6.Enabled = true;
                textBox5.ReadOnly = false;
                serialPort1.Write("O");
            }
            catch
            {
                MessageBox.Show("Serial port open error!");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serialPort1.IsOpen)
                {
                    MessageBox.Show("Serial port  is already closed!");
                }
                else
                {
                    serialPort1.Close();
                }
                button5.Enabled = true;
                button6.Enabled = false;
                textBox5.ReadOnly = true;
            }
            catch
            {
                MessageBox.Show("Serial port close error!");
            }
        }

        private void DisplayText(object sender, EventArgs e)
        {
            textBox5.AppendText(RxString);
            
            if (RxString == "M")
            {
                button2.PerformClick();
                button4.PerformClick();
                if (textBox2.Text == "NOT Circle")
                {
                    serialPort1.Write("X");
                }
                else
                {
                    if (textBox1.Text == "RED") serialPort1.Write("R");
                    if (textBox1.Text == "GREEN") serialPort1.Write("G");
                    if (textBox1.Text == "BLUE") serialPort1.Write("B");
                    if (textBox1.Text == "YELLOW") serialPort1.Write("Y");
                }
            }
            
        }

        

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen) serialPort1.Close();
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            RxString = serialPort1.ReadExisting();
            this.Invoke(new EventHandler(DisplayText));
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
