using System;
using System.Collections.Generic;
using System.Linq;
using Remotion.Linq;

namespace LinqToElk
{
    public class ElasticQueryExecutor : IQueryExecutor
    {
        // Set up a proeprty that will hold the current item being enumerated.
        public SampleDataSourceItem Current { get; private set; }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var whereExpressions = ElasticGeneratorQueryModelVisitor.GenerateElasticQuery(queryModel);
            
            var sampleDataSourceItems = new List<SampleDataSourceItem>();
            
            for (var i = 0; i < 10; i++)
            {
                // Set the current item so currentItemExpression can access it.
                sampleDataSourceItems.Add(new SampleDataSourceItem
                {
                    Name = "Name " + i,
                    Description = "This describes the item in position " + i
                });
            }

            foreach (var whereExpression in whereExpressions)
            {
                var right = whereExpression.Right.ToString();
                right = right.Substring(1, right.Length - 2);
                
                sampleDataSourceItems = sampleDataSourceItems.Where(x => x.Name == right).ToList();
            }
            
            return (IEnumerable<T>) sampleDataSourceItems;
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            var sequence = ExecuteCollection<T>(queryModel);

            return returnDefaultWhenEmpty ? sequence.SingleOrDefault() : sequence.Single();
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            // We'll get to this one later...
            throw new NotImplementedException();
        }
    }
}