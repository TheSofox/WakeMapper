using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace WakeMapper
{
    public partial class WakeMapper : Form
    {
        Bitmap bufferBitmap;
        Graphics drawArea;
        Graphics buffer;
        Image mapImage;
        String baseURL = "http://example.com/WakeMap";
        String onlineMessage;
        public WakeMapper()
        {
            InitializeComponent();
            myTimer.Tick += new EventHandler(getWakePosition);
            drawArea = map.CreateGraphics();
            
            mapImage = map.Image;
            bufferBitmap = new Bitmap(map.Width, map.Height);
            buffer = Graphics.FromImage(bufferBitmap);

            onlineMessage = "You can now see this at: " + baseURL;
            onlineDisplay.Text = onlineMessage;
            onlineDisplay.LinkArea = new System.Windows.Forms.LinkArea(onlineDisplay.Text.IndexOf(baseURL), onlineDisplay.Text.Length);
            onlineDisplay.Text = "";
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Boolean bInheritHandle, UInt32 dwProcessId);
        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
        byte[] lpBuffer, UIntPtr nSize, uint lpNumberOfBytesWritten);

        IntPtr Handle;
  



        private void label1_Click(object sender, EventArgs e)
        {

        }
        HttpClient client = new HttpClient();
        private void sendCoord(double north, double east)
        {
            
            client.GetAsync(baseURL + "/set.php?key=woken&n=" +north+"&e="+east);
        }

        int mode = 0;

        private Tuple<int,int> wakeToMap(double n, double e)
        {
            var scale = 4.1333 *4;
            var xo = 460/2;
            var yo = 453/2;
            var my = yo - (n / scale);

            var mx = xo + (e / scale);
            

            return Tuple.Create((int)mx, (int)my);
        }

        Pen pen = new Pen(Color.Red);
        SolidBrush brush = new SolidBrush(Color.Red);
        int lastX; int lastY;
        private void drawMap(double north, double east)
        {
            var res = wakeToMap(north, east);

            /*var srcRect = new Rectangle(lastX*4, lastY*4, 40, 40);
            var destRect = new Rectangle(lastX, lastY, 10, 10);
            drawArea.DrawImage(mapImage,destRect,srcRect,GraphicsUnit.Pixel);*/
            buffer.DrawImage(mapImage, 0, 0, map.Width, map.Height); 

            pen.Width = 5;
            //drawArea.DrawLine(pen, new Point(0, 0), new Point(100, 100));
            var radius = 5;
            // drawArea.DrawArc(pen, res.Item1, res.Item2, radius, radius, 0, 360);
            //drawArea.DrawEllipse(pen, res.Item1 - radius, res.Item2 - radius, radius * 2, radius * 2);
            buffer.FillEllipse(brush, res.Item1 - radius, res.Item2 - radius, radius * 2, radius * 2);
            
            lastX = res.Item1;
            lastY = res.Item2;

            drawArea.DrawImage(bufferBitmap, 0, 0, map.Width, map.Height);
            // drawArea.Clear(Color.Beige);

        }
        IntPtr northAddress, eastAddress;
        private void getWakePosition(Object myObject,
                                            EventArgs myEventArgs)
        {
            byte[] northBytes = new byte[8];
            ReadProcessMemory(Handle, northAddress, northBytes, (UIntPtr)northBytes.Length, 0);
            byte[] eastBytes = new byte[8];
            ReadProcessMemory(Handle, eastAddress, eastBytes, (UIntPtr)eastBytes.Length, 0);
            double north = BitConverter.ToDouble(northBytes, 0);
            double east = BitConverter.ToDouble(eastBytes, 0);
            displayOutput.Text = "Running. Wake at: " + north + "  " + east;
            if(north!=0 && east != 0)
            {
                if (onlineCheckbox.Checked)
                {
                    sendCoord(north, east);
                }
                drawMap(north, east);
            }
        }        

        private void setupWake()
        {
            var procs = Process.GetProcessesByName("AlanWake");
            if (procs == null || procs.Length == 0)
            {
                displayOutput.Text = "Alan Wake must be running for this whole thing to work.";
                return;
            }
            else
            {
                var process = procs[0];
                //displayOutput.Text = "Okay, Alan Wake is running, let's see what we can do....\n";
                foreach (ProcessModule module in process.Modules)
                {
                    if (module.ModuleName == "renderer_sf_Win32.dll")
                    {
                        //Console.WriteLine(module.FileName);
                        //Console.WriteLine("Process loaded at 0x{0:X16}", (long)process.Handle);
                        //Console.WriteLine("DLL loaded at 0x{0:X16}", (long)module.BaseAddress);

                        northAddress = IntPtr.Add(module.BaseAddress, 0x137A54);
                        eastAddress = IntPtr.Add(module.BaseAddress, 0x137A44);


                        try
                        {
                            Process[] Processes = Process.GetProcessesByName("AlanWake");
                            Process nProcess = Processes[0];
                            Handle = OpenProcess(0x10, false, (uint)nProcess.Id);
                            //getWakePosition();
                            mode = 1;
                            onlineDisplay.Visible = true;
                            displayOutput.Text = "Getting ready...";

                            // Sets the timer interval to 5 seconds.
                            myTimer.Interval = 1000;
                            myTimer.Start();
                            startButton.Text = "Stop";
                        }
                        catch
                        {
                            Console.WriteLine("Failed to open process");
                        }

                    }

                }
            }
        }
        private void stopWake()
        {
            myTimer.Stop();
            mode = 0;
            onlineDisplay.Visible = false;
            startButton.Text = "Start";
            displayOutput.Text = "Wake Mapper stopped!";
        }

        private void loop()
        {
            while (mode == 1)
            {

            }
        }
        public  void testTime(Object myObject,
                                            EventArgs myEventArgs)
        {
            Console.WriteLine("hey");
        }
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        private void button1_Click(object sender, EventArgs e)
        {

            /*var srcRect = new Rectangle(0, 0, 400, 400);
            var destRect = new Rectangle(0, 0, 100, 100);
            drawArea.DrawImage(mapImage, destRect, srcRect, GraphicsUnit.Pixel);
            return;*/
            // drawArea.Clear(Color.Beige);
            /* System.Timers.Timer aTimer = new System.Timers.Timer();
             aTimer.Elapsed += new System.Timers.ElapsedEventHandler(testTime);
             aTimer.Interval = 5000;
             aTimer.Enabled = true;*/

            if (mode == 0)
            {
                setupWake();
            }
            else
            {
                stopWake();
            }
            

            return;




        }

        private void WakeMapper_Load(object sender, EventArgs e)
        {

        }

        private void onlineCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (onlineCheckbox.Checked)
            {
                onlineDisplay.Text = onlineMessage;

            }
            else
            {
                onlineDisplay.Text = "";
            }
        }

        private void onlineDisplay_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(baseURL);

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
