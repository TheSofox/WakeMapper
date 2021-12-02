﻿using System;
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
using System.Reflection;

namespace WakeMapper
{
    public partial class WakeMapper : Form
    {
        Bitmap bufferBitmap;
        Graphics drawArea;
        Graphics buffer;
        Image mapImage;
        String baseURL = "";
        String urlKey = "";
        String onlineMessage;
        public WakeMapper()
        {
            InitializeComponent();
            initGameProfiles();
            myTimer.Tick += new EventHandler(getWakePosition);


            setupMap();

            mapImage = map.Image;

            onlineMessage = "You can now see this at: " + baseURL;
            onlineDisplay.Text = onlineMessage;
            onlineDisplay.LinkArea = new System.Windows.Forms.LinkArea(onlineDisplay.Text.IndexOf(baseURL), onlineDisplay.Text.Length);
            onlineDisplay.Text = "";

            if (baseURL.Length > 0)
                onlineCheckbox.Visible = true;
        }
        bool justResized = false;
        private void setupMap()
        {
            if(map.Width>0 && map.Height > 0)
            {
                drawArea = map.CreateGraphics();
                bufferBitmap = new Bitmap(map.Width, map.Height);
                buffer = Graphics.FromImage(bufferBitmap);
                justResized = true;
            }
        }

        private void map_Resize(object sender, EventArgs e)
        {
            setupMap();
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
  
        class gameLevel
        {
            public gameLevel(string n,  double s, double xOffset, double yOffset, bool sm = true)
            {
                name = n;
                scale = s;
                xo = xOffset;
                yo = yOffset;
                showMarker = sm;
            }
            public string name { get; set; }
            public double scale { get; set; }
            public double xo { get; set; }
            public double yo { get; set; }
            public bool showMarker { get; set; }

        }
        class gameProfile
        {
            private List<gameLevel> levels = new List<gameLevel>();
            public gameProfile(string n, string fn, Bitmap m, bool id, double s, double xOffset, double yOffset, bool sm = true)
            {
                name = n;
                fullName = fn;
                map = m;
                isDouble = id;
                scale = s;
                xo = xOffset;
                yo = yOffset;
                mapName = name + "Map";
                eastIndex = 0;
                heightIndex = 1;
                northIndex = 2;
            }
            public string name { get; set; }
            public string fullName { get; set; }
            public bool isDouble { get; set; }
            public double scale { get; set; }
            public double xo { get; set; }
            public double yo { get; set; }
            public bool showMarker { get; set; }
            public string mapName { get; set; }
            public Bitmap map { get; set; }

            public string dllName { get; set; }
            public Int32 offset { get; set; }
            public Int32 [] offsets { get; set; }
            public int eastIndex { get; set; }
            public int northIndex { get; set; }
            public int heightIndex { get; set; }


            public void addLevel(string name, double scale, double xOffset, double yOffset)
            {
                levels.Add(new gameLevel(name, scale, xOffset, yOffset));
            }
            public gameLevel getLevel(string name)
            {
                foreach(var level in levels)
                {
                    if (level.name.Equals(name))
                    {
                        return level;
                    }
                }
                return null;
            }

        }

        gameProfile[] gameProfiles = null;
        gameProfile currentProfile = null;
        private void initGameProfiles()
        {
            gameProfile wakeProfile = new gameProfile("AlanWake", "Alan Wake", Properties.Resources.WakeMap, true, 4.1333 * 2, 460, 453);
            wakeProfile.dllName = "renderer_sf_Win32.dll";
            wakeProfile.offset = 0x137A44;

            gameProfile firewatchProfile = new gameProfile("Firewatch", "Firewatch", Properties.Resources.FirewatchMapLarge, false, -1.93, 698 * 2, -175 * 2);
            firewatchProfile.offset = 0x01348320;
            firewatchProfile.offsets = new int[]{ 0x4B0, 0x18, 0x18, 0x10, 0x07C8 };

            gameProfile skyrimProfile = new gameProfile("SkyrimSE", "Skyrim", Properties.Resources.SkyrimMap, false, (131181 + 81244) / 507 , 1305.0 / 2.720, 1405.0 / 2.720);
            skyrimProfile.dllName = null;
            skyrimProfile.offset = 0x304BDA0;
            skyrimProfile.northIndex = 1;
            skyrimProfile.heightIndex = 2;


            gameProfile tldProfile = new gameProfile("tld", "The Long Dark", Properties.Resources.TLD_Default_Map, false, 1637.0 / 805, 98, 883);
            tldProfile.addLevel("LakeRegion", 1637.0 / 805, 98, 883);
            tldProfile.addLevel("RavineTransitionZone", 1637.0 / 805, 772, 421-332);
            tldProfile.addLevel("CoastalRegion", 2400.0 / 973, 538, 650); // 2.461538461538462, 538, 650);
            tldProfile.addLevel("WhalingStationRegion", 1100.0/915, -52, 1548);
            tldProfile.addLevel("RuralRegion", 2700.0 / 927, -28, 1017);
            tldProfile.addLevel("CrashMountainRegion", 1800.0 / 947, -25, 1000);
            tldProfile.addLevel("MarshRegion", 1900.0 / 985, 63, 885);
            tldProfile.addLevel("DamRiverTransitionZoneB", 900.0 / 890, -335, 710);
            tldProfile.addLevel("HighwayTransitionZone", 900.0 / 887, 20, 560);



            if (Environment.Is64BitProcess)
            {
                gameProfiles = new gameProfile[] { wakeProfile, firewatchProfile, tldProfile, skyrimProfile };
            }
            else
            {
                gameProfiles = new gameProfile[] { wakeProfile };
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        HttpClient client = new HttpClient();
        private void sendCoord(double x, double y)
        {
            if (!string.IsNullOrWhiteSpace(baseURL))
            {
                baseURL.TrimEnd('/');
                client.GetAsync(baseURL + "/set.php?game=" + currentProfile.mapName + "&key=" + urlKey + "&x=" + x + "&y=" + y);
            }
        }

        int mode = 0;

        private Tuple<double,double> wakeToMap(double n, double e, double mapSize)
        {
            var scale = currentProfile.scale;//4.1333 *4;
            var xo = currentProfile.xo;
            var yo = currentProfile.yo;
            var my = yo - (n / scale);

            var mx = xo + (e / scale);

            var mapScale = mapSize / 1000;

            return Tuple.Create(mx* mapScale, my* mapScale);
        }

        Pen pen = new Pen(Color.Red);
        SolidBrush brush = new SolidBrush(Color.Red);
        int lastX; int lastY;
        private void drawMap(double north, double east)
        {
            var lowest = map.Width < map.Height ? map.Width : map.Height;

            var markerSize = lowest / 100;
            if (markerSize < 4)
                markerSize = 4;

            var xBorder = map.Width > lowest ? (map.Width - lowest) / 2 : 0;
            var yBorder = map.Height > lowest ? (map.Height - lowest) / 2 : 0;

          

            var res = wakeToMap(north, east, lowest);// 500);

            buffer.DrawImage(mapImage, xBorder, yBorder, lowest, lowest);
            
            /*
            if (justResized)
            {
                buffer.DrawImage(mapImage, xBorder, yBorder, lowest, lowest);
                justResized = false;
            } else
            {
                var ratio = mapImage.Width / lowest; 
                var srcRect = new Rectangle((lastX-xBorder) * ratio, (lastY - yBorder) * ratio, markerSize * 2 *ratio, markerSize * 2 * ratio);
                var destRect = new Rectangle(lastX, lastY, markerSize*2, markerSize*2);
                buffer.DrawImage(mapImage, destRect, srcRect, GraphicsUnit.Pixel); 
            }*/

            /*
            
            drawArea.DrawImage(mapImage,destRect,srcRect,GraphicsUnit.Pixel);*/

            //buffer.DrawImage(mapImage, 0, 0, map.Width, map.Height); 

            
            //drawArea.DrawLine(pen, new Point(0, 0), new Point(100, 100));
            var radius = markerSize;
            // drawArea.DrawArc(pen, res.Item1, res.Item2, radius, radius, 0, 360);
            //drawArea.DrawEllipse(pen, res.Item1 - radius, res.Item2 - radius, radius * 2, radius * 2);
            int x = (int)res.Item1 + xBorder;
            int y = (int)res.Item2 + yBorder;

            buffer.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
            
            lastX = x;
            lastY = y;
            //buffer.FillRectangle(brush, new RectangleF(0, 0, 100, 100));

            drawArea.DrawImage(bufferBitmap, 0, 0);
            // drawArea.Clear(Color.Beige);

        }
        IntPtr northAddress, eastAddress, heightAddress = IntPtr.Zero;
        string currentLevel = "";
        private void getWakePosition(Object myObject,
                                            EventArgs myEventArgs)
        {
            bool enableMapDraw = true;
            string extraText = "";
            double north, east,height =0;
            if (currentProfile.isDouble)
            {
                byte[] northBytes = new byte[8];
                ReadProcessMemory(ProcessHandle, northAddress, northBytes, (UIntPtr)northBytes.Length, 0);
                byte[] eastBytes = new byte[8];
                ReadProcessMemory(ProcessHandle, eastAddress, eastBytes, (UIntPtr)eastBytes.Length, 0);
                if (heightAddress != IntPtr.Zero)
                {
                    byte[] heightBytes = new byte[8];
                    ReadProcessMemory(ProcessHandle, heightAddress, heightBytes, (UIntPtr)heightBytes.Length, 0);
                    height = BitConverter.ToDouble(heightBytes, 0);
                }
                north = BitConverter.ToDouble(northBytes, 0);
                east = BitConverter.ToDouble(eastBytes, 0);
            } else
            {
                if (heightAddress != IntPtr.Zero)
                {
                    byte[] heightBytes = new byte[4];
                    ReadProcessMemory(ProcessHandle, heightAddress, heightBytes, (UIntPtr)heightBytes.Length, 0);
                    height = BitConverter.ToSingle(heightBytes, 0);
                }

                byte[] northBytes = new byte[4];
                ReadProcessMemory(ProcessHandle, northAddress, northBytes, (UIntPtr)northBytes.Length, 0);
                byte[] eastBytes = new byte[4];
                ReadProcessMemory(ProcessHandle, eastAddress, eastBytes, (UIntPtr)eastBytes.Length, 0);
                north = BitConverter.ToSingle(northBytes, 0);
                east = BitConverter.ToSingle(eastBytes, 0);

                if (currentProfile.name == "tld")
                {
                    extraText = readString(tld_activeScene);
                    if (false==currentLevel.Equals(extraText))
                    {
                        currentLevel = extraText;
                        var level = currentProfile.getLevel(currentLevel);
                        if (level != null)
                        {
                            mapImage = (Image)Properties.Resources.ResourceManager.GetObject("TLD_"+level.name+"_Map");
                            currentProfile.mapName = "TLD_" + level.name + "_Map";
                            currentProfile.scale = level.scale;
                            currentProfile.xo = level.xo;
                            currentProfile.yo = level.yo;
                        }
                    }
                    if (currentProfile.getLevel(currentLevel) == null && !currentLevel.Equals(""))
                    {
                        enableMapDraw = false;
                    }
                }

                if(currentProfile.name == "SkyrimSE")
                {
                    if (!readBool(skyrim_isMapArea))
                    {
                        enableMapDraw = false;
                    }
                }
                
            }
            displayOutput.Text = "Running. You're at: " + east + " " + north + " ";
            if(!String.IsNullOrEmpty(extraText))
                displayOutput.Text+= "\n" + extraText;
            //if (false == enableMapDraw)
              //  displayOutput.Text += "\n" + "Using last known location";
            if (north!=0 && east != 0 && enableMapDraw)
            {
                if (onlineCheckbox.Checked)
                {
                    var res = wakeToMap(north, east, 1000);
                    sendCoord(res.Item1, res.Item2);
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

        public IntPtr getOffsetValue(IntPtr ProcessHandle, IntPtr addr, Int32 offset)
        {
            byte[] testBytes = new byte[4];
            ReadProcessMemory(ProcessHandle, addr, testBytes, (UIntPtr)testBytes.Length, 0);
            return IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), offset);

        }

        private void setupGame(Process process)
        {
            mapImage = currentProfile.map;
            map.Image = mapImage;
            string dllName = currentProfile.dllName;
            Int32 offset = currentProfile.offset;
            Int32[] offsets = currentProfile.offsets;
            try
            {

                IntPtr baseAddress = IntPtr.Zero;

                if (String.IsNullOrEmpty(dllName))
                {
                    baseAddress = IntPtr.Add(process.MainModule.BaseAddress, offset);
                    if (currentProfile.name == "SkyrimSE")
                    {
                        skyrim_isMapArea = IntPtr.Add(process.MainModule.BaseAddress, 0x2F4DC68);
                    }
                }
                else
                {
                    baseAddress = getProcessModuleBaseAddress(process, dllName);
                    if (baseAddress == IntPtr.Zero)
                        return;
                    baseAddress = IntPtr.Add(baseAddress, offset);
                }

                if (offsets != null && offsets.Length > 0)
                {
                    ProcessHandle = OpenProcess(0x10, false, (uint)process.Id);
                    foreach (var of in offsets)
                    {
                        baseAddress = getOffsetValue(ProcessHandle, baseAddress, of);
                    }

                }

                eastAddress = IntPtr.Add(baseAddress, (currentProfile.isDouble ? 0x8 : 0x4) * currentProfile.eastIndex);
                heightAddress = IntPtr.Add(baseAddress, (currentProfile.isDouble ? 0x8 : 0x4) * currentProfile.heightIndex);
                northAddress = IntPtr.Add(baseAddress, (currentProfile.isDouble ? 0x8 : 0x4) * currentProfile.northIndex);

                setupExecutable(process);

            }
            catch
            {
                Console.WriteLine("Failed to open process");
            }
        }
        


        IntPtr tld_gameManager, tld_activeScene, skyrim_isMapArea;
        public void setupTheLongDark(Process nProcess)
        {
            mapImage = currentProfile.map;
            map.Image = mapImage;
            currentLevel = "";
            currentProfile.mapName = "TLD_Default_Map";
            sendCoord(-100, -100);
            try
            {
                ProcessHandle = OpenProcess(0x10, false, (uint)nProcess.Id);
                mode = 1;
                IntPtr addr;
                byte[] testBytes = new byte[4];

                IntPtr modAddr = getProcessModuleBaseAddress(nProcess, "mono.dll");
                

                addr = IntPtr.Add(modAddr, 0x001F62CC);
                ReadProcessMemory(ProcessHandle, addr, testBytes, (UIntPtr)testBytes.Length, 0);
                addr = IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), 0x54);

                ReadProcessMemory(ProcessHandle, addr, testBytes, (UIntPtr)testBytes.Length, 0);
                tld_gameManager = IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), 0x3B8);

                ReadProcessMemory(ProcessHandle, tld_gameManager, testBytes, (UIntPtr)testBytes.Length, 0);
                tld_activeScene = IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), 0x3c);
                

                //AkSoundEngine.dll+1450A0
                modAddr = getProcessModuleBaseAddress(nProcess, "AkSoundEngine.dll");//IntPtr.Add(nProcess.MainModule.BaseAddress, 0x01020110);// (IntPtr)0x01348320;// IntPtr.Add(IntPtr.Zero, 0x00265C4A);// 0x137A54);//0x00265C4A


                eastAddress = IntPtr.Add(modAddr, 0x1450A0);
                heightAddress = IntPtr.Add(modAddr, 0x1450A0 + (currentProfile.isDouble ? 0x8 : 0x4));
                northAddress = IntPtr.Add(modAddr, 0x1450A0 + (currentProfile.isDouble ? 0x8 : 0x4) * 2);

                setupExecutable(nProcess);
            }
            catch
            {
                Console.WriteLine("Failed to open process");
            }
        }

        public bool readBool(IntPtr address)
        {
            byte[] testBytes = new byte[4];
            ReadProcessMemory(ProcessHandle, address, testBytes, (UIntPtr)testBytes.Length, 0);
            return BitConverter.ToBoolean(testBytes, 0);
        }

        public string readString(IntPtr address)
        {
            byte[] testBytes = new byte[4];
            ReadProcessMemory(ProcessHandle, address, testBytes, (UIntPtr)testBytes.Length, 0);
            IntPtr strAddr = IntPtr.Add((IntPtr)BitConverter.ToUInt32(testBytes, 0), 8);
            ReadProcessMemory(ProcessHandle, strAddr, testBytes, (UIntPtr)testBytes.Length, 0);
            Int32 length = BitConverter.ToInt32(testBytes, 0)*2;
            byte[] stringBytes = new byte[length];

            strAddr = IntPtr.Add(strAddr, 4);
            ReadProcessMemory(ProcessHandle, strAddr, stringBytes, (UIntPtr)stringBytes.Length, 0);

            return Encoding.Unicode.GetString(stringBytes);
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
                            case "tld":
                                setupTheLongDark(procs[0]);
                                break;
                            default:
                                setupGame(procs[0]);
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
