﻿using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System;
using System.Collections.Generic;

namespace MapCreation
{
    public partial class MainForm : Form
    {//TODO проверка на отсутствие белых пикселей на indoor-карте в зоне supposed пути (от scan0 до scan1) робота (пока что сектор)
        //Как рисовать мелкую картинку с BPP == 4, а не 3: в PS сохраняем как bmp, а настройки такие: Изображение->Режим->RGB, 8 бит/канал
        public MainForm()
        {
            InitializeComponent();
            Mode1ManualCrosslinking mode1ManualCrosslinking = new Mode1ManualCrosslinking(this);
            button1.MouseClick += button1_MouseClick;
            environment = new Environment(@"./Maps/PreciseMap1.png"); //default map
            updateLabel1Log();

            /*    preciseMap = new PixelMap("C:\\Adocuments\\Library\\Clapeyron_ind\\task6 map creation\\PreciseMap1.png");
                mouseMoveMap = new PixelMap(preciseMap);
                preciseIndoorMap = getIndoorMap(preciseMap);
                drawBitmapOnPictureBox(pictureBox1, preciseMap.GetBitmap());*/
        }

        /// <summary>
        /// Загружаем новую карту для environment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.RestoreDirectory = true;
                dlg.Title = "Open Image";
                dlg.Filter = "images (*.png;*.jpg;*bmp)|*.png;*.jpg;*bmp";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    String s = dlg.FileName;
                    environment = new Environment(s);
                    updateLabel1Log();
                }
                else { }
            }
        }

        public Environment environment;

        /*     private PixelMap preciseMap; //точная карта

             private PixelMap preciseIndoorMap; //точная карта indoor-среды: с отступами от препятствий точной карты

             private PixelMap mouseMoveMap; //DEBUG для отображения пикселя мышки

             private Scan scan0;
             private Scan scan1;

             private int X0 = -1, Y0 = -1; //0 scan center
             private int X1 = -1, Y1 = -1; //real 1 scan center
             private int X2 = -1, Y2 = -1; //supposed 1 scan center

             private byte positionCounter = 0;
             private double l_rl = 0; //[0,l_max]
             private int l_rl_rounded = 0;
             private int l_rl2 = 0; //[0,l_max^2]
             private double psi_rl_rad = 0; //in radians
             private double psi_rl_deg = 0; //in degrees
             private double l_rlPlus3sgm;
             private double l_rlMinus3sgm;

             private double l_sp = 0; //[0,l_max]
         //    private int l_rl_rounded = 0;
             private int l_sp2 = 0; //[0,l_max^2]
             private double psi_sp_rad = 0; //in radians
         //    private double psi_rl_deg = 0; //in degrees

             /// <summary>
             /// Обработчик кликов на pictureBox1 (где отрисована preciseMap).
             /// Клик обрабатывается только в случае попадания его в indoor-среду.
             /// Первый клик устанавливает центр нулевого скана
             /// Второй клик устанавливает центр первого скана: расстояние от центра scan1 до центра scan0 должно быть меньше l_max
             /// Третий клик устанавливает supposed центр скана 1: может быть установлен только в зоне погрешности скана 1
             /// </summary>
             /// <param name="sender"></param>
             /// <param name="e"></param>
             private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
             {
                 int X = e.X * preciseMap.Width / pictureBox1.Width;
                 int Y = e.Y * preciseMap.Height / pictureBox1.Height;

                 if (preciseIndoorMap[X, Y].Color == indoorColor)
                 {
                     Bitmap preciseMapBmp = preciseMap.GetBitmap();
                     Pen pen;
                     SolidBrush brush;
                     Graphics graphics = Graphics.FromImage(preciseMapBmp);
                     switch (positionCounter)
                     {
                         case 0:
                             X0 = X;
                             Y0 = Y;
                             scan0 = new Scan();
                             getScanFromPreciseMap(X, Y, scan0, startColor);
                             drawBitmapOnPictureBox(pictureBox2,scan0.getBitmap());
                             positionCounter++;
                             break;
                         case 1:
                             l_rl2 = getSquaredDistance(X0, Y0, X, Y);
                             l_rl = Math.Pow(l_rl2,0.5);
                             l_rl_rounded = (int)Math.Round(l_rl);
                             psi_rl_rad = getAngleRadian(X0, Y0, X, Y);
                             psi_rl_deg = psi_rl_rad*180/Math.PI;
                             if (l_rl2<=l_max2) {
                                 X1 = X;
                                 Y1 = Y;
                                 scan1 = new Scan();
                                 getScanFromPreciseMap(X, Y, scan1, finishColor);
                                 drawBitmapOnPictureBox(pictureBox3, scan1.getBitmap());
                                 positionCounter++;
                                 double sgm_lrl = l_rl * sgm_lmax / l_max;
                                 l_rlPlus3sgm = l_rl + 3 * sgm_lrl;
                                 l_rlMinus3sgm = l_rl - 3 * sgm_lrl;
                                 drawPieZone(preciseMapBmp, X0, Y0, X1, Y1);
                             }
                             break;
                         case 2:
                             //пока простая проверка: ровный разброс по углу и по длине
                             l_sp2 = getSquaredDistance(X0, Y0, X, Y);
                             l_sp = Math.Pow(l_sp2, 0.5);
                             psi_sp_rad = getAngleRadian(X0,Y0,X,Y);
                             bool angleFlag = false; //входит ли по угловой зоне
                             if ((psi_rl_rad > Math.PI / 2) && (psi_sp_rad < -Math.PI / 2))
                             {
                                 if ((psi_sp_rad + 2 * Math.PI >= psi_rl_rad - 3 * sgm_psi_rad) && (psi_sp_rad + 2 * Math.PI <= psi_rl_rad + 3 * sgm_psi_rad)) angleFlag = true;
                                 else angleFlag = false;
                             }
                             else
                             {
                                 if ((psi_rl_rad < -Math.PI / 2) && (psi_sp_rad > Math.PI / 2))
                                 {
                                     if ((psi_sp_rad - 2 * Math.PI >= psi_rl_rad - 3 * sgm_psi_rad) && (psi_sp_rad - 2 * Math.PI <= psi_rl_rad + 3 * sgm_psi_rad)) angleFlag = true;
                                     else angleFlag = false;
                                 }
                                 else
                                 {
                                     if ((psi_sp_rad >= psi_rl_rad - 3 * sgm_psi_rad) && (psi_sp_rad <= psi_rl_rad + 3 * sgm_psi_rad)) angleFlag = true;
                                 }
                             }
                             if (((l_sp >= l_rlMinus3sgm) && (l_sp <= l_rlPlus3sgm)) && angleFlag)
                             {
                                 X2 = X;
                                 Y2 = Y;
                                 positionCounter = 0;
                             }
                             else
                                 drawPieZone(preciseMapBmp, X0, Y0, X1, Y1);
                             break;
                     }
                     //отрисовать все три центра
                     //отрисовать радиусы для scan0 и scan1
                     try
                     {
                         if ((X0 >= 0) && (Y0 >= 0))
                         {
                             preciseMapBmp.SetPixel(X0, Y0, startColor);
                             pen = new Pen(startColor);
                             graphics.DrawEllipse(pen, X0 - r_scan, Y0 - r_scan, d_scan, d_scan);
                             graphics.DrawEllipse(pen, X0 - r_robot, Y0 - r_robot, d_robot, d_robot);
                         }
                         if ((X1 >= 0) && (Y1 >= 0))
                         {
                             preciseMapBmp.SetPixel(X1, Y1, finishColor);
                             pen = new Pen(finishColor);
                             graphics.DrawEllipse(pen, X1 - r_scan, Y1 - r_scan, d_scan, d_scan);
                             graphics.DrawEllipse(pen, X1 - r_robot, Y1 - r_robot, d_robot, d_robot);
                         }
                         if ((X2 >= 0) && (Y2 >= 0))
                         {
                             preciseMapBmp.SetPixel(X2, Y2, routeColor);
                         }
                     }
                     catch (Exception ex) { }
                     mouseMoveMap = new PixelMap(preciseMapBmp);
                     drawBitmapOnPictureBox(pictureBox1, preciseMapBmp);
                 }
             }

             //For mouseMove pixel:
             private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
             {
                 int X = e.X * preciseMap.Width / pictureBox1.Width;
                 int Y = e.Y * preciseMap.Height / pictureBox1.Height;
                 Bitmap bmp = new Bitmap(mouseMoveMap.GetBitmap());
                 bmp.SetPixel(X,Y,mouseMoveColor);
                 drawBitmapOnPictureBox(pictureBox1,bmp);
             }

             private void button1_Click(object sender, EventArgs e)
             {
                 if ((positionCounter==0)&&(X2>=0))
                 {
                     int[] real_coords = getRealCoords4();
                     Bitmap bmp = new Bitmap(pictureBox1.Image);
                     bmp.SetPixel(real_coords[0], real_coords[1], predictionColor);
                     pictureBox1.Image = bmp; //TODO
                     drawCrosslinkedScans(real_coords[0], real_coords[1]);
                 }
             }

             /// <summary>
             /// Возвращает скан с точной карты в заданных координатах. Также рисует этот скан в заданном PixelMap.
             /// </summary>
             /// <param name="X">X координата центра скана на точной карте</param>
             /// <param name="Y">Y координата центра скана на точной карте</param>
             /// <param name="scanBmp">Холст для отрисовки сделанного скана</param>
             /// <returns></returns>
             private void getScanFromPreciseMap(int X, int Y, Scan scan, Color scanColor)
             {
                 int y = new int();
                 int x = new int();
                 bool flagR; //Будет true, если на текущем угле сканирования видно препятствие, иначе false и радиус от текущего угла будет равен нулю
                 bool flagRepeated; //Будет true, если точка уже сохранена в списке скана

                 for (int i = 0; i < n_phi; i++)
                 {
                     flagR = false;
                     for (ushort r = 1; r < r_scan+1; r++)
                     {
                         x = (int)Math.Round(r * Math.Cos(i * step));
                         y = (int)Math.Round(r * Math.Sin(i * step));
                         if (preciseMap[x+X, y+Y].Color == wallColor)
                         {
                             scan.rByPhi[i] = r;
                             flagRepeated = false;
                             for (int j = 0; j < scan.xyScan.Count; j++)
                                 if ((scan.xyScan[j][0] == x) && (scan.xyScan[j][1] == y))
                                     flagRepeated = true;
                             if (!flagRepeated)
                                 scan.xyScan.Add(new int[2] { x, y });
                             scan.scanBmp[x + r_scan, y + r_scan] = new Pixel(scanColor);
                             flagR = true;
                             break;
                         }
                     }
                     if (!flagR)
                         scan.rByPhi[i] = 0;
                 }
             }

             /// <summary>
             /// Возвращает карту indoor-среды для заданной карты.
             /// </summary>
             /// <param name="map"></param>
             /// <returns></returns>
             private PixelMap getIndoorMap(PixelMap map)
             {
                 Bitmap preciseIndoorMapBmp = map.GetBitmap();
                 Pen pen = new Pen(wallColor);
                 SolidBrush brush = new SolidBrush(wallColor);
                 Graphics graphics = Graphics.FromImage(preciseIndoorMapBmp);
                 for (int i = 0; i < map.Width; i++)
                 {
                     for (int j = 0; j < map.Height; j++)
                     {
                         if (map[i,j].Color == wallColor)
                         {
                             try
                             {
                                 FillCircle(ref graphics,ref pen,ref brush, r_robot, d_robot,ref i,ref j);
                             }
                             catch (Exception ex) { }
                         }
                     }
                 }
             //    preciseIndoorMapBmp.Save("C:\\Adocuments\\Library\\Clapeyron_ind\\task6 map creation\\PreciseIndoorMap13.png");
                 PixelMap preciseIndoorMap = new PixelMap(preciseIndoorMapBmp);
                 return preciseIndoorMap;
             }

             /// <summary>
             /// Рисует зону, в которой может быть supposed положение робота относительно real положения в центре scan1.
             /// Зона рисуется примерная, к сожалению.
             /// </summary>
             /// <param name="graphics"></param>
             private void drawPieZone (Graphics graphics)
             {
                 if (l_rl > 0)
                 {
                     //отрисовать зону погрешности, в которую попадает supposed центр
                     double sgm_lrl = l_rl * sgm_lmax / l_max; //вычисляем погрешность передвижения, считая зависимость погрешности от пройденного расстояния линейной
                     l_rlPlus3sgm = l_rl + 3 * sgm_lrl;
                     l_rlMinus3sgm = l_rl - 3 * sgm_lrl;
                     int lplus3sgmInt = (int)Math.Round(l_rlPlus3sgm);
                     int lminus3sgmInt = (int)Math.Round(l_rlMinus3sgm);
                     SolidBrush brush = new SolidBrush(routeColor);
                     Pen pen = new Pen(routeColor);
                     graphics.FillPie(brush, X0 - lplus3sgmInt, Y0 - lplus3sgmInt, 2 * lplus3sgmInt, 2 * lplus3sgmInt, (float)psi_rl_deg - 3 * sgm_psi_deg, 6 * sgm_psi_deg);
                     brush = new SolidBrush(indoorColor);
                     graphics.FillPie(brush, X0 - lminus3sgmInt, Y0 - lminus3sgmInt, 2 * lminus3sgmInt, 2 * lminus3sgmInt, (float)psi_rl_deg - 3 * sgm_psi_deg, 6 * sgm_psi_deg);
                 }
             }

             private void drawPieZone(Bitmap bmp, int X1, int Y1, int X2, int Y2)
             {
                 List<int[]> pieZone = pieErrorZoneSearch(X1, Y1, X2, Y2);
                 for (int i = 0; i < pieZone.Count; i++)
                 {
                     bmp.SetPixel(pieZone[i][0], pieZone[i][1], routeColor);
                 }
             }

             /// <summary>
             /// Думаем, что центр scan1 - это X2,Y2. Нужно вернуть X1,Y1.
             /// </summary>
             /// <returns></returns>
             private int[] getRealCoords3()
             {
                 List<int[]> across0 = new List<int[]>(); //точки из пересечения сканов
                 List<int[]> across1 = new List<int[]>();
                 List<int[]> pointsLess, pointsMore;
                 double minsum = 100000000;
                 int limXY = 10;
                 double summ;
                 double min;
                 int optX = 0, optY = 0;

                 for (int x = -limXY; x < limXY + 1; x++)
                 {
                     for (int y = -limXY; y < limXY + 1; y++)
                     {
                         computeAcrossingPoints(ref across0, ref across1, X2+x, Y2+y);
                         if (across0.Count < across1.Count)
                         {
                             pointsLess = across0;
                             pointsMore = across1;
                         }
                         else
                         {
                             pointsLess = across1;
                             pointsMore = across0;
                         }
                         summ = 0;
                         for (int k = 0; k < across1.Count; k++)
                         {
                             across1[k][0] = across1[k][0] + (X2 - X0) + x;
                             across1[k][1] = across1[k][1] + (Y2 - Y0) + y;
                         }
                         for (int i = 0; i < pointsLess.Count; i++)
                         {
                             min = 1000000;
                             for (int j = 0; j < pointsMore.Count; j++)
                             {
                                 if (min > getSquaredDistance(pointsLess[i][0], pointsLess[i][1], pointsMore[j][0], pointsMore[j][1]))
                                     min = getSquaredDistance(pointsLess[i][0], pointsLess[i][1], pointsMore[j][0], pointsMore[j][1]);
                             }
                             summ += min;
                         }
                         for (int k = 0; k < across1.Count; k++)
                         {
                             across1[k][0] = across1[k][0] - (X2 - X0) - x;
                             across1[k][1] = across1[k][1] - (Y2 - Y0) - y;
                         }
                         Console.WriteLine("(" + x + "," + y + "): " + summ);
                         if (minsum > summ)
                         {
                             minsum = summ;
                             optX = x;
                             optY = y;
                         }
                     }
                 }
                 scan1.setCenter(X2+optX,Y2+optY);
                 Console.WriteLine("minsum: "+minsum);
                 Console.WriteLine("opt coords: "+optX+","+optY);
                 Console.WriteLine("center1-center2 error: " + (X1-X2) + "," + (Y1-Y2));
                 return new int[2]{X2+optX,Y2+optY};
             }

             /// <summary>
             /// Добавляет в списки точки из пересечения сканов scan0 и scan1, при этом за центр последнего берется задаваемый X_sp и Y_sp.
             /// </summary>
             /// <param name="across0"></param>
             /// <param name="across1"></param>
             /// <param name="X_sp">Предполагаемый центр scan1</param>
             /// <param name="Y_sp">Предполагаемый центр scan1</param>
             private void computeAcrossingPoints(ref List<int[]> across0, ref List<int[]> across1, int X_sp, int Y_sp)
             {
                 across0.Clear();
                 across1.Clear();
                 int x0, y0, x1, y1;
                 for (int i = 0; i < scan0.xyScan.Count; i++)
                 {
                     x0 = scan0.xyScan[i][0];
                     y0 = scan0.xyScan[i][1];
                     if ((getSquaredDistance(x0+X0, y0+Y0, X_sp, Y_sp) <= r_scan2))
                         across0.Add(new int[2] { x0, y0 });
                 }
                 for (int i = 0; i < scan1.xyScan.Count; i++)
                 {
                     x1 = scan1.xyScan[i][0];
                     y1 = scan1.xyScan[i][1];
                     if ((getSquaredDistance(x1+X_sp, y1+Y_sp, X0, Y0) <= r_scan2))
                         across1.Add(new int[2] { x1, y1 });
                 }
             }


             private int[] getRealCoords4()
             {
                 double minsum = 100000000;
                 int limXY = 20;
                 double summ;
                 double min;
                 int optX = 0, optY = 0;
                 int C = (d_scan + r_scan) / 2;
                 for (int x = -limXY; x < limXY + 1; x++)
                 {
                     for (int y = -limXY; y < limXY + 1; y++)
                     {
                         PixelMap map01 = new PixelMap(d_scan1 + r_scan, d_scan1 + r_scan, 0, 0, 0);
                         List<int[]> irrelevantPoints0 = new List<int[]>();
                         List<int[]> irrelevantPoints1 = new List<int[]>();
                         int X = X2 - X0 + x;
                         int Y = Y2 - Y0 + y;
                         for (int i = 0; i < scan0.xyScan.Count; i++)
                         {
                             map01[scan0.xyScan[i][0] + C, scan0.xyScan[i][1] + C] = new Pixel(wallColor);
                         }
                         for (int i = 0; i < scan1.xyScan.Count; i++)
                         {
                             map01[scan1.xyScan[i][0] + X + C, scan1.xyScan[i][1] + Y + C] = new Pixel(wallColor);
                         }
                         int y1 = new int();
                         int x1 = new int();
                         bool flagR; //Будет true, если на текущем угле сканирования видно препятствие, иначе false и радиус от текущего угла будет равен нулю
                         bool flagRepeated; //Будет true, если точка уже сохранена в списке скана
                         int rPhi = -1;

                         for (int i = 0; i < n_phi; i++)
                         {
                             //---------------------------------------scan1
                             flagR = false;
                             for (ushort r = 1; r < r_scan + 1; r++)
                             {
                                 x1 = (int)Math.Round(r * Math.Cos(i * step));
                                 y1 = (int)Math.Round(r * Math.Sin(i * step));
                                 if (map01[x1 + X + C, y1 + Y + C].Color == wallColor)
                                 {
                                     rPhi = r;
                                     flagR = true;
                                     break;
                                 }
                             }
                             if (!flagR)
                                 rPhi = 0;
                             if (rPhi == -1) Console.WriteLine("Scanning problems r == -1 on scan1, angle: " + i * step / Math.PI * 180);
                             if (scan1.rByPhi[i] != rPhi)
                             {
                                 flagRepeated = false;
                                 for (int j = 0; j < irrelevantPoints1.Count; j++)
                                     if ((irrelevantPoints1[j][0] == x1) && (irrelevantPoints1[j][1] == y1))
                                         flagRepeated = true;
                                 if (!flagRepeated)
                                     irrelevantPoints1.Add(new int[2] { x1, y1 });
                             }
                             //---------------------------------------scan0
                             flagR = false;
                             for (ushort r = 1; r < r_scan + 1; r++)
                             {
                                 x1 = (int)Math.Round(r * Math.Cos(i * step));
                                 y1 = (int)Math.Round(r * Math.Sin(i * step));
                                 if (map01[x1 + C, y1 + C].Color == wallColor)
                                 {
                                     rPhi = r;
                                     flagR = true;
                                     break;
                                 }
                             }
                             if (!flagR)
                                 rPhi = 0;
                             if (rPhi == -1) Console.WriteLine("Scanning problems r == -1 on scan0, angle: " + i * step / Math.PI * 180);
                             if (scan0.rByPhi[i] != rPhi)
                             {
                                 flagRepeated = false;
                                 for (int j = 0; j < irrelevantPoints0.Count; j++)
                                     if ((irrelevantPoints0[j][0] == x1) && (irrelevantPoints0[j][1] == y1))
                                         flagRepeated = true;
                                 if (!flagRepeated)
                                     irrelevantPoints0.Add(new int[2] { x1, y1 });
                             }
                         }

                         summ = 0;
                         for (int i = 0; i < irrelevantPoints1.Count; i++)
                         {
                             min = 1000000;
                             for (int j = 0; j < scan1.xyScan.Count; j++)
                             {
                                 if (min > getSquaredDistance(irrelevantPoints1[i][0], irrelevantPoints1[i][1], scan1.xyScan[j][0], scan1.xyScan[j][1]))
                                     min = getSquaredDistance(irrelevantPoints1[i][0], irrelevantPoints1[i][1], scan1.xyScan[j][0], scan1.xyScan[j][1]);
                             }
                             summ += min;
                         }
                         for (int i = 0; i < irrelevantPoints0.Count; i++)
                         {
                             min = 1000000;
                             for (int j = 0; j < scan0.xyScan.Count; j++)
                             {
                                 if (min > getSquaredDistance(irrelevantPoints0[i][0], irrelevantPoints0[i][1], scan0.xyScan[j][0], scan0.xyScan[j][1]))
                                     min = getSquaredDistance(irrelevantPoints0[i][0], irrelevantPoints0[i][1], scan0.xyScan[j][0], scan0.xyScan[j][1]);
                             }
                             summ += min;
                         }
                         if (minsum > summ)
                         {
                             minsum = summ;
                             optX = x;
                             optY = y;
                         }
                     }
                 }
                 return new int[2] { X2 + optX, Y2 + optY };
             }

             /// <summary>
             /// Рисует сшитые сканы на одном холсте.
             /// </summary>
             /// <param name="X1_rl"></param>
             /// <param name="Y1_rl"></param>
             private void drawCrosslinkedScans(int X1_rl, int Y1_rl)
             {
                 PixelMap scan01 = new PixelMap(d_scan1+r_scan, d_scan1 + r_scan,0,0,0);
                 for (int i = 0; i < scan0.xyScan.Count; i++)
                 {
                     scan01[scan0.xyScan[i][0] + (d_scan + r_scan) / 2, scan0.xyScan[i][1]+(d_scan + r_scan) / 2] = new Pixel(startColor);
                 }
                 for (int i = 0; i < scan1.xyScan.Count; i++)
                 {
                     scan01[scan1.xyScan[i][0] + X1_rl - X0 + (d_scan + r_scan) / 2, scan1.xyScan[i][1] + Y1_rl - Y0 + (d_scan + r_scan) / 2] = new Pixel(finishColor);
                 }
                 drawBitmapOnPictureBox(pictureBox4,scan01.GetBitmap());
             }

             private void drawBitmapOnPictureBox(PictureBox pictureBox, Bitmap bmp)
             {
                 if (pictureBox.Size != bmp.Size)
                 {
                     float zoom = 60.0f;
                     Bitmap zoomed = new Bitmap((int)(bmp.Width * zoom), (int)(bmp.Height * zoom));

                     using (Graphics g = Graphics.FromImage(zoomed))
                     {
                         g.InterpolationMode = InterpolationMode.NearestNeighbor;
                         g.PixelOffsetMode = PixelOffsetMode.Half;
                         g.DrawImage(bmp, new Rectangle(Point.Empty, zoomed.Size));
                     }
                     pictureBox.Image = zoomed;
                 }
                 else
                     pictureBox.Image = bmp;
             }

             /// <summary>
             /// Заполняет круг радиуса r, работает правильно. d должно быть равно 2r (для скорости).
             /// pen и brush должны быть одного цвета.
             /// Метод скоростной.
             /// </summary>
             /// <param name="graphics"></param>
             /// <param name="r">радиус круга</param>
             /// <param name="d">2r</param>
             /// <param name="X">центр круга</param>
             /// <param name="Y">центр круга</param>
             /// <param name="color">заливка и граница круга</param>
             private static void FillCircle(ref Graphics graphics, ref Pen pen, ref SolidBrush brush, int r, int d, ref int X, ref int Y)
             {
                 graphics.DrawEllipse(pen, X - r, Y - r, d, d);
                 graphics.FillEllipse(brush, X - r, Y - r, d, d);
             }

             private List<int[]> pieErrorZoneSearch(int X1, int Y1, int X2, int Y2)
             {
                 List<int[]> pieErrorZone = new List<int[]>();
                 double l2 = getSquaredDistance(X1,Y1,X2,Y2);
                 double l = Math.Pow(l2,0.5);
                 double lminus3sgm = l - 3 * getSgm_l(l);
                 double lplus3sgm = l + 3*getSgm_l(l);

                 int searchingSquareHalfSide = (int)Math.Ceiling(Math.Pow(Math.Pow(lplus3sgm*3*sgm_psi_rad,2)+ Math.Pow(3*getSgm_l(l), 2), 0.5));
                 Console.WriteLine("searchingSquareHalfSide " + searchingSquareHalfSide);

                 for (int x = -searchingSquareHalfSide; x <= searchingSquareHalfSide; x++)
                     for (int y = -searchingSquareHalfSide; y <= searchingSquareHalfSide; y++)
                         if (isPointInPieZone(X0, Y0, X2 + x, Y2 + y))
                             pieErrorZone.Add(new int[2] { X2 + x, Y2 + y });

                 return pieErrorZone;
             }

             private bool isPointInPieZone(int X1, int Y1, int X2, int Y2)
             {
                 l_sp2 = getSquaredDistance(X1, Y1, X2, Y2);
                 l_sp = Math.Pow(l_sp2, 0.5);
                 psi_sp_rad = getAngleRadian(X1, Y1, X2, Y2);
                 bool angleFlag = false; //входит ли по угловой зоне
                 if ((psi_rl_rad > Math.PI / 2) && (psi_sp_rad < -Math.PI / 2))
                 {
                     if ((psi_sp_rad + 2 * Math.PI >= psi_rl_rad - 3 * sgm_psi_rad) && (psi_sp_rad + 2 * Math.PI <= psi_rl_rad + 3 * sgm_psi_rad)) angleFlag = true;
                     else angleFlag = false;
                 }
                 else
                 {
                     if ((psi_rl_rad < -Math.PI / 2) && (psi_sp_rad > Math.PI / 2))
                     {
                         if ((psi_sp_rad - 2 * Math.PI >= psi_rl_rad - 3 * sgm_psi_rad) && (psi_sp_rad - 2 * Math.PI <= psi_rl_rad + 3 * sgm_psi_rad)) angleFlag = true;
                         else angleFlag = false;
                     }
                     else
                     {
                         if ((psi_sp_rad >= psi_rl_rad - 3 * sgm_psi_rad) && (psi_sp_rad <= psi_rl_rad + 3 * sgm_psi_rad)) angleFlag = true;
                     }
                 }
                 if (((l_sp >= l_rlMinus3sgm) && (l_sp <= l_rlPlus3sgm)) && angleFlag)
                     return true;
                 else
                     return false;
             }*/
    }
}