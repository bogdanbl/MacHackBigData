using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Raven.Database;
using Raven.Database.Config;
using Raven.Database.Server;

namespace RavenWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private DocumentDatabase documentDatabase = null;
        private HttpServer httpServer = null;

        public override void Run()
        {
            Trace.WriteLine("RavenDbWorkerRole entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.WriteLine("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            Trace.WriteLine("RavenDbWorkerRole: OnStart() called", "Information");

            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;
            string connectionString = "StorageAccount";

            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                if ((RoleEnvironment.IsAvailable) && (!RoleEnvironment.IsEmulated))
                {
                    connectionString = RoleEnvironment.GetConfigurationSettingValue(configName);
                }
                else
                {
                    connectionString = "UseDevelopmentStorage=true";
                }

                configSetter(connectionString);
            });

            CloudStorageAccount storageAccount = CloudStorageAccount.FromConfigurationSetting(connectionString);
            LocalResource localCache = RoleEnvironment.GetLocalResource("RavenCache");
            CloudDrive.InitializeCache(localCache.RootPath, localCache.MaximumSizeInMegabytes);

            // let's create the cloud drive
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.GetContainerReference("drives").CreateIfNotExist();

            CloudDrive cloudDrive = storageAccount.CreateCloudDrive(
                blobClient
                .GetContainerReference("drives")
                .GetPageBlobReference("ravendb.vhd")
                .Uri.ToString()
            );

            try
            {
                // create a 1GB Virtual Hard Drive
                cloudDrive.Create(1024);
            }
            catch (CloudDriveException /*ex*/ )
            {
                // the most likely exception here is ERROR_BLOB_ALREADY_EXISTS
                // exception is also thrown if the drive already exists 
            }

            string driveLetter = cloudDrive.Mount(25, DriveMountOptions.Force);

            if (!driveLetter.EndsWith("\\"))
            {
                driveLetter += "\\";
            }

            var config = new RavenConfiguration
            {
                DataDirectory = driveLetter,
                AnonymousUserAccessMode = AnonymousUserAccessMode.All,
                HttpCompression = true,
                DefaultStorageTypeName = "munin",
                Port = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Raven"].IPEndpoint.Port,
                PluginsDirectory = "plugins"
            };

            StartRaven(config);

            return base.OnStart();
        }

        private void StartRaven(RavenConfiguration config)
        {
            try
            {
                documentDatabase = new DocumentDatabase(config);
                documentDatabase.SpinBackgroundWorkers();
                httpServer = new HttpServer(config, documentDatabase);
                try
                {
                    httpServer.StartListening();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("StartRaven Error: " + ex.ToString(), "Error");

                    if (httpServer != null)
                    {
                        httpServer.Dispose();
                        httpServer = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("StartRaven Error: " + ex.ToString(), "Error");

                if (documentDatabase != null)
                {
                    documentDatabase.Dispose();
                    documentDatabase = null;
                }
            }
        }

        private void StopRaven()
        {
            if (httpServer != null)
            {
                httpServer.Dispose();
                httpServer = null;
            }

            if (documentDatabase != null)
            {
                documentDatabase.Dispose();
                documentDatabase = null;
            }
        }
    }
}
