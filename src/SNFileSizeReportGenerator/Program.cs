using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace SNFileSizeReportGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var appStarted = DateTime.Now;
            string appStartedString = appStarted.ToString("yyyyMMdd_HH_mm_ss");
            Log.StartTime = appStartedString;
            string folderName = "Report" + appStartedString;            

            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }
            string folderPath = System.IO.Path.Combine(folderName);
            System.IO.Directory.CreateDirectory(folderPath);
            Log.FolderPath = folderPath;

            string csvFileName = string.Empty;

            try
            {
                Log.LogRunEvent("Application started!");                
                using (SqlConnection sqlConnection1 = new SqlConnection(Config.ConnectionString))
                {
                    Log.LogRunEvent("Open connection...");
                    sqlConnection1.Open();
                    string queryFirstPart = "SELECT V.VersionId ,N.Path ,N.DisplayName ,N.Name ,[ContentType] , SPS.Name as NodeTypeName ,[FileNameWithoutExtension] ,[Extension] ,[Size] ";
                    string countQueryFirstPart = "SELECT Count(BP.BinaryPropertyId) ";
                    string baseQueryText = "FROM[accelsiors].[dbo].[BinaryProperties] as BP INNER JOIN Versions as V ON  BP.VersionId = V.VersionId INNER JOIN Nodes as N ON V.NodeId = N.NodeId  INNER JOIN SchemaPropertySets  SPS ON SPS.PropertySetId = N.NodeTypeId  ";
                    
                    //Feltételek összerakása
                    string queryFilter = string.Empty;
                    #region ExtensionPart filter

                    string extensionPart = string.Empty;
                    for (int i = 0; i < Config.Extensions.Length; i++)
                    {
                        string ext = Config.Extensions[i];
                        extensionPart += " Extension like '." + ext + "'";
                        if (i < Config.Extensions.Length - 1)
                        {
                            extensionPart += " OR";
                        }
                    }
                    #endregion
                    #region Add extensionfilter to baseQuery
                    if (Config.ExtensionsBlackList.Length > 0)
                    {
                        extensionPart = "(" + extensionPart + ") AND ";
                    }

                    if (Config.Extensions.Length > 0)
                    {
                        queryFilter += extensionPart;
                    }
                    #endregion
                    #region Blacklist Filter
                    string blacklistPart = string.Empty;
                    for (int i = 0; i < Config.ExtensionsBlackList.Length; i++)
                    {
                        string ext = Config.ExtensionsBlackList[i];
                        blacklistPart += " Extension not like '." + ext + "'";
                        if (i < Config.ExtensionsBlackList.Length - 1)
                        {
                            blacklistPart += " OR";
                        }
                    }
                    #endregion
                    #region AddBlacklist filter to BaseQuery
                    if (Config.ExtensionsBlackList.Length > 0)
                    {
                        if (Config.Extensions.Length > 0)
                        {
                            queryFilter += "(" + blacklistPart + ")";
                        }
                        else
                        {
                            queryFilter += blacklistPart;
                        }
                    }
                    #endregion
                    #region Add ContentType filter to baseQuery                                        
                    string contenTypesPart = string.Empty;
                    for (int i = 0; i < Config.ContentTypes.Length; i++)
                    {
                        string ct = Config.ContentTypes[i];
                        contenTypesPart += " SPS.Name like '" + ct + "'";
                        if (i < Config.ContentTypes.Length - 1)
                        {
                            contenTypesPart += " OR";
                        }
                    }
                    if (Config.ExcludePreviews)
                    {
                        if (Config.ContentTypes.Length > 0)
                        {
                            contenTypesPart += " AND";
                        }
                        contenTypesPart += " SPS.Name NOT like 'PreviewImage' ";
                    }
                    if (!String.IsNullOrWhiteSpace(contenTypesPart))
                    {
                        queryFilter += (Config.ExtensionsBlackList.Any() || Config.Extensions.Any() ? " AND " : string.Empty) + contenTypesPart;
                    }
                    #endregion
                    #region InTree filter
                    baseQueryText += "WHERE " +(String.IsNullOrWhiteSpace(queryFilter)? string.Empty : "(" + queryFilter + ") AND " ) + " N.Path like '" + Config.InTree + "%'";                    
                    #endregion

                    long allCount = 0;
                    Log.LogRunEvent("Get Item Count...");

                    SqlCommand cmdCount = new SqlCommand();
                    cmdCount.CommandText = countQueryFirstPart + baseQueryText;
                    cmdCount.CommandType = CommandType.Text;
                    cmdCount.CommandTimeout = Config.SQLCommandTimeoutInSeconds;
                    cmdCount.Connection = sqlConnection1;

                    allCount = long.Parse(cmdCount.ExecuteScalar().ToString());
                    Log.LogRunEvent("Fetch items (" + allCount + "):");

                    List<DataObject> resultList = new List<DataObject>();
                    for (int i = 0; i < Math.Ceiling(allCount / Config.ItemsCountInIteration); i++)
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = queryFirstPart + baseQueryText + string.Format(" Order by N.Path OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", i * Config.ItemsCountInIteration, Config.ItemsCountInIteration);
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = Config.SQLCommandTimeoutInSeconds;
                        cmd.Connection = sqlConnection1;

                        Console.WriteLine("{0}/{1}", i * Config.ItemsCountInIteration, allCount);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    resultList.Add(new DataObject()
                                    {
                                        VersionId = int.Parse(reader["VersionId"].ToString()),
                                        DisplayName = reader["DisplayName"].ToString(),
                                        Path = reader["Path"].ToString(),
                                        Name = reader["Name"].ToString(),
                                        ContentType = reader["ContentType"].ToString(),
                                        NodeTypeName = reader["NodeTypeName"].ToString(),
                                        FileNameWithoutExtension = reader["FileNameWithoutExtension"].ToString(),
                                        Extension = reader["Extension"].ToString(),
                                        Size = long.Parse(reader["Size"].ToString())
                                    });

                                }
                            }
                        }
                    }

                    //csoportosítsuk path alapján
                    Log.LogRunEvent("Process items...");

                    csvFileName = "report_" + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".csv";

                    using (var csvFile = new SNFileSizeReportGenerator.CsvFile<CsvObject>(csvFileName))
                    {
                        var groupResult = resultList.GroupBy(o => o.Path);
                        foreach (var item in groupResult)
                        {
                            var versionCount = item.Count();

                            long sumSize = 0;
                            CsvObject co = new CsvObject();
                            bool dataSetted = false;
                            foreach (var f in item)
                            {
                                Console.Write(".");
                                sumSize += f.Size;
                                if (!dataSetted)
                                {
                                    co.Path = f.Path;
                                    co.DisplayName = f.DisplayName;
                                    co.Name = f.Name;
                                    co.ContentType = f.ContentType;
                                    co.NodeTypeName = f.NodeTypeName;
                                    co.FileNameWithoutExtension = f.FileNameWithoutExtension;
                                    co.Extension = f.Extension;
                                    dataSetted = true;
                                }
                            }

                            co.Versions = versionCount;
                            co.SumSize = sumSize;
                            csvFile.Append(co);
                        }
                    }
                                        
                    Console.WriteLine(Environment.NewLine);
                    #region File uploading to given path
                    if (Config.UploadNetworkPath)
                    {
                        try
                        {
                            if (!Directory.Exists(Config.NetworkPath))
                            {
                                throw new Exception("Invalid filepath");
                            }
                            string destFile = System.IO.Path.Combine(Config.NetworkPath + "/", csvFileName);
                            File.Copy(csvFileName, destFile);
                            Log.LogRunEvent("File uploaded to given path.");
                        }
                        catch (Exception e)
                        {
                            Log.LogErrorEvent(e.ToString(), "Could not upload file to given filepath.");
                        }
                    }
                    #endregion
                    watch.Stop();
                    var appFinished = DateTime.Now;                    
                    TimeSpan scriptRuntime = watch.Elapsed;
                    Log.LogRunEvent("Script started at: " + appStarted.ToString("yyyy.MM.dd HH:mm:ss"));
                    Log.LogRunEvent("Script finished at: " + appFinished.ToString("yyyy.MM.dd HH:mm:ss"));
                    Log.LogRunEvent("Script running time was: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}", scriptRuntime.Hours, scriptRuntime.Minutes, scriptRuntime.Seconds, scriptRuntime.Milliseconds / 10));
                }
            }
            catch (Exception e)
            {
                watch.Stop();
                var appFinished = DateTime.Now;
                TimeSpan scriptRuntime = watch.Elapsed;
                Log.LogErrorEvent(e.ToString(), "Script run terminated. An error has occured.");
                Log.LogRunEvent("Script started at: " + appStarted.ToString("yyyy.MM.dd HH:mm:ss"));
                Log.LogRunEvent("Script terminated at: " + appFinished.ToString("yyyy.MM.dd HH:mm:ss"));
                Log.LogRunEvent("Script running time was: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}", scriptRuntime.Hours, scriptRuntime.Minutes, scriptRuntime.Seconds, scriptRuntime.Milliseconds / 10));               
            }
            finally
            {
                Log.LogRunEvent("Preparing e-mail.");
                try
                {
                    if (File.Exists(folderName + ".zip"))
                    {
                        File.Delete(folderName + ".zip");
                    }
                    if (File.Exists(csvFileName))
                    {
                        string destFile = System.IO.Path.Combine(folderName + "/", csvFileName);
                        File.Copy(csvFileName, destFile);
                    }

                    string zipPath = folderName + ".zip";
                    ZipFile.CreateFromDirectory(folderName, zipPath);

                    if (Email.Send(zipPath, appStarted.ToString("yyyyMMdd_HH_mm_ss")))
                    {
                        Console.WriteLine("E-mail sent.");
                    }
                    if (File.Exists(csvFileName))
                    {
                        File.Delete(csvFileName);
                    }                    
                    Directory.Delete(folderName, true);                   
                }
                catch (Exception e)
                {
                    Log.LogErrorEvent(e.ToString(), "Could not sent e-mail.");                    
                }
            }

        }
    }
}
