namespace CB.CV.Imaging.Filtering {
  public class Filter {
    private readonly int[,] _filter;
    private readonly int _k;
    private readonly int _l;



    public Filter(int[,] mask) {
      _filter = (int[,])mask.Clone();
      _k = _filter.GetLength(0);
      _l = _filter.GetLength(1);
    }



    public int[,] Apply(int[,] input, float scale = 1) {
      // in, out are m x n images (integer data)
      // K is the kernel size (KxK) - currently needs to be an odd number, e.g. 3
      // coeffs[K][K] is a 2D array of integer coefficients
      // scale is a scaling factor to normalise the filter gain

      var m = input.GetLength(0);
      var n = input.GetLength(1);

      var result = new int[m, n];

      for (var i = _k / 2; i < m - _k / 2; ++i) // iterate through image
      {
        for (var j = _l / 2; j < n - _l / 2; ++j) {
          var sum = 0; // sum will be the sum of input data * coeff terms
          for (var ii = -_k / 2; ii <= _k / 2; ++ii) // iterate over kernel
          {
            for (var jj = -_l / 2; jj <= _l / 2; ++jj) {
              var data = input[i + ii, j + jj];
              var coeff = _filter[ii + _k / 2, jj + _l / 2];
              sum += data * coeff;
            }
          }

          result[i, j] = (int)(sum / scale); // scale sum of convolution products and store in output
        }
      }

      return result;
    }
  }
}
