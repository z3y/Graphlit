using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Graphlit
{
  [PublicAPI]
  public class FreeImage
  {
    public enum ImageFormat
    {
      TARGA = 17,
      PSD = 20,
      TIFF = 18,
      EXR = 29,
      JPEG = 2,
      BMP = 0,
      ICO = 1,
      JNG = 3,
      KOALA = 4,
      LBM = 5,
      IFF = LBM,
      MNG = 6,
      PBM = 7,
      PBMRAW = 8,
      PCD = 9,
      PCX = 10,
      PGM = 11,
      PGMRAW = 12,
      PNG = 13,
      PPM = 14,
      PPMRAW = 15,
      RAS = 16,
      WBMP = 19,
      CUT = 21,
      XBM = 22,
      XPM = 23,
      DDS = 24,
      GIF = 25,
      HDR = 26,
      FAXG3 = 27,
      SGI = 28,
      J2K = 30,
      JP2 = 31,
      PFM = 32,
      PICT = 33,
      RAW = 34,
      WEBP = 35,
      JXR = 36,
      FIF_UNKNOWN = -1
    }

    internal enum ImageType
    {
      FIT_UNKNOWN = 0,
      FIT_BITMAP = 1,
      FIT_UINT16 = 2,
      FIT_INT16 = 3,
      FIT_UINT32 = 4,
      FIT_INT32 = 5,
      FIT_FLOAT = 6,
      FIT_DOUBLE = 7,
      FIT_COMPLEX = 8,
      FIT_RGB16 = 9,
      FIT_RGBA16 = 10,
      FIT_RGBF = 11,
      FIT_RGBAF = 12
    }

    internal enum ColorType
    {
      FIC_MINISWHITE = 0,
      FIC_MINISBLACK = 1,
      FIC_RGB = 2,
      FIC_PALETTE = 3,
      FIC_RGBALPHA = 4,
      FIC_CMYK = 5
    }

    // https://freeimage.sourceforge.io/fnet/html/FA33955C.htm
    public enum FREE_IMAGE_COLOR_CHANNEL
    {
      FICC_RGB,
      FICC_RED,
      FICC_GREEN,
      FICC_BLUE,
      FICC_ALPHA,
      FICC_BLACK,
      FICC_REAL,
      FICC_IMAG,
      FICC_MAG,
      FICC_PHASE
    }

    // https://freeimage.sourceforge.io/fnet/html/A732273F.htm
    public enum FREE_IMAGE_COLOR_TYPE
    {
      FIC_MINISWHITE,
      FIC_MINISBLACK,
      FIC_RGB,
      FIC_PALETTE,
      FIC_RGBALPHA,
      FIC_CMYK,
    }

    public enum FREE_IMAGE_FILTER
    {
      Box,
      Bicubic,
      Bilinear,
      Bspline,
      Catmullrom,
      Lanczos3
    }

    [Flags]
    public enum FREE_IMAGE_COLOR_DEPTH
    {
      FICD_UNKNOWN,
      FICD_AUTO,
      FICD_01_BPP,
      FICD_01_BPP_DITHER,
      FICD_01_BPP_THRESHOLD,
      FICD_04_BPP,
      FICD_08_BPP,
      FICD_16_BPP_555,
      FICD_16_BPP,
      FICD_24_BPP,
      FICD_32_BPP,
      FICD_REORDER_PALETTE,
      FICD_FORCE_GREYSCALE,
      FICD_COLOR_MASK
    }

    const string FreeImageDLL = "FreeImage";

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_GetFIFFromFormat")]
    private static extern ImageFormat GetFIFFromFormat(string format);

    public static ImageFormat GetImageFormat(string extension)
    {
      if (extension.Equals("tga", StringComparison.OrdinalIgnoreCase)) return ImageFormat.TARGA;
      if (extension.Equals("jpg", StringComparison.OrdinalIgnoreCase)) return ImageFormat.JPEG;
      if (extension.Equals("tif", StringComparison.OrdinalIgnoreCase)) return ImageFormat.TIFF;



      return GetFIFFromFormat(extension);
    }

    public static ImageFormat GetImageFormatAtPath(string path)
    {
      var extension = System.IO.Path.GetExtension(path).Remove(0, 1);
      return GetImageFormat(extension);
    }

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_Load")]
    public static extern IntPtr FreeImage_Load(ImageFormat format, string filename, int flags = 0);

    public static IntPtr FreeImage_Load(string path, int flags = 0)
    {
      string absolutePath = path;
      if (path.StartsWith("Packages", StringComparison.OrdinalIgnoreCase))
      {
        absolutePath = System.IO.Path.GetFullPath(path);
      }

      return FreeImage_Load(GetImageFormatAtPath(absolutePath), absolutePath, flags);
    }

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_Unload")]
    public static extern void FreeImage_Unload(IntPtr handle);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_FlipHorizontal")]
    public static extern bool FlipHorizontal(IntPtr dib);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_AdjustContrast")]
    public static extern bool AdjustContrast(IntPtr dib, double percentage);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_GetChannel")]
    public static extern IntPtr GetChannel(IntPtr dib, FREE_IMAGE_COLOR_CHANNEL channel);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_SetChannel")]
    public static extern bool SetChannel(IntPtr dib, IntPtr dib8, FREE_IMAGE_COLOR_CHANNEL channel);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_Save")]
    public static extern bool FreeImage_Save(ImageFormat format, IntPtr handle, string filename, int flags = 0);

    //[DllImport(FreeImageDLL, EntryPoint = "FreeImage_FillBackground")]
    //public static extern bool FillBackground(IntPtr dib, Color color, int options = 0);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_Rescale")]
    public static extern IntPtr Rescale(IntPtr dib, int dst_width, int dst_height, FREE_IMAGE_FILTER filter);

    public delegate void OutputMessageFunction(ImageFormat fif, string message);
    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_SetOutputMessage")]
    public static extern void SetOutputMessage(OutputMessageFunction messageFunction);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_GetWidth")]
    internal static extern uint GetWidth(IntPtr handle);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_GetHeight")]
    internal static extern uint GetHeight(IntPtr handle);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_GetImageType")]
    internal static extern ImageType GetImageType(IntPtr dib);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_GetBPP")]
    internal static extern uint GetBPP(IntPtr dib);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_SetTransparent")]
    public static extern void SetTransparent(IntPtr dib, bool enabled = true);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_SetTransparentIndex")]
    public static extern void SetTransparentIndex(IntPtr dib, int index = 3);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_IsTransparent")]
    public static extern bool IsTransparent(IntPtr dib);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_ConvertTo32Bits")]
    public static extern IntPtr ConvertTo32Bits(IntPtr dib);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_ConvertTo8Bits")]
    public static extern IntPtr ConvertTo8Bits(IntPtr dib);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_Invert")]
    public static extern bool Invert(IntPtr dib);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_ConvertToGreyscale")]
    public static extern IntPtr ConvertToGreyscale(IntPtr dib);

    [DllImport(FreeImageDLL, EntryPoint = "FreeImage_Allocate")]
    public static extern IntPtr Allocate(int width, int height, int bpp);
    public static (uint, uint) GetWithAndHeight(IntPtr dib)
    {
      uint width = GetWidth(dib);
      uint height = GetHeight(dib);

      return (width, height);
    }
  }
}