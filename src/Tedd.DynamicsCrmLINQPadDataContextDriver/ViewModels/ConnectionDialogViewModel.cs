using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tedd.DynamicsCrmLINQPadDataContextDriver.Annotations;
using Tedd.DynamicsCrmLINQPadDataContextDriver.Models;

namespace Tedd.DynamicsCrmLINQPadDataContextDriver.ViewModels
{
    class ConnectionDialogViewModel:INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ConnectionData ConnectionData { get; set; }

    }
}
