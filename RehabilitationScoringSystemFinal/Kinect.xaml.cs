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
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Windows.Threading;
using System.Windows.Forms;//sendkey  include system.windows.form.dll

///use excel
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;

using System.Diagnostics;

namespace RehabilitationScoringSystemFinal
{
    /// <summary>
    /// Kinect.xaml 的互動邏輯
    /// </summary>
    public partial class Kinect : Window
    {
        skeleton skwindow_ref = null;

        KinectSensor myKinect;
        ColorImagePoint cpH, cpSC, cpSR, cpSL, cpER, cpEL, cpWR, cpWL, cpHR, cpHL,cpSP;
        ColorImagePoint cpCH, cpRH, cpLH, cpRK, cpLK, cpRA, cpLA, cpRF, cpLF;
        Joint jH, jSC, jSR, jSL, jER, jEL, jWR, jWL, jHR, jHL,jSP;
        Joint jCH, jRH, jLH, jRK, jLK, jRA, jLA, jRF, jLF;
        //jH=2, jSC=3, jSR=4, jSL=5, jER=6, jEL=7, jWR=8, jWL=9,jSP=10, jHR=0, jHL=1;
        //CH=11,RH=12,LH=13,RK=14,LK=15,RA=16,LA=17,RF=18,LF=19
        int detectionX = 570, detectionY = 70,waittime=0,starttime=0;

        int check_time = 0, start_time = 0;
        int[,] checkpointX = new int[10000, 20];//確認座標   
        int[,] checkpointY = new int[10000, 20];

        int standard_count = 0;
        double standard_time = 0;//標準動作執行時間
        double[,] standardX = new double[10000, 20];//標準動作
        double[,] standardY = new double[10000, 20];
        double[,] standardZ = new double[10000, 20];
        double[,] standardT = new double[10000, 20];

        double[,] normalstandardX = new double[10000, 20];//標準動作
        double[,] normalstandardY = new double[10000, 20];
        double[,] normalstandardZ = new double[10000, 20];

        int self_count = 0;
        double self_time = 0;//參考動作執行時間
        double[,] selfX = new double[10000, 20];//參考動作
        double[,] selfY = new double[10000, 20];
        double[,] selfZ = new double[10000, 20];
        double[,] selfT = new double[10000, 20];

        double[,] normalselfX = new double[10000, 20];//參考動作
        double[,] normalselfY = new double[10000, 20];
        double[,] normalselfZ = new double[10000, 20];

        //重複內插用
        double[,] interpolationX = new double[10000, 20];
        double[,] interpolationY = new double[10000, 20];
        double[,] interpolationZ = new double[10000, 20];

        double[,] averagingX = new double[10000, 20];
        double[,] averagingY = new double[10000, 20];
        double[,] averagingZ = new double[10000, 20];
        //

        //計算結果用
        double[,] calculateFORstandardX = new double[10000, 20];
        double[,] calculateFORstandardY = new double[10000, 20];
        double[,] calculateFORstandardZ = new double[10000, 20];

        double[,] calculateFORselfX = new double[10000, 20];
        double[,] calculateFORselfY = new double[10000, 20];
        double[,] calculateFORselfZ = new double[10000, 20];
        //

        double[]  vectorScore= new double[20];
        double[] coordinateScore = new double[20];
        double[] DTWvectorScore = new double[20];
        double[] DTWcoordinateScore = new double[20];
        double[] SDTWvectorScore = new double[20];
        double[] SDTWcoordinateScore = new double[20];
        //分數轉換用


       /* double xfar_max = -999;
        double xfar_min =999;
        double yfar_max = -999;
        double yfar_min = 999;
        double zfar_max = -999;
        double zfar_min = 999;*/

        double[] xfar_max = { -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999 };
        double[] xfar_min = { 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999 };
        double[] yfar_max = { -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999 };
        double[] yfar_min = { 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999 };
        double[] zfar_max = { -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999 };
        double[] zfar_min = { 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999 };

        double[,] standardfarX = new double[10000, 20];//標準最遠
        double[,] standardfarY = new double[10000, 20];
        double[,] standardfarZ = new double[10000, 20];
        
        //==畫線==
        private const int MoveThreshold = 1;  //有效移動量門檻
        private ColorImagePoint _beginPoint;  //線段起點
        Brush _brushColor = Brushes.Black;
        bool _draw = false;
        int colorcheck = 0;
        //==畫線==

        //DTW用
        double[,] DTW = new double[1000, 1000];
        double[,] DTW_dis = new double[1000, 1000];
        double[,] DTW_ang = new double[1000, 1000];
        //
        //DTWSLT用
        
        double slottedTime = 0;
        
        //

        //執行時間
        double timeRI = 0, timeV = 0, timeC = 0, timeDV = 0, timeDC = 0, timeDSV = 0, timeDSC = 0;

        DispatcherTimer tm1 = new DispatcherTimer();
        DispatcherTimer tm2 = new DispatcherTimer();
        
        public Kinect(skeleton temp)
        {
            skwindow_ref = temp;
            InitializeComponent();
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            

        }
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    break;
                case KinectStatus.Disconnected:
                    break;
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                System.Windows.MessageBox.Show("請將kinect接到電腦");
                

            }
            else if (KinectSensor.KinectSensors[0].Status == KinectStatus.Connected)
            {
                this.myKinect = KinectSensor.KinectSensors[0];
                InitialStream();
            }
        }
        private void InitialStream()
        {

            this.myKinect.ColorStream.Enable();//要求Kinect感應器產生資料串流
            this.myKinect.ColorFrameReady += Kinect_ColorFrameReady;

            this.myKinect.SkeletonStream.Enable();
            this.myKinect.SkeletonFrameReady += Kinect_SkeletonFrameReady;


            this.myKinect.Start();//啟動Kinect感應器硬體

           
            tm1.Tick += new EventHandler(tm1Tick_point);
            tm1.Interval = TimeSpan.FromSeconds(double.Parse(text_time.Text));

            tm2.Tick += new EventHandler(tm2Tick_point);
            tm2.Interval = TimeSpan.FromSeconds(1);

            //myKinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            /*
            tmfordraw.Tick += new EventHandler(tmfordrawTick_point);
            tmfordraw.Interval = TimeSpan.FromSeconds(double.Parse(time1_second.Text));*/

        }
        private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frameData = e.OpenColorImageFrame())
            {
                if (frameData != null)
                {
                    byte[] imageDataByte = new byte[frameData.PixelDataLength];
                    frameData.CopyPixelDataTo(imageDataByte);
                    colorImage.Source = BitmapSource.Create(frameData.Width, frameData.Height, 96, 96,
                        PixelFormats.Bgr32, null, imageDataByte, frameData.Width * frameData.BytesPerPixel);
                }
            }
        }
        private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skframe = e.OpenSkeletonFrame())
            {
                if (skframe != null)
                {
                    Skeleton[] FrameSkeletons = new Skeleton[skframe.SkeletonArrayLength];
                    skframe.CopySkeletonDataTo(FrameSkeletons);



                    for (int i = 0; i < FrameSkeletons.Length; i++)
                    {
                        if (FrameSkeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                        {
                            jH = FrameSkeletons[i].Joints[JointType.Head];
                            jSC = FrameSkeletons[i].Joints[JointType.ShoulderCenter];
                            jSR = FrameSkeletons[i].Joints[JointType.ShoulderRight];
                            jSL = FrameSkeletons[i].Joints[JointType.ShoulderLeft];
                            jER = FrameSkeletons[i].Joints[JointType.ElbowRight];
                            jEL = FrameSkeletons[i].Joints[JointType.ElbowLeft];
                            jWR = FrameSkeletons[i].Joints[JointType.WristRight];
                            jWL = FrameSkeletons[i].Joints[JointType.WristLeft];
                            jHR = FrameSkeletons[i].Joints[JointType.HandRight];
                            jHL = FrameSkeletons[i].Joints[JointType.HandLeft];
                            jSP = FrameSkeletons[i].Joints[JointType.Spine];
                            jCH = FrameSkeletons[i].Joints[JointType.HipCenter];
                            jRH = FrameSkeletons[i].Joints[JointType.HipRight];
                            jLH = FrameSkeletons[i].Joints[JointType.HipLeft];
                            jRK = FrameSkeletons[i].Joints[JointType.KneeRight];
                            jLK = FrameSkeletons[i].Joints[JointType.KneeLeft];
                            jRA = FrameSkeletons[i].Joints[JointType.AnkleRight];
                            jLA = FrameSkeletons[i].Joints[JointType.AnkleLeft];
                            jRF=FrameSkeletons[i].Joints[JointType.FootRight];
                            jLF = FrameSkeletons[i].Joints[JointType.HandLeft];

                            cpH = MapToColorImage(FrameSkeletons[i].Joints[JointType.Head]);//頭
                            cpSC = MapToColorImage(FrameSkeletons[i].Joints[JointType.ShoulderCenter]);//肩膀中間
                            cpSR = MapToColorImage(FrameSkeletons[i].Joints[JointType.ShoulderRight]);//右肩
                            cpSL = MapToColorImage(FrameSkeletons[i].Joints[JointType.ShoulderLeft]);//左肩
                            cpER = MapToColorImage(FrameSkeletons[i].Joints[JointType.ElbowRight]);//右手肘
                            cpEL = MapToColorImage(FrameSkeletons[i].Joints[JointType.ElbowLeft]);//左手肘
                            cpWR = MapToColorImage(FrameSkeletons[i].Joints[JointType.WristRight]);//右手腕
                            cpWL = MapToColorImage(FrameSkeletons[i].Joints[JointType.WristLeft]);//左手腕
                            cpHR = MapToColorImage(FrameSkeletons[i].Joints[JointType.HandRight]);//右手
                            cpHL = MapToColorImage(FrameSkeletons[i].Joints[JointType.HandLeft]);//左手
                            cpRK = MapToColorImage(FrameSkeletons[i].Joints[JointType.KneeRight]);//


                            DrawHead(cpH);
                            DrawHandRight(cpHR);
                            //DrawKneeRight(cpRK);
                            /* DrawShoulderCenter(cpSC);
                             DrawShoulderRight(cpSR);
                             DrawShoulderLeft(cpSL);
                             DrawElbowRight(cpER);
                             DrawElbowLeft(cpEL);
                             DrawWristRight(cpWR);
                             DrawWristLeft(cpWL);
                            
                             DrawHandLeft(cpHL);*/

                            if (cpHR.X > detectionX && cpHR.Y < detectionY)
                            {
                                if (colorcheck != 0)
                                {
                                    reset();
                                    colorcheck++;
                                    tm1.Start();
                                    tm2.Start();
                                    Console.Beep();
                                    detectionX = 1000;
                                    detectionY = 1000;
                                }
                                else
                                {
                                    tm1.Start();
                                    tm2.Start();
                                    Console.Beep();
                                    detectionX = 1000;
                                    detectionY = 1000;
                                }
                                
                                   
                                
                            }

                            if (_draw)
                            {
                                DrawingLine();  //呼叫線段建立函式
                            }
                            //0105
                            else
                                _beginPoint.X = -1;       //恢復尚未設定起點狀態*/
                        }
                    }
                }
            }
        }

        private void tm1Tick_point(object sender, EventArgs e)
        {

            check_time++;
            checkpointX[check_time, 0] = cpHR.X;
            checkpointY[check_time, 0] = cpHR.Y;

            double distance = System.Math.Sqrt((cpHR.X - checkpointX[check_time - 1, 0]) * (cpHR.X - checkpointX[check_time - 1, 0]) +
                (cpHR.Y - checkpointY[check_time - 1, 0]) * (cpHR.Y - checkpointY[check_time - 1, 0]));

            text_distance.Text = distance.ToString();

            if (starttime >= 5)
            {
                _draw = true;
            }

            if (distance >= 2)
                waittime = 0;

            /*if (starttime == 5)
                Console.Beep();*/

            if (distance > 0)
            {
                //start_time++;
                //text_starttime.Text = start_time.ToString();
                if (_draw == true)
                {

                    if (colorcheck == 0)
                    {
                        standard_time += double.Parse(text_time.Text);
                        //記錄標準動作
                        standardX[standard_count, 0] = jHR.Position.X;
                        standardY[standard_count, 0] = jHR.Position.Y;
                        standardZ[standard_count, 0] = jHR.Position.Z;
                        standardT[standard_count, 0] = standard_time;

                        standardX[standard_count, 1] = jHL.Position.X;
                        standardY[standard_count, 1] = jHL.Position.Y;
                        standardZ[standard_count, 1] = jHL.Position.Z;
                        standardT[standard_count, 1] = standard_time;

                        standardX[standard_count, 2] = jH.Position.X;
                        standardY[standard_count, 2] = jH.Position.Y;
                        standardZ[standard_count, 2] = jH.Position.Z;
                        standardT[standard_count, 2] = standard_time;

                        standardX[standard_count, 3] = jSC.Position.X;
                        standardY[standard_count, 3] = jSC.Position.Y;
                        standardZ[standard_count, 3] = jSC.Position.Z;
                        standardT[standard_count, 3] = standard_time;

                        standardX[standard_count, 4] = jSR.Position.X;
                        standardY[standard_count, 4] = jSR.Position.Y;
                        standardZ[standard_count, 4] = jSR.Position.Z;
                        standardT[standard_count, 4] = standard_time;

                        standardX[standard_count, 5] = jSL.Position.X;
                        standardY[standard_count, 5] = jSL.Position.Y;
                        standardZ[standard_count, 5] = jSL.Position.Z;
                        standardT[standard_count, 5] = standard_time;

                        standardX[standard_count, 6] = jER.Position.X;
                        standardY[standard_count, 6] = jER.Position.Y;
                        standardZ[standard_count, 6] = jER.Position.Z;
                        standardT[standard_count, 6] = standard_time;

                        standardX[standard_count, 7] = jEL.Position.X;
                        standardY[standard_count, 7] = jEL.Position.Y;
                        standardZ[standard_count, 7] = jEL.Position.Z;
                        standardT[standard_count, 7] = standard_time;

                        standardX[standard_count, 8] = jWR.Position.X;
                        standardY[standard_count, 8] = jWR.Position.Y;
                        standardZ[standard_count, 8] = jWR.Position.Z;
                        standardT[standard_count, 8] = standard_time;

                        standardX[standard_count, 9] = jWL.Position.X;
                        standardY[standard_count, 9] = jWL.Position.Y;
                        standardZ[standard_count, 9] = jWL.Position.Z;
                        standardT[standard_count, 9] = standard_time;

                        standardX[standard_count, 10] = jSP.Position.X;
                        standardY[standard_count, 10] = jSP.Position.Y;
                        standardZ[standard_count, 10] = jSP.Position.Z;
                        standardT[standard_count, 10] = standard_time;

                        standardX[standard_count, 11] = jCH.Position.X;
                        standardY[standard_count, 11] = jCH.Position.Y;
                        standardZ[standard_count, 11] = jCH.Position.Z;
                        standardT[standard_count, 11] = standard_time;

                        standardX[standard_count, 12] = jRH.Position.X;
                        standardY[standard_count, 12] = jRH.Position.Y;
                        standardZ[standard_count, 12] = jRH.Position.Z;
                        standardT[standard_count, 12] = standard_time;

                        standardX[standard_count, 13] = jLH.Position.X;
                        standardY[standard_count, 13] = jLH.Position.Y;
                        standardZ[standard_count, 13] = jLH.Position.Z;
                        standardT[standard_count, 13] = standard_time;

                        standardX[standard_count, 14] = jRK.Position.X;
                        standardY[standard_count, 14] = jRK.Position.Y;
                        standardZ[standard_count, 14] = jRK.Position.Z;
                        standardT[standard_count, 14] = standard_time;

                        standardX[standard_count, 15] = jLK.Position.X;
                        standardY[standard_count, 15] = jLK.Position.Y;
                        standardZ[standard_count, 15] = jLK.Position.Z;
                        standardT[standard_count, 15] = standard_time;

                        standardX[standard_count, 16] = jRA.Position.X;
                        standardY[standard_count, 16] = jRA.Position.Y;
                        standardZ[standard_count, 16] = jRA.Position.Z;
                        standardT[standard_count, 16] = standard_time;

                        standardX[standard_count, 17] = jLA.Position.X;
                        standardY[standard_count, 17] = jLA.Position.Y;
                        standardZ[standard_count, 17] = jLA.Position.Z;
                        standardT[standard_count, 17] = standard_time;

                        standardX[standard_count, 18] = jRF.Position.X;
                        standardY[standard_count, 18] = jRF.Position.Y;
                        standardZ[standard_count, 18] = jRF.Position.Z;
                        standardT[standard_count, 18] = standard_time;

                        standardX[standard_count, 19] = jLF.Position.X;
                        standardY[standard_count, 19] = jLF.Position.Y;
                        standardZ[standard_count, 19] = jLF.Position.Z;
                        standardT[standard_count, 19] = standard_time;

                        text_standard.Text += "[" + standard_count + "]" + standardX[standard_count, 0] + "," + standardY[standard_count, 0] + "," + standardZ[standard_count, 0] +","+standardT[standard_count, 0]+ "\r\n";             
                        standard_count++;

                        //
                    }

                    //=======================================================================

                    else if (colorcheck != 0)
                    {
                        self_time += double.Parse(text_time.Text);
                        //記錄參考動作

                        
                        selfX[self_count, 0] = jHR.Position.X;
                        selfY[self_count, 0] = jHR.Position.Y;
                        selfZ[self_count, 0] = jHR.Position.Z;
                        selfT[self_count, 0] = self_time;

                        selfX[self_count, 1] = jHL.Position.X;
                        selfY[self_count, 1] = jHL.Position.Y;
                        selfZ[self_count, 1] = jHL.Position.Z;
                        selfT[self_count, 1] = self_time;

                        selfX[self_count, 2] = jH.Position.X;
                        selfY[self_count, 2] = jH.Position.Y;
                        selfZ[self_count, 2] = jH.Position.Z;
                        selfT[self_count, 2] = self_time;

                        selfX[self_count, 3] = jSC.Position.X;
                        selfY[self_count, 3] = jSC.Position.Y;
                        selfZ[self_count, 3] = jSC.Position.Z;
                        selfT[self_count, 3] = self_time;

                        selfX[self_count, 4] = jSR.Position.X;
                        selfY[self_count, 4] = jSR.Position.Y;
                        selfZ[self_count, 4] = jSR.Position.Z;
                        selfT[self_count, 4] = self_time;

                        selfX[self_count, 5] = jSL.Position.X;
                        selfY[self_count, 5] = jSL.Position.Y;
                        selfZ[self_count, 5] = jSL.Position.Z;
                        selfT[self_count, 5] = self_time;

                        selfX[self_count, 6] = jER.Position.X;
                        selfY[self_count, 6] = jER.Position.Y;
                        selfZ[self_count, 6] = jER.Position.Z;
                        selfT[self_count, 6] = self_time;

                        selfX[self_count, 7] = jEL.Position.X;
                        selfY[self_count, 7] = jEL.Position.Y;
                        selfZ[self_count, 7] = jEL.Position.Z;
                        selfT[self_count, 7] = self_time;

                        selfX[self_count, 8] = jWR.Position.X;
                        selfY[self_count, 8] = jWR.Position.Y;
                        selfZ[self_count, 8] = jWR.Position.Z;
                        selfT[self_count, 8] = self_time;

                        selfX[self_count, 9] = jWL.Position.X;
                        selfY[self_count, 9] = jWL.Position.Y;
                        selfZ[self_count, 9] = jWL.Position.Z;
                        selfT[self_count, 9] = self_time;

                        selfX[self_count, 10] = jSP.Position.X;
                        selfY[self_count, 10] = jSP.Position.Y;
                        selfZ[self_count, 10] = jSP.Position.Z;
                        selfT[self_count, 10] = self_time;

                        selfX[self_count, 11] = jCH.Position.X;
                        selfY[self_count, 11] = jCH.Position.Y;
                        selfZ[self_count, 11] = jCH.Position.Z;
                        selfT[self_count, 11] = self_time;

                        selfX[self_count, 12] = jRH.Position.X;
                        selfY[self_count, 12] = jRH.Position.Y;
                        selfZ[self_count, 12] = jRH.Position.Z;
                        selfT[self_count, 12] = self_time;

                        selfX[self_count, 13] = jLH.Position.X;
                        selfY[self_count, 13] = jLH.Position.Y;
                        selfZ[self_count, 13] = jLH.Position.Z;
                        selfT[self_count, 13] = self_time;

                        selfX[self_count, 14] = jRK.Position.X;
                        selfY[self_count, 14] = jRK.Position.Y;
                        selfZ[self_count, 14] = jRK.Position.Z;
                        selfT[self_count, 14] = self_time;

                        selfX[self_count, 15] = jLK.Position.X;
                        selfY[self_count, 15] = jLK.Position.Y;
                        selfZ[self_count, 15] = jLK.Position.Z;
                        selfT[self_count, 15] = self_time;

                        selfX[self_count, 16] = jRA.Position.X;
                        selfY[self_count, 16] = jRA.Position.Y;
                        selfZ[self_count, 16] = jRA.Position.Z;
                        selfT[self_count, 16] = self_time;

                        selfX[self_count, 17] = jLA.Position.X;
                        selfY[self_count, 17] = jLA.Position.Y;
                        selfZ[self_count, 17] = jLA.Position.Z;
                        selfT[self_count, 17] = self_time;

                        selfX[self_count, 18] = jRF.Position.X;
                        selfY[self_count, 18] = jRF.Position.Y;
                        selfZ[self_count, 18] = jRF.Position.Z;
                        selfT[self_count, 18] = self_time;

                        selfX[self_count, 19] = jLF.Position.X;
                        selfY[self_count, 19] = jLF.Position.Y;
                        selfZ[self_count, 19] = jLF.Position.Z;
                        selfT[self_count, 19] = self_time;

                        text_self.Text += "[" + self_count + "]" + selfX[self_count, 0] + "," + selfY[self_count, 0] + "," + selfZ[self_count, 0] + "\r\n";                       
                        self_count++;
                        //
                    }
                }
               
            }

            if (waittime == 3 && colorcheck == 0)
            {
                    _draw = false;
                    colorcheck = 1;
                    starttime = 0;
                    waittime = 0;
                    
            }
            if (waittime == 3&&colorcheck == 1)
             {
                    _draw = false;
                    tm1.Stop();
                    tm2.Stop();
                    AutoDemo();
                    
             }
            if (waittime == 3 && colorcheck == 2)
            {
                _draw = false;
                tm1.Stop();
                tm2.Stop();
                AutoDemo();

            }
            if (waittime == 3 && colorcheck == 3)
            {
                _draw = false;
                tm1.Stop();
                tm2.Stop();
                AutoDemo();

            }
            if (waittime == 3 && colorcheck == 4)
            {
                _draw = false;
                tm1.Stop();
                tm2.Stop();
                

            }
            /*if (starttime== 15 && colorcheck == 0)
            {
                _draw = false;
                colorcheck = 1;
                starttime = 0;
                waittime = 0;
                lab_show.Content = "標準";

            }
            if (starttime == 15 && colorcheck == 1)
            {
                
                _draw = false;
                tm1.Stop();
                tm2.Stop();
                lab_show.Content = "較快(10)";
                

            }
            if (starttime == 11 && colorcheck == 2)
            {
                
                _draw = false;
                tm1.Stop();
                tm2.Stop();
                lab_show.Content = "較慢(20)";
                

            }
            if (starttime == 20 && colorcheck == 3)
            {
                _draw = false;
                tm1.Stop();
                tm2.Stop();
                lab_show.Content = "隨意(15)";
               

            }
            if (starttime == 15 && colorcheck == 4)
            {
                _draw = false;
                tm1.Stop();
                tm2.Stop();
               

            }*/
  
                /*
                if (rb_inter.IsChecked == true)
                    RepeatInterpolationAUTO();
                else if (rb_dtw.IsChecked == true)
                    DynamicTimeWarpingAUTO();
                else if (rb_dtwsbt.IsChecked == true)
                    DTWslottedbytimeAUTO();
                tm2.Start();
                //Console.Beep();*/

        }
        private void tm2Tick_point(object sender, EventArgs e)
        {
            waittime++;
            starttime++;
            text_starttime.Text = starttime.ToString();
            text_waittime.Text = waittime.ToString();
            
        }
        private void DrawHead(ColorImagePoint cpH)
        {
            Canvas.SetLeft(Head, cpH.X - Head.Width / 2);
            Canvas.SetTop(Head, cpH.Y - Head.Height / 2);
        }
        private void DrawHandRight(ColorImagePoint cpHR)
        {
            Canvas.SetLeft(HandRight, cpHR.X - HandRight.Width / 2);
            Canvas.SetTop(HandRight, cpHR.Y - HandRight.Height / 2);
        }
        /*private void DrawKneeRight(ColorImagePoint cpRK)
        {
            Canvas.SetLeft(KneeRight, cpRK.X - KneeRight.Width / 2);
            Canvas.SetTop(KneeRight, cpRK.Y - KneeRight.Height / 2);
        }*/

        ColorImagePoint MapToColorImage(Joint jp)
        {
            ColorImagePoint cp = myKinect.MapSkeletonPointToColor(jp.Position, myKinect.ColorStream.Format);
            return cp;
        }
        //線段建立函式--Code 5-12
        //private void DrawingLine(ColorImagePoint point)
        private void DrawingLine()
        {
            //如果還未設定劃圖的第一個起點,則直接將傳遞參數指定給beginPoint
            if (_beginPoint.X == -1)
            {
                _beginPoint.X = cpHR.X;
                _beginPoint.Y = cpHR.Y;
            }
            else  //如果劃圖的第一個起點早已設定,則嘗試建立線段
            {
                //呼叫JitterDetect函式判斷右手移動是否有效
                bool jitter = JitterDetect(_beginPoint, cpHR);
                //如果右手移動不是手抖動造成,則建立新的線段
                if (!jitter)
                {
                    Line line = new Line();
                    if (colorcheck == 0)
                        line.Stroke = Brushes.Black;  //設定畫刷顏色
                    else if (colorcheck == 1)
                        line.Stroke = Brushes.Blue;
                    else if (colorcheck == 2)
                        line.Stroke = Brushes.Yellow;
                    else if (colorcheck == 3)
                        line.Stroke = Brushes.Green;
                    else if (colorcheck == 4)
                        line.Stroke = Brushes.Red;
                    /* else if (colorcheck == 2)
                         line.Stroke = Brushes.Red;
                     else if (colorcheck == 3)
                         line.Stroke = Brushes.Yellow;
                     else if (colorcheck == 4)
                         line.Stroke = Brushes.Green;*/
                    line.StrokeThickness = 3;     //設定畫刷寬度
                    //以beginPoint為線段起點,以傳遞過來的參數為終點
                    line.X1 = _beginPoint.X;
                    line.Y1 = _beginPoint.Y;
                    line.X2 = cpHR.X;
                    line.Y2 = cpHR.Y;

                    canvas1.Children.Add(line);//將建立的線段加至Canvas物件,透過Canvas顯示出來
                    //將傳遞過來的參數設為下一條線段起點
                    _beginPoint.X = cpHR.X;
                    _beginPoint.Y = cpHR.Y;
                }
            }
        }
        //手抖動判斷函式--Code 5-13
        private bool JitterDetect(ColorImagePoint p1, ColorImagePoint p2)
        {
            //計算2點之間距離
            double distance = Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            //如果距離超過門檻,則視為有效移動,回傳false;否則回傳true,表示位移是抖動造成
            if (distance > MoveThreshold)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void RepeatInterpolation_Click(object sender, RoutedEventArgs e)
        {
            Random random1 = new Random();
            int path,rd1;
            int usemethod = standard_count - self_count;
            textBox1.Text += "use:" + usemethod + "\r\n";
            release();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//引用stopwatch物件
            sw.Reset();//碼表歸零
            sw.Start();//碼表開始計時

           
                
                if (usemethod >= 1)//當標準動作大於參考動作，使用隨機內插的方式補足參考動作
                {
                    for (int n = 0; n < 20; n++)
                    {
                        path = standard_count - self_count;

                        for (int i = 0; i < self_count; i++)
                        {

                            interpolationX[i, n] = 999;
                            interpolationY[i, n] = 999;
                            interpolationZ[i, n] = 999;

                        }
                        for (int i = 0; i < standard_count; i++)
                        {

                            calculateFORstandardX[i, n] = normalstandardX[i, n];
                            calculateFORstandardY[i, n] = normalstandardY[i, n];
                            calculateFORstandardZ[i, n] = normalstandardZ[i, n];
                        }
                        while (path >= 1)
                        {
                            rd1 = random1.Next(0, self_count - 2);

                            double scalex = 0, scaley = 0, scalez = 0;
                            if (interpolationX[rd1, n] == 999)
                            {

                               /* interpolationX[rd1, n] = (normalselfX[rd1, n] + normalselfX[rd1 + 1, n]) / 2;
                                interpolationY[rd1, n] = (normalselfY[rd1, n] + normalselfY[rd1 + 1, n]) / 2;
                                interpolationZ[rd1, n] = (normalselfZ[rd1, n] + normalselfZ[rd1 + 1, n]) / 2;*/
                                //textBox1.Text += rd1 + ",s_rd1:" + selfX[rd1, 0] + "s_rd1+1:" + selfX[rd1 + 1, 0] + "inter" + interpolationX[rd1, 0] + "\r\n";

                                scalex = (normalstandardX[rd1 + 1, n] - normalstandardX[rd1, n]) / (normalstandardX[rd1 + 2, n] - normalstandardX[rd1, n]);
                                scaley = (normalstandardY[rd1 + 1, n] - normalstandardY[rd1, n]) / (normalstandardY[rd1 + 2, n] - normalstandardY[rd1, n]);
                                scalez = (normalstandardZ[rd1 + 1, n] - normalstandardZ[rd1, n]) / (normalstandardZ[rd1 + 2, n] - normalstandardZ[rd1, n]);
                                /*interpolationX[rd1, n] = (normalselfX[rd1, n] + normalselfX[rd1 + 1, n]) / 2;
                                interpolationY[rd1, n] = (normalselfY[rd1, n] + normalselfY[rd1 + 1, n]) / 2;
                                interpolationZ[rd1, n] = (normalselfZ[rd1, n] + normalselfZ[rd1 + 1, n]) / 2;*/

                                interpolationX[rd1, n] = normalselfX[rd1, n] + (normalselfX[rd1 + 1, n] - normalselfX[rd1, n]) * scalex;
                                interpolationY[rd1, n] = normalselfY[rd1, n] + (normalselfY[rd1 + 1, n] - normalselfY[rd1, n]) * scaley;
                                interpolationZ[rd1, n] = normalselfZ[rd1, n] + (normalselfZ[rd1 + 1, n] - normalselfZ[rd1, n]) * scalez;

                                path--;
                            }
                        }

                        for (int i = 0, j = 0; i < standard_count; i++, j++)
                        {

                            calculateFORselfX[i, n] = normalselfX[j, n];
                            calculateFORselfY[i, n] = normalselfY[j, n];
                            calculateFORselfZ[i, n] = normalselfZ[j, n];

                            if (interpolationX[j, n] != 999)
                            {

                                calculateFORselfX[i + 1, n] = interpolationX[j, n];
                                calculateFORselfY[i + 1, n] = interpolationY[j, n];
                                calculateFORselfZ[i + 1, n] = interpolationZ[j, n];
                                i++;
                            }
                        }
                        /*for (int i = 0; i < standard_count; i++)
                          textBox1.Text += "[" + i + "]" + calculateFORselfX[i, 0] + "," + calculateFORselfY[i, 0] + "," + calculateFORselfZ[i, 0] + "\r\n";
                    */}
                }
            
            if (usemethod < 0)//當標準動作小於參考動作，使用自動校正的方式縮短參考動作
            {
                for (int n = 0; n < 20; n++)
                {
                    path = Math.Abs(standard_count - self_count) + 1;
                    for (int i = 0; i < standard_count; i++)
                    {
                        for (int j = i; j < path + i; j++)
                        {
                            averagingX[i, n] += normalselfX[j, n];
                            averagingY[i, n] += normalselfY[j, n];
                            averagingZ[i, n] += normalselfZ[j, n];
                        }
                        calculateFORselfX[i, n] = averagingX[i, n] / path;
                        calculateFORselfY[i, n] = averagingY[i, n] / path;
                        calculateFORselfZ[i, n] = averagingZ[i, n] / path;

                        calculateFORstandardX[i, n] = normalstandardX[i, n];
                        calculateFORstandardY[i, n] = normalstandardY[i, n];
                        calculateFORstandardZ[i, n] = normalstandardZ[i, n];


                        // textBox1.Text += "[" + i + "]." + calculateFORselfX[i, 0] + "\r\n";
                    }
                }
            }
            if (usemethod == 0)//標準動作等於參考動作
            {
                for (int n = 0; n < 20; n++)
                {
                    for (int i = 0; i < standard_count; i++)
                    {

                        calculateFORstandardX[i, n] = normalstandardX[i, n];
                        calculateFORstandardY[i, n] = normalstandardY[i, n];
                        calculateFORstandardZ[i, n] = normalstandardZ[i, n];

                        calculateFORselfX[i, n] = normalselfX[i, n];
                        calculateFORselfY[i, n] = normalselfY[i, n];
                        calculateFORselfZ[i, n] = normalselfZ[i, n];

                    }
                }
            }
            sw.Stop();//碼錶停止
            timeRI = sw.Elapsed.TotalMilliseconds;
        }

        void FindEdge()//尋找邊界範圍值
        {
            release();

            for (int n = 0; n < 20; n++)
            {
                for (int i = 0; i < standard_count; i++)
                {
                    xfar_max[n] = Math.Max(normalstandardX[i, n], xfar_max[n]);
                    xfar_min[n] = Math.Min(normalstandardX[i, n], xfar_min[n]);
                    yfar_max[n] = Math.Max(normalstandardY[i, n], yfar_max[n]);
                    yfar_min[n] = Math.Min(normalstandardY[i, n], yfar_min[n]);
                    zfar_max[n] = Math.Max(normalstandardZ[i, n], zfar_max[n]);
                    zfar_min[n] = Math.Min(normalstandardZ[i, n], zfar_min[n]);

                   
                }

                
                
            }
            
           
        }
        void FindEdge2()//尋找邊界範圍值
        {
            for (int n = 0; n < 20; n++)
            {
                for (int i = 0; i < 20; i++)
                {
                    xfar_max[n] = Math.Max(xfar_max[n], xfar_max[i]);
                    xfar_min[n] = Math.Max(xfar_min[n], xfar_min[i]);
                    yfar_max[n] = Math.Max(yfar_max[n], yfar_max[i]);
                    yfar_min[n] = Math.Max(yfar_min[n], yfar_min[i]);
                    zfar_max[n] = Math.Max(zfar_max[n], zfar_max[i]);
                    zfar_min[n] = Math.Max(zfar_min[n], zfar_min[i]);
                }

               

            }
            for (int n = 0; n < 20; n++)
            {
                textBox1.Text += "xmax:" + xfar_max[n] + ",min:" + xfar_min[n] + "\r\n";
                textBox1.Text += "ymax:" + yfar_max[n] + ",min:" + yfar_min[n] + "\r\n";
                textBox1.Text += "zmax:" + zfar_max[n] + ",min:" + zfar_min[n] + "\r\n";
            }
        }
        
        private void SkeletonNormalize_Click(object sender, RoutedEventArgs e)
        {
            //jH=2, jSC=3, jSR=4, jSL=5, jER=6, jEL=7, jWR=8, jWL=9,jSP=10, jHR=0, jHL=1;

            if (check_yes.IsChecked == true)
            {
                for (int n = 0; n < 20; n++)
                {
                    for (int i = 0; i < standard_count; i++)
                    {
                        double LR_S = Math.Sqrt((standardX[i, 4] - standardX[i, 5]) * (standardX[i, 4] - standardX[i, 5]) +
                                            (standardY[i, 4] - standardY[i, 5]) * (standardY[i, 4] - standardY[i, 5]) +
                                            (standardZ[i, 4] - standardZ[i, 5]) * (standardZ[i, 4] - standardZ[i, 5]));

                        if (LR_S == 0)
                            LR_S = 1;
                        normalstandardX[i, n] = (standardX[i, n] - standardX[i, 3]) / LR_S;
                        normalstandardY[i, n] = (standardY[i, n] - standardY[i, 3]) / LR_S;
                        normalstandardZ[i, n] = (standardZ[i, n] - standardZ[i, 3]) / LR_S;
                        //text_standard.Text += i + "." + standardX[i, 0] + standardY[i, 0] + standardZ[i, 0] + "\r\n";
                    }
                    for (int i = 0; i < self_count; i++)
                    {
                        double LR_R = Math.Sqrt((selfX[i, 4] - selfX[i, 5]) * (selfX[i, 4] - selfX[i, 5]) +
                                            (selfY[i, 4] - selfY[i, 5]) * (selfY[i, 4] - selfY[i, 5]) +
                                            (selfZ[i, 4] - selfZ[i, 5]) * (selfZ[i, 4] - selfZ[i, 5]));

                        if (LR_R == 0)
                            LR_R = 1;
                        normalselfX[i, n] = (selfX[i, n] - selfX[i, 3]) / LR_R;
                        normalselfY[i, n] = (selfY[i, n] - selfY[i, 3]) / LR_R;
                        normalselfZ[i, n] = (selfZ[i, n] - selfZ[i, 3]) / LR_R;
                        //text_self.Text += i + "." + selfX[i, 0] + selfY[i, 0] + selfZ[i, 0] + "\r\n";
                    }
                }
            }
            else
            {
                for (int n = 0; n < 20; n++)
                {
                    for (int i = 0; i < standard_count; i++)
                    {

                        normalstandardX[i, n] = standardX[i, n];
                        normalstandardY[i, n] = standardY[i, n];
                        normalstandardZ[i, n] = standardZ[i, n];
                    }
                    for (int i = 0; i < self_count; i++)
                    {
                        normalselfX[i, n] = selfX[i, n];
                        normalselfY[i, n] = selfY[i, n];
                        normalselfZ[i, n] = selfZ[i, n];
                    }
                }
            }
        }
        
        private void DtwSlotC_Click(object sender, RoutedEventArgs e)
        {
            FindEdge();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//引用stopwatch物件
            sw.Reset();//碼表歸零
            sw.Start();//碼表開始計時

            for (int n = 0; n < 20; n++)
            {
                int startI = 1, startJ = 1;//每個分割的起始位置
                int i = 1, j = 1;
                slottedTime = 0;
                double[] score = new double[20];
                // double DTW_ang2 = 0, DTW_dis2 = 0, DTW_ang1 = 0, DTW_dis1 = 0;
                double DTWC_score1 = 0, DTWC_score2 = 0;

                for (int k = 1; k <= standard_count; k++)
                    DTW[k, 0] = 9999;
                for (int l = 1; l <= self_count; l++)
                    DTW[0, l] = 9999;

                DTW[0, 0] = 0;

                while (startI < standard_count && startJ < self_count)
                {
                    DTWC_score1 += DTW[i - 1, j - 1];

                    slottedTime += Convert.ToDouble(text_slbt.Text);

                    startI = i;
                    startJ = j;

                    for (i = startI; i <= standard_count; i++)
                    {
                        double time = standardT[i - 1, n];
                        if (standardT[i - 1, n] <= slottedTime && standardT[i - 1, n] != 0)
                        {
                            //textBox1.Text += "standardT:" + standardT[i - 1, 0] + ",slottedTime" + slottedTime + "\r\n";

                            for (j = startJ; j <= self_count; j++)
                            {
                                time = standardT[j - 1, n];
                                if (selfT[j - 1, n] <= slottedTime && selfT[j - 1, n] != 0)
                                {

                                    double dis1 = Math.Sqrt((normalstandardX[i - 1, n] - normalselfX[j - 1, n]) * (normalstandardX[i - 1, n] - normalselfX[j - 1, n])
                                                + (normalstandardY[i - 1, n] - normalselfY[j - 1, n]) * (normalstandardY[i - 1, n] - normalselfY[j - 1, n])
                                                + (normalstandardZ[i - 1, n] - normalselfZ[j - 1, n]) * (normalstandardZ[i - 1, n] - normalselfZ[j - 1, n]));

                                    DTW[i, j] = Math.Min(Math.Min(DTW[i - 1, j], DTW[i, j - 1]), DTW[i - 1, j - 1]) + dis1;

                                }
                                else
                                    break;
                            }

                        }

                        else
                            break;

                    }

                }
                //text_scorelist.Text += DTW_dis1 + "," + DTW_ang1 + "\r\n";

                //textBox3.Text += "================我是分隔線=======================" + "\r\n";
                //textBox2.Text += "================我是分隔線=======================" + "\r\n";

                for (i = 0; i < standard_count; i++)//相距最遠座標
                {

                    if (Math.Abs(normalstandardX[i, n] - xfar_max[n]) >= Math.Abs(normalstandardX[i, n] - xfar_min[n]))
                        standardfarX[i, n] = xfar_max[n];
                    else
                        standardfarX[i, n] = xfar_min[n];
                    if (Math.Abs(normalstandardY[i, n] - yfar_max[n]) >= Math.Abs(normalstandardY[i, n] - yfar_min[n]))
                        standardfarY[i, n] = yfar_max[n];
                    else
                        standardfarY[i, n] = yfar_min[n];
                    if (Math.Abs(normalstandardZ[i, n] - zfar_max[n]) >= Math.Abs(normalstandardZ[i, n] - zfar_min[n]))
                        standardfarZ[i, n] = zfar_max[n];
                    else
                        standardfarZ[i, n] = zfar_min[n];
                    //textBox1.Text += i + ":" + standardfarX[i, 0] + "," + standardfarY[i, 0] + "," + standardfarZ[i, 0] + "\r\n";
                }

                for (int k = 1; k <= standard_count; k++)
                    DTW[k, 0] = 9999;
                for (int l = 1; l <= standard_count; l++)
                    DTW[0, l] = 9999;

                DTW[0, 0] = 0;

                //初始化
                startI = 1;
                startJ = 1;
                slottedTime = 0;
                i = 1;
                j = 1;
                //初始化

                while (startI < standard_count && startJ < standard_count)
                {
                    DTWC_score2 += DTW[i - 1, j - 1];
                    slottedTime += Convert.ToDouble(text_slbt.Text);

                    startI = i;
                    startJ = j;

                    for (i = startI; i <= standard_count; i++)
                    {
                        double time = standardT[i - 1, n];

                        if (standardT[i - 1, n] <= slottedTime && standardT[i - 1, n] != 0)
                        {
                            for (j = startJ; j <= standard_count; j++)
                            {
                                time = standardT[j - 1, n];
                                if (standardT[j - 1, n] <= slottedTime && standardT[j - 1, n] != 0)
                                {

                                    double dis1 = Math.Sqrt((normalstandardX[i - 1, n] - standardfarX[j - 1, n]) * (normalstandardX[i - 1, n] - standardfarX[j - 1, n])
                                               + (normalstandardY[i - 1, n] - standardfarY[j - 1, n]) * (normalstandardY[i - 1, n] - standardfarY[j - 1, n])
                                               + (normalstandardZ[i - 1, n] - standardfarZ[j - 1, n]) * (normalstandardZ[i - 1, n] - standardfarZ[j - 1, n]));

                                    DTW[i, j] = Math.Min(Math.Min(DTW[i - 1, j], DTW[i, j - 1]), DTW[i - 1, j - 1]) + dis1;

                                }
                                else
                                    break;
                            }
                            //textBox3.Text += "\r\n";
                            // textBox2.Text += "\r\n";
                        }

                        else
                            break;

                    }



                }




                //計算分數


                score[n] = 100 - DTWC_score1 / DTWC_score2 * 100;
                if (score[n] < 0) score[n] = 0;
                //text_scorelist.Text += "s_all:" + score + "\r\n";
                text_score.Text += "DTWS座標分數:" + "[" + n + "]" + "\r\n" + score[n] + "\r\n";
                SDTWcoordinateScore[n] = score[n];
            }
            sw.Stop();//碼錶停止
            timeDSC = sw.Elapsed.TotalMilliseconds;
        }

        void AutoDemo()
        {
            //===========================
            //jH=2, jSC=3, jSR=4, jSL=5, jER=6, jEL=7, jWR=8, jWL=9,jSP=10, jHR=0, jHL=1;

            
                for (int n = 0; n < 20; n++)
                {
                    for (int i = 0; i < standard_count; i++)
                    {
                        double LR_S = Math.Sqrt((standardX[i, 4] - standardX[i, 5]) * (standardX[i, 4] - standardX[i, 5]) +
                                            (standardY[i, 4] - standardY[i, 5]) * (standardY[i, 4] - standardY[i, 5]) +
                                            (standardZ[i, 4] - standardZ[i, 5]) * (standardZ[i, 4] - standardZ[i, 5]));

                        if (LR_S == 0)
                            LR_S = 1;
                        normalstandardX[i, n] = (standardX[i, n] - standardX[i, 3]) / LR_S;
                        normalstandardY[i, n] = (standardY[i, n] - standardY[i, 3]) / LR_S;
                        normalstandardZ[i, n] = (standardZ[i, n] - standardZ[i, 3]) / LR_S;
                        //text_standard.Text += i + "." + standardX[i, 0] + standardY[i, 0] + standardZ[i, 0] + "\r\n";
                    }
                    for (int i = 0; i < self_count; i++)
                    {
                        double LR_R = Math.Sqrt((selfX[i, 4] - selfX[i, 5]) * (selfX[i, 4] - selfX[i, 5]) +
                                            (selfY[i, 4] - selfY[i, 5]) * (selfY[i, 4] - selfY[i, 5]) +
                                            (selfZ[i, 4] - selfZ[i, 5]) * (selfZ[i, 4] - selfZ[i, 5]));

                        if (LR_R == 0)
                            LR_R = 1;
                        normalselfX[i, n] = (selfX[i, n] - selfX[i, 3]) / LR_R;
                        normalselfY[i, n] = (selfY[i, n] - selfY[i, 3]) / LR_R;
                        normalselfZ[i, n] = (selfZ[i, n] - selfZ[i, 3]) / LR_R;
                        //text_self.Text += i + "." + selfX[i, 0] + selfY[i, 0] + selfZ[i, 0] + "\r\n";
                    }
                }
            
           
            //======================
            Random random1 = new Random();
            int path, rd1;
            int usemethod = standard_count - self_count;
            textBox1.Text += "use:" + usemethod + "\r\n";
            release();
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//引用stopwatch物件
            //sw.Reset();//碼表歸零
            //sw.Start();//碼表開始計時



            if (usemethod >= 1)//當標準動作大於參考動作，使用隨機內插的方式補足參考動作
            {
                for (int n = 0; n < 20; n++)
                {
                    path = standard_count - self_count;

                    for (int i = 0; i < self_count; i++)
                    {

                        interpolationX[i, n] = 999;
                        interpolationY[i, n] = 999;
                        interpolationZ[i, n] = 999;

                    }
                    for (int i = 0; i < standard_count; i++)
                    {

                        calculateFORstandardX[i, n] = normalstandardX[i, n];
                        calculateFORstandardY[i, n] = normalstandardY[i, n];
                        calculateFORstandardZ[i, n] = normalstandardZ[i, n];
                    }
                    while (path >= 1)
                    {
                        rd1 = random1.Next(0, self_count - 2);

                        double scalex = 0, scaley = 0, scalez = 0;
                        if (interpolationX[rd1, n] == 999)
                        {

                            /* interpolationX[rd1, n] = (normalselfX[rd1, n] + normalselfX[rd1 + 1, n]) / 2;
                             interpolationY[rd1, n] = (normalselfY[rd1, n] + normalselfY[rd1 + 1, n]) / 2;
                             interpolationZ[rd1, n] = (normalselfZ[rd1, n] + normalselfZ[rd1 + 1, n]) / 2;*/
                            //textBox1.Text += rd1 + ",s_rd1:" + selfX[rd1, 0] + "s_rd1+1:" + selfX[rd1 + 1, 0] + "inter" + interpolationX[rd1, 0] + "\r\n";

                            scalex = (normalstandardX[rd1 + 1, n] - normalstandardX[rd1, n]) / (normalstandardX[rd1 + 2, n] - normalstandardX[rd1, n]);
                            scaley = (normalstandardY[rd1 + 1, n] - normalstandardY[rd1, n]) / (normalstandardY[rd1 + 2, n] - normalstandardY[rd1, n]);
                            scalez = (normalstandardZ[rd1 + 1, n] - normalstandardZ[rd1, n]) / (normalstandardZ[rd1 + 2, n] - normalstandardZ[rd1, n]);
                            /*interpolationX[rd1, n] = (normalselfX[rd1, n] + normalselfX[rd1 + 1, n]) / 2;
                            interpolationY[rd1, n] = (normalselfY[rd1, n] + normalselfY[rd1 + 1, n]) / 2;
                            interpolationZ[rd1, n] = (normalselfZ[rd1, n] + normalselfZ[rd1 + 1, n]) / 2;*/

                            interpolationX[rd1, n] = normalselfX[rd1, n] + (normalselfX[rd1 + 1, n] - normalselfX[rd1, n]) * scalex;
                            interpolationY[rd1, n] = normalselfY[rd1, n] + (normalselfY[rd1 + 1, n] - normalselfY[rd1, n]) * scaley;
                            interpolationZ[rd1, n] = normalselfZ[rd1, n] + (normalselfZ[rd1 + 1, n] - normalselfZ[rd1, n]) * scalez;

                            path--;
                        }
                    }

                    for (int i = 0, j = 0; i < standard_count; i++, j++)
                    {

                        calculateFORselfX[i, n] = normalselfX[j, n];
                        calculateFORselfY[i, n] = normalselfY[j, n];
                        calculateFORselfZ[i, n] = normalselfZ[j, n];

                        if (interpolationX[j, n] != 999)
                        {

                            calculateFORselfX[i + 1, n] = interpolationX[j, n];
                            calculateFORselfY[i + 1, n] = interpolationY[j, n];
                            calculateFORselfZ[i + 1, n] = interpolationZ[j, n];
                            i++;
                        }
                    }
                    /*for (int i = 0; i < standard_count; i++)
                      textBox1.Text += "[" + i + "]" + calculateFORselfX[i, 0] + "," + calculateFORselfY[i, 0] + "," + calculateFORselfZ[i, 0] + "\r\n";
                */
                }
            }

            if (usemethod < 0)//當標準動作小於參考動作，使用自動校正的方式縮短參考動作
            {
                for (int n = 0; n < 20; n++)
                {
                    path = Math.Abs(standard_count - self_count) + 1;
                    for (int i = 0; i < standard_count; i++)
                    {
                        for (int j = i; j < path + i; j++)
                        {
                            averagingX[i, n] += normalselfX[j, n];
                            averagingY[i, n] += normalselfY[j, n];
                            averagingZ[i, n] += normalselfZ[j, n];
                        }
                        calculateFORselfX[i, n] = averagingX[i, n] / path;
                        calculateFORselfY[i, n] = averagingY[i, n] / path;
                        calculateFORselfZ[i, n] = averagingZ[i, n] / path;

                        calculateFORstandardX[i, n] = normalstandardX[i, n];
                        calculateFORstandardY[i, n] = normalstandardY[i, n];
                        calculateFORstandardZ[i, n] = normalstandardZ[i, n];


                        // textBox1.Text += "[" + i + "]." + calculateFORselfX[i, 0] + "\r\n";
                    }
                }
            }
            if (usemethod == 0)//標準動作等於參考動作
            {
                for (int n = 0; n < 20; n++)
                {
                    for (int i = 0; i < standard_count; i++)
                    {

                        calculateFORstandardX[i, n] = normalstandardX[i, n];
                        calculateFORstandardY[i, n] = normalstandardY[i, n];
                        calculateFORstandardZ[i, n] = normalstandardZ[i, n];

                        calculateFORselfX[i, n] = normalselfX[i, n];
                        calculateFORselfY[i, n] = normalselfY[i, n];
                        calculateFORselfZ[i, n] = normalselfZ[i, n];

                    }
                }
            }
            //sw.Stop();//碼錶停止
            //timeRI = sw.Elapsed.TotalMilliseconds;
            //================================



            FindEdge();
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//引用stopwatch物件
            //sw.Reset();//碼表歸零
            //sw.Start();//碼表開始計時

            for (int n = 0; n < 20; n++)
            {
                int startI = 1, startJ = 1;//每個分割的起始位置
                int i = 1, j = 1;
                slottedTime = 0;
                double[] score = new double[20];
                // double DTW_ang2 = 0, DTW_dis2 = 0, DTW_ang1 = 0, DTW_dis1 = 0;
                double DTWC_score1 = 0, DTWC_score2 = 0;

                for (int k = 1; k <= standard_count; k++)
                    DTW[k, 0] = 9999;
                for (int l = 1; l <= self_count; l++)
                    DTW[0, l] = 9999;

                DTW[0, 0] = 0;

                while (startI < standard_count && startJ < self_count)
                {
                    DTWC_score1 += DTW[i - 1, j - 1];

                    slottedTime += Convert.ToDouble(text_slbt.Text);

                    startI = i;
                    startJ = j;

                    for (i = startI; i <= standard_count; i++)
                    {
                        double time = standardT[i - 1, n];
                        if (standardT[i - 1, n] <= slottedTime && standardT[i - 1, n] != 0)
                        {
                            //textBox1.Text += "standardT:" + standardT[i - 1, 0] + ",slottedTime" + slottedTime + "\r\n";

                            for (j = startJ; j <= self_count; j++)
                            {
                                time = standardT[j - 1, n];
                                if (selfT[j - 1, n] <= slottedTime && selfT[j - 1, n] != 0)
                                {

                                    double dis1 = Math.Sqrt((normalstandardX[i - 1, n] - normalselfX[j - 1, n]) * (normalstandardX[i - 1, n] - normalselfX[j - 1, n])
                                                + (normalstandardY[i - 1, n] - normalselfY[j - 1, n]) * (normalstandardY[i - 1, n] - normalselfY[j - 1, n])
                                                + (normalstandardZ[i - 1, n] - normalselfZ[j - 1, n]) * (normalstandardZ[i - 1, n] - normalselfZ[j - 1, n]));

                                    DTW[i, j] = Math.Min(Math.Min(DTW[i - 1, j], DTW[i, j - 1]), DTW[i - 1, j - 1]) + dis1;

                                }
                                else
                                    break;
                            }

                        }

                        else
                            break;

                    }

                }
                //text_scorelist.Text += DTW_dis1 + "," + DTW_ang1 + "\r\n";

                //textBox3.Text += "================我是分隔線=======================" + "\r\n";
                //textBox2.Text += "================我是分隔線=======================" + "\r\n";

                for (i = 0; i < standard_count; i++)//相距最遠座標
                {

                    if (Math.Abs(normalstandardX[i, n] - xfar_max[n]) >= Math.Abs(normalstandardX[i, n] - xfar_min[n]))
                        standardfarX[i, n] = xfar_max[n];
                    else
                        standardfarX[i, n] = xfar_min[n];
                    if (Math.Abs(normalstandardY[i, n] - yfar_max[n]) >= Math.Abs(normalstandardY[i, n] - yfar_min[n]))
                        standardfarY[i, n] = yfar_max[n];
                    else
                        standardfarY[i, n] = yfar_min[n];
                    if (Math.Abs(normalstandardZ[i, n] - zfar_max[n]) >= Math.Abs(normalstandardZ[i, n] - zfar_min[n]))
                        standardfarZ[i, n] = zfar_max[n];
                    else
                        standardfarZ[i, n] = zfar_min[n];
                    //textBox1.Text += i + ":" + standardfarX[i, 0] + "," + standardfarY[i, 0] + "," + standardfarZ[i, 0] + "\r\n";
                }

                for (int k = 1; k <= standard_count; k++)
                    DTW[k, 0] = 9999;
                for (int l = 1; l <= standard_count; l++)
                    DTW[0, l] = 9999;

                DTW[0, 0] = 0;

                //初始化
                startI = 1;
                startJ = 1;
                slottedTime = 0;
                i = 1;
                j = 1;
                //初始化

                while (startI < standard_count && startJ < standard_count)
                {
                    DTWC_score2 += DTW[i - 1, j - 1];
                    slottedTime += Convert.ToDouble(text_slbt.Text);

                    startI = i;
                    startJ = j;

                    for (i = startI; i <= standard_count; i++)
                    {
                        double time = standardT[i - 1, n];

                        if (standardT[i - 1, n] <= slottedTime && standardT[i - 1, n] != 0)
                        {
                            for (j = startJ; j <= standard_count; j++)
                            {
                                time = standardT[j - 1, n];
                                if (standardT[j - 1, n] <= slottedTime && standardT[j - 1, n] != 0)
                                {

                                    double dis1 = Math.Sqrt((normalstandardX[i - 1, n] - standardfarX[j - 1, n]) * (normalstandardX[i - 1, n] - standardfarX[j - 1, n])
                                               + (normalstandardY[i - 1, n] - standardfarY[j - 1, n]) * (normalstandardY[i - 1, n] - standardfarY[j - 1, n])
                                               + (normalstandardZ[i - 1, n] - standardfarZ[j - 1, n]) * (normalstandardZ[i - 1, n] - standardfarZ[j - 1, n]));

                                    DTW[i, j] = Math.Min(Math.Min(DTW[i - 1, j], DTW[i, j - 1]), DTW[i - 1, j - 1]) + dis1;

                                }
                                else
                                    break;
                            }
                            //textBox3.Text += "\r\n";
                            // textBox2.Text += "\r\n";
                        }

                        else
                            break;

                    }



                }




                //計算分數


                score[n] = 100 - DTWC_score1 / DTWC_score2 * 100;
                if (score[n] < 0) score[n] = 0;
                //text_scorelist.Text += "s_all:" + score + "\r\n";
                //text_score.Text += "DTWS座標分數:" + "[" + n + "]" + "\r\n" + score[n] + "\r\n";
                SDTWcoordinateScore[n] = score[n];
            }
            //sw.Stop();//碼錶停止
            //timeDSC = sw.Elapsed.TotalMilliseconds;
            //==============================
            double averageVectorScore = 0, averagecoordinateScore = 0, averageDTWvectorScore = 0, averageDTWcoordinateScore = 0,
                averageSDTWvectorScore = 0, averageSDTWcoordinateScore = 0, skeletonNum = 0;
            if (skwindow_ref.check_head.IsChecked == true)
            {
                averageVectorScore += vectorScore[2];
                averagecoordinateScore += coordinateScore[2];
                averageDTWvectorScore += DTWvectorScore[2];
                averageDTWcoordinateScore += DTWcoordinateScore[2];
                averageSDTWvectorScore += SDTWvectorScore[2];
                averageSDTWcoordinateScore += SDTWcoordinateScore[2];
                skeletonNum++;
            }
            if (skwindow_ref.check_shouldercenter.IsChecked == true)
            {
                averageVectorScore += vectorScore[3];
                averagecoordinateScore += coordinateScore[3];
                averageDTWvectorScore += DTWvectorScore[3];
                averageDTWcoordinateScore += DTWcoordinateScore[3];
                averageSDTWvectorScore += SDTWvectorScore[3];
                averageSDTWcoordinateScore += SDTWcoordinateScore[3];
                skeletonNum++;
            }
            if (skwindow_ref.check_shoulderright.IsChecked == true)
            {
                averageVectorScore += vectorScore[4];
                averagecoordinateScore += coordinateScore[4];
                averageDTWvectorScore += DTWvectorScore[4];
                averageDTWcoordinateScore += DTWcoordinateScore[4];
                averageSDTWvectorScore += SDTWvectorScore[4];
                averageSDTWcoordinateScore += SDTWcoordinateScore[4];
                skeletonNum++;
            }
            if (skwindow_ref.check_shoulderleft.IsChecked == true)
            {
                averageVectorScore += vectorScore[5];
                averagecoordinateScore += coordinateScore[5];
                averageDTWvectorScore += DTWvectorScore[5];
                averageDTWcoordinateScore += DTWcoordinateScore[5];
                averageSDTWvectorScore += SDTWvectorScore[5];
                averageSDTWcoordinateScore += SDTWcoordinateScore[5];
                skeletonNum++;
            }
            if (skwindow_ref.check_elbowright.IsChecked == true)
            {
                averageVectorScore += vectorScore[6];
                averagecoordinateScore += coordinateScore[6];
                averageDTWvectorScore += DTWvectorScore[2];
                averageDTWcoordinateScore += DTWcoordinateScore[6];
                averageSDTWvectorScore += SDTWvectorScore[6];
                averageSDTWcoordinateScore += SDTWcoordinateScore[6];
                skeletonNum++;
            }
            if (skwindow_ref.check_elbowleft.IsChecked == true)
            {
                averageVectorScore += vectorScore[7];
                averagecoordinateScore += coordinateScore[7];
                averageDTWvectorScore += DTWvectorScore[7];
                averageDTWcoordinateScore += DTWcoordinateScore[7];
                averageSDTWvectorScore += SDTWvectorScore[7];
                averageSDTWcoordinateScore += SDTWcoordinateScore[7];
                skeletonNum++;
            }
            if (skwindow_ref.check_wristright.IsChecked == true)
            {
                averageVectorScore += vectorScore[8];
                averagecoordinateScore += coordinateScore[8];
                averageDTWvectorScore += DTWvectorScore[8];
                averageDTWcoordinateScore += DTWcoordinateScore[8];
                averageSDTWvectorScore += SDTWvectorScore[8];
                averageSDTWcoordinateScore += SDTWcoordinateScore[8];
                skeletonNum++;
            }
            if (skwindow_ref.check_wristleft.IsChecked == true)
            {
                averageVectorScore += vectorScore[9];
                averagecoordinateScore += coordinateScore[9];
                averageDTWvectorScore += DTWvectorScore[9];
                averageDTWcoordinateScore += DTWcoordinateScore[9];
                averageSDTWvectorScore += SDTWvectorScore[9];
                averageSDTWcoordinateScore += SDTWcoordinateScore[9];
                skeletonNum++;
            }
            if (skwindow_ref.check_spine.IsChecked == true)
            {
                averageVectorScore += vectorScore[10];
                averagecoordinateScore += coordinateScore[10];
                averageDTWvectorScore += DTWvectorScore[10];
                averageDTWcoordinateScore += DTWcoordinateScore[10];
                averageSDTWvectorScore += SDTWvectorScore[10];
                averageSDTWcoordinateScore += SDTWcoordinateScore[10];
                skeletonNum++;
            }
            if (skwindow_ref.check_handright.IsChecked == true)
            {
                averageVectorScore += vectorScore[0];
                averagecoordinateScore += coordinateScore[0];
                averageDTWvectorScore += DTWvectorScore[0];
                averageDTWcoordinateScore += DTWcoordinateScore[0];
                averageSDTWvectorScore += SDTWvectorScore[0];
                averageSDTWcoordinateScore += SDTWcoordinateScore[0];
                skeletonNum++;
            }
            if (skwindow_ref.check_handleft.IsChecked == true)
            {
                averageVectorScore += vectorScore[1];
                averagecoordinateScore += coordinateScore[1];
                averageDTWvectorScore += DTWvectorScore[1];
                averageDTWcoordinateScore += DTWcoordinateScore[1];
                averageSDTWvectorScore += SDTWvectorScore[1];
                averageSDTWcoordinateScore += SDTWcoordinateScore[1];
                skeletonNum++;
            }
            if (skwindow_ref.check_hipcenter.IsChecked == true)
            {
                averageVectorScore += vectorScore[11];
                averagecoordinateScore += coordinateScore[11];
                averageDTWvectorScore += DTWvectorScore[11];
                averageDTWcoordinateScore += DTWcoordinateScore[11];
                averageSDTWvectorScore += SDTWvectorScore[11];
                averageSDTWcoordinateScore += SDTWcoordinateScore[11];
                skeletonNum++;
            }
            if (skwindow_ref.check_hipright.IsChecked == true)
            {
                averageVectorScore += vectorScore[12];
                averagecoordinateScore += coordinateScore[12];
                averageDTWvectorScore += DTWvectorScore[12];
                averageDTWcoordinateScore += DTWcoordinateScore[12];
                averageSDTWvectorScore += SDTWvectorScore[12];
                averageSDTWcoordinateScore += SDTWcoordinateScore[12];
                skeletonNum++;
            }
            if (skwindow_ref.check_hipleft.IsChecked == true)
            {
                averageVectorScore += vectorScore[13];
                averagecoordinateScore += coordinateScore[13];
                averageDTWvectorScore += DTWvectorScore[13];
                averageDTWcoordinateScore += DTWcoordinateScore[13];
                averageSDTWvectorScore += SDTWvectorScore[13];
                averageSDTWcoordinateScore += SDTWcoordinateScore[13];
                skeletonNum++;
            }
            if (skwindow_ref.check_kneeright.IsChecked == true)
            {
                averageVectorScore += vectorScore[14];
                averagecoordinateScore += coordinateScore[14];
                averageDTWvectorScore += DTWvectorScore[14];
                averageDTWcoordinateScore += DTWcoordinateScore[14];
                averageSDTWvectorScore += SDTWvectorScore[14];
                averageSDTWcoordinateScore += SDTWcoordinateScore[14];
                skeletonNum++;
            }
            if (skwindow_ref.check_kneeleft.IsChecked == true)
            {
                averageVectorScore += vectorScore[15];
                averagecoordinateScore += coordinateScore[15];
                averageDTWvectorScore += DTWvectorScore[15];
                averageDTWcoordinateScore += DTWcoordinateScore[15];
                averageSDTWvectorScore += SDTWvectorScore[15];
                averageSDTWcoordinateScore += SDTWcoordinateScore[15];
                skeletonNum++;
            }
            if (skwindow_ref.check_ankleright.IsChecked == true)
            {
                averageVectorScore += vectorScore[16];
                averagecoordinateScore += coordinateScore[16];
                averageDTWvectorScore += DTWvectorScore[16];
                averageDTWcoordinateScore += DTWcoordinateScore[16];
                averageSDTWvectorScore += SDTWvectorScore[16];
                averageSDTWcoordinateScore += SDTWcoordinateScore[16];
                skeletonNum++;
            }
            if (skwindow_ref.check_ankleleft.IsChecked == true)
            {
                averageVectorScore += vectorScore[17];
                averagecoordinateScore += coordinateScore[17];
                averageDTWvectorScore += DTWvectorScore[17];
                averageDTWcoordinateScore += DTWcoordinateScore[17];
                averageSDTWvectorScore += SDTWvectorScore[17];
                averageSDTWcoordinateScore += SDTWcoordinateScore[17];
                skeletonNum++;
            }
            if (skwindow_ref.check_footright.IsChecked == true)
            {
                averageVectorScore += vectorScore[18];
                averagecoordinateScore += coordinateScore[18];
                averageDTWvectorScore += DTWvectorScore[18];
                averageDTWcoordinateScore += DTWcoordinateScore[18];
                averageSDTWvectorScore += SDTWvectorScore[18];
                averageSDTWcoordinateScore += SDTWcoordinateScore[18];
                skeletonNum++;
            }
            if (skwindow_ref.check_footleft.IsChecked == true)
            {
                averageVectorScore += vectorScore[19];
                averagecoordinateScore += coordinateScore[19];
                averageDTWvectorScore += DTWvectorScore[19];
                averageDTWcoordinateScore += DTWcoordinateScore[19];
                averageSDTWvectorScore += SDTWvectorScore[19];
                averageSDTWcoordinateScore += SDTWcoordinateScore[2];
                skeletonNum++;
            }
            averageVectorScore = averageVectorScore / skeletonNum;
            averagecoordinateScore = averagecoordinateScore / skeletonNum;
            averageDTWvectorScore = averageDTWvectorScore / skeletonNum;
            averageDTWcoordinateScore = averageDTWcoordinateScore / skeletonNum;
            averageSDTWvectorScore = averageSDTWvectorScore / skeletonNum;
            averageSDTWcoordinateScore = averageSDTWcoordinateScore / skeletonNum;

            text_score.Text += "SDTW(座標):" + averageSDTWcoordinateScore + "\r\n";
                               //+"SDTW(向量):" + averageSDTWvectorScore;

            if (averageSDTWcoordinateScore < 60)
            {
                SendKeys.SendWait("{F1}");
                text_key.Text = "F1";
               
            }
            else if (averageSDTWcoordinateScore >= 60 && averageSDTWcoordinateScore < 70)
            {
                SendKeys.SendWait("{F2}");
                text_key.Text = "F2";
               
            }
            else if (averageSDTWcoordinateScore >= 70 && averageSDTWcoordinateScore < 80)
            {
                SendKeys.SendWait("{F3}");
                text_key.Text = "F3";
                
            }
            else if (averageSDTWcoordinateScore >= 80 && averageSDTWcoordinateScore < 90)
            {
                SendKeys.SendWait("{F4}");
                text_key.Text = "F4";
                
            }
            else
            {
                SendKeys.SendWait("{F5}");
                text_key.Text = "F5";
                
            }
            detectionX = 570;
            detectionY = 70;

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

            reset();

            colorcheck++;
            tm1.Start();
            tm2.Start();
            
        }
        void reset()
        {
            starttime = 0;
            waittime = 0;
            self_count = 0;
            self_time = 0;


            release();
             

             
        }
        void release()
        {
            xfar_max = new double[] { -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999 };
            xfar_min = new double[] { 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999 };
            yfar_max = new double[] { -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999 };
            yfar_min = new double[] { 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999 };
            zfar_max = new double[] { -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999, -999 };
            zfar_min = new double[] { 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999, 999 };
            
            for (int i = 0; i < 10000; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    interpolationX[i, j] = 0;
                    interpolationY[i, j] = 0;
                    interpolationZ[i, j] = 0;

                    averagingX[i, j] = 0;
                    averagingY[i, j] = 0;
                    averagingZ[i, j] = 0;
                }
            }
        }

        

        
    }
}
