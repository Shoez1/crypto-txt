using System;
using CryptoTxt.Security;

namespace CryptoTxt.Tests;

[TestClass]
public sealed class DebugChecksTests
{
    [TestMethod]
    public void Fast_DoesNotInvokeHeuristicProbe()
    {
        int heuristicCalls = 0;
        var checks = new DebugChecks(
            new Func<bool>[] { () => false },
            new Func<bool>[] { () => { heuristicCalls++; return true; } });

        bool result = checks.IsFast();

        Assert.IsFalse(result);
        Assert.AreEqual(0, heuristicCalls);
    }

    [TestMethod]
    public void Full_InvokesHeuristicProbeWhenNativeProbesFindNothing()
    {
        int heuristicCalls = 0;
        var checks = new DebugChecks(
            new Func<bool>[] { () => false },
            new Func<bool>[] { () => { heuristicCalls++; return true; } });

        bool result = checks.IsFull();

        Assert.IsTrue(result);
        Assert.AreEqual(1, heuristicCalls);
    }

    [TestMethod]
    public void Full_NativeProbePositive_DoesNotInvokeHeuristicProbe()
    {
        int heuristicCalls = 0;
        var checks = new DebugChecks(
            new Func<bool>[] { () => true },
            new Func<bool>[] { () => { heuristicCalls++; return true; } });

        bool result = checks.IsFull();

        Assert.IsTrue(result);
        Assert.AreEqual(0, heuristicCalls);
    }

    [TestMethod]
    public void Fast_ProbeFailure_DoesNotBlockRemainingProbes()
    {
        var checks = new DebugChecks(
            new Func<bool>[] { () => throw new InvalidOperationException(), () => true },
            Array.Empty<Func<bool>>());

        Assert.IsTrue(checks.IsFast());
    }
}
