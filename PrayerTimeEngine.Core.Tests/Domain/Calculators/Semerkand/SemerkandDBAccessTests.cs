using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Semerkand
{
    public class SemerkandDBAccessTests
    {
        private readonly SemerkandDBAccess _semerkandDBAccess;

        public SemerkandDBAccessTests()
        {
            AppDbContext appDbContext = null;
            _semerkandDBAccess = new SemerkandDBAccess(appDbContext);
        }

        [Fact]
        public void X_X_X()
        {
            throw new NotImplementedException();
        }
    }
}
