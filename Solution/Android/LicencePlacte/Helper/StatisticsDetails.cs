using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Java.IO;
using Java.Lang;
using LicencePlacte.DTOs;
using System;
using System.Collections.Generic;

namespace LicencePlacte.Helper
{
    public class StatisticsDetails
    {
        static readonly object _syncLock = new object();
        static string  result = "";
        static StatisticalDTO resultStatistic = new StatisticalDTO();

        public static StatisticalDTO GetDetailsResult()
        {
            GetCPUDetails();
            GetCpuUsageStatistic();
            GetBatteryDetails();            
            return resultStatistic;
            
        }

        private static void GetBatteryDetails()
        {
            lock (_syncLock)
            {
                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                {
                    using (var batteryService = Application.Context.RegisterReceiver(null, filter))
                    {
                        //Intent receiver = context.RegisterReceiver(null, new IntentFilter(Intent.ActionBatteryChanged));
                        if (batteryService != null)
                        {
                            var tempExtra = batteryService.GetIntExtra(BatteryManager.ExtraTemperature, 0) / 10;
                            var level = batteryService.GetIntExtra(BatteryManager.ExtraLevel, 0);

                            result += "Battery Temp: " + tempExtra.ToString() + "oC\nBattery level: " + level + "%";
                            resultStatistic.BatteryTemp = tempExtra;
                            resultStatistic.BatteryLevel = level;
                        }
                    }
                }
            }
        }

        private static void GetCPUDetails()
        {
            Java.Lang.Process p;
            try
            {
                p = Runtime.GetRuntime().Exec("cat sys/class/thermal/thermal_zone0/temp");
                p.WaitForAsync();

                BufferedReader reader = new BufferedReader(new InputStreamReader(p.InputStream));

                string line = reader.ReadLine();
                float temp = Float.ParseFloat(line) / 1000.0f;
                resultStatistic.CpuTemp = temp;
                result = "CPU Temp: " + temp.ToString() + "oC\n";

            }
            catch (System.Exception e)
            {
                //return 0.0f;
            }
        }

        private static string ExecuteTop()
        {
            Java.Lang.Process p = null;
            BufferedReader reader;
            string returnString = null;
            try
            {
                p = Runtime.GetRuntime().Exec("top -n 1");
                reader = new BufferedReader(new InputStreamReader(p.InputStream));
                while (returnString == null || returnString.Equals(""))
                {
                    returnString = reader.ReadLine();
                }
            }
            catch (System.IO.IOException e)
            {

            }
            finally
            {
                try
                {
                    //reader.Close();
                    p.Destroy();
                }
                catch (System.IO.IOException e)
                {
                    //Log.e("executeTop",
                    //        "error in closing and destroying top process");
                    //e.printStackTrace();
                }
            }
            return returnString;
        }


        public static void GetCpuUsageStatistic()
        {

            string tempString = ExecuteTop();

            tempString = tempString.Replace(",", "");
            tempString = tempString.Replace("User", "");
            tempString = tempString.Replace("System", "");
            tempString = tempString.Replace("IOW", "");
            tempString = tempString.Replace("IRQ", "");
            tempString = tempString.Replace("%", "");
            for (int i = 0; i < 10; i++)
            {
                tempString = tempString.Replace("  ", " ");
            }
            tempString = tempString.Trim();
            var myString = tempString.Split(new string[] { " " }, StringSplitOptions.None);
            int[] cpuUsageAsInt = new int[myString.Length];
            for (int i = 0; i < myString.Length; i++)
            {
                myString[i] = myString[i].Trim();
                cpuUsageAsInt[i] = Integer.ParseInt(myString[i]);
            }
            result += "CPU Usage - User: " + cpuUsageAsInt[0].ToString() + "%\n";
            result += "CPU Usage - System: " + cpuUsageAsInt[1].ToString() + "%\n";
            result += "CPU Usage - IOW: " + cpuUsageAsInt[2].ToString() + "%\n";
            result += "CPU Usage - IRQ: " + cpuUsageAsInt[3].ToString() + "%\n";

            resultStatistic.CpuUser = cpuUsageAsInt[0];
            resultStatistic.CpuSystem = cpuUsageAsInt[1];
            resultStatistic.CpuIOW = cpuUsageAsInt[2];
            resultStatistic.CpuIRQ = cpuUsageAsInt[3];
        }

    }
}