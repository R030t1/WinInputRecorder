using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Protobuf;
using static PInvoke.Kernel32;
using static PInvoke.User32;
using static LocalKernel32;
using static LocalUser32;

namespace Recorder
{
    public unsafe partial class MainForm : Form
    {
        protected int sz;
        protected RawInput *ri;
        protected FileStream fs;
        protected GZipStream gs;

        public MainForm()
        {
            AppDomain.CurrentDomain.ProcessExit += Exited;

            sz = 8192;
            ri = (RawInput *)Marshal.AllocHGlobal(8192);
            fs = new FileStream(
                "inputrec.dat.gz",
                System.IO.FileMode.Append,
                System.IO.FileAccess.Write
            );
            gs = new GZipStream(fs, CompressionLevel.Optimal);

            InitializeComponent();
            AllocConsole();

            // Low-level hooks do not require an injectable DLL.
            // Using WH_KEYBOARD allows detection of per-application
            // keyboard translation information; no benefit is known
            // for WH_MOUSE.
            SetWindowsHookEx(
                WindowsHookType.WH_MOUSE_LL,
                LowLevelMouseProc,
                IntPtr.Zero, 0
            );

            var regs = new RawInputDevice[] {
                new RawInputDevice {
                    UsagePage = HIDUsagePage.Generic,
                    Usage = HIDUsage.Mouse,
                    Flags = RawInputDeviceFlags.InputSink |
                        RawInputDeviceFlags.NoLegacy,
                    WindowHandle = this.Handle
                },
                new RawInputDevice {
                    UsagePage = HIDUsagePage.Generic,
                    Usage = HIDUsage.Gamepad,
                    Flags = RawInputDeviceFlags.InputSink,
                    WindowHandle = this.Handle
                },
                new RawInputDevice {
                    UsagePage = HIDUsagePage.Digitizer,
                    Usage = HIDUsage.Joystick,
                    Flags = RawInputDeviceFlags.InputSink,
                    WindowHandle = this.Handle
                }
            };

            RegisterRawInputDevices(
                regs, regs.Length,
                Marshal.SizeOf(typeof(RawInputDevice))
            );
        }

        ~MainForm()
        {
            gs.Close();
            new FileStream("didexit.dat", FileMode.Create);
        }

        public void Exited(object sender, EventArgs e)
        {
            gs.Close();
        }

        protected List<byte> EnumerateInput()
        {
            return null;
        }

        protected void ProcessInput(ref Message m)
        {
            // N.B. this is supposed to be called twice. We
            // preallocate a buffer that should not overflow.
            var rc = GetRawInputData(
                m.LParam,
                RawInputCommand.Input,
                (IntPtr)ri, ref sz,
                Marshal.SizeOf(typeof(RawInputHeader))
            );
            if (rc < 0)
                new WarningRecord {
                    Text = "GetRawInput fails with rc < 0."
                }.WriteTo(fs);

            Console.WriteLine(rc + " " + ri->Header.Type);
            if (ri->Header.Type != RawInputType.HID)
                return;

            // TODO: Use protobuf optional fields to potentially
            // save space. Many report values are zero at any given
            // time.
            FILETIME time;
            GetSystemTimePreciseAsFileTime(out time);
            switch (ri->Header.Type) {
                case RawInputType.Mouse:
                    new MouseRecord {
                        Time = time,
                        Flags = (uint)ri->Mouse.Flags,
                        ButtonFlags = (uint)ri->Mouse.Data.ButtonFlags,
                        ButtonData = ri->Mouse.Data.ButtonData,
                        Buttons = ri->Mouse.RawButtons,
                        X = ri->Mouse.LastX,
                        Y = ri->Mouse.LastY,
                        Extra = ri->Mouse.ExtraInformation
                    }.WriteTo(gs);
                    break;
                case RawInputType.Keyboard:
                    // XXX: Do not implement yet.
                    break;
                case RawInputType.HID:
                    // This means two copies. Seriously?
                    // There's a private ByteString constructor that takes
                    // an array, but nothing else.
                    var data = new byte[ri->Hid.Size * ri->Hid.Count];
                    Marshal.Copy(
                            (IntPtr)ri->Hid.Data, data,
                            0, data.Length
                    );
                    
                    new HidRecord {
                        Time = time,
                        Size = (uint)ri->Hid.Size,
                        Count = (uint)ri->Hid.Count,
                        Data = ByteString.CopyFrom(data)
                    }.WriteTo(gs);
                    break;
            }
        }

        protected int LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            WindowMessage m = (WindowMessage)wParam;
            Console.WriteLine(m);
            return CallNextHookEx(
                IntPtr.Zero, nCode, wParam, lParam
            );
        }

        protected unsafe override void WndProc(ref Message m)
        {
            switch ((WindowMessage)m.Msg) {
                case WindowMessage.WM_CLOSE:
                case WindowMessage.WM_DESTROY:
                case WindowMessage.WM_ENDSESSION:
                    gs.Close();
                    break;
                case WindowMessage.WM_INPUT:
                    ProcessInput(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}
