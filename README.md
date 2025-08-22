# Uzi.Core
"Uzi" is the phonetic pronounciation of my last name, so I latched onto it in my namespaces quite some time ago while I was using binary (de-)serialization. 
I thought about trying to rename it before publishing this, but then the saved game packages I built would fail to load.

Anyway _Uzi.Ikosa.Workshop_, _Uzi.Ikosa.Host_ and _Uzi.Ikosa.Client_ are the three main projects.
Workshop and Host are included in _Uzi.Core.sln_.
- __Uzi.Ikosa.Workshop__: for building and manipulating resource packages and map packages
- __Uzi.Ikosa.Host__: for setting up and running a game session
- __Uzi.Ikosa.Client__: for actually playing or game-mastering (depending on the user's login capabilities as adjustable in workshop)

The files for feeding the game system are in [this repository](https://github.com/sageikosa/Ikosa35ResourcesRoot) under the "Root" folder.
config.json in the project (and ultimately runtime) folder for Workshop and Host needs to be set to match the location of "Root" from the resources repository.

It all runs on .NET Framework 4.8 and uses WPF (ie, DirectX 9) as well as WCF for C/S communication, so setting it up can be a bit of a fun ride and exploration
into what are now legacy and seldom used features outside of older enterprise applications.

## More Stuff
[SageIkosa: the Blog](https://sageikosa.guildsmanship.com/)

## Host and Workshop Module Hierarchy
- Contracts are the WCF data contracts and service contracts.
- Core is an abstraction on game mechanics.
- Ikosa is specific SRD implementations.
- UI here is for native model UI presentation

``` mermaid
stateDiagram-v2
   Visualize : Uzi.Visualize
   Packaging : Uzi.Packaging
   Contracts : Uzi.Core.Contracts
   IkosaContracts : Uzi.Ikosa.Contracts
   Core : Uzi.Core
   Ikosa : Uzi.Ikosa
   IkosaUI : Uzi.Ikosa.UI
   IkosaHost : Uzi.Ikosa.Host
   IkosaWorkshop : Uzi.Ikosa.Workshop
   Packaging --> Visualize
   Contracts --> Visualize
   Visualize --> Core
   Visualize --> IkosaContracts
   Core --> Ikosa
   IkosaContracts --> Ikosa
   Ikosa --> IkosaUI
   IkosaUI --> IkosaHost
   IkosaUI --> IkosaWorkshop
```

## Client Module Hierarchy
- UI here is for data contract and view-model presentation
- Proxy includes some view models

``` mermaid
stateDiagram-v2
   Visualize : Uzi.Visualize
   Packaging : Uzi.Packaging
   Contracts : Uzi.Core.Contracts
   IkosaContracts : Uzi.Ikosa.Contracts
   IkosaCUI : Uzi.Ikosa.Client.UI
   Proxy : Uzi.Ikosa.Proxy
   IkosaClient : Uzi.Ikosa.Client
   Packaging --> Visualize
   Contracts --> Visualize
   Visualize --> IkosaContracts
   IkosaContracts --> IkosaCUI
   IkosaContracts --> Proxy
   Proxy --> IkosaClient
   IkosaCUI --> IkosaClient
