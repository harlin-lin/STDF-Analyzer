using DataContainer;
using FileReader;
using Newtonsoft.Json;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace ProdLogAnalyzer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("helloworld");
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
                return;
            }

            var configPath = args[0];
            if (!File.Exists(configPath))
            {
                Console.Error.WriteLine($"Config file not found: {configPath}");
                return;
            }

            ProdLogConfiguration config;
            try
            {
                // 使用 Newtonsoft.Json 读取配置文件（支持可读性更好、兼容性更强）
                var json = File.ReadAllText(configPath, Encoding.UTF8);
                config = JsonConvert.DeserializeObject<ProdLogConfiguration>(json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to read config: {ex.Message}");
                return;
            }

            if (config?.DataFiles == null || config.DataFiles.Count == 0)
            {
                Console.WriteLine("No data files defined in configuration.");
                return;
            }

            List<SubData> subDataList = new List<SubData>();

            //var loadTasks = new List<Task<Tuple<string, int>>>();
            foreach (var df in config.DataFiles)
            {
                var path = df.Path;
                //loadTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        Console.WriteLine($"Start loading: {path}");
                        if (!File.Exists(path))
                        {
                            Console.Error.WriteLine($"Data file not found: {path}");
                            //return Tuple.Create(path, -1);
                        }

                        if (StdDB.IfExsistFile(path))
                        {
                            var existing = StdDB.GetDataAcquire(path);
                            var existFilter = existing.CreateFilter();
                            Console.WriteLine($"Already loaded: {path} -> filter {existFilter}");
                            //return Tuple.Create(path, existFilter);
                        }

                        var da = StdDB.CreateSubContainer(path);
                        using (var reader = new StdReader(path, StdFileType.STD))
                        {
                            reader.ExtractStdf();
                        }

                        var filterId = da.CreateFilter();

                        //df.Filter = DeepCopy(da.GetFilterSetup(filterId)); // 确保外部配置的 FilterSetup 不受内部修改影响

                        if (df.Filter != null)
                        {
                            // 将外部 FilterSetup 应用到新建的 filter
                            try
                            {
                                da.UpdateFilter(filterId, df.Filter);
                            } catch (Exception ex)
                            {
                                Console.Error.WriteLine($"Failed to apply FilterSetup for {path}: {ex.Message}");
                            }
                        }

                        Console.WriteLine($"Loaded {path} -> filter {filterId}");

                        subDataList.Add(new SubData(da.FilePath, filterId, da.GetFilterIndex(filterId)));

                        //return Tuple.Create(path, filterId);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Failed to load {path}: {ex.Message}");
                        //return Tuple.Create(path, -1);
                    }
                }
            }
            
            //Task.WaitAll(loadTasks.ToArray());

            Console.WriteLine("Loading completed. Summary:");
            //foreach (var t in loadTasks)
            //{
            //    var r = t.Result;
            //    Console.WriteLine($"File: {r.Item1}  FilterId: {r.Item2}");
            //}


            if(!StdDB.MergeSubData(config.Name, subDataList))
            {
                Console.Error.WriteLine("Failing at file mergering");
            }
            foreach (var df in config.DataFiles)
            {
                StdDB.RemoveFile(df.Path);
            }

            var dataAcquire = StdDB.GetDataAcquire(config.Name);

            int filterId_raw = dataAcquire.CreateFilter();
            int filterId_pass = dataAcquire.CreateFilter();
            var allBins = new List<ushort>(dataAcquire.GetHardBins());
            allBins.Remove((ushort)(config.PassBin));
            dataAcquire.UpdateFilter(filterId_pass, new FilterSetup("PassOnly") { MaskHardBins = allBins});


            DataParser.ParseDataFile(config, dataAcquire, filterId_raw, filterId_pass);

            //// 使用 Newtonsoft.Json 输出为格式化（缩进）JSON
            //try
            //{
            //    var outputPath = Path.Combine(config.OutputFolder, $"{config.Name}_output.json");
            //    var dir = Path.GetDirectoryName(outputPath);
            //    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            //    {
            //        Directory.CreateDirectory(dir);
            //    }

            //    var formatted = JsonConvert.SerializeObject(config, Formatting.Indented);
            //    File.WriteAllText(outputPath, formatted, Encoding.UTF8);
            //} catch (Exception ex)
            //{
            //    Console.Error.WriteLine($"Failed to write output JSON: {ex.Message}");
            //}
        }
    }
}
