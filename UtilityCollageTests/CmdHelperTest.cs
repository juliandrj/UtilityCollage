using UtilityCollage;

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
        public void ComandoValidoMultiplesSalidasTest()
        {
            var exception = Record.Exception(() => CmdHelper.Cmd("C:\\Windows\\System32", "ping.exe", "", 0, 1));
            Assert.Null(exception);
        }

        [Fact]
        public void ComandoNoValidoTest()
        {
            Assert.Throws<Exception>(() => CmdHelper.Cmd("C:\\Windows\\System32", "ping.exe", "", 0));
        }

        [Fact]
        public void SustituirVariablesEntornoTest()
        {
            Environment.SetEnvironmentVariable("VARIABLE_TEST", "%WINDIR%\\sistem32");
            Assert.Equal("JDRJC:\\windows\\sistem32JDRJC:\\windows\\sistem32", CmdHelper.SustituirTokensConVariablesDeEntorno("JDRJ%VARIABLE_TEST%JDRJ%VARIABLE_TEST%"));
        }
    }
}