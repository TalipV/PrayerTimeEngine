using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Presentation.ViewModel.Custom
{
    public interface ISettingConfigurationViewModel
    {
        public BaseCalculationConfiguration BuildSetting(int minuteAdjustment, bool isTimeShown);
        public IView GetUI();
        public void AssignSettingValues(BaseCalculationConfiguration configuration);
    }
}
