using Prism.Mvvm;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using Microsoft.Practices.Unity;
using MicroVision.Core.Events;
using MicroVision.Core.Models;
using MicroVision.Services.Models;
using Newtonsoft.Json;
using Prism.Events;

namespace MicroVision.Services
{
    public interface IParameterServices
    {
        FieldParameter<string> CameraControllerUri { get; set; }
        FieldParameter<string> CameraUri { get; set; }
        FieldParameter<string> ProcessorUri { get; set; }
        FieldParameter<int> ExposureTime { get; set; }
        FieldParameter<double> Gain { get; set; }
        FieldParameter<int> LaserDuration { get; set; }
        FieldParameter<int> CaptureInterval { get; set; }
        FieldParameter<string> OutputDirectory { get; set; }
        SelectionParameter<string> ComSelection { get; set; }
        SelectionParameter<string> VimbaSelection { get; set; }
        CheckParameter ManualPowerCheck { get; set; }
        CheckParameter MasterPowerCheck { get; set; }
        CheckParameter FanPowerCheck { get; set; }
        CheckParameter LaserPowerCheck { get; set; }
        CheckParameter MotorPowerCheck { get; set; }
    }

    public class ParameterServices : IParameterServices
    {
        private readonly IUnityContainer _container;
        private readonly ILogService _log;
        private readonly IEventAggregator _eventAggregator;
        private const string DefaultFileName = "settings.json";
        /// <summary>
        /// Parameterless constructor for xml serialziation
        /// </summary>
        public ParameterServices() { }
        public ParameterServices(IUnityContainer container, ILogService log, IEventAggregator eventAggregator)
        {
            _container = container;
            _log = log;
            _eventAggregator = eventAggregator;
            _log.ConfigureLogger("ParameterService");

            // set the manual power override logic
            ManualPowerCheck.PropertyChanged += ManualPowerCheck_PropertyChanged;
            ConfigurationEventHandler();
            ConfigurationInitialization();
        }

        private void ConfigurationEventHandler()
        {
            _eventAggregator.GetEvent<SaveAsEvent>().Subscribe(filename => Serialize(filename));
            _eventAggregator.GetEvent<SaveEvent>().Subscribe(() => Serialize());
            _eventAggregator.GetEvent<LoadEvent>().Subscribe(filename => Load(filename));
        }

        private void ManualPowerCheck_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var senderObj = (CheckParameter) sender;
        }

        public FieldParameter<string> CameraControllerUri { get; set; } =
            new FieldParameter<string>() {Label = "Camera Controller Server Uri", IsEnabled = true, Value = ""};

        public FieldParameter<string> CameraUri { get; set; } =
            new FieldParameter<string>() {Label = "Camera Server Uri", IsEnabled = true, Value = ""};

        public FieldParameter<string> ProcessorUri { get; set; } =
            new FieldParameter<string>() {Label = "Image Processing Server Uri", IsEnabled = true, Value = ""};

        public FieldParameter<int> ExposureTime { get; set; } = new FieldParameter<int>()
        {
            Label = "Exposure Time (us)",
            Value = 44,
            Minimum = 44,
            Maximum = 100000
        };

        public FieldParameter<double> Gain { get; set; } =
            new FieldParameter<double>() {Label = "Gain", Value = 0, Minimum = 0, Maximum = 20};

        public FieldParameter<int> LaserDuration { get; set; } = new FieldParameter<int>()
        {
            Label = "Laser duration (us)",
            Value = 20,
            Minimum = 0,
            Maximum = 100000
        };

        public FieldParameter<int> CaptureInterval { get; set; } = new FieldParameter<int>()
        {
            Label = "Capture Interval (ms)",
            Value = 1000,
            Minimum = 100,
            Maximum = 100000
        };

        public FieldParameter<string> OutputDirectory { get; set; } =
            new FieldParameter<string>() {Label = "Output directory", Value = @"C:\"};

        public SelectionParameter<string> ComSelection { get; set; } = new SelectionParameter<string>("COM");
        public SelectionParameter<string> VimbaSelection { get; set; } = new SelectionParameter<string>("Camera");

        public CheckParameter ManualPowerCheck { get; set; } = new CheckParameter("Manual");
        public CheckParameter MasterPowerCheck { get; set; } = new CheckParameter("Master", false);
        public CheckParameter FanPowerCheck { get; set; } = new CheckParameter("Fan", false);
        public CheckParameter LaserPowerCheck { get; set; } = new CheckParameter("Laser", false);
        public CheckParameter MotorPowerCheck { get; set; } = new CheckParameter("Motor", false);

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
            using(var fs = File.OpenRead(filename))
            using (var sr = new StreamReader(fs))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                obj = (ParameterServices) serializer.Deserialize(jsonTextReader, typeof(ParameterServices));
            }

            Mapper.Initialize(config => config.CreateMap(typeof(ParameterServices), typeof(ParameterServices)));
            Mapper.Map(obj,this);
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
    }
}