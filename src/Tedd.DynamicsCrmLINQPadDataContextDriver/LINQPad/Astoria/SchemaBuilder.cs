using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using LINQPad;
using LINQPad.Extensibility.DataContext;
using Microsoft.CSharp;
using Tedd.DynamicsCrmLINQPadDataContextDriver.Models;
using Tedd.DynamicsCrmLINQPadDataContextDriver.Utils;

namespace Tedd.DynamicsCrmLINQPadDataContextDriver.LINQPad.Astoria
{
    public static class SchemaBuilder
    {
        //public static List<ExplorerItem> GetSchemaAndBuildAssembly(ConnectionData connectionData, AssemblyName assemblyName,
        //    ref string nameSpace, ref string typeName)
        //{
        //    var code = GenerateCode(connectionData);
        //    CompileCode(code, assemblyName);

        //}
        internal static List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName name,
            ref string nameSpace, ref string typeName)
        {
            
            //using (var progress = new ProgressIndicatorHost( 5, true))
            {
                //progress.SetStatus(1, "Fetching entities (slow)...");
                // Generate the code using the ADO.NET Data Services classes:
                var code = GenerateCode(new ConnectionData(cxInfo), nameSpace);

                //progress.SetStatus(2, "Compiling assemblies...");
                // Compile the code into the assembly, using the assembly name provided:
                BuildAssembly(code, name);

                //progress.SetStatus(3, "Loading assemblies...");
                var assembly = DataContextDriver.LoadAssemblySafely(name.CodeBase);
                var a = assembly.GetType(nameSpace + ".TypedDataContext");

                //progress.SetStatus(4, "Calculating schema...");
                // Use the schema to populate the Schema Explorer:
                List<ExplorerItem> schema = GetSchema(cxInfo, a);

                return schema;
            }
        }

        public static List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType)
        {
            // Return the objects with which to populate the Schema Explorer by reflecting over customType.

            // We'll start by retrieving all the properties of the custom type that implement IEnumerable<T>:
            var topLevelProps =
            (
                from prop in customType.GetProperties()
                where prop.PropertyType != typeof(string)

                // Display all properties of type IEnumerable<T> (except for string!)
                let ienumerableOfT = prop.PropertyType.GetInterface("System.Collections.Generic.IEnumerable`1")
                where ienumerableOfT != null

                orderby prop.Name

                select new ExplorerItem(prop.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
                {
                    IsEnumerable = true,
                    ToolTipText = DataContextDriver.FormatTypeName(prop.PropertyType, false),

                    // Store the entity type to the Tag property. We'll use it later.
                    Tag = ienumerableOfT.GetGenericArguments()[0]
                }

            ).ToList();

            // Create a lookup keying each element type to the properties of that type. This will allow
            // us to build hyperlink targets allowing the user to click between associations:
            var elementTypeLookup = topLevelProps.ToLookup(tp => (Type)tp.Tag);

            // Populate the columns (properties) of each entity:
            foreach (ExplorerItem table in topLevelProps)
                table.Children = ((Type)table.Tag)
                    .GetProperties()
                    .Select(childProp => GetChildItem(elementTypeLookup, childProp))
                    .OrderBy(childItem => childItem.Kind)
                    .ToList();

            return topLevelProps;
        }

        static ExplorerItem GetChildItem(ILookup<Type, ExplorerItem> elementTypeLookup, PropertyInfo childProp)
        {
            // If the property's type is in our list of entities, then it's a Many:1 (or 1:1) reference.
            // We'll assume it's a Many:1 (we can't reliably identify 1:1s purely from reflection).
            if (elementTypeLookup.Contains(childProp.PropertyType))
                return new ExplorerItem(childProp.Name, ExplorerItemKind.ReferenceLink, ExplorerIcon.ManyToOne)
                {
                    HyperlinkTarget = elementTypeLookup[childProp.PropertyType].First(),
                    // FormatTypeName is a helper method that returns a nicely formatted type name.
                    ToolTipText = DataContextDriver.FormatTypeName(childProp.PropertyType, true)
                };

            // Is the property's type a collection of entities?
            Type ienumerableOfT = childProp.PropertyType.GetInterface("System.Collections.Generic.IEnumerable`1");
            if (ienumerableOfT != null)
            {
                Type elementType = ienumerableOfT.GetGenericArguments()[0];
                if (elementTypeLookup.Contains(elementType))
                    return new ExplorerItem(childProp.Name, ExplorerItemKind.CollectionLink, ExplorerIcon.OneToMany)
                    {
                        HyperlinkTarget = elementTypeLookup[elementType].First(),
                        ToolTipText = DataContextDriver.FormatTypeName(elementType, true)
                    };
            }

            // Ordinary property:
            return new ExplorerItem(childProp.Name + " (" + DataContextDriver.FormatTypeName(childProp.PropertyType, false) + ")",
                ExplorerItemKind.Property, ExplorerIcon.Column);
        }

        public static string GenerateCode(ConnectionData connectionData, string ns)
        {
            var baseDir = Path.GetDirectoryName(typeof(SchemaBuilder).Assembly.Location);
            var codefile = Path.Combine(baseDir, connectionData.CacheEntityModelFile);

            // IF WE ARE CACHING THEN WE WILL CHECK IF WE HAVE FILE
            if (connectionData.CacheEntityModel && File.Exists(codefile))
                return File.ReadAllText(codefile);


            //CrmSvcUtil.exe.config.template
            var src = Path.Combine(baseDir, "CrmSvcUtil.exe.config.template");
            var dst = Path.Combine(baseDir, "CrmSvcUtil.exe.config");
            string code = null;
            try
            {
                // Set up config for CrmSvcUtil.exe
                var keys = new Dictionary<string, string>();
                keys.Add("connectionstring", connectionData.GetConnectionString());
                keys.Add("o", codefile);
                //keys.Add("u", connectionData.Username);
                //keys.Add("p", connectionData.Password);
                //keys.Add("d", connectionData.Domain);
                keys.Add("namespace", ns);
                keys.Add("serviceContextName", "TypedDataContext");
                //keys.Add("url", connectionData.OrganizationUrlWithCrmService2011);

                XDocument doc = XDocument.Load(src);
                foreach (var kvp in keys)
                {
                    XElement root = new XElement("add");
                    root.Add(new XAttribute("key", kvp.Key));
                    root.Add(new XAttribute("value", kvp.Value));
                    doc.Element("configuration").Element("appSettings").Add(root);
                }
                doc.Save(dst);

                // Execute CrmSvcUtil.exe
                var process = new Process();
                var startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.WorkingDirectory = baseDir;
                startInfo.FileName = "CrmSvcUtil.exe";
                startInfo.Arguments = "";
                process.StartInfo = startInfo;
                process.Start();

                process.WaitForExit();

                // Read output
                code = File.ReadAllText(codefile);

                if (!connectionData.CacheEntityModel)
                {
                    if (File.Exists(codefile))
                        File.Delete(codefile);
                }
            }
            finally
            {
                // Make sure we clean it up, it contains password
                if (File.Exists(dst))
                    File.Delete(dst);
            }

            return code;
        }

        public static void BuildAssembly(string code, AssemblyName assemblyName)
        {
            var baseDir = Path.GetDirectoryName(typeof(SchemaBuilder).Assembly.Location);
            var assemblies1 = new string[]
            {
                "System.dll",
                "System.Core.dll",
                "System.Xml.dll",
                "System.Runtime.Serialization.dll",
                "System.Data.dll",

                "System.Data.Services.Client.dll"
            };

            var assemblies2 = new string[]
            {
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
            var assemblies = new List<string>();
            assemblies.AddRange(assemblies1);
            assemblies.AddRange(assemblies2.Select(s => Path.Combine(baseDir, s)));

            // Use the CSharpCodeProvider to compile the generated code:
            CompilerResults results;
            //using (var codeProvider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } }))
            using (var codeProvider = new CSharpCodeProvider())
            {
                var options = new CompilerParameters(
                    assemblies.ToArray(),
                    assemblyName.CodeBase,
                    true);
                results = codeProvider.CompileAssemblyFromSource(options, code);
            }
            if (results.Errors.Count > 0)
                throw new Exception
                    ("Cannot compile typed context: " + results.Errors[0].ErrorText + " (line " + results.Errors[0].Line + ")");
        }
    }
}
