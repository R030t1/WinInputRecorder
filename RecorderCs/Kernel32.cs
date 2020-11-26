using System.Runtime.InteropServices;
using static PInvoke.Kernel32;

public partial class LocalKernel32
{
    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern void GetSystemTimePreciseAsFileTime(
        out FILETIME lpSystemTimeAsFileTime
    );
}