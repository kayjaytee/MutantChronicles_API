using MutantChroniclesAPI.Interface;
using NSubstitute;

namespace MutantChroniclesAPITests.Interface;

[TestFixture]
public class ICombatServiceTests
{
    private ICombatService combatService;

    [SetUp]
    public void SetUp()
    {
        combatService = Substitute.For<ICombatService>();
    }
}
