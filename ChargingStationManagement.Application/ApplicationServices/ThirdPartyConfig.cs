namespace ChargingStationManagement.Services.ApplicationServices
{
    internal class ThirdPartyConfig
    {
        public string Name { get; internal set; }
        public object OperatorID { get; internal set; }
        public object OperatorSecret { get; internal set; }
        public object BaseUrl { get; internal set; }
        public object Version { get; internal set; }
        public string DataSecret { get; internal set; }
        public string DataSecretIV { get; internal set; }
        public char[] SigSecret { get; internal set; }
    }
}