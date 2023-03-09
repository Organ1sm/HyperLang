
## stage 1
### Completed items

* Generalized parsing using precedences
* Support `+`, `-`, `*`, `/` operators.
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