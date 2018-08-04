namespace LicencePlate.DTOs
{
    public class StatisticalDTO
    {
        public double CpuTemp { get; set; }
        public int CpuUser { get; set; }
        public int CpuSystem { get; set; }
        public int CpuIOW { get; set; }
        public int CpuIRQ { get; set; }
        public float BatteryTemp { get; set; }
        public int BatteryLevel { get; set; }
        public string TimeSpend { get; set; }

    }
}