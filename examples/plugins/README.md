# Plugins

You can already create integrated circuits that you can export and share if you'd like - but these components might become very complex and therefore slow down the simulation quite substantially. 

## Custom Components

Plugins are an easy way to create components which are very flexible and operate in a single simulation tick. With plugins you can make your own **Custom Components** which basically override the basic functionality of components in LogiX, allowing you to take control of the component completely.

## Plugin Methods

Plugins also allow you to create **Plugin Methods** which are void methods that can be run from the editor UI, allowing you access to the entire editor. With access to the editor, you can add/delete/copy components in the simulation, change the UI, whatever you feel like. 