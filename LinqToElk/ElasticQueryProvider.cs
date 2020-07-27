using System;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToElk
{
    public class ElasticQueryProvider: IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {            
            // var elementType = TypeHelper.GetSequenceElementType(expression.Type);
            // return new ElasticQuery<TElement>(this, expression);
            //TODO
            throw new Exception();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        { 
            return new ElasticQuery<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            //TODO
            return "";
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);;
        }
    }
}