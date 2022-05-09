using System;
using UtilityCollage;
using Xunit;

namespace UtilityCollageTests
{
    public class CmdHelperTest
    {
        [Fact]
        public void ComandoValidoTest()
        {
            int exitCode = CmdHelper.Cmd("C:\\Windows\\System32", "ping.exe", "-n 5 127.0.0.1");
            Assert.Equal(0, exitCode);
        }

        [Fact]
        public void ComandoNoValidoTest()
        {
            Assert.Throws<Exception>(()=>CmdHelper.Cmd("C:\\Windows\\System32", "ping.exe", "", 0));
        }
    }
}