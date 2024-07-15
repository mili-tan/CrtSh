namespace CrtSh
{
    internal class Program
    {
        public static int RemainingReminderDays = 15;
        public static int StopReminderDays = 15;


        static void Main(string[] args)
        {
            var reminderCerts = new List<CrtshSharp.CertificateInformation>();
            var result = CrtshSharp.Search("example.com").Result;
            foreach (var cert in result)
            {
                if (cert.NotAfter == null) continue;
                var days = (cert.NotAfter.Value - DateTime.Now).Days;
                if (days < RemainingReminderDays && days + StopReminderDays > 0)
                {
                    reminderCerts.Add(cert);
                }
            }
        }
    }
}
