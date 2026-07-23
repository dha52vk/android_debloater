using System.Collections.Generic;
using System.ComponentModel;

namespace AndroidDebloaterStudio.Models
{
    public class AndroidPackage : INotifyPropertyChanged
    {
        private string _name = "";
        private string _packageName = "";
        private string _riskLevel = "";
        private string _oem = "";
        private string _state = "";
        private string _description = "";
        private List<string> _dependencies = new();
        private List<string> _neededBy = new();
        private List<string> _labels = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string PackageName
        {
            get => _packageName;
            set { _packageName = value; OnPropertyChanged(nameof(PackageName)); }
        }

        public string RiskLevel
        {
            get => _riskLevel;
            set { _riskLevel = value; OnPropertyChanged(nameof(RiskLevel)); }
        }

        public string Oem
        {
            get => _oem;
            set { _oem = value; OnPropertyChanged(nameof(Oem)); }
        }

        public string State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(nameof(State)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public List<string> Dependencies
        {
            get => _dependencies;
            set { _dependencies = value; OnPropertyChanged(nameof(Dependencies)); }
        }

        public List<string> NeededBy
        {
            get => _neededBy;
            set { _neededBy = value; OnPropertyChanged(nameof(NeededBy)); }
        }

        public List<string> Labels
        {
            get => _labels;
            set { _labels = value; OnPropertyChanged(nameof(Labels)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
