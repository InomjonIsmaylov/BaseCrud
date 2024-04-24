﻿using BaseCrud.General.Expressions;

namespace BaseCrud.General.Entities;

public sealed record PaginationMetaData(int First, int Rows);

public record FilterExpressionMetaData(string PropertyName, ExpressionConstraintsEnum Constraint, object Value);

public record SortingExpressionMetaData(string PropertyName, bool Ascending);

public record GlobalFilterExpressionMetaData(string SearchString);