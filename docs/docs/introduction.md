# Introduction

**BaseCrud** is an abstraction with base implementation of simple CRUD operations.

*Motto: Getting rid of repetitive implementation of CRUD operations that differs only by model and DTO types.*

## Benefits

1. Crud operations are already implemented, no need to waste time writing them again
2. Flexible if one wants to change the behavior of crud operation
3. One can easily override any method if default behavior is not suitable
4. Has predefined abstraction level that helps writing readable code

### Shortcomings (for now)

1. For the most part suitable for web development only
2. Implemented only for EntityFramework for working with database
3. Implemented only for Angular PrimeNG for working with frontend data-tables