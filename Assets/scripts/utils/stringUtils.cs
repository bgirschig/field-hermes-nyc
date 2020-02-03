using System;

public static class StringUtils {
  // inspired from https://www.likeanswer.com/question/325821#!answer_form
  static string[] prefixes= { "f", "a", "p", "n", "μ", "m", string.Empty, "k", "M", "G", "T", "P", "E" };
  
  public static string nice(float x, int significant_digits=3) {
    return nice((double)x, significant_digits);
  }

  public static string nice(double x, int significant_digits=3) {
      //Check for special numbers and non-numbers
      if(double.IsInfinity(x)||double.IsNaN(x)||x==0||significant_digits<=0) {
          return x.ToString();
      }
      // extract sign so we deal with positive numbers only
      int sign=Math.Sign(x);
      x=Math.Abs(x);
      // get scientific exponent, 10^3, 10^6, ...
      int sci= x==0? 0 : (int)Math.Floor(Math.Log(x, 10)/3)*3;
      // scale number to exponent found
      x=x*Math.Pow(10, -sci);
      // find number of digits to the left of the decimal
      int dg= x==0? 0 : (int)Math.Floor(Math.Log(x, 10))+1;
      // adjust decimals to display
      int decimals=Math.Min(significant_digits-dg, 15);
      // format for the decimals
      string fmt=new string('0', decimals);
      if(sci==0) {
          //no exponent
          return string.Format("{0}{1:0."+fmt+"}",
              sign<0?"-":string.Empty,
              Math.Round(x, decimals));
      }
      // find index for prefix. every 3 of sci is a new index
      int index=sci/3+6;
      if(index>=0&&index<prefixes.Length) {
          // with prefix
          return string.Format("{0}{1:0."+fmt+"}{2}",
              sign<0?"-":string.Empty,
              Math.Round(x, decimals),
              prefixes[index]);
      }
      // with 10^exp format
      return string.Format("{0}{1:0."+fmt+"}·10^{2}",
          sign<0?"-":string.Empty,
          Math.Round(x, decimals),
          sci);
  }
}