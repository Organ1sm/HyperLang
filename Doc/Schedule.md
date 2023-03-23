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
for i = 1 to
10
{
    result = result + i
}

result // output: 55

```

# Stage 8

## Completed items

* Add support for bitwise operators
* Add ability to output the bound tree
* Add ability to lower bound tree
* Lower `for`-statements into `while`-statements
* Print syntax and bound tree before evaluation
* Lower `if`, `while`, and `for` into gotos

### Lowering

Right now, the interpreter is directly executing the output of the binder. The
binder produces the bound tree, which is essentially an abstract syntax tree
with rich semantic information associated with each node in the tree.
It represents the semantic understanding of the program, such as the
symbols the names are bound to and the types of intermediary expressions.

Usually, this representation is as rich as the input language. That
characteristic is very useful as it allows exposing it to tooling, for example,
to produce code completion, tool tips, or even refactoring tools.

While it's possible to generate code directly out of this representation it's
not the most convenient approach. Many language constructs can be reduced, also
called *lowered*, to other constructs. That's because languages often provide
syntactic sugar that is merely a shorthand for other constructs. For example:

```js
for i = 1 to 100
< statement >
```

is just a shorthand for this `while`-statement:

```js
let i = 1
while i <= 100:
{
    <statement>
    i = i + 1
}
```

Instead of having to generate code for both, `for`- and `while`-statements, it's
easier to reduce `for` to `while`.

To do this, we're adding the concept of a `BoundTreeRewriter`. This class has
virtual methods for all nodes that can appear in the tree and allows derived
classes to replace specific nodes. Since our bound tree is immutable, the
replacement is happening in a bottom up fashion, which is relatively efficient
for immutable trees because it only requires to rewrite the spine of the tree
(i.e. all ancestors of the nodes that need to be replaced); all other parts of
the tree can be reused.

### Gotos

Actual processors -- or even virtual machines like the .NET runtime -- usually
don't have representation for `if` statements, or specific loops such as `for`
or `while`. Instead, they provide two primitives: *unconditional jumps* and
*conditional jumps*.

In order to make generating code easier, we've added representations for those:
`BoundGotoStatement` and `BoundConditionalGotoStatement`. In order to specify
the target of the jump, we need a representation for the label, for which we use
the new `LabelSymbol`, as well as a way to label a specific statement, for which
we use `BoundLabelStatement`. It's tempting to define the `BoundLabelStatement`
similar to how C# represents them in the syntax, which means that it references
a label and a statement but that's very inconvenient. Very often, we need a way
to create a label for whatever comes after the current node. However, since
nodes cannot navigate to their siblings, one usually cannot easily get "the
following" statement. The easiest way to solve this problem is by not
referencing a statement from `BoundLabelStatement` and simply have the semantics
that the label it references applies to the next statement.

With these primitives, it's pretty straightforward to replace the flow-control
elements.