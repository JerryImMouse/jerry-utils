# jerry-utils
Its a pack of utilities created for [personal](#disclaimer) use.
Currently it contains:
- **[IoC realization](#inversion-of-control-realization)**
- **Simple Dynamic type factory**
- **Simple reflection manager**
- **TypeHelpers, CollectionExtensions**
- **MathHelper, LockUtilities**
> **Note**<br/>
> All this utilities depends on each other, so make sure you copied all parts of this pack into your project.

## Inversion of Control realization
IoC manager built with two simple DependencyContainers which inherits IDependencyCollection interface, so you can
choose what realization you need to use in your case. Also provided simple IoCManager to retrieve or inject dependencies into DependencyCollection.

Currently there are two realizations of IDependencyCollection interface:
- [FrozenDependencyCollection](#frozendependencycollection)
- [DependencyCollection](#dependencycollection)
- [ReferencedDependencyCollection](#referenceddependencycollection)

### FrozenDependencyCollection
Uses FrozenDictionary to store dependency types and instances.
Created for fast searching values in it.
FrozenDictionary is immutable, so if I wanted to inject something I had to convert it back to Dictionary and then again to FrozenDictioanary, this operations takes a lot of runtime time(~500 microseconds).<br/>
So I created another implementation with usual Dictionary for periodical registering


### DependencyCollection
Uses simple Dicionary type to store dependency types and their instances.<br/>
Created to replace FrozenDictionary because of high speed of adding items.

### ReferencedDependencyCollection
Uses FrozenDictionary to store dependency types and instances, BUT it index its values, so if you need to get value by type it can be easily indexed.
To look at its realization you can check DepIdx.cs file in Referenced folder

## Disclaimer
If someone found this repository and he found this useless or shitty wrote for himself, please remember its a personal use pack, i posted it here to easily copy and paste into my **own** projects.
