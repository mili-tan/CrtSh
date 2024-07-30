using McMaster.Extensions.CommandLineUtils;

namespace CrtSh
{
    internal class Program
    {
        public static int RemainingReminderDays = 15;
        public static int StopReminderDays = 15;
        public static List<string> CaNameList = new() {"O=Let's Encrypt"};
        public static string TelegramBotToken = string.Empty;
        public static string TelegramChatId = string.Empty;
        public static string Select = "all";


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
                    ? "要筛选的证书（全部、过期与即将过期、包含的 CA、不包含的 CA）[all/expire/include-ca/not-include-ca]"
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
            var tokenOption = cmd.Option("--tg-token",
                isZh ? "用于提醒的 Telegram Bot Token" : "Telegram Bot Token for reminders",
                CommandOptionType.SingleValue);
            var chatidOption = cmd.Option("--tg-chatid",
                isZh ? "用于提醒的 Telegram Chat ID" : "Telegram Chat ID for reminders",
                CommandOptionType.SingleValue);

            cmd.OnExecute(() =>
            {
                if (!queryArgument.HasValue)
                {
                    Console.WriteLine((isZh ? "缺少所需参数，请尝试: " : "Required parameter is missing, try: ") +
                                      "crtsh example.com all");
                    cmd.ShowHelp();
                    return;
                }

                if (rOption.HasValue()) RemainingReminderDays = int.Parse(rOption.Value()!);
                if (sOption.HasValue()) StopReminderDays = int.Parse(sOption.Value()!);
                if (cOption.HasValue()) CaNameList = cOption.Value()!.Split(',').ToList();
                if (chatidOption.HasValue()) TelegramChatId = chatidOption.Value()!.Trim();
                if (tokenOption.HasValue()) TelegramBotToken = tokenOption.Value()!.Trim();
                if (selectArgument.HasValue) Select = selectArgument.Value ?? "all";

                var result = CrtshSharp.Search(queryArgument.Value!).Result;
                var selectedCerts = new List<CrtshSharp.CertificateInformation>();
                foreach (var cert in result)
                {
                    if (Select == "all")
                    {
                        selectedCerts.Add(cert);
                    }
                    else if (Select == "exp" || Select == "expire")
                    {
                        var days = (cert.NotAfter!.Value - DateTime.Now).Days;
                        if (days < RemainingReminderDays && days + StopReminderDays > 0)
                        {
                            selectedCerts.Add(cert);
                        }
                    }
                    else if (Select == "include-ca")
                    {
                        selectedCerts.AddRange(CaNameList.Where(item => cert.IssuerName!.Contains(item))
                            .Select(item => cert));
                    }
                    else if (Select == "not-include-ca")
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
