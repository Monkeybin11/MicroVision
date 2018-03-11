using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroVision.Modules.Statusbar.Models;
using MicroVision.Modules.Statusbar.ViewModels;

namespace MicroVision.Modules.Statusbar.SampleData
{
    public class StatusLogSampleDataSource : StatusLogViewModel
    {
        public StatusLogSampleDataSource()
        {
            StatusItems = new ObservableCollection<StatusEntry>
            {
                new StatusEntry(DateTime.Now, "Error, please restart"),
                new StatusEntry(DateTime.Now, "short one"),
                new StatusEntry(DateTime.Now,
                    "long long long long long longlong long longlong long longlong long longlong long longlong long longlong long longlong long longlong long longlong long long")
            };
        }
    }
}
