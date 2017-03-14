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
            initGameProfiles();
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
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll", SetLastError = true)]
        static public extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32.dll")]
        static public extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll")]
        static public extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        static public extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

        public const short INVALID_HANDLE_VALUE = -1;

        [Flags]
        public enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct MODULEENTRY32
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            public IntPtr modBaseAddr;
            public uint modBaseSize;
            IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExePath;
        };

        
        IntPtr ProcessHandle;
  
        class gameProfile
        {
            public gameProfile(string n, string fn, bool id, double s, double xOffset, double yOffset)
            {
                name = n;
                fullName = fn;
                isDouble = id;
                scale = s;
                xo = xOffset;
                yo = yOffset;
            }
            public string name { get; set; }
            public string fullName { get; set; }
            public bool isDouble { get; set; }
            public double scale { get; set; }
            public double xo { get; set; }
            public double yo { get; set; }

        }

        gameProfile[] gameProfiles = null;
        gameProfile currentProfile = null;
        private void initGameProfiles()
        {
            if (Environment.Is64BitProcess)
            {
                gameProfiles = new gameProfile[] {
                new gameProfile("AlanWake", "Alan Wake", true, 4.1333 *4,460/2, 453/2),
                new gameProfile("Firewatch", "Firewatch", false, -1.93*2,698, -175)
            };
            }
            else
            {
                gameProfiles = new gameProfile[] {
                new gameProfile("AlanWake", "Alan Wake", true, 4.1333 *4,460/2, 453/2)
            };
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        HttpClient client = new HttpClient();
        private void sendCoord(double north, double east)
        {

            client.GetAsync(baseURL + "/set.php?game=" + currentProfile.name + "&key=woken&n=" + north + "&e=" + east);
        }

        int mode = 0;

        private Tuple<int,int> wakeToMap(double n, double e)
        {
            var scale = currentProfile.scale;//4.1333 *4;
            var xo = currentProfile.xo;
            var yo = currentProfile.yo;
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
            double north, east;
            if (currentProfile.isDouble)
            {
                byte[] northBytes = new byte[8];
                ReadProcessMemory(ProcessHandle, northAddress, northBytes, (UIntPtr)northBytes.Length, 0);
                byte[] eastBytes = new byte[8];
                ReadProcessMemory(ProcessHandle, eastAddress, eastBytes, (UIntPtr)eastBytes.Length, 0);
                north = BitConverter.ToDouble(northBytes, 0);
                east = BitConverter.ToDouble(eastBytes, 0);
            } else
            {
                byte[] northBytes = new byte[4];
                ReadProcessMemory(ProcessHandle, northAddress, northBytes, (UIntPtr)northBytes.Length, 0);
                byte[] eastBytes = new byte[4];
                ReadProcessMemory(ProcessHandle, eastAddress, eastBytes, (UIntPtr)eastBytes.Length, 0);
                north = BitConverter.ToSingle(northBytes, 0);
                east = BitConverter.ToSingle(eastBytes, 0);
            }
            displayOutput.Text = "Running. You're at: " + north + "  " + east;
            if(north!=0 && east != 0)
            {
                if (onlineCheckbox.Checked)
                {
                    sendCoord(north, east);
                }
                drawMap(north, east);
            }
        }        

        private void setupExecutable(Process nProcess)
        {
            displayOutput.Text = "Setting up executable....\n";
            try
            {
                ProcessHandle = OpenProcess(0x10, false, (uint)nProcess.Id);
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
        private IntPtr getProcessModuleBaseAddress(Process process, string name)
        {
            var snapshot = CreateToolhelp32Snapshot(SnapshotFlags.Module | SnapshotFlags.Module32, (uint)process.Id);
            MODULEENTRY32 mod = new MODULEENTRY32() { dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32)) };
            if (!Module32First(snapshot, ref mod))
                return IntPtr.Zero;

            List<string> modules = new List<string>();
            do
            {
                if (mod.szModule == name)
                {
                    return mod.modBaseAddr;
                }
                modules.Add(mod.szModule);
            }
            while (Module32Next(snapshot, ref mod));
            return IntPtr.Zero;
        }

        private void setupWake(Process process)
        {
            mapImage = Properties.Resources.WakeMap;
            //displayOutput.Text = "Okay, Alan Wake is running, let's see what we can do....\n";
            IntPtr modAddr = getProcessModuleBaseAddress(process, "renderer_sf_Win32.dll");
            if (modAddr != IntPtr.Zero)
            {
                northAddress = IntPtr.Add(modAddr, 0x137A54);
                eastAddress = IntPtr.Add(modAddr, 0x137A44);

                setupExecutable(process);

            }
             /*
            hModuleSnap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, dwPID)

           
            foreach (ProcessModule module in process.Modules)
                {
                    if (module.ModuleName == "renderer_sf_Win32.dll")
                    {
                        //Console.WriteLine(module.FileName);
                        //Console.WriteLine("Process loaded at 0x{0:X16}", (long)process.Handle);
                        //Console.WriteLine("DLL loaded at 0x{0:X16}", (long)module.BaseAddress);

                        northAddress = IntPtr.Add(module.BaseAddress, 0x137A54);
                        eastAddress = IntPtr.Add(module.BaseAddress, 0x137A44);

                        setupExecutable(process);



                    }

                }*/

        }
        private void stopExecutable()
        {
            myTimer.Stop();
            mode = 0;
            onlineDisplay.Visible = false;
            startButton.Text = "Start";
            displayOutput.Text = "Mapper stopped!";
            currentProfile = null;
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



        public void setupFirewatch(Process nProcess)
        {
            mapImage = Properties.Resources.FirewatchMapLarge;
            try
            {
                //  var mod = nProcess.MainModule.BaseAddress;

                // displayOutput.Text = "works." + mod; 
                // VAMemory vam = new VAMemory("Firewatch");


                ProcessHandle = OpenProcess(0x10, false, (uint)nProcess.Id);
                mode = 1;


                IntPtr addr = IntPtr.Add(nProcess.MainModule.BaseAddress, 0x01348320);// (IntPtr)0x01348320;// IntPtr.Add(IntPtr.Zero, 0x00265C4A);// 0x137A54);//0x00265C4A

                byte[] testBytes = new byte[4];
                ReadProcessMemory(ProcessHandle, addr, testBytes, (UIntPtr)testBytes.Length, 0);

                addr = IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), 0x4B0);
                ReadProcessMemory(ProcessHandle, addr, testBytes, (UIntPtr)testBytes.Length, 0);

                addr = IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), 0x18);
                ReadProcessMemory(ProcessHandle, addr, testBytes, (UIntPtr)testBytes.Length, 0);

                addr = IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), 0x18);
                ReadProcessMemory(ProcessHandle, addr, testBytes, (UIntPtr)testBytes.Length, 0);

                addr = IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), 0x10);
                ReadProcessMemory(ProcessHandle, addr, testBytes, (UIntPtr)testBytes.Length, 0);

                eastAddress = IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), 0x07C8);
                northAddress = IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), 0x07D0);

                ReadProcessMemory(ProcessHandle, addr, testBytes, (UIntPtr)testBytes.Length, 0);

                //IntPtr Base1 = IntPtr.Add((IntPtr)vam.ReadInt32(BaseAddress), 0x58);


                //displayOutput.Text = BitConverter.ToSingle(testBytes,0).ToString();
                setupExecutable(nProcess);



            }
            catch
            {
                Console.WriteLine("Failed to open process");
            }


        }


        private void button1_Click(object sender, EventArgs e)
        {
            //Environment.Is64BitProcess

            if (mode == 0)
            {
                foreach (var game in gameProfiles)
                {
                    var procs = Process.GetProcessesByName(game.name);
                    if (procs == null || procs.Length == 0)
                    {
                        continue;
                        //
                    } else
                    {
                        currentProfile = game;
                        switch (currentProfile.name)
                        {
                            case "AlanWake":
                                setupWake(procs[0]);
                                break;
                            case "Firewatch":
                                setupFirewatch(procs[0]);
                                break;
                            default:
                                continue;


                        }
                        break;
                    }
                }
                if (currentProfile == null)
                {
                    var gameList = String.Join(" or ", gameProfiles.Select(x => x.fullName));
                    
                    displayOutput.Text = gameList + " must be running for this whole thing to work.";
                    return;
                }

            }
            else
            {
                stopExecutable();
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
