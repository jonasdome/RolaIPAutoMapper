using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

class SocksUserConfig
{
    public string ProxyIp { get; set; }
    public int ProxyPort { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

class AppConfig
{
    public string DeviceName { get; set; }
    public string LocalIp { get; set ; }
    public string DNSServer { get; set; }
    public List<string> Domains { get; set; } = new();
}

class Program
{
    static readonly string UserConfigFile = "userConfig.json";
    static readonly string AppConfigFile = "config.json";

    static SocksUserConfig userConfig;
    static AppConfig appConfig;

    static Process tun2socksProcess;

    static HashSet<string> addedRoutes = new();
    static readonly TimeSpan ResolveInterval = TimeSpan.FromMinutes(30);

    static async Task Main()
    {
  

        await LoadOrPromptUserConfig();
        await LoadOrPromptAppConfig();

        //Tun2Socks 
        StartTun2Socks();
        //Aguardando tun2socks iniciar completamente antes de adicionar domínios
        await Task.Delay(1000);
        AddLocalIP();
        AddDNSServer();

        // Adicionando rotas para domínios configurados
        await StartDomainResolverLoop();

        Console.WriteLine("[INFO] Aplicação iniciada com sucesso.");

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Console.WriteLine("[FATAL] Exceção não tratada: " + (e.ExceptionObject as Exception)?.Message);
            StopTun2Socks();
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            StopTun2Socks(); // chamado em encerramento "normal" (sem Ctrl+C)
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            Console.WriteLine("[FATAL] Task com exceção não observada: " + e.Exception?.Message);
            StopTun2Socks();
            e.SetObserved(); // evita crash do processo
        };


        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("\n[EXIT] Encerrando a aplicação...");
            StopTun2Socks();
            e.Cancel = true; // evita encerramento imediato, vamos encerrar manualmente
            Environment.Exit(0);
        };

        Console.ReadKey();

        //while (!tun2socksProcess.HasExited)
        //{
        //    Console.Write("> ");
        //    var command = Console.ReadLine()?.ToLower();

        //    switch (command)
        //    {
        //        case "status":
        //            Console.WriteLine(tun2socksProcess.HasExited ? "tun2socks está parado." : "tun2socks está rodando.");
        //            break;

        //        case "restart":
        //            RestartTun2Socks();
        //            break;

        //        case "show domains":
        //            Console.WriteLine("Domínios configurados:");
        //            appConfig.Domains.ForEach(d => Console.WriteLine($"- {d}"));
        //            break;

        //        case "exit":
        //            Console.WriteLine("Encerrando...");
        //            StopTun2Socks();
        //            return;

        //        default:
        //            Console.WriteLine("Comando desconhecido. Use: status | restart | show domains | exit");
        //            break;
        //    }
        //}
    }



    static void StopTun2Socks()
    {
        if (tun2socksProcess != null && !tun2socksProcess.HasExited)
        {
            try
            {
                Console.WriteLine("[tun2socks] Encerrando processo...");
                tun2socksProcess.Kill(true); // encerra também filhos, se houver
                tun2socksProcess.WaitForExit();
                Console.WriteLine("[tun2socks] Encerrado com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] Falha ao encerrar tun2socks: {ex.Message}");
            }
        }
    }


    static async Task LoadOrPromptUserConfig()
    {
        if (File.Exists(UserConfigFile))
        {
            userConfig = JsonSerializer.Deserialize<SocksUserConfig>(await File.ReadAllTextAsync(UserConfigFile));
            Console.WriteLine("userConfig.json carregado.");
        }
        else
        {
            userConfig = new SocksUserConfig();

            Console.Write("IP do Proxy SOCKS5: ");
            userConfig.ProxyIp = Console.ReadLine();

            Console.Write("Porta: ");
            userConfig.ProxyPort = int.Parse(Console.ReadLine());

            Console.Write("Usuário (deixe em branco se não houver): ");
            userConfig.Username = Console.ReadLine();

            Console.Write("Senha (deixe em branco se não houver): ");
            userConfig.Password = Console.ReadLine();

            Console.Write("Salvar configurações de proxy em userConfig.json? (s/n): ");
            if (Console.ReadLine()?.ToLower() == "s")
            {
                var json = JsonSerializer.Serialize(userConfig, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(UserConfigFile, json);
            }
        }
    }

    static async Task LoadOrPromptAppConfig()
    {
        if (!File.Exists(AppConfigFile))
            throw new FileNotFoundException($"Arquivo de configuração '{AppConfigFile}' não encontrado.");

        try
        {
            var json = await File.ReadAllTextAsync(AppConfigFile);
            appConfig = JsonSerializer.Deserialize<AppConfig>(json)
                        ?? throw new Exception("Falha ao desserializar config.json.");
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao carregar config.json: " + ex.Message, ex);
        }
    }

    static void StartTun2Socks()
    {
        string credentials = string.IsNullOrWhiteSpace(userConfig.Username)
            ? ""
            : $"{userConfig.Username}:{userConfig.Password}@";

        string proxyArg = $"socks5://{credentials}{userConfig.ProxyIp}:{userConfig.ProxyPort}";

        tun2socksProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "tun2socks.exe",
                Arguments = $"-device {appConfig.DeviceName} -proxy {proxyArg}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        tun2socksProcess.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine("[tun2socks] " + e.Data);
        };

        tun2socksProcess.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine("[ERRO] " + e.Data);
        };

        tun2socksProcess.Start();
        tun2socksProcess.BeginOutputReadLine();
        tun2socksProcess.BeginErrorReadLine();

        Console.WriteLine($"tun2socks iniciado com device '{appConfig.DeviceName}' e proxy {proxyArg}");
    }


    static async Task StartDomainResolverLoop()
    {
        await ResolveDomains();

        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(ResolveInterval);
                await ResolveDomains();
            }
        });
    }

    static async Task ResolveDomains()
    {
        Console.WriteLine($"[DNS] Resolvendo domínios em {DateTime.Now}");

        foreach (var domain in appConfig.Domains)
        {
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(domain);

                foreach (var ip in addresses.Select(ip => ip.ToString()))
                {
                    if (addedRoutes.Contains(ip))
                        continue;

                    AddRouteForIp(ip);
                    addedRoutes.Add(ip);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] Falha ao resolver {domain}: {ex.Message}");
            }
        }
    }

    static void AddLocalIP()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"interface ipv4 set address name=\"{appConfig.DeviceName}\" source=static addr={appConfig.LocalIp} mask=255.255.255.0",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            Console.WriteLine($"[LocalIP] Local IP {appConfig.LocalIp} Adicionado a Reder Virtual {appConfig.DeviceName}");
        }
        else
        {
            Console.WriteLine($"[LocalIP] Erro ao adicionar LocalIP {appConfig.LocalIp}: {error}");
        }
    }

    static void AddDNSServer()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"interface ipv4 set dnsservers name=\"{appConfig.DeviceName}\" static address={appConfig.DNSServer} register=none validate=no ",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            Console.WriteLine($"[DNS] DNS {appConfig.DNSServer} Adicionado a Reder Virtual ${appConfig.DeviceName}");
        }
        else
        {
            Console.WriteLine($"[ERRO] Erro ao adicionar DNS {appConfig.DNSServer}: {error}");
        }
    }

    static void AddRouteForIp(string ip)
    {
        if (addedRoutes.Contains(ip))
            return;

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"interface ipv4 add route {ip}/32 {appConfig.DeviceName} 192.168.123.1 metric=1",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            Console.WriteLine($"[ROUTE] Rota adicionada: {ip}/32");
            addedRoutes.Add(ip);
        }
        else
        {
            Console.WriteLine($"[ERRO] Falha ao adicionar rota {ip}/32: {error}");
        }
    }
}