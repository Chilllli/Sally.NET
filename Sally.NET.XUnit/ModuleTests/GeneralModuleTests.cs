using Sally.NET.Module;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sally.NET.XUnit.ModuleTests
{
    public class GeneralModuleTests
    {
        [Fact]
        public void CurrentUptime_ShouldReturnValidUptimeString()
        {
            GeneralModule generalModule = new GeneralModule();
            TimeSpan span = new TimeSpan(2, 1, 5);

            string expect = $" {span.Hours} Hours {span.Minutes} Minute {span.Seconds} Seconds";

            string actual = generalModule.CurrentUptime(span);

            Assert.Equal(expect, actual);
        }
    }
}