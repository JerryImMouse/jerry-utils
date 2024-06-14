# jerry-utils
Its a pack of utilities created for personal use.
Currently it contains:
- **[IoC realization](#inversion-of-control-realization)**
- **[Logging](#logging)**
- **Simple Dynamic type factory**
- **Simple reflection manager**
- **TypeHelpers, CollectionExtensions**
- **MathHelper, LockUtilities**
- **[Http Utility](#http-utility)**

## Inversion of Control realization
IoC manager built with three simple DependencyContainers which inherits IDependencyCollection interface, so you can
choose what realization you need to use in your case. Also provided simple IoCManager to retrieve or inject dependencies into DependencyCollection.
You can find its realization [here](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/IoC/General/IoCManager.cs)
> ### Important Note
> IoCManager is being built using IoCManagerBuilder class, in coupe with InitializeDependencies method  
> So you need to build settings with builder and pass them into InitializeDependencies method


Currently there are three realizations of IDependencyCollection interface:
- [FrozenDependencyCollection](#frozendependencycollection)
- [DependencyCollection](#dependencycollection)
- [ReferencedDependencyCollection](#referenceddependencycollection)

### FrozenDependencyCollection
Uses FrozenDictionary to store dependency types and instances.
Created for fast searching values in it.
FrozenDictionary is immutable, so if I wanted to inject something I had to convert it back to Dictionary and then again to FrozenDictioanary, this operations takes a lot of runtime time(~500 microseconds).
So I created another implementation with usual Dictionary for periodical registering.  
[Realization](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/IoC/Frozen/FrozenDependencyCollection.cs)

### DependencyCollection
Uses simple Dicionary type to store dependency types and their instances.
Created to replace FrozenDictionary because of high speed of adding items.  
[Realization](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/IoC/Default/DependencyCollection.cs)

### ReferencedDependencyCollection
Uses FrozenDictionary to store dependency types and instances, BUT it index its values, so if you need to get value by type it can be easily indexed.
If you want to know more you can check out its realization [here](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/IoC/Referenced/DepIdx.cs)

## Logging
This pack includes "advanced" logging system with ANSI colors for windows consoles and colorless if linux. Also supports logging straight to files, you can specify which handler to use by HandlerFlags enum. If you want to know how to use it look [here](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/Examples/LoggingExample/ExampleProgram.cs)  
[Logger Realization](https://github.com/JerryImMouse/jerry-utils/blob/master/Project.Utilities/Logging/Logger.cs)

## Http Utility
This one implements simple MainListener class that wraps HttpListener for ease of use, this also implements handlers group interface that can be inherited and used to create a class of handlers for http requests. HttpContext is a wrapper class that wraps around HttpListenerContext and provides a lot of tools to respond on http requests easily. 
Created for my personal use as I needed it.

# Thanks
As this repository recently became public, I'll leave thanks here for all people who made an impact to this package.  

[Space Wizards Federation]("https://github.com/space-wizards") for giving me an idea and realization samples, logging and referenced IoC container were partially taken from their code.
