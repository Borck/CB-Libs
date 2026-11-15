namespace CB.System.IO;

using global::System;
using global::System.IO;
using global::System.Text;
using global::System.Threading.Tasks;

public class IndexedStreamReader : StreamReader {
  public int LineIndex { get; private set; }



  public IndexedStreamReader(Stream stream)
    : base(stream) { }



  public IndexedStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
    : base(
      stream,
      detectEncodingFromByteOrderMarks
    ) { }



  public IndexedStreamReader(Stream stream, Encoding encoding)
    : base(stream, encoding) { }



  public IndexedStreamReader(Stream stream,
                             Encoding encoding,
                             bool detectEncodingFromByteOrderMarks)
    : base(
      stream,
      encoding,
      detectEncodingFromByteOrderMarks
    ) { }



  public IndexedStreamReader(Stream stream,
                             Encoding encoding,
                             bool detectEncodingFromByteOrderMarks,
                             int bufferSize)
    : base(
      stream,
      encoding,
      detectEncodingFromByteOrderMarks,
      bufferSize
    ) { }



  public IndexedStreamReader(Stream stream,
                             Encoding encoding,
                             bool detectEncodingFromByteOrderMarks,
                             int bufferSize,
                             bool leaveOpen)
    : base(
      stream,
      encoding,
      detectEncodingFromByteOrderMarks,
      bufferSize,
      leaveOpen
    ) { }



  public IndexedStreamReader(string path)
    : base(path) { }



  public IndexedStreamReader(string path, bool detectEncodingFromByteOrderMarks)
    : base(
      path,
      detectEncodingFromByteOrderMarks
    ) { }



  public IndexedStreamReader(string path, Encoding encoding)
    : base(path, encoding) { }



  public IndexedStreamReader(string path,
                             Encoding encoding,
                             bool detectEncodingFromByteOrderMarks)
    : base(
      path,
      encoding,
      detectEncodingFromByteOrderMarks
    ) { }



  public IndexedStreamReader(string path,
                             Encoding encoding,
                             bool detectEncodingFromByteOrderMarks,
                             int bufferSize)
    : base(
      path,
      encoding,
      detectEncodingFromByteOrderMarks,
      bufferSize
    ) { }



  public override string ReadLine() {
    var line = base.ReadLine();
    LineIndex++;
    return line;
  }



  public override Task<string> ReadLineAsync() => throw new NotImplementedException();



  public override int Read() => throw new NotImplementedException();



  public override int Read(char[] buffer, int index, int count) => throw new NotImplementedException();



  public override Task<int> ReadAsync(char[] buffer, int index, int count) => throw new NotImplementedException();



  public override int ReadBlock(char[] buffer, int index, int count) => throw new NotImplementedException();



  public override Task<int> ReadBlockAsync(char[] buffer, int index, int count) => throw new NotImplementedException();



  public override string ReadToEnd() => throw new NotImplementedException();



  public override Task<string> ReadToEndAsync() => throw new NotImplementedException();
}
