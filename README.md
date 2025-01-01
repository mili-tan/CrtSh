# CrtSh
```
crtsh example.com all
```

```
CrtSh - CLI interface for Crt.Sh
Copyright (c) 2025 Milkey Tan. Code released under the MIT License

Usage: CrtSh [options] <query> <select>

Arguments:
  query         Enter an Identity (Domain Name, Organization Name, etc)
  select        Certificates to filter (all, expired and expiring) [all/expire/include-ca/not-include-ca]

Options:
  -?|-h|--help  Show help information.
  -r            How many days are left to start reminding (Default 15)
  -s            How many days after expiration will the reminder stop? (Default is 15)
  -c            The issuer names to be selected contain (comma separated)
  --tg-token    Telegram Bot Token for reminders
  --tg-chatid   Telegram Chat ID for reminders
```
