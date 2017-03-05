using System.Collections.Generic;

namespace Tiesmaster.Dcc
{
    public class TapeRepository
    {
        private readonly Dictionary<RequestKey, TapedResponse> _tapes = new Dictionary<RequestKey, TapedResponse>();

        public TapedResponse Get(RequestKey key)
        {
            TapedResponse tapedResponse;
            _tapes.TryGetValue(key, out tapedResponse);
            return tapedResponse;
        }

        public void Store(RequestKey requestKey, TapedResponse tapedResponse)
        {
            _tapes[requestKey] = tapedResponse;
        }
    }
}