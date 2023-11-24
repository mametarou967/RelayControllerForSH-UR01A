namespace RelayControllerForSHUR01A.ViewModels
{
    public sealed class ComboBoxViewModel { 
        public ComboBoxViewModel(int value, string displayValue)
        { 
            Value = value;
            DisplayValue = displayValue;
        } 
        
        public int Value { get; } 
        public string DisplayValue { get; } 
    }
}
