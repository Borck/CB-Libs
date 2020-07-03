using NUnit.Framework;



namespace CB.Win32 {
  public class EnvironmentsTests {
    [SetUp]
    public void Setup() { }



    [Test]
    public void GetLocalizedName_ComputerAsCompleteString_ExpectComputer() {
      var resource = @"@%windir%\system32\windows.storage.dll,-9012";
      var name = Environments.GetLocalizedName(resource);
      Assert.AreEqual("Computer", name);
    }



    [Test]
    public void GetLocalizedName_ComputerAsFileAndId_ExpectComputer() {
      var resource = @"%windir%\system32\windows.storage.dll";
      var id = 9012;
      var name = Environments.GetLocalizedName(resource, id);
      Assert.AreEqual("Computer", name);
    }
  }
}
