//using System;
//using System.Collections.Generic;
//using System.Linq;
//using LINQPad.Extensibility.DataContext;
//using Tedd.DynamicsCrmLINQPadDataContextDriver.DataConnection;
//using Tedd.DynamicsCrmLINQPadDataContextDriver.Models;
//using Tedd.DynamicsCrmLINQPadDataContextDriver.ViewModels;
//using Tedd.DynamicsCrmLINQPadDataContextDriver.Views;

//namespace Tedd.DynamicsCrmLINQPadDataContextDriver.LINQPad.Universal
//{
//    public class UniversalStaticDriver : StaticDataContextDriver
//    {
//        public override string Name
//        {
//            get { return "Tedds Dynamics Crm LINQPad DataContext Driver"; }
//        }
//        public override string Author { get { return "Tedd Hansen"; } }
//        public override string GetConnectionDescription(IConnectionInfo cxInfo)
//        {
//            // For static drivers, we can use the description of the custom type & its assembly:
//            return cxInfo.CustomTypeInfo.GetCustomTypeDescription();
//        }

//        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
//        {
//            var ConnectionData = new ConnectionData(cxInfo);
//            // Prompt the user for connection string
//            var dialog = new ConnectionDialog(cxInfo);
//            var connectionDialogViewModel = new ConnectionDialogViewModel();
//            connectionDialogViewModel.ConnectionData = ConnectionData;
//            dialog.DataContext = connectionDialogViewModel;
//            var dialogResult = dialog.ShowDialog();
//            cxInfo.DisplayName = $"{ConnectionData.Username} @ {ConnectionData.OrganizationUrl}";
//            return dialogResult == true;
//        }

//        public override List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType)
//        {
//            using (var connection = new XrmConnection())
//            {
//                var connectionData = new ConnectionData(cxInfo);
//                connection.Connect(connectionData);


//                var entities = connection.GetEntities().OrderBy(e => e.LogicalName).ToList();


//                var topItems = new List<ExplorerItem>();
//                foreach (var entity in entities)
//                {
//                    var name = entity.LogicalName;
//                    var item = new ExplorerItem(name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
//                    {
//                        IsEnumerable = true,
//                        ToolTipText = "Entity",
//                    };
//                    topItems.Add(item);
//                    item.Children = new List<ExplorerItem>();

//                    foreach (var attribute in entity.Attributes.OrderBy(a => a.SchemaName))
//                    {

//                        // Populate children
//                        var child = new ExplorerItem($"{attribute.SchemaName} ({attribute.AttributeType.Value.ToString()})", ExplorerItemKind.Property, ExplorerIcon.Column)
//                        {
//                            IsEnumerable = true,
//                            ToolTipText = attribute.AttributeType.Value.ToString(),
//                        };
//                        item.Children.Add(child);
//                    }
//                }


//                return topItems;
//            }
//        }


//        public override IEnumerable<string> GetAssembliesToAdd()
//        {
//            // We need the following assembly for compiliation and autocompletion:
//            return new[] {
//                "Microsoft.Crm.Outlook.Sdk.dll",
//                "Microsoft.Crm.Sdk.Proxy.dll",
//                "Microsoft.Crm.Tools.EmailProviders.dll",
//                "Microsoft.IdentityModel.Clients.ActiveDirectory.dll",
//                "Microsoft.IdentityModel.Clients.ActiveDirectory.WindowsForms.dll",
//                "Microsoft.Xrm.Sdk.dll",
//                "Microsoft.Xrm.Sdk.Deployment.dll",
//                "Microsoft.Xrm.Sdk.Workflow.dll",
//                "Microsoft.Xrm.Tooling.Connector.dll",
//                "Microsoft.Xrm.Tooling.CrmConnectControl.dll",
//                "Microsoft.Xrm.Tooling.CrmConnector.Powershell.dll",
//                "Microsoft.Xrm.Tooling.PackageDeployment.Powershell.dll",
//                "Microsoft.Xrm.Tooling.Ui.Styles.dll",
//                "Microsoft.Xrm.Tooling.WebResourceUtility.dll"
//            };
//        }

//        public override IEnumerable<string> GetNamespacesToAdd()
//        {
//            // Import the commonly used namespaces as a courtesy to the user:
//            return new[]
//            {
//                "Microsoft.Crm.Sdk.Messages",
//                "Microsoft.Xrm.Sdk",
//                "Microsoft.Xrm.Sdk.Client",
//                "Microsoft.Xrm.Sdk.Messages",
//                "Microsoft.Xrm.Sdk.Metadata",
//                "Microsoft.Xrm.Sdk.Query"
//        };
//        }

//        /// <summary>Returns true if two <see cref="IConnectionInfo"/> objects are semantically equal.</summary>
//        public override bool AreRepositoriesEquivalent(IConnectionInfo r1, IConnectionInfo r2)
//        {
//            // Two repositories point to the same endpoint if their URIs are the same.
//            return object.Equals(
//                string.Join("|",
//                    r1.DriverData.Element("OrganizationUrl"),
//                    r1.DriverData.Element("Username"),
//                    r1.DriverData.Element("Password")
//                ),
//                string.Join("|",
//                    r2.DriverData.Element("OrganizationUrl"),
//                    r2.DriverData.Element("Username"),
//                    r2.DriverData.Element("Password")
//                )
//                );
//        }



//    }
//}
