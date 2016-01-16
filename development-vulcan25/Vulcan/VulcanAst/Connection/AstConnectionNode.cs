using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Connection
{
    public partial class AstConnectionNode
    {
        public AstConnectionNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();

            SingletonPropertyChanged += AstConnectionNode_SingletonPropertyChanged;
        }

        // TODO: The Ast needs to add a PreviewPropertyChanged or a PropertyChanging event in order to do this correctly with old and new values
        private void AstConnectionNode_SingletonPropertyChanged(object sender, Vulcan.Utility.ComponentModel.VulcanPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Server" || e.PropertyName == "Authentication" || e.PropertyName == "Login" || e.PropertyName == "Password" || e.PropertyName == "Database"
                || e.PropertyName == "AdditionalParameters" || e.PropertyName == "Protocol" || e.PropertyName == "Provider")
            {
                VulcanOnPropertyChanged("OleConnectionString", OleConnectionString, OleConnectionString);
                VulcanOnPropertyChanged("SqlConnectionString", SqlConnectionString, SqlConnectionString);
            }
        }

        [Browsable(false)]
        public string OleConnectionString
        {
            get { return BuildOleConnectionString(); }
        }

        private string BuildOleConnectionString()
        {
            StringBuilder connectionBuilder = new StringBuilder();
            connectionBuilder.Append(GetConnectionStringHeader(Provider, ConnectionStringHeaderType.Provider, Provider));
            if (!String.IsNullOrEmpty(FilePath))
            {
                connectionBuilder.Append(GetConnectionStringHeader(Provider, ConnectionStringHeaderType.Server, FilePath));
                connectionBuilder.Append(GetConnectionStringHeader(Provider, ConnectionStringHeaderType.PersistSecurityInfo, PersistSecurityInfo.ToString()));
            }

            if (!String.IsNullOrEmpty(Server))
            {
                connectionBuilder.Append(GetConnectionStringHeader(Provider, ConnectionStringHeaderType.Server, Server));
            }

            if (!String.IsNullOrEmpty(Database))
            {
                connectionBuilder.Append(GetConnectionStringHeader(Provider, ConnectionStringHeaderType.Database, Database));
            }

            if (string.IsNullOrEmpty(Login) && string.IsNullOrEmpty(Password))
            {
                connectionBuilder.Append(GetConnectionStringHeader(Provider, ConnectionStringHeaderType.Trusted, null));
            }
            else
            {
                connectionBuilder.Append(GetConnectionStringHeader(Provider, ConnectionStringHeaderType.Username, Login));
                connectionBuilder.Append(GetConnectionStringHeader(Provider, ConnectionStringHeaderType.Password, Password));
            }

            if (!String.IsNullOrEmpty(AdditionalParameters))
            {
                connectionBuilder.Append(GetConnectionStringHeader(Provider, ConnectionStringHeaderType.AdditionalParameters, AdditionalParameters));
            }
            if (IsMarsConnectionExplicitlySet || MarsConnection)
            {
                connectionBuilder.Append(GetConnectionStringHeader(Provider, ConnectionStringHeaderType.MarsConnection, MarsConnection.ToString()));
            }

            /*
            switch (Protocol)
            {
                case NetworkProtocol.Pipes:
                    connectionBuilder.Append("Network Library=dbnmpntw");
                    break;

                case NetworkProtocol.Shared:
                    connectionBuilder.Append("Network Library=dbmslpcn");
                    break;

                case NetworkProtocol.Tcp:
                    connectionBuilder.Append("Network Library=dbmssocn");
                    break;

                default:
                    break;
            }
            */

            return connectionBuilder.ToString();
        }

        [BrowsableAttribute(false)]
        public string SqlConnectionString
        {
            get { return BuildSqlConnectionString(); }
        }

        private enum ConnectionStringHeaderType
        {
            Provider,
            Server,
            Database,
            Username,
            Password,
            Trusted,
            PersistSecurityInfo,
            AdditionalParameters,
            MarsConnection
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "ConnectionString logic is naturally complex.")]
        private static string GetConnectionStringHeader(string provider, ConnectionStringHeaderType headerType, string value)
        {
            if (provider == "SQLOLEDB" || String.IsNullOrEmpty(provider))
            {
                switch (headerType)
                {
                    case ConnectionStringHeaderType.Provider: return "Provider=SQLOLEDB;";
                    case ConnectionStringHeaderType.Server: return String.Format(CultureInfo.InvariantCulture, "Data Source={0};", value);
                    case ConnectionStringHeaderType.Database: return String.Format(CultureInfo.InvariantCulture, "Initial Catalog={0};", value);
                    case ConnectionStringHeaderType.Username: return String.Format(CultureInfo.InvariantCulture, "User Id={0};", value);
                    case ConnectionStringHeaderType.Password: return String.Format(CultureInfo.InvariantCulture, "Password={0};", value);
                    case ConnectionStringHeaderType.Trusted: return "Integrated Security=SSPI;";
                    case ConnectionStringHeaderType.MarsConnection: return String.Format(CultureInfo.InvariantCulture,"MARS Connection={0};", value);
                }
            }
            else if (provider == "SQLNCLI10" || provider == "SQLNCLI10.1" || provider == "SQLNCLI")
            {
                switch (headerType)
                {
                    case ConnectionStringHeaderType.Provider: return String.Format(CultureInfo.InvariantCulture, "Provider={0};", provider);
                    case ConnectionStringHeaderType.Server: return String.Format(CultureInfo.InvariantCulture, "Data Source={0};", value);
                    case ConnectionStringHeaderType.Database: return String.Format(CultureInfo.InvariantCulture, "Initial Catalog={0};", value);
                    case ConnectionStringHeaderType.Username: return String.Format(CultureInfo.InvariantCulture, "User ID={0};", value);
                    case ConnectionStringHeaderType.Password: return String.Format(CultureInfo.InvariantCulture, "Password={0};", value);
                    case ConnectionStringHeaderType.Trusted: return "Trusted_Connection=yes;Integrated Security=SSPI;";
                    case ConnectionStringHeaderType.MarsConnection: return String.Format(CultureInfo.InvariantCulture,"MARS Connection={0};", value);
                }
            }
            else if (provider == "{SQL Server Native Client 10.0}" || provider == "{SQL Native Client}")
            {
                switch (headerType)
                {
                    case ConnectionStringHeaderType.Provider: return String.Format(CultureInfo.InvariantCulture, "Driver={0};", provider);
                    case ConnectionStringHeaderType.Server: return String.Format(CultureInfo.InvariantCulture, "Server={0};", value);
                    case ConnectionStringHeaderType.Database: return String.Format(CultureInfo.InvariantCulture, "Database={0};", value);
                    case ConnectionStringHeaderType.Username: return String.Format(CultureInfo.InvariantCulture, "UId={0};", value);
                    case ConnectionStringHeaderType.Password: return String.Format(CultureInfo.InvariantCulture, "Pwd={0};", value);
                    case ConnectionStringHeaderType.Trusted: return "Trusted_Connection=yes;";
                    case ConnectionStringHeaderType.MarsConnection: return String.Format(CultureInfo.InvariantCulture,"MARS Connection={0};", value);
                }
            }
            else if (provider == "SQLXMLOLEDB")
            {
            }
            else if (provider == "Microsoft.ACE.OLEDB.12.0" || provider == "Microsoft.Jet.OLEDB.4.0")
            {
                switch (headerType)
                {
                    case ConnectionStringHeaderType.Provider: return String.Format(CultureInfo.InvariantCulture, "Provider={0};", provider);
                    case ConnectionStringHeaderType.Server: return String.Format(CultureInfo.InvariantCulture, "Data Source={0};", value);

                    case ConnectionStringHeaderType.PersistSecurityInfo: return String.Format(CultureInfo.InvariantCulture, "Persist Security Info={0};", value);
                    case ConnectionStringHeaderType.AdditionalParameters: return String.Format(CultureInfo.InvariantCulture, "Extended Properties=\"{0}\";", value);
                }
            }

            return null;
        }

        private string BuildSqlConnectionString()
        {
            var connectionBuilder = new StringBuilder();

            if (!String.IsNullOrEmpty(Server))
            {
                connectionBuilder.Append("Data Source=" + Server + ";");
            }

            if (!String.IsNullOrEmpty(Database))
            {
                connectionBuilder.Append("Initial Catalog=" + Database + ";");
            }

            if (Authentication == AuthenticationType.Windows)
            {
                connectionBuilder.Append("Integrated Security=SSPI;");
            }
            else
            {
                connectionBuilder.Append("User ID=" + Login + ";");
                connectionBuilder.Append("Password=" + Password + ";");
            }

            /*
            switch (Protocol)
            {
                case NetworkProtocol.Pipes:
                    connectionBuilder.Append("Network=dbnmpntw");
                    break;

                case NetworkProtocol.Shared:
                    connectionBuilder.Append("Network=dbmslpcn");
                    break;

                case NetworkProtocol.Tcp:
                    connectionBuilder.Append("Network=dbmssocn");
                    break;

                default:
                    break;
            }
            */
            if (!String.IsNullOrEmpty(AdditionalParameters))
            {
                connectionBuilder.Append(AdditionalParameters);
            }
            if (MarsConnection)
            {
                connectionBuilder.Append("MultipleActiveResultSets=true;");
            }

            return connectionBuilder.ToString();
        }
    }
}
