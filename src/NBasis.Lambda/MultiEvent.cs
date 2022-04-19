using System.Text;

namespace NBasis.Lambda
{
    public class MultiEvent
    {
        readonly Stream _requestStream;

        public MultiEvent(Stream requestStream)
        {
            _requestStream = requestStream;
        }

        public Type DetectType(IDictionary<Type, Func<string,bool>> typeTable)
        {
            // read stream by line and detect type
            using (var streamReader = new StreamReader(GetStream(), Encoding.UTF8, false, 4096, true))
            {
                var line = streamReader.ReadLine();
                while (line != null)
                {
                    foreach (var type in typeTable.Keys)
                    {
                        if (typeTable[type].Invoke(line))
                        {
                            return type;
                        }
                    }

                    line = streamReader.ReadLine();
                }
            }

            return null;
        }

        public Stream GetStream()
        {
            _requestStream.Seek(0, SeekOrigin.Begin);
            return _requestStream;
        }

        internal string GetString()
        {
            using var streamReader = new StreamReader(GetStream(), Encoding.UTF8, false, 4096, true);

            return streamReader.ReadToEnd();
        }
    }
}
