using Nest;

namespace LinqToElk
{
    public class PropertyNameInferrerParser
    {
        private readonly IElasticClient _elasticClient;

        public PropertyNameInferrerParser(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public string Parser(string input)
        {
            return _elasticClient.ConnectionSettings.DefaultFieldNameInferrer(input);
        }
    }
}