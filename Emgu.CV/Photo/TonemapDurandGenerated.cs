//----------------------------------------------------------------------------
//  This file is automatically generated, do not modify.      
//----------------------------------------------------------------------------



using System;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

namespace Emgu.CV
{
   public static partial class CvInvoke
   {

     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)] 
     internal static extern float cveTonemapDragoGetSaturation(IntPtr obj);
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
     internal static extern void cveTonemapDragoSetSaturation(
        IntPtr obj,  
        float val);
     
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)] 
     internal static extern float cveTonemapDragoGetBias(IntPtr obj);
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
     internal static extern void cveTonemapDragoSetBias(
        IntPtr obj,  
        float val);
     
   }

   public partial class TonemapDrago
   {

     /// <summary>
     /// Positive saturation enhancement value. 1.0 preserves saturation, values greater than 1 increase saturation and values less than 1 decrease it.
     /// </summary>
     public float Saturation
     {
        get { return CvInvoke.cveTonemapDragoGetSaturation(_ptr); } 
        set { CvInvoke.cveTonemapDragoSetSaturation(_ptr, value); }
     }
     
     /// <summary>
     /// Value for bias function in [0, 1] range. Values from 0.7 to 0.9 usually give best results, default value is 0.85.
     /// </summary>
     public float Bias
     {
        get { return CvInvoke.cveTonemapDragoGetBias(_ptr); } 
        set { CvInvoke.cveTonemapDragoSetBias(_ptr, value); }
     }
     
   }
}
