using System.Drawing;



namespace CB.CV.Imaging.LightField {
  public class PlenopticLightFieldParameters {
    public float MaskSize;
    public float MaskRatio;
    public float Pitch;
    public float StereoOffset;
    public Point Offset;
    public RectangleF Cut;
    public Point Shear;
    public PlenopticLightfieldType CameraLightfieldType;
  }
}
