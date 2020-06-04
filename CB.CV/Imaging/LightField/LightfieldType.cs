using System.ComponentModel;



namespace CB.CV.Imaging.LightField {
  public enum LightfieldType {
    None = 1,

    [Description("Camera-Array")]
    CamArray = 2,
    Heterodyned = 4,
    Plenoptic = 8,
    Polymorphic = 16,
    //[Description("Single-Image")]
    //SingleImage = 32
  }
}
