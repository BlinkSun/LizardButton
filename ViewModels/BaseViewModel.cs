using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LizardButton.ViewModels;

/// <summary>
/// Base ViewModel implementing INotifyPropertyChanged for property notification.
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifies listeners of a property value change.
    /// </summary>
    /// <param name="propertyName">Name of the property changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets the property and notifies if value has changed.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    /// <param name="backingField">Reference to the property field.</param>
    /// <param name="value">New value.</param>
    /// <param name="propertyName">Property name.</param>
    /// <returns>True if value changed, false otherwise.</returns>
    protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(backingField, value))
        {
            return false;
        }
        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}