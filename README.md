# logix
🔌 simulator for logic gates and integrated circuits, created with [raylib](https://github.com/raysan5/raylib) and made cross-platform using .NET (6)

## Table of contents

- [Getting started](#getting-started)
- [FLIS-Processor](#flis-processor)
- [Plugins](#plugins)
    - [Custom Components](#custom-components)
    - [Plugin Methods](#plugin-methods)

## Getting started

If you just want to get your hands dirty and start placing out logic gates and make circuits, all you'll need is a `dotnet` runtime, and to clone down the repository. 

The `.csproj` targets `.NET 6`, so you'll need a runtime with version >= 6.

```
git clone https://github.com/dcronqvist/logix
cd logix
dotnet run
```

## FLIS-Processor

The FLIS-Processor described in the course [EDA452 Introduction to computer engineering](https://student.portal.chalmers.se/en/chalmersstudies/courseinformation/Pages/SearchCourse.aspx?course_id=31745&parsergrp=3) at Chalmers has been implemented in LogiX and exists as an example project in [examples/microprocessors/flisp/](examples/microprocessors/flisp/) for you to play around with.

It is not complete, since only a small subset of the instruction set has been implemented, but feel free to contribute since it is quite tedious to implement the control sequences of all ~250 instructions (more info [here](examples/microprocessors/flisp/)).

## Plugins

You can already create integrated circuits that you can export and share if you'd like - but these components might become very complex and therefore slow down the simulation quite substantially. 

### Custom Components

Plugins are an easy way to create components which are very flexible and operate in a single simulation tick. With plugins you can make your own **Custom Components** which basically override the basic functionality of components in LogiX, allowing you to take control of the component completely.

### Plugin Methods

Plugins also allow you to create **Plugin Methods** which are void methods that can be run from the editor UI, allowing you access to the entire editor. With access to the editor, you can add/delete/copy components in the simulation, change the UI, whatever you feel like. 