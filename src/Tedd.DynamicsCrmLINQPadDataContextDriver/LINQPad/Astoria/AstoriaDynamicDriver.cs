using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Net;
using System.Windows;
using LINQPad.Extensibility.DataContext;
using Microsoft.Xrm.Tooling.Connector;
using Tedd.DynamicsCrmLINQPadDataContextDriver.Models;
using Tedd.DynamicsCrmLINQPadDataContextDriver.ViewModels;
using Tedd.DynamicsCrmLINQPadDataContextDriver.Views;

namespace Tedd.DynamicsCrmLINQPadDataContextDriver.LINQPad.Astoria
{
    public class AstoriaDynamicDriver : DynamicDataContextDriver
    {
        public AstoriaDynamicDriver() : base()
        {
            //AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
            //{
            //    MessageBox.Show("First chance:\r\n" + args.Exception.ToString());
            //    Debugger.Break();
            //};
            //AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            //{
            //    MessageBox.Show("Unhandled:\r\n" + args.ExceptionObject.ToString());
            //    Debugger.Break();
            //};
        }

        public override string Name { get { return "Dynamics CRM DataContext Driver"; } }
        public override string Author { get { return "Tedd Hansen"; } }

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            // The URI of the service best describes the connection:
            var c = new ConnectionData(cxInfo);
            return c.Username + "@" + c.OrganizationUrl;
        }
        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            // We need to pass the chosen URI into the DataServiceContext's constructor:
            return new[] { new ParameterDescriptor("serviceRoot", "Microsoft.Xrm.Tooling.Connector.CrmServiceClient") };
            //return new[] { new ParameterDescriptor("serviceRoot", "System.Uri") };
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            // We need to pass the chosen URI into the DataServiceContext's constructor:
            //return new object[] { new Microsoft.Xrm.Tooling.Connector.CrmServiceClient(new ConnectionData(cxInfo).GetConnectionString()) };
            //Debugger.Break();
            var cd = new ConnectionData(cxInfo);
            return new object[] { new Microsoft.Xrm.Tooling.Connector.CrmServiceClient(
                credential: (NetworkCredential)cd.GetCredentials(),
                authType:cd.AuthenticationType,
                hostName:cd.GetOrganizationUrl_Host(),
                port:cd.GetOrganizationUrl_Port().ToString(),
                orgName:cd.GetOrganizationUrl_Organization(),
                useSsl:cd.GetOrganizationUrl_UsingSSL()
                ) };


        }
        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            var ConnectionData = new ConnectionData(cxInfo);
            // Prompt the user for connection string
            var dialog = new ConnectionDialog(cxInfo);
            var connectionDialogViewModel = new ConnectionDialogViewModel();
            connectionDialogViewModel.ConnectionData = ConnectionData;
            dialog.DataContext = connectionDialogViewModel;
            var dialogResult = dialog.ShowDialog();
            cxInfo.DisplayName = $"{ConnectionData.Username} @ {ConnectionData.OrganizationUrl}";
            return dialogResult == true;
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild,
            ref string nameSpace, ref string typeName)
        {

            return SchemaBuilder.GetSchemaAndBuildAssembly(
                cxInfo,
                assemblyToBuild,
                ref nameSpace,
                ref typeName);
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            //    // This method gets called after a DataServiceContext has been instantiated. It gives us a chance to
            //    // perform further initialization work.
            //    //
            //    // And as it happens, we have an interesting problem to solve! The typed data service context class
            //    // that Astoria's EntityClassGenerator generates handles the ResolveType delegate as follows:
            //    //
            //    //   return this.GetType().Assembly.GetType (string.Concat ("<namespace>", typeName.Substring (19)), true);
            //    //
            //    // Because LINQPad subclasses the typed data context when generating a query, GetType().Assembly returns
            //    // the assembly of the user query rather than the typed data context! To work around this, we must take
            //    // over the ResolveType delegate and resolve using the context's base type instead:
            //    MessageBox.Show("Here");
            //    Debugger.Break();
            //    var dsContext = (DataServiceContext)context;
            //var typedDataServiceContextType = context.GetType().BaseType;
            var xrmContext = (Microsoft.Xrm.Sdk.Client.OrganizationServiceContext)context;



            //    dsContext.ResolveType = name => typedDataServiceContextType.Assembly.GetType
            //        (typedDataServiceContextType.Namespace + "." + name.Split('.').Last());

            //    // The next step is to feed any supplied credentials into the Astoria service.
            //    // (This could be enhanced to support other authentication modes, too).
            //    var props = new ConnectionData(cxInfo);
            //    dsContext.Credentials = props.GetCredentials();

            //    // Finally, we handle the SendingRequest event so that it writes the request text to the SQL translation window:
            //    dsContext.SendingRequest += (sender, e) => executionManager.SqlTranslationWriter.WriteLine(e.Request.RequestUri);
        }

        /// <summary>Returns true if two <see cref="IConnectionInfo"/> objects are semantically equal.</summary>
        public override bool AreRepositoriesEquivalent(IConnectionInfo r1, IConnectionInfo r2)
        {
            // Two repositories point to the same endpoint if their URIs are the same.
            return object.Equals(
                string.Join("|",
                    r1.DriverData.Element("OrganizationUrl"),
                    r1.DriverData.Element("Username"),
                    r1.DriverData.Element("Password")
                ),
                string.Join("|",
                    r2.DriverData.Element("OrganizationUrl"),
                    r2.DriverData.Element("Username"),
                    r2.DriverData.Element("Password")
                )
            );
        }
        #region Assemblies
        public override IEnumerable<string> GetAssembliesToAdd()
        {
            // We need the following assembly for compiliation and autocompletion:
            return new[] {
                //"Microsoft.Crm.Outlook.Sdk.dll",
                "Microsoft.Crm.Sdk.Proxy.dll",
                //"Microsoft.Crm.Tools.EmailProviders.dll",
                "Microsoft.IdentityModel.Clients.ActiveDirectory.dll",
                "Microsoft.IdentityModel.Clients.ActiveDirectory.WindowsForms.dll",
                "Microsoft.Xrm.Sdk.dll",
                "Microsoft.Xrm.Sdk.Deployment.dll",
                //"Microsoft.Xrm.Sdk.Workflow.dll",
                "Microsoft.Xrm.Tooling.Connector.dll",
                "Microsoft.Xrm.Tooling.CrmConnectControl.dll",
                //"Microsoft.Xrm.Tooling.CrmConnector.Powershell.dll",
                //"Microsoft.Xrm.Tooling.PackageDeployment.Powershell.dll",
                //"Microsoft.Xrm.Tooling.Ui.Styles.dll",
                //"Microsoft.Xrm.Tooling.WebResourceUtility.dll"
            };
        }

        public override IEnumerable<string> GetNamespacesToAdd()
        {
            // Import the commonly used namespaces as a courtesy to the user:
            return new[]
            {
                "Microsoft.Crm.Sdk.Messages",
                "Microsoft.Xrm.Sdk",
                "Microsoft.Xrm.Sdk.Client",
                "Microsoft.Xrm.Sdk.Messages",
                "Microsoft.Xrm.Sdk.Metadata",
                "Microsoft.Xrm.Sdk.Query"
            };
        }
        #endregion
    }
}
