using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EvalComparisons
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T originalValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(originalValue, newValue)) return false;

            originalValue = newValue;
            NotifyPropertyChanged(propertyName);
            return true;
        }
    }
}
