using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Tedd.DynamicsCrmLINQPadDataContextDriver.Views;

namespace Tedd.DynamicsCrmLINQPadDataContextDriver.Utils
{
    public class ProgressIndicatorHost : IDisposable
    {
        private readonly object _lock = new object();
        private readonly Dispatcher _dispatcher;
        private ProgressIndicator _progressIndicatorWindow;
        private string _status = "Init...";
        private int _maxValue;
        private int _currentValue;

        public ProgressIndicatorHost(Dispatcher dispatcher,int max = 0, bool startVisible = false)
        {
            _dispatcher = dispatcher;

            _maxValue = max;
            

            _dispatcher.Invoke(() =>
            {
                _progressIndicatorWindow = new ProgressIndicator();
            });

            if (startVisible)
                Show();
        }
        
        public ProgressIndicatorHost Show()
        {
            lock (_lock)
            {
                DispatchIfNeed(() =>
                {
                    _progressIndicatorWindow.Show();
                    _progressIndicatorWindow.Visibility = Visibility.Visible;
                });
                Update();
            }
            return this;
        }

        private void DispatchIfNeed(Action action)
        {
            if (_progressIndicatorWindow.Dispatcher.CheckAccess())
                action.Invoke();
            else
                _progressIndicatorWindow.Dispatcher.Invoke(action);
        }

        public void Hide()
        {
            lock (_lock)
            {
                if (_progressIndicatorWindow != null)
                {
                    DispatchIfNeed(() =>
                    {
                        _progressIndicatorWindow.Visibility = Visibility.Hidden;
                    });
                }
            }
        }

        private void Update()
        {
            lock (_lock)
            {
                if (_progressIndicatorWindow != null)
                {
                    DispatchIfNeed(() =>
                    {
                        _progressIndicatorWindow.Text.Content = _status;
                        _progressIndicatorWindow.ProgressBar.Minimum = 0;
                        _progressIndicatorWindow.ProgressBar.Maximum = _maxValue;
                        _progressIndicatorWindow.ProgressBar.Value = Math.Min(_maxValue, _currentValue);
                    });
                }
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                Update();
            }
        }

        public int Maximum
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                Update();
            }
        }
        public int Current
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                Update();
            }
        }


        public void Dispose()
        {
            ProgressIndicator piw;
            lock (_lock)
            {
                piw = _progressIndicatorWindow;
                _progressIndicatorWindow = null;
            }
            if (piw != null)
            {
                if (piw.Dispatcher.CheckAccess())
                    piw.Close();
                else
                    piw.Dispatcher.Invoke(() => piw.Close());
            }
        }


        public ProgressIndicatorHost SetStatus(int current, string status)
        {
            _currentValue = current;
            _status = status;
            Update();
            return this;
        }
    }
}
