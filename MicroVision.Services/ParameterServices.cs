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

    /// <summary>
    /// Based on bindable base because when the properties are loaded externally, the binding must know it is updated.
    /// </summary>
    public class ParameterServices : BindableBase, IParameterServices
    {
        #region Exposed Properties

        #region Backing Fields

        private FieldParameter<int> _exposureTime = new FieldParameter<int>
        {
            Label = "Exposure Time (us)",
            Value = 44,
            Minimum = 44,
            Maximum = 100000
        };

        private FieldParameter<double> _gain =
            new FieldParameter<double> {Label = "Gain", Value = 0, Minimum = 0, Maximum = 20};

        private FieldParameter<int> _laserDuration = new FieldParameter<int>
        {
            Label = "Laser duration (us)",
            Value = 20,
            Minimum = 0,
            Maximum = 100000
        };

        private FieldParameter<int> _captureInterval = new FieldParameter<int>
        {
            Label = "Capture Interval (ms)",
            Value = 1000,
            Minimum = 100,
            Maximum = 100000
        };

        private FieldParameter<string> _outputDirectory =
            new FieldParameter<string> {Label = "Output directory", Value = @"C:\"};

        private SelectionParameter<string> _comSelection = new SelectionParameter<string> { Label = "COM" };
        private SelectionParameter<string> _vimbaSelection = new SelectionParameter<string> { Label = "Camera" };

        private CheckParameter _manualPowerCheck =
            new CheckParameter {Label = "Manual", Value = false, Enabled = false};

        private CheckParameter _masterPowerCheck =
            new CheckParameter {Label = "Master", Value = false, Enabled = true};

        private CheckParameter _fanPowerCheck = new CheckParameter {Label = "Fan", Value = false, Enabled = true};

        private CheckParameter _laserPowerCheck =
            new CheckParameter {Label = "Laser", Value = false, Enabled = true};

        private CheckParameter _motorPowerCheck =
            new CheckParameter {Label = "Motor", Value = false, Enabled = true};

        #endregion

        public FieldParameter<int> ExposureTime
        {
            get => _exposureTime;
            private set => SetProperty(ref _exposureTime, value);
        }


        public FieldParameter<double> Gain
        {
            get => _gain;
            private set => SetProperty(ref _gain, value);
        }

        public FieldParameter<int> LaserDuration
        {
            get => _laserDuration;
            private set => SetProperty(ref _laserDuration, value);
        }

        public FieldParameter<int> CaptureInterval
        {
            get => _captureInterval;
            private set => SetProperty(ref _captureInterval, value);
        }

        public FieldParameter<string> OutputDirectory
        {
            get => _outputDirectory;
            private set => SetProperty(ref _outputDirectory, value);
        }


        public SelectionParameter<string> ComSelection
        {
            get => _comSelection;
            private set => SetProperty(ref _comSelection, value);
        }

        public SelectionParameter<string> VimbaSelection
        {
            get => _vimbaSelection;
            private set => SetProperty(ref _vimbaSelection, value);
        }

        public CheckParameter ManualPowerCheck
        {
            get => _manualPowerCheck;
            private set => SetProperty(ref _manualPowerCheck, value);
        }

        public CheckParameter MasterPowerCheck
        {
            get => _masterPowerCheck;
            private set => SetProperty(ref _masterPowerCheck, value);
        }

        public CheckParameter FanPowerCheck
        {
            get => _fanPowerCheck;
            private set => SetProperty(ref _fanPowerCheck, value);
        }

        public CheckParameter LaserPowerCheck
        {
            get => _laserPowerCheck;
            private set => SetProperty(ref _laserPowerCheck, value);
        }

        public CheckParameter MotorPowerCheck
        {
            get => _motorPowerCheck;
            private set => SetProperty(ref _motorPowerCheck, value);
        }

        #endregion

        private readonly ILogService _log;
        private readonly IEventAggregator _eventAggregator;
        private const string DefaultFileName = "settings.json";

        public ParameterServices(ILogService log, IEventAggregator eventAggregator)
        {
            _log = log;
            _eventAggregator = eventAggregator;
            _log.ConfigureLogger("ParameterService");

            ConfigurationEventHandler();
            ConfigurationInitialization();
        }

        /// <summary>
        /// Configure load save saveas events
        /// </summary>
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
    }
}