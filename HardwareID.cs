namespace HardLicKey
{
    using System;
    using System.Management;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /* Универсальный ключ на выходе: хххххххххххххххххх=
       Пример использования: File.WriteAllText("Result.txt", HardwareID.GET_ID);
       Требуется NetFramework 4.5 и выше.
    */

    public static class HardwareID
    {
        public static string GET_ID = ReturnHWID().Result;

        private static async Task<string> ReturnHWID()
        {
            string info = string.Join("|", GetProcessorID(), GetBiosVersion(), GetDiskDrive());
            byte[] hashedBytes = null;
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(info);
                using (var Hash = SHA256.Create())
                {
                    hashedBytes = Hash.ComputeHash(bytes);
                }

                return await Task.FromResult(Convert.ToBase64String(hashedBytes).Substring(0x19));
            }
            catch { return GET_ID; }
        }

        private static string GetProcessorID()
        {
            const string NAME = "SELECT * FROM Win32_Processor";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection ProcessorWin = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject obj in ProcessorWin)
                    {
                        result = obj["ProcessorId"]?.ToString().Substring(0, 4);
                        break;
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        private static string GetBiosVersion()
        {
            const string NAME = "SELECT * FROM Win32_BIOS";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection BiosWin = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject bios_Collection in BiosWin)
                    {
                        result = bios_Collection["Version"]?.ToString().Substring(0, 4);
                        break;
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        private static string GetDiskDrive()
        {
            const string NAME = "SELECT * FROM Win32_DiskDrive";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection BiosWin = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject hdd_Collection in BiosWin) 
                    {
                        result = hdd_Collection["Signature"]?.ToString().Substring(0, 4);
                        break;
                    }
                }
            }
            catch (Exception) { }
            return result;
        }
    }
}
