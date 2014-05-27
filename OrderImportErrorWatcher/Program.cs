/** 
 * This file is part of the OrderImportErrorWatcher project.
 * Copyright (c) 2014 Dai Nguyen
 * Author: Dai Nguyen
**/

using Newtonsoft.Json;
using OrderImportErrorWatcher.DataAccess;
using OrderImportErrorWatcher.Models;
using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Text.RegularExpressions;

namespace OrderImportErrorWatcher
{
    class Program
    {
        static CancellationTokenSource _source;
        static FileSystemWatcher _watcher;
        static Timer _timer;

        static void Main(string[] args)
        {
            Console.WriteLine(@"
Order Import Error Watcher  Copyright (C) 2014  Dai Nguyen
This program comes with ABSOLUTELY NO WARRANTY.
This is free software, and you are welcome to redistribute it
under certain conditions.
");

            Initialize();
                        
            Console.WriteLine("Press any key to exit");

            Console.ReadKey();
            _source.Cancel();
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _timer.Dispose();

            ClearAll();
        }

        static async void ClearAll()
        {
            using (FileImportService service = new FileImportService())
            {
                foreach (var file in await service.All().ToListAsync())
                {
                    Console.WriteLine(file.FileName);
                    await service.DeleteAsync(file, _source.Token);
                }
            }
        }

        static void Initialize()
        {
            _source = new CancellationTokenSource();

            Config config = GetConfig();

            if (config == null)
                return;

            _timer = new Timer(new TimerCallback(CheckImportStatusAsync), null, 1000, config.WaitInSeconds * 1000);            
            
            _watcher = new FileSystemWatcher();
            _watcher.Filter = "*.txt";
            _watcher.Path = config.ActiveFolder;
            _watcher.Deleted += new FileSystemEventHandler(OnChanged);
            _watcher.Created += new FileSystemEventHandler(OnChanged);
            _watcher.EnableRaisingEvents = true;            
        }

        private static async void CheckImportStatusAsync(object state)
        {
            var config = GetConfig();

            if (config == null)
                return;

            using (FileImportService service = new FileImportService())
            {
                var list = await service.GetReadyToCheckImports(config.WaitInSeconds);

                foreach (var file in list)
                {
                    file.DateChecked = DateTime.Now;
                    
                    string sumfile = Path.Combine(config.SummaryFolder, Path.GetFileNameWithoutExtension(file.FileName) + ".sum");

                    if (File.Exists(sumfile) && IsImportSucceeded(sumfile))
                    {
                        file.Result = "Imported";
                        Console.WriteLine(string.Format("{0} imported successfully", file.FileName));                  
                    }
                    else
                    {
                        file.Result = "Failed";

                        string body = GetAllErrors(file.FileName, config.ErrorFolder, config.SummaryFolder);                        
                        bool emailed = await service.SendSmtpEmailAsync(config as SmtpConfig,
                                    string.Format("Import Error - {0}", file.FileName), body);
                        
                        Console.WriteLine(string.Format("{0} failed import", file.FileName));
                    }

                    await service.UpdateAsync(file, _source.Token);
                }
            }            
        }

        private static string GetAllErrors(string file, string errorFolder, string sumFolder)
        {
            StringBuilder builder = new StringBuilder();

            string sumfile = Path.Combine(sumFolder, Path.GetFileNameWithoutExtension(file) + ".sum");

            if (File.Exists(sumfile))
            {
                builder.AppendLine(@"<span style=""font-weight:bold"">" + sumfile + "</span><br />");
                builder.AppendLine("<p>" + File.ReadAllText(sumfile).Replace("\n", "<br />") + "</p>");
            }

            Match m = Regex.Match(file, @"\d+");

            foreach (string errorfile in Directory.GetFiles(errorFolder, string.Format("*{0}.err", m.Value.ToString())))
            {
                builder.AppendLine(@"<span style=""font-weight:bold"">" + errorfile + "</span><br />");
                builder.AppendLine("<p>" + File.ReadAllText(errorfile).Replace("\n", "<br />") + "</p>");
            }

            return builder.ToString();
        }

        private static bool IsImportSucceeded(string sumfile)
        {
            return File.ReadAllLines(sumfile).Any(t => t.Contains("<order_number 1>"));
        }

        private static async void OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created && e.Name.StartsWith("WOH"))
            {
                using (FileImportService service = new FileImportService())
                {
                    var succeeded = await service.CreateAsync(new FileImport
                    {
                        FileName = e.Name
                    }, _source.Token);
                    
                    string msg = succeeded != null ? "is created and saved" : "is created but FAILED to save";                    
                    Console.WriteLine(string.Format("{0} {1}", e.Name, msg));
                }
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted && e.Name.StartsWith("WOH"))
            {
                using (FileImportService service = new FileImportService())
                {
                    var found = await service.GetByFileNameAsync(e.Name);

                    if (found != null)
                    {
                        found.DateDeleted = DateTime.Now;
                        var succeeded = await service.UpdateAsync(found, _source.Token);

                        string msg = succeeded != null ? "is deleted and updated" : "is deleted but FAILED to update";
                        Console.WriteLine(string.Format("{0} {1}", e.Name, msg));
                    }
                }
            }
        }

        static Config GetConfig()
        {
            try
            {
                string current = Path.Combine(Directory.GetCurrentDirectory(), "Data");
                string filename = Path.Combine(current, "Config.json");

                if (File.Exists(filename))
                {                    
                    return JsonConvert.DeserializeObject<Config>(File.ReadAllText(filename));                    
                }                
            }
            catch { }
            return null;
        }
    }
}
