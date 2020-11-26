using System;
using System.Runtime.InteropServices;
using PInvoke;

// TODO: Upstream this to PInvoke.User32.
public partial class LocalUser32
{
    /// <summary>
    /// Enumeration containing the type device the raw input is coming from.
    /// </summary>
    public enum RawInputType
    {
        /// <summary>
        /// Mouse input.
        /// </summary>
        Mouse = 0,

        /// <summary>
        /// Keyboard input.
        /// </summary>
        Keyboard = 1,

        /// <summary>
        ///  Human interface device input.
        /// </summary>
        HID = 2,

        /// <summary>
        /// Another device that is not the keyboard or the mouse.
        /// </summary>
        Other = 3
    }

    /// <summary>
    /// Value type for a raw input header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RawInputHeader
    {
        /// <summary>Type of device the input is coming from.</summary>
        public RawInputType Type;
        /// <summary>Size of the packet of data.</summary>
        public int Size;
        /// <summary>Handle to the device sending the data.</summary>
        public IntPtr Device;
        /// <summary>wParam from the window message.</summary>
        public IntPtr wParam;
    }

    /// <summary>
    /// Enumeration containing the flags for raw mouse data.
    /// </summary>
    [Flags()]
    public enum RawMouseFlags : ushort
    {
        /// <summary>Relative to the last position.</summary>
        MoveRelative = 0,
        /// <summary>Absolute positioning.</summary>
        MoveAbsolute = 1,
        /// <summary>Coordinate data is mapped to a virtual desktop.</summary>
        VirtualDesktop = 2,
        /// <summary>Attributes for the mouse have changed.</summary>
        AttributesChanged = 4
    }

    /// <summary>
    /// Enumeration containing the button data for raw mouse input.
    /// </summary>
    [Flags()]
    public enum RawMouseButtons
        : ushort
    {
        /// <summary>No button.</summary>
        None = 0,
        /// <summary>Left (button 1) down.</summary>
        LeftDown = 0x0001,
        /// <summary>Left (button 1) up.</summary>
        LeftUp = 0x0002,
        /// <summary>Right (button 2) down.</summary>
        RightDown = 0x0004,
        /// <summary>Right (button 2) up.</summary>
        RightUp = 0x0008,
        /// <summary>Middle (button 3) down.</summary>
        MiddleDown = 0x0010,
        /// <summary>Middle (button 3) up.</summary>
        MiddleUp = 0x0020,
        /// <summary>Button 4 down.</summary>
        Button4Down = 0x0040,
        /// <summary>Button 4 up.</summary>
        Button4Up = 0x0080,
        /// <summary>Button 5 down.</summary>
        Button5Down = 0x0100,
        /// <summary>Button 5 up.</summary>
        Button5Up = 0x0200,
        /// <summary>Mouse wheel moved.</summary>
        MouseWheel = 0x0400
    }

    /// <summary>
    /// Contains information about the state of the mouse.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RawMouse
    {
        /// <summary>
        /// The mouse state.
        /// </summary>
        public RawMouseFlags Flags;

        [StructLayout(LayoutKind.Explicit)]
        public struct RawMouseData
        {
            [FieldOffset(0)]
            public uint Buttons;
            /// <summary>
            /// If the mouse wheel is moved, this will contain the delta amount.
            /// </summary>
            
            [FieldOffset(2)]
            public ushort ButtonData;
            /// <summary>
            /// Flags for the event.
            /// </summary>
            [FieldOffset(0)]
            public RawMouseButtons ButtonFlags;
        }

        public RawMouseData Data;

        /// <summary>
        /// Raw button data.
        /// </summary>
        public uint RawButtons;
        /// <summary>
        /// The motion in the X direction. This is signed relative motion or
        /// absolute motion, depending on the value of usFlags.
        /// </summary>
        public int LastX;
        /// <summary>
        /// The motion in the Y direction. This is signed relative motion or absolute motion,
        /// depending on the value of usFlags.
        /// </summary>
        public int LastY;
        /// <summary>
        /// The device-specific additional information for the event.
        /// </summary>
        public uint ExtraInformation;
    }

    /// <summary>
    /// Enumeration containing flags for raw keyboard input.
    /// </summary>
    [Flags]
    public enum RawKeyboardFlags : ushort
    {
        /// <summary></summary>
        KeyMake = 0,
        /// <summary></summary>
        KeyBreak = 1,
        /// <summary></summary>
        KeyE0 = 2,
        /// <summary></summary>
        KeyE1 = 4,
        /// <summary></summary>
        TerminalServerSetLED = 8,
        /// <summary></summary>
        TerminalServerShadow = 0x10,
        /// <summary></summary>
        TerminalServerVKPACKET = 0x20
    }

    /// <summary>
    /// Value type for raw input from a keyboard.
    /// </summary>    
    [StructLayout(LayoutKind.Sequential)]
    public struct RawKeyboard
    {
        /// <summary>Scan code for key depression.</summary>
        public short MakeCode;
        /// <summary>Scan code information.</summary>
        public RawKeyboardFlags Flags;
        /// <summary>Reserved.</summary>
        public short Reserved;
        /// <summary>Virtual key code.</summary>
        public VirtualKeys VirtualKey;
        /// <summary>Corresponding window message.</summary>
        public User32.WindowMessage Message;
        /// <summary>Extra information.</summary>
        public int ExtraInformation;
    }

    /// <summary>
    /// Value type for raw input from a HID.
    /// </summary>    
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RawHID
    {
        /// <summary>Size of the HID data in bytes.</summary>
        public int Size;
        /// <summary>Number of HID in Data.</summary>
        public int Count;
        /// <summary>Data for the HID.</summary>
        public fixed byte Data[1];
    }

    /// <summary>
    /// Value type for raw input.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct RawInput
    {        
        /// <summary>Header for the data.</summary>
        [FieldOffset(0)]
        public RawInputHeader Header;
        /// <summary>Mouse raw input data.</summary>
        [FieldOffset(24)]        
        public RawMouse Mouse;
        /// <summary>Keyboard raw input data.</summary>
        [FieldOffset(24)]
        public RawKeyboard Keyboard;
        /// <summary>HID raw input data.</summary>
        [FieldOffset(24)]
        public RawHID Hid;

        // The explicit offsets for these were wrong. Was 16, needs to
        // be 24.
    }

    /// <summary>
    /// Enumeration contanining the command types to issue.
    /// </summary>
    public enum RawInputCommand
    {
        /// <summary>
        /// Get input data.
        /// </summary>
        Input = 0x10000003,
        /// <summary>
        /// Get header data.
        /// </summary>
        Header = 0x10000005
    }

    /// <summary>Enumeration containing flags for a raw input device.</summary>
    [Flags()]
    public enum RawInputDeviceFlags
    {
        /// <summary>No flags.</summary>
        None = 0,
        /// <summary>If set, this removes the top level collection from the inclusion list. This tells the operating system to stop reading from a device which matches the top level collection.</summary>
        Remove = 0x00000001,
        /// <summary>If set, this specifies the top level collections to exclude when reading a complete usage page. This flag only affects a TLC whose usage page is already specified with PageOnly.</summary>
        Exclude = 0x00000010,
        /// <summary>If set, this specifies all devices whose top level collection is from the specified usUsagePage. Note that Usage must be zero. To exclude a particular top level collection, use Exclude.</summary>
        PageOnly = 0x00000020,
        /// <summary>If set, this prevents any devices specified by UsagePage or Usage from generating legacy messages. This is only for the mouse and keyboard.</summary>
        NoLegacy = 0x00000030,
        /// <summary>If set, this enables the caller to receive the input even when the caller is not in the foreground. Note that WindowHandle must be specified.</summary>
        InputSink = 0x00000100,
        /// <summary>If set, the mouse button click does not activate the other window.</summary>
        CaptureMouse = 0x00000200,
        /// <summary>If set, the application-defined keyboard device hotkeys are not handled. However, the system hotkeys; for example, ALT+TAB and CTRL+ALT+DEL, are still handled. By default, all keyboard hotkeys are handled. NoHotKeys can be specified even if NoLegacy is not specified and WindowHandle is NULL.</summary>
        NoHotKeys = 0x00000200,
        /// <summary>If set, application keys are handled.  NoLegacy must be specified.  Keyboard only.</summary>
        AppKeys = 0x00000400
    }

    /// <summary>Value type for raw input devices.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RawInputDevice
    {
        /// <summary>Top level collection Usage page for the raw input device.</summary>
        public HIDUsagePage UsagePage;
        /// <summary>Top level collection Usage for the raw input device. </summary>
        public HIDUsage Usage;
        /// <summary>Mode flag that specifies how to interpret the information provided by UsagePage and Usage.</summary>
        public RawInputDeviceFlags Flags;
        /// <summary>Handle to the target device. If NULL, it follows the keyboard focus.</summary>
        public IntPtr WindowHandle;
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct RawInputDeviceList
    {
        public IntPtr hDevice;
        public RawInputType Type;
    }

    // This is new.
    public enum RawInputDeviceInfoCommand : uint
    {
        // TODO: Finish and test preparsed data.
        /// <summary>Request a top-level collection's prepased data in a PHIDP_PREPARSED_DATA structure.</summary>
        PreparsedData = 0x20000005,
        /// <summary>Request the device interface name.</summary>
        DeviceName = 0x20000007,
        /// <summary>Request the device's info in a RID_DEVICE_INFO structure.</summary>
        DeviceInfo = 0x2000000b
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DeviceInfo
    {
        [FieldOffset(0)]
        public int Size;
        [FieldOffset(4)]
        public RawInputType Type;

        [FieldOffset(8)]
        public DeviceInfoMouse MouseInfo;
        [FieldOffset(8)]
        public DeviceInfoKeyboard KeyboardInfo;
        [FieldOffset(8)]
        public DeviceInfoHID HIDInfo;
    }

    public struct DeviceInfoMouse
    {
        public uint ID;
        public uint NumberOfButtons;
        public uint SampleRate;
    }

    public struct DeviceInfoKeyboard
    {
        public uint Type;
        public uint SubType;
        public uint KeyboardMode;
        public uint NumberOfFunctionKeys;
        public uint NumberOfIndicators;
        public uint NumberOfKeysTotal;
    }

    public struct DeviceInfoHID
    {
        public uint VendorID;
        public uint ProductID;
        public uint VersionNumber;
        public HIDUsagePage UsagePage;
        public HIDUsage Usage;
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern uint GetRawInputDeviceList(
        [In, Out] RawInputDeviceList[] RawInputDeviceList,
        ref uint NumDevices,
        int Size /* = (uint)Marshal.SizeOf(typeof(RawInputDeviceList)) */
    );

    [DllImport("user32.dll")]
    public static extern uint GetRawInputDeviceInfo(
        IntPtr deviceHandle,
        RawInputDeviceInfoCommand command,
        IntPtr data,
        ref int dataSize
    );

    [DllImport("user32.dll", CharSet=CharSet.Unicode)]
    public static extern uint GetRawInputDeviceInfo(
        IntPtr deviceHandle,
        RawInputDeviceInfoCommand command,
        char[] data,
        ref int dataSize
    );

    [DllImport("user32.dll")]
    public static extern uint GetRawInputDeviceInfo(
        IntPtr deviceHandle,
        RawInputDeviceInfoCommand command,
        ref DeviceInfo data,
        ref int dataSize
    );

    /// <summary>Function to register a raw input device.</summary>
    /// <param name="pRawInputDevices">Array of raw input devices.</param>
    /// <param name="uiNumDevices">Number of devices.</param>
    /// <param name="cbSize">Size of the RAWINPUTDEVICE structure.</param>
    /// <returns>TRUE if successful, FALSE if not.</returns>
    [DllImport("user32.dll")]
    public static extern bool RegisterRawInputDevices(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] RawInputDevice[] pRawInputDevices,
        int uiNumDevices,
        int cbSize
    );

    [DllImport("user32.dll")]
    public static extern int GetRawInputData(
        IntPtr hRawInput,
        RawInputCommand command,
        IntPtr pData,
        ref int pcbSize,
        int cbSizeHeader
    );

    [DllImport("user32.dll")]
    public static extern int GetRawInputData(
        IntPtr hRawInput,
        RawInputCommand command,
        out RawInputHeader pData,
        ref int pcbSize,
        int cbSizeHeader
    );

    [DllImport("user32.dll")]
    public static extern int GetRawInputData(
        IntPtr hRawInput,
        RawInputCommand command,
        out RawInput pData,
        ref int pcbSize,
        int cbSizeHeader
    );
}