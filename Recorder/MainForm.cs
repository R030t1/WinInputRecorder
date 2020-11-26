using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

        public MainForm()
        {
            sz = 8192;
            ri = (RawInput *)Marshal.AllocHGlobal(8192);
            fs = new FileStream(
                "inputrec.dat",
                System.IO.FileMode.Create,
                System.IO.FileAccess.Write
            );

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
            // TODO: Record overflow in the log.

            Console.WriteLine(rc + " " + ri->Header.Type);
            if (ri->Header.Type != RawInputType.HID)
                return;

            // This means two copies. Seriously?
            var data = new byte[ri->Hid.Size * ri->Hid.Count];
            Marshal.Copy(
                    (IntPtr)ri->Hid.Data, data,
                    0, data.Length
            );
                    
            FILETIME time;
            GetSystemTimePreciseAsFileTime(out time);
            new HidRecord {
                Time = time,
                Size = (uint)ri->Hid.Size,
                Count = (uint)ri->Hid.Count,
                Data = ByteString.CopyFrom(data)
            }.WriteTo(fs);
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
