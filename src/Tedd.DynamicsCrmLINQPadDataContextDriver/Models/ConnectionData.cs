using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;
using Microsoft.Xrm.Tooling.Connector;
using Tedd.DynamicsCrmLINQPadDataContextDriver.Annotations;

namespace Tedd.DynamicsCrmLINQPadDataContextDriver.Models
{
	public class ConnectionData : INotifyPropertyChanged
	{
		readonly IConnectionInfo _cxInfo;
		readonly XElement _driverData;

		public ConnectionData(IConnectionInfo cxInfo)
		{
			_cxInfo = cxInfo;
			_driverData = cxInfo.DriverData;
			_cxInfo.Persist = true;
		}

		public string CrmConnectionString
		{
			get { return (string)_driverData.Element("ConnectionString") ?? ""; }
			set { _driverData.SetElementValue("ConnectionString", value?.Trim()); OnPropertyChanged(); }
		}

		public AuthenticationType AuthenticationType
        {
            get
            {
                AuthenticationType ret = AuthenticationType.AD;
                Enum.TryParse((string)_driverData.Element("AuthenticationType") ?? "AD", out ret);
                return ret;
            }
            set { _driverData.SetElementValue("AuthenticationType", value); OnPropertyChanged(); }
        }
        public string Domain
        {
            get { return (string)_driverData.Element("Domain") ?? ""; }
            set { _driverData.SetElementValue("Domain", value?.Trim()); OnPropertyChanged(); }
        }
        public string Username
        {
            get { return (string)_driverData.Element("Username") ?? ""; }
            set { _driverData.SetElementValue("Username", value?.Trim()); OnPropertyChanged(); }
        }

        public string Password
        {
            get { return _cxInfo.Decrypt((string)_driverData.Element("Password") ?? ""); }
            set { _driverData.SetElementValue("Password", _cxInfo.Encrypt(value?.Trim())); OnPropertyChanged(); }
        }

        public string OrganizationUrl
        {
            get { return (string)_driverData.Element("OrganizationUrl") ?? ""; }
            set
            {
                _driverData.SetElementValue("OrganizationUrl", value?.Trim());
                OnPropertyChanged();
            }
        }

        public bool CacheEntityModel
        {
            get
            {
                var ret = true;
                var v = (string)_driverData.Element("CacheEntityModel");
                if (string.IsNullOrWhiteSpace(v))
                    return true;
                bool.TryParse(v, out ret);
                return ret;
            }
            set { _driverData.SetElementValue("CacheEntityModel", value); OnPropertyChanged();
            }
        }
        public string CacheEntityModelFile
        {
            get
            {
                var keyBytes = Encoding.UTF8.GetBytes(CrmConnectionString);
                using (var hasher = SHA256.Create())
                {

                    var hash = hasher.ComputeHash(keyBytes);
                    var sb = new StringBuilder();
                    foreach (var h in hash)
                    {
                        sb.Append(h.ToString("X2"));
                    }
                    return "CrmProxy_Cache_" + sb.ToString() + ".cs";
                }
            }

        }

        public string GetConnectionString()
        {
	        return CrmConnectionString;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
