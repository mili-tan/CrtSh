using McMaster.Extensions.CommandLineUtils;

namespace CrtSh
{
    internal class Program
    {
        public static int RemainingReminderDays = 15;
        public static int StopReminderDays = 15;
        public static List<string> CaNameList = new() {"O=Let's Encrypt"};
        public static string TelegramBotToken = String.Empty;
        public static string TelegramChatId = String.Empty;


        static void Main(string[] args)
        {
            var cmd = new CommandLineApplication
            {
                Name = "CrtSh",
                Description = "CrtSh - CLI interface for Crt.Sh" +
                              Environment.NewLine +
                              $"Copyright (c) {DateTime.Now.Year} Milkey Tan. Code released under the MIT License"
            };
            cmd.HelpOption("-?|-h|--help");
            var isZh = Thread.CurrentThread.CurrentCulture.Name.Contains("zh");
            var queryArgument = cmd.Argument("query",
                isZh ? "输入查询识别信息（域名、组织名称等）" : " Enter an Identity (Domain Name, Organization Name, etc) ");
            var selectArgument = cmd.Argument("select",
                isZh
                    ? "要筛选的证书（全部、过期与即将过期）[all/expire/include-ca/not-include-ca]"
                    : "Certificates to filter (all, expired and expiring) [all/expire/include-ca/not-include-ca]");
            var rOption = cmd.Option("-r",
                isZh ? "当剩余多少天时开始提醒(默认15)" : "How many days are left to start reminding (Default 15)",
                CommandOptionType.SingleValue);
            var sOption = cmd.Option("-s",
                isZh ? "当过期多少天时停止提醒(默认15)" : "How many days after expiration will the reminder stop? (Default is 15)",
                CommandOptionType.SingleValue);
            var cOption = cmd.Option("-c",
                isZh ? "要筛选的签发者名称内容（以逗号分隔）" : "The issuer names to be selected contain (comma separated)",
                CommandOptionType.SingleValue);


            cmd.OnExecute(() =>
            {
                if (!queryArgument.HasValue || !selectArgument.HasValue)
                {
                    Console.WriteLine((isZh ? "缺少所需参数，请尝试: " : "Required parameter is missing, try: ") +
                                      "crtsh all example.com");
                    cmd.ShowHelp();
                    return;
                }

                if (rOption.HasValue()) RemainingReminderDays = int.Parse(rOption.Value()!);
                if (sOption.HasValue()) StopReminderDays = int.Parse(sOption.Value()!);
                if (cOption.HasValue()) CaNameList = cOption.Value()!.Split(',').ToList();

                var result = CrtshSharp.Search(queryArgument.Value!).Result;
                var selectedCerts = new List<CrtshSharp.CertificateInformation>();
                foreach (var cert in result)
                {
                    if (selectArgument.Value == "all")
                    {
                        selectedCerts.Add(cert);
                    }
                    else if (selectArgument.Value == "exp" || selectArgument.Value == "expire")
                    {
                        var days = (cert.NotAfter!.Value - DateTime.Now).Days;
                        if (days < RemainingReminderDays && days + StopReminderDays > 0)
                        {
                            selectedCerts.Add(cert);
                        }
                    }
                    else if (selectArgument.Value == "include-ca")
                    {
                        selectedCerts.AddRange(CaNameList.Where(item => cert.IssuerName!.Contains(item))
                            .Select(item => cert));
                    }
                    else if (selectArgument.Value == "not-include-ca")
                    {
                        selectedCerts.AddRange(CaNameList.Where(item => !cert.IssuerName!.Contains(item))
                            .Select(item => cert));
                    }
                }

                foreach (var item in selectedCerts)
                {
                    Console.WriteLine(item.ToString());
                    Console.WriteLine("----------------------");

                    if (!string.IsNullOrWhiteSpace(TelegramBotToken) && !string.IsNullOrWhiteSpace(TelegramChatId))
                    {
                        new HttpClient().GetStringAsync(
                            $"https://api.telegram.org/bot{TelegramBotToken}/sendMessage?chat_id={TelegramChatId}&text=" +
                            Uri.EscapeDataString(item.ToString()));
                    }
                }
            });
            cmd.Execute(args);
        }
    }
}
