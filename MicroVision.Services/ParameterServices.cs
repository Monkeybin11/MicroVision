using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Practices.Unity;
using MicroVision.Core.Models;
using MicroVision.Services.Models;
using Newtonsoft.Json;

namespace MicroVision.Services
{
    public interface IParameterServices
    {
        FieldParameter<string> CameraControllerUri { get; }
        FieldParameter<string> CameraUri { get; }
        FieldParameter<string> ProcessorUri { get; }
        FieldParameter<int> ExposureTime { get; }
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

    public class ParameterServices : IParameterServices
    {
        private readonly IUnityContainer _container;
        private readonly ILogService _log;
        private const string DefaultFileName = "settings.json";
        /// <summary>
        /// Parameterless constructor for xml serialziation
        /// </summary>
        private ParameterServices() { }
        public ParameterServices(IUnityContainer container, ILogService log)
        {
            _container = container;
            _log = log;
            _log.ConfigureLogger("ParameterService");

            // set the manual power override logic
            ManualPowerCheck.PropertyChanged += ManualPowerCheck_PropertyChanged;

            ConfigurationInitialization();
        }

        private void ManualPowerCheck_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var senderObj = (CheckParameter) sender;
        }

        public FieldParameter<string> CameraControllerUri { get; } =
            new FieldParameter<string>() {Label = "Camera Controller Server Uri", IsEnabled = true, Value = ""};

        public FieldParameter<string> CameraUri { get; } =
            new FieldParameter<string>() {Label = "Camera Server Uri", IsEnabled = true, Value = ""};

        public FieldParameter<string> ProcessorUri { get; } =
            new FieldParameter<string>() {Label = "Image Processing Server Uri", IsEnabled = true, Value = ""};

        public FieldParameter<int> ExposureTime { get; } = new FieldParameter<int>()
        {
            Label = "Exposure Time (us)",
            Value = 44,
            Minimum = 44,
            Maximum = 100000
        };

        public FieldParameter<double> Gain { get; } =
            new FieldParameter<double>() {Label = "Gain", Value = 0, Minimum = 0, Maximum = 20};

        public FieldParameter<int> LaserDuration { get; } = new FieldParameter<int>()
        {
            Label = "Laser duration (us)",
            Value = 20,
            Minimum = 0,
            Maximum = 100000
        };

        public FieldParameter<int> CaptureInterval { get; } = new FieldParameter<int>()
        {
            Label = "Capture Interval (ms)",
            Value = 1000,
            Minimum = 100,
            Maximum = 100000
        };

        public FieldParameter<string> OutputDirectory { get; } =
            new FieldParameter<string>() {Label = "Output directory", Value = @"C:\"};

        public SelectionParameter<string> ComSelection { get; } = new SelectionParameter<string>("COM");
        public SelectionParameter<string> VimbaSelection { get; } = new SelectionParameter<string>("Camera");

        public CheckParameter ManualPowerCheck { get; } = new CheckParameter("Manual");
        public CheckParameter MasterPowerCheck { get; } = new CheckParameter("Master", false);
        public CheckParameter FanPowerCheck { get; } = new CheckParameter("Fan", false);
        public CheckParameter LaserPowerCheck { get; } = new CheckParameter("Laser", false);
        public CheckParameter MotorPowerCheck { get; } = new CheckParameter("Motor", false);

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
                obj = (ParameterServices)serializer.Deserialize(jsonTextReader);
            }
            _container.RegisterInstance<IParameterServices>(obj);
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