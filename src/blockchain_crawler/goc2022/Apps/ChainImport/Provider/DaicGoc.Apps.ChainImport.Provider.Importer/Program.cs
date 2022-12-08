using Autofac;
using Autofac.Extensions.DependencyInjection;
using DaicGoc.Domain.AppServices.ChainImport;
using DaicGoc.Domain.ChainAnalysis;
using DaicGoc.Infrastructure.ChainAnalysis.CosmosSdkChain;
using DaicGoc.Infrastructure.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace DaicGoc.Apps.ChainImport.Provider.Importer
{
    public class Program
    {
        private static IContainer DiContainer { get; set; }
        private static IConfigurationRoot Config { get; set; }

        private static void RegisterDependencyInjection(MainSettings mainSettings)
        {
            var services = new ServiceCollection();
            services.AddLogging();

            // Initialize Autofac
            var builder = new ContainerBuilder();

            builder.Populate(services);

            builder
                .RegisterAssemblyTypes(Assembly.GetAssembly(typeof(ChainAnalysisCosmosSdkChain)))
                .AsImplementedInterfaces();

            builder
                .RegisterAssemblyTypes(Assembly.GetAssembly(typeof(MySqlDatabaseConnector)))
                .AsImplementedInterfaces();

            builder
                .RegisterAssemblyTypes(Assembly.GetAssembly(typeof(ImporterAppService)))
                .AsSelf()
                .WithParameter("chain", new ChainAnalysisCosmosSdkChain(new ChainAnalysisCosmosSdkChainRepository()))
                .WithParameter("dbHost", mainSettings.DbHost)
                .WithParameter("dbName", mainSettings.DbName)
                .WithParameter("dbUser", mainSettings.DbUser)
                .WithParameter("dbPwd", mainSettings.DbPwd)
                .WithParameter("rpcHost", mainSettings.RpcHost)
                .WithParameter("rpcPort", mainSettings.RpcPort)
                .WithParameter("rpcUser", mainSettings.RpcUser)
                .WithParameter("rpcPwd", mainSettings.RpcPwd)
                .WithParameter("startBlock", mainSettings.StartBlock)
                .WithParameter("endBlock", mainSettings.EndBlock)
                .WithParameter("chain_db_prefix", mainSettings.ChainDbPrefix);

            DiContainer = builder.Build();
        }

        private static void Main(string[] args)
        {
            //Config file initialisation
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Config = configBuilder.Build();

            RegisterDependencyInjection(new MainSettings(Config["dbHost"],
                                                         Config["dbName"],
                                                         Config["dbUser"],
                                                         Config["dbPwd"],
                                                         Config["rpcHost"],
                                                         Config["rpcPort"],
                                                         Config["rpcUser"],
                                                         Config["rpcPwd"],
                                                         Config["startBlock"],
                                                         Config["endBlock"],
                                                         Config["chain_db_prefix"]));

            using (var scope = DiContainer.BeginLifetimeScope())
            {
                var appService = DiContainer.Resolve<ImporterAppService>();
                appService.RunAsync().GetAwaiter().GetResult();
            }
        }

        internal class MainSettings
        {
            public string DbHost { get; }
            public string DbName { get; }
            public string DbUser { get; }
            public string DbPwd { get; }

            public string RpcHost { get; }
            public int RpcPort { get; }
            public string RpcUser { get; }
            public string RpcPwd { get; }

            public long StartBlock { get; }
            public long EndBlock { get; }
            public string ChainDbPrefix { get; }

            public MainSettings(string dbHost, string dbName, string dbUser, string dbPwd,
                                string rpcHost, string rpcPort, string rpcUser, string rpcPwd,
                                string startBlock, string endBlock, string chainDbPrefix)
            {
                DbHost = dbHost;
                DbName = dbName;
                DbUser = dbUser;
                DbPwd = dbPwd;

                RpcHost = rpcHost;
                RpcPort = Convert.ToInt32(rpcPort);
                RpcUser = rpcUser;
                RpcPwd = rpcPwd;

                StartBlock = Convert.ToInt64(startBlock);
                EndBlock = Convert.ToInt64(endBlock);

                ChainDbPrefix = chainDbPrefix;
            }
        }
    }
}