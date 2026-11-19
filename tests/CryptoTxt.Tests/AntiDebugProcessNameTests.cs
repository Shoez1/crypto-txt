using CryptoTxt.Security;

namespace CryptoTxt.Tests;

[TestClass]
public sealed class AntiDebugProcessNameTests
{
    [TestMethod]
    public void IsDebuggerProcessName_GenericNames_AreNotTreatedAsDebuggers()
    {
        string[] genericNames = { "ida", "kd", "cdb", "xdbg" };

        foreach (string name in genericNames)
        {
            Assert.IsFalse(AntiDebug.IsDebuggerProcessName(name), $"'{name}' é genérico demais para bloquear o app.");
        }
    }

    [TestMethod]
    public void IsDebuggerProcessName_SpecificDebuggerNames_AreStillTreatedAsDebuggers()
    {
        string[] debuggerNames = { "x64dbg", "ollydbg", "ida64", "idaq64", "windbg", "dnspy", "cheatengine" };

        foreach (string name in debuggerNames)
        {
            Assert.IsTrue(AntiDebug.IsDebuggerProcessName(name), $"'{name}' deve continuar sendo detectado.");
        }
    }
}
