namespace BaseCrud.Internal;

using FilterFunction = Func<object, Expression<Func<object, bool>>>;

internal record CompiledPredicate(FilterFunction FilterFunction, Type FilterType);