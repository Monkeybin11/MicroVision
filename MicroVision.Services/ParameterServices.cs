using Prism.Mvvm;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using Microsoft.Practices.Unity;
using MicroVision.Core.Events;
using MicroVision.Core.Models;
using MicroVision.Services.Annotations;
using MicroVision.Services.Models;
using Newtonsoft.Json;
using Prism.Events;

namespace MicroVision.Services
{
    public interface IParameterServices
    {
        FieldParameter<int> ExposureTime { get; set; }

        FieldParameter<double> Gain { get; } 
        FieldParameter<int> LaserDuration { get; } 
        FieldParameter<int> CaptureInterval { get; } 
        FieldParameter<string> OutputDirectory { get; }

        SelectionParameter<string> ComSelection { get; }
        SelectionParameter<string> VimbaSelection { get; } 

        CheckParameter ManualPowerCheck { get; }
        CheckParameter MasterPowerCheck { get; } 
        CheckParameter FanPowerCheck { get; }
        CheckParameter LaserPowerCheck { get; }
        CheckParameter MotorPowerCheck { get; } 
    }

    /// <summary>
    /// Based on bindable base because when the properties are loaded externally, the binding must know it is updated.
    /// </summary>
    public class ParameterServices : BindableBase, IParameterServices
    {
        //    ExposureTime =
        //,
        //CaptureInterval =
        //{
        //Label = "Capture Interval (ms)",
        //Value = 1000,
        //Minimum = 100,
        //Maximum = 100000
        //},
        //LaserDuration =
        //{
        //Label = "Laser duration (us)",
        //Value = 20,
        //Minimum = 0,
        //Maximum = 100000
        //},
        //Gain = {Label = "Gain", Value = 0, Minimum = 0, Maximum = 20},
        //OutputDirectory = {Label = "Output directory", Value = @"C:\"}

        #region Exposed Properties

        private FieldParameter<int> _exposureTime = new FieldParameter<int>()
        {
            Label = "Exposure Time (us)",
            Value = 44,
            Minimum = 44,
            Maximum = 100000
        };

        public FieldParameter<int> ExposureTime
        {
            get => _exposureTime;
            set => SetProperty(ref _exposureTime, value);
        }

        public FieldParameter<double> Gain { get; } = new FieldParameter<double>();
        public FieldParameter<int> LaserDuration { get; } = new FieldParameter<int>();
        public FieldParameter<int> CaptureInterval { get; } = new FieldParameter<int>();
        public FieldParameter<string> OutputDirectory { get; } = new FieldParameter<string>();

        public SelectionParameter<string> ComSelection { get; } = new SelectionParameter<string>();
        public SelectionParameter<string> VimbaSelection { get; } = new SelectionParameter<string>();

        public CheckParameter ManualPowerCheck { get; } = new CheckParameter();
        public CheckParameter MasterPowerCheck { get; } = new CheckParameter();
        public CheckParameter FanPowerCheck { get; } = new CheckParameter();
        public CheckParameter LaserPowerCheck { get; } = new CheckParameter();
        public CheckParameter MotorPowerCheck { get; } = new CheckParameter();

        #endregion

        private readonly IUnityContainer _container;
        private readonly ILogService _log;
        private readonly IEventAggregator _eventAggregator;
        private const string DefaultFileName = "settings.json";

        /// <summary>
        /// Parameterless constructor for xml serialziation
        /// </summary>
        public ParameterServices()
        {
        }

        public ParameterServices(IUnityContainer container, ILogService log, IEventAggregator eventAggregator)
        {
            _container = container;
            _log = log;
            _eventAggregator = eventAggregator;
            _log.ConfigureLogger("ParameterService");

            ConfigurationEventHandler();
            ConfigurationInitialization();
        }

        private void ConfigurationEventHandler()
        {
            _eventAggregator.GetEvent<SaveAsEvent>().Subscribe(filename => Serialize(filename));
            _eventAggregator.GetEvent<SaveEvent>().Subscribe(() => Serialize());
            _eventAggregator.GetEvent<LoadEvent>().Subscribe(filename => Load(filename));
        }

        public void Serialize(string filename = DefaultFileName)
        {
            using (var file = File.CreateText(filename))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, this);
            }
        }

        /// <summary>
        /// This will replace the instance in the container
        /// </summary>
        /// <param name="filename"></param>
        public void Load(string filename = DefaultFileName)
        {
            ParameterServices obj;
            var serializer = new JsonSerializer();
            using (var fs = File.OpenRead(filename))
            using (var sr = new StreamReader(fs))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                try
                {
                    obj = (ParameterServices) serializer.Deserialize(jsonTextReader, typeof(ParameterServices));
                    var mapper = new MapperConfiguration(config =>
                        config.CreateMap<ParameterServices, ParameterServices>()).CreateMapper();
                    
                    mapper.Map(obj, this);
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Profile file is not valid");
                }
            }
        }

        private void ConfigurationInitialization()
        {
            if (File.Exists(DefaultFileName))
            {
                try
                {
                    Load();
                }
                catch (Exception e)
                {
                    _log.Logger.Info("Configuration file corrupted, restore the default configuration");
                    File.Delete(DefaultFileName);
                    Serialize();
                }
            }
            else
            {
                // first execution, generate the configuration file
                Serialize();
            }
        }

        #region Properties initialization 

        //public AcquisitionParameters AcquisitionParameters { get; set; } = new AcquisitionParameters()
        //{
        //};

        //public DeviceSelections DeviceSelections { get; set; } =
        //    new DeviceSelections()
        //    {
        //        ComSelection = {Label = "COM"},
        //        VimbaSelection = {Label = "Camera"}
        //    };

        //public PowerConfigurations PowerConfigurations { get; set; } = new PowerConfigurations()
        //{
        //    ManualPowerCheck = {Label = "Manual", Value = false, IsEnabled = true},
        //    FanPowerCheck = {Label = "Fan", Value = false, IsEnabled = false},
        //    MotorPowerCheck = {Label = "Motor", Value = false, IsEnabled = false},
        //    LaserPowerCheck = {Label = "Laser", Value = false, IsEnabled = false},
        //    MasterPowerCheck = {Label = "Master", Value = false, IsEnabled = false}
        //};

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}