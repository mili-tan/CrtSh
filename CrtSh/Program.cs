using System.Runtime.ConstrainedExecution;
using System.Text.Json;
using McMaster.Extensions.CommandLineUtils;

namespace CrtSh
{
    internal class Program
    {
        public static int RemainingReminderDays = 15;
        public static int StopReminderDays = 15;


        static void Main(string[] args)
        {
            var cmd = new CommandLineApplication
            {
                Name = "CrtSh",
                Description = "CrtSh - CLI interface for Crt.Sh\r\n" +
                              Environment.NewLine +
                              $"Copyright (c) {DateTime.Now.Year} Milkey Tan. Code released under the MIT License"
            };
            cmd.HelpOption("-?|-h|--help");
            var isZh = Thread.CurrentThread.CurrentCulture.Name.Contains("zh");
            var selectArgument = cmd.Argument("select",
                isZh ? "要筛选的证书（全部、过期与即将过期）[all/exp]" : "Certificates to filter (all, expired and expiring) [all/exp]");
            var queryArgument = cmd.Argument("query",
                isZh ? "输入查询识别信息（域名、组织名称等）" : " Enter an Identity (Domain Name, Organization Name, etc), ");

            cmd.OnExecute(() =>
            {
                if (!queryArgument.HasValue || !queryArgument.HasValue)
                {
                    Console.WriteLine((isZh ? "缺少所需参数，请尝试: " : "Required parameter is missing, try: ") +
                                      "crtsh all example.com");
                    cmd.ShowHelp();
                    return;
                }

                var result = CrtshSharp.Search(queryArgument.Value!).Result;
                var selectedCerts = new List<CrtshSharp.CertificateInformation>();
                foreach (var cert in result)
                {
                    if (selectArgument.Value == "all")
                    {
                        selectedCerts.Add(cert);
                    }
                    else if (selectArgument.Value == "exp")
                    {
                        var days = (cert.NotAfter!.Value - DateTime.Now).Days;
                        if (days < RemainingReminderDays && days + StopReminderDays > 0)
                        {
                            selectedCerts.Add(cert);
                        }
                    }
                }

                foreach (var item in selectedCerts)
                {
                    Console.WriteLine(item.ToString());
                    Console.WriteLine("----------------------");
                }
            });
            cmd.Execute(args);
        }
    }
}
