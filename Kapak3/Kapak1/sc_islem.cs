using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Kapak1
{
    class sc_islem
    {

        public unsafe Bitmap gri_tonlama_filtresi(Bitmap resim)
        {
            BitmapData bmpveri = resim.LockBits(new Rectangle(0, 0, resim.Width, resim.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte* p = (byte*)bmpveri.Scan0;

            for (int i = 0; i < bmpveri.Height; i++)
            {
                for (int j = 0; j < bmpveri.Width; j++)
                {
                    double ort = (p[0] + p[1] + p[2]) / 3;
                    p[0] = p[1] = p[2] = (byte)ort;
                    p += 4;
                }
            }

            resim.UnlockBits(bmpveri);
            return resim;
        }

        public unsafe Bitmap siyahlari_beyaz_yap_filtresi(Bitmap resim)
        {
            BitmapData bmpveri = resim.LockBits(new Rectangle(0, 0, resim.Width, resim.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte* p = (byte*)bmpveri.Scan0;

            for (int i = 0; i < bmpveri.Height; i++)
            {
                for (int j = 0; j < bmpveri.Width; j++)
                {
                    double ort = (p[0] + p[1] + p[2]) / 3;
                    if(ort<5) p[0] = p[1] = p[2] = 255;
                    p += 4;
                }
            }

            resim.UnlockBits(bmpveri);
            return resim;
        }

        public unsafe Bitmap siyah_beyaz_filtresi(Bitmap resim)
        {
            BitmapData bmpveri = resim.LockBits(new Rectangle(0, 0, resim.Width, resim.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte* p = (byte*)bmpveri.Scan0;

            int toplam_piksel = 0;

            for (int i = 0; i < bmpveri.Height; i++)
            {
                for (int j = 0; j < bmpveri.Width; j++)
                {
                    toplam_piksel+=p[0];
                    p += 4;
                }
            }
            toplam_piksel = (byte)(toplam_piksel / (bmpveri.Height * bmpveri.Width));

            p -= (resim.Width * resim.Height) * 4;
            for (int i = 0; i < bmpveri.Height; i++)
            {
                for (int j = 0; j < bmpveri.Width; j++)
                {
                    if (p[0] > 128)
                    {
                        p[0] = 0;
                    }
                    else
                    {
                        p[0] = 255;
                    }
                    p[1] = p[0];
                    p[2] = p[0];
                    p += 4;
                }
            }

            resim.UnlockBits(bmpveri);
            return resim;
        }

        public unsafe Bitmap gurultu_filtresi(Bitmap resim)
        {
            BitmapData bmpveri = resim.LockBits(new Rectangle(0, 0, resim.Width, resim.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte* p = (byte*)bmpveri.Scan0;

            byte[,] XY_piksel = new byte[resim.Height, resim.Width];
            short[,] XY_piksel_Yeni = new short[resim.Height, resim.Width];

            int i, j;

            for (i = 0; i < bmpveri.Height; i++)
            {
                for (j = 0; j < bmpveri.Width; j++)
                {
                    XY_piksel[i, j] = p[0];
                    XY_piksel_Yeni[i, j] = p[0];
                    p += 4;
                }
            }



            p -= (resim.Width * resim.Height) * 4;
            for (i = 0; i < bmpveri.Height; i++)
            {
                for (j = 0; j < bmpveri.Width; j++)
                {
                    if (XY_piksel_Yeni[i, j] == -1)
                    {
                        p[0] = 0;
                        p[1] = 255;
                        p[2] = 255;
                    }
                    p += 4;
                }
            }

            resim.UnlockBits(bmpveri);
            return resim;
        }

        


        
    }
}
