# LINQ To ElasticSearch

> 🚧 Experimental State 🚧

This repository is a LINQ Provider To ElasticSearch

# Where can I get it?
Install using the [LinqToElasticSearch NuGet package](https://www.nuget.org/packages/LinqToElasticSearch):
```
dotnet add package LinqToElasticSearch
```


# Getting Started

https://www.nuget.org/packages/LinqToElasticSearch/

![Image of Exemple](https://raw.githubusercontent.com/SapiensiaTecnologia/LinqToElasticSearch/master/github/exemple.png)


# Compatibility
This Library has compatibility with Elasticsearch .Net & NEST Client and Elastic Search 7.x version.

# Support Clauses with Integration Tests
* Select
* Where
  * Where(x => x.Any(single expression))
  * Where(x => x.All(single expression))
  * Where(x => x.Contains(string))
  * Where(x => x.StartsWith(string))
  * Where(x => x.EndsWith(string))
* OrderBy
* Skip
* Take
* GroupBy(x => x.Prop1)
* GroupBy(x => new {x.Prop1, x.Prop2})
