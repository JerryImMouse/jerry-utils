# jerry-utils
Its a pack of utilities created for personal use.
Currently it contains:
- **[IoC realization](#inversion-of-control-realization)**
- **[Logging](#logging)**
- **Simple Dynamic type factory**
- **Simple reflection manager**
- **TypeHelpers, CollectionExtensions**
- **MathHelper, LockUtilities**
> ### Important Note<br/>
> All this utilities depends on each other, so make sure you copied all parts of this pack into your project.

## Inversion of Control realization
IoC manager built with three simple DependencyContainers which inherits IDependencyCollection interface, so you can
choose what realization you need to use in your case. Also provided simple IoCManager to retrieve or inject dependencies into DependencyCollection.
You can find its realization [here](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/IoC/General/IoCManager.cs)
> ### Important Note<br>
> IoCManager is being built using IoCManagerBuilder class, in coupe with InitializeDependencies method
> So you need to build settings with builder and pass them into InitializeDependencies method


Currently there are three realizations of IDependencyCollection interface:
- [FrozenDependencyCollection](#frozendependencycollection)
- [DependencyCollection](#dependencycollection)
- [ReferencedDependencyCollection](#referenceddependencycollection)

### FrozenDependencyCollection
Uses FrozenDictionary to store dependency types and instances.
Created for fast searching values in it.
FrozenDictionary is immutable, so if I wanted to inject something I had to convert it back to Dictionary and then again to FrozenDictioanary, this operations takes a lot of runtime time(~500 microseconds).<br/>
So I created another implementation with usual Dictionary for periodical registering.<br>
[Realization](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/IoC/Frozen/FrozenDependencyCollection.cs)

### DependencyCollection
Uses simple Dicionary type to store dependency types and their instances.<br/>
Created to replace FrozenDictionary because of high speed of adding items.<br>
[Realization](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/IoC/Default/DependencyCollection.cs)

### ReferencedDependencyCollection
Uses FrozenDictionary to store dependency types and instances, BUT it index its values, so if you need to get value by type it can be easily indexed.
If you want to know more you can checkout its realization [here](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/IoC/Referenced/DepIdx.cs)

## Logging
This pack includes "advanced" logging system with ANSI colors for windows consoles and colorless if linux. Also supports logging straight to files, you can specify which handler to use by HandlerFlags enum.<br> If you want to know how to use it look [here](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/Examples/LoggingExample/ExampleProgram.cs)<br>
[Logger Realization](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/Logging/Logger.cs)
