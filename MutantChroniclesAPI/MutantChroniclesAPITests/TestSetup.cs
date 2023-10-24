using MutantChroniclesAPI.Repository;

namespace MutantChroniclesAPI.Tests;


/// <summary>
/// CharacterRepository is cleaned before- and after every test to make sure the lists don't cross over to eachother.
/// Alternative would be to use an interface to create a Mock; but this is not possible for technical reasons when the original repository is a static class.
/// </summary>

[SetUpFixture]
public class TestSetup
{
    [OneTimeSetUp]
    public void TestSetUp()
    {
        CharacterRepository.Characters.Clear();
    }

    [OneTimeTearDown]
    public void TestTearDown()
    {
        CharacterRepository.Characters.Clear();
    }
}
