using System.Threading.Tasks;
using Orleans.TestingHost;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.Tests;

[TestClass]
public class RobotGrainTests
{
    static TestCluster? _cluster;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _cluster = new TestClusterBuilder()
            .AddSiloBuilderConfigurator<SiloBuliderConfigurator>()
            .Build();

        _cluster.Deploy();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _cluster?.StopAllSilos();
    }

    [TestMethod]
    public async Task TestInstructions()
    {
        var robot = _cluster!.GrainFactory
            .GetGrain<IRobotGrain>("test");

        await robot.AddInstruction("Do the dishes");
        await robot.AddInstruction("Take out the trash");
        await robot.AddInstruction("Do the laundry");

        Assert.AreEqual(3, await robot.GetInstructionCount());
        Assert.AreEqual("Do the dishes", await robot.GetNextInstruction());
        Assert.AreEqual("Take out the trash", await robot.GetNextInstruction());
        Assert.AreEqual("Do the laundry", await robot.GetNextInstruction());
        Assert.AreEqual(0, await robot.GetInstructionCount());
        Assert.IsNull(await robot.GetNextInstruction());
    }
}