namespace NBasis.Lambda
{
    public class MultiEventSerializer : Amazon.Lambda.Core.ILambdaSerializer
    {
        public T Deserialize<T>(Stream requestStream)
        {
            if (typeof(T) == typeof(MultiEvent))
            {
                return (T) (object) new MultiEvent(requestStream);
            }
            return default;
        }

        public void Serialize<T>(T response, Stream responseStream)
        {
            throw new NotImplementedException("Cannot serialize MultiEvents");
        }
    }
}
