## Stage 1

### Completed items

* Basic REPL (read-eval-print loop) for an expression evaluator
* Implement a lexer, parser, and an evaluator.
* Support `+`, `-`, `*`, `/` operators.
* Print syntax trees.

When parsing the expression `1 + 2 * 3`, the ast like this:

```
└──BinaryExpression
    ├──NumberExpression
    │   └──NumberToken 1
    ├──PlusToken
    └──BinaryExpression
        ├──NumberExpression
        │   └──NumberToken 2
        ├──StarToken
        └──NumberExpression
            └──NumberToken 3
```

## Stage 2

### Completed items

* Generalized parsing using precedences
* Support unary operators, such as `+2` and `-3`
* Support for Boolean literals (`false`, `true`)
* Support for conditions such as `1 == 3 && 2 != 3 || true`
* Support for parenthesized expression, such as (true == false) && (false == false)
* Internal representation for type checking (`Binder`, and `BoundNode`)

### Bound Tree

The first version of the vm was walking the abstract syntax tree directly.
But the ast doesn't have any any *semantic* information. For Example, it doesn't
know which types an expression will be evaluating to, which leading to more
complicated features to impossible. The *bound tree* is created by
the Binder class by walking the ast and binding the nodes to symbolic information.
The binder represents the semantic analysis.

## Stage 3

### Completed items

* Extract compiler into a separate library
* Expose span on diagnostics that indicate where the error occurred
* Support for assignments and variable

### Compilation API

We have added a class called `Compilation` which holds onto the entire state of the
program. For now, it only provide an `Envaluate` API that will interpret the expression.

## Stage 4

### Completed items

* Added tests for lexing all tokens and their combinations
* Added tests for parsing unary and binary operators
* Added tests for evaluating

## Stage5

### Completed items

* Clean-up lexer, parser
* Added `SourceText`, which allow us to know token line number information.

## Stage6

### Completed items

* Add colorization to REPL
* Add compilation unit
* Add chaining to compilations
* Add statements
* Add variable declaration statements

### Scoping and shadowing ;

Logically, scopes are a tree and mirror the structure of the code, for example:

```
{
    var x = 10
    {
        var y = x * 2
        {
            var z = x * y
        }
        {
            var result = x + y
        }
    }
}
```

### Variable Declaration

```
var b = true // variable `b` 's type is boolean.

let a = 1
a = 2 // error, let keyword indicates that the variable is a read-only variable.
```

# Stage 7

## Completed items

* Make evaluation tests more declarative, especially for diagnostics
* Add support for `<,` `<=`, `>=`, and `>`
* Add support for if-statements
* Add support for while-statements
* Add support for for-statements
* Ensure parser doesn't loop infinitely on malformed block
* Ensure binder doesn't crash when binding fabricated identifiers

### If-statement

```js
var a = 1
if a == 1:
    a = 10
else:
    a = 100
    
 a  // output: 10
```

### While-statement

```js
var i = 10 
var result = 0 
while i > 0: 
{ 
    result = result + i 
    i = i - 1
}

result          // output 55
```

### For-Statement
```js
var result = 0 
for i = 1 to 10 
{ 
    result = result + i 
} 

result // output: 55

```
