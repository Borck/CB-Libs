using System;
using CB.Win32.Native;
using CB.Win32.Native.Structures;
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
    public void GetLocalizedName_SoundAsCompleteString_ExpectException() {
      Kernel32.SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS);
      string name;
      try {
        var resource = @"@%SystemRoot%\System32\mmsys.cpl,-300#immutable1";
        name = Environments.GetLocalizedName(resource);
      } catch (SystemException e) {
        Console.WriteLine(e);
        return;
      }

      Assert.IsFalse(string.IsNullOrEmpty(name));
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
