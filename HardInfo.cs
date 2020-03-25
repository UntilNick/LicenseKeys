namespace HardLicKey
{
    using System;
    using System.Management;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    /*
     Возможность привязки не только к железу но и к операционной системе.
     Ключ на выходе (XXXXXXXX-XXXXXXXX-XXXXXXXX-XXXXXXXX-XXXXXXXX)

     Используется так: 
     HardInfo.GetLicenseKey(); - Без привязки 
     HardInfo.GetLicenseKey(true); - С привязкой
    */

    public static class HardInfo
    {
        // Output: AuthenticAMD || 178BFBFF00100F53 ||
        // AMD Phenom™ II P820 Triple-Core Processor, AMD64 Family 16 Model 5 Stepping 3, Socket ASB2
        private static string GetProcessor(bool namescheck, string name) // "Manufacturer", "processorID", "Name"
        {
            const string NAME = "SELECT * FROM Win32_Processor";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection searcher = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject obj in searcher)
                    {
                        if (namescheck == true && name == "Name")
                        {
                            string names = obj[name]?.ToString();
                            names = names.Replace("(TM)", "™").Replace("(tm)", "™")
                            .Replace("(R)", "®").Replace("(r)", "®").Replace("(C)", "©")
                            .Replace("(c)", "©").Replace("    ", " ").Replace("  ", " ");

                            result = $"{names}, {obj["Caption"]?.ToString()}, {obj["SocketDesignation"]?.ToString()}";
                        }
                        else
                        {
                            result = obj.Properties[name]?.Value.ToString();
                        }
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        // Output: B20A826CD46BB7A1A403675C
        private static string GetHDDSerialNo()
        {
            const string NAME = "SELECT * FROM Win32_LogicalDisk";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection searcher = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject obj in searcher)
                    {
                        result += obj["VolumeSerialNumber"]?.ToString();
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        // Output: B4749FE4DE74
        private static string GetMACAddress()
        {
            const string NAME = "SELECT * FROM Win32_NetworkAdapterConfiguration";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection searcher = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject obj in searcher)
                    {
                        if ((bool)obj["IPEnabled"] == true)
                        {
                            result = obj["MacAddress"]?.ToString();
                        }
                    }
                }
                if (!string.IsNullOrEmpty(result))
                {
                    result = result.Replace(":", "");
                }
            }
            catch (Exception) { }
            return result;
        }

        // Output: SAMSUNG ELECTRONICS CO., LTD. || R425D/R525D  
        private static string GetBoardMaker(string name) // "Manufacturer", "Product"
        {
            const string NAME = "SELECT * FROM Win32_BaseBoard";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection searcher = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject obj in searcher)
                    {
                        result = obj[name]?.ToString();
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        // Output: Phoenix Technologies Ltd.  || HKJ993BB800691  ||
        // Phoenix SecureCore(tm) NB Version 01PC.M002.20110507.LEO
        private static string GetBIOSmaker(string name) // "Manufacturer", "SerialNumber", "Caption"
        {
            const string NAME = "SELECT * FROM Win32_BIOS";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection searcher = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject obj in searcher)
                    {
                        result = obj[name]?.ToString();
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        // Output: Admin
        private static string GetAccountName()
        {
            const string NAME = "SELECT * FROM Win32_UserAccount";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection searcher = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject obj in searcher)
                    {
                        result = obj["Name"]?.ToString();
                        break;
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        // Output: 8GB || 12288MB
        private static string GetPhysicalMemory()
        {
            const string NAME = "SELECT * FROM Win32_PhysicalMemory";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection searcher = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject obj in searcher)
                    {
                        string ConverterMB = Convert.ToString(Math.Round(Convert.ToDouble(obj["Capacity"]) / 0x4000_0000 * 0x3E8, 0x2)),
                               ConverterGB = Convert.ToString(Math.Round(Convert.ToDouble(obj["Capacity"]) / 0x4000_0000, 0x2));
                        result = Convert.ToDouble(obj["Capacity"]) / 0x4000_0000 <= 0x1 ? $"{ConverterMB}MB" : $"{ConverterGB}GB";
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        // Output: Microsoft Windows 7 Максимальная, 6.1.7601, 64-bit
        private static string GetOSInfo()
        {
            const string NAME = "SELECT * FROM Win32_OperatingSystem";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection searcher = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject obj in searcher)
                    {
                        result = $"{(obj["Caption"]?.ToString()).Trim()}, {obj["Version"]?.ToString()}, {obj["OSArchitecture"]?.ToString()}";
                        break;
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        // Output: ADMIN-PC
        private static string GetComputerName()
        {
            const string NAME = "SELECT * FROM Win32_ComputerSystem";
            string result = string.Empty;
            try
            {
                using (ManagementObjectCollection searcher = new ManagementObjectSearcher(NAME).Get())
                {
                    foreach (ManagementBaseObject obj in searcher)
                    {
                        result = obj["Name"]?.ToString();
                        break;
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        // Output: М58D11A1-0014536А-ТВ92320-0X9D7391-6D5839F3
        // Output with true: 8CGRB166-83F96E1B-B3BD6890-BCCDBB21-75AE5D26
        public static string GetLicenseKey(bool bind_to_windows = !true)
        {
            string info = string.Join("|", GetProcessor(false, "processorID"), GetProcessor(true, "Name"),
            GetProcessor(false, "Manufacturer"),
            GetHDDSerialNo(),
            GetMACAddress(),
            GetBoardMaker("Manufacturer"),
            GetBoardMaker("Product"),
            GetBIOSmaker("Manufacturer"),
            GetBIOSmaker("SerialNumber"),
            GetBIOSmaker("Caption"),
            GetPhysicalMemory());

            if (bind_to_windows) 
            {
                info += $"|{string.Join("|", GetOSInfo(), GetAccountName(), GetComputerName())}";
            }

            string result = string.Empty;
            string[] MsResult = null;
            try
            {
                var encoder = Encoding.UTF8.GetBytes(info);
                using (var manager = new SHA1Managed())
                {
                    byte[] ByteResult = manager.ComputeHash(encoder);
                    foreach (byte b in ByteResult)
                    {
                        result += $"{b:x2}".ToUpper();
                    }
                }
            }
            catch (Exception) { }
            try
            {
                var regex = new Regex(@"(\w{1,8})");
                MatchCollection Matches = regex.Matches(result);
                MsResult = new string[Matches.Count];
                for (int i = 0; i < Matches.Count; i++)
                {
                    MsResult[i] = Matches[i].Groups[1].Value;
                }
            }
            catch (Exception) { }
            return string.Join("-", MsResult);
        }
    }
}
