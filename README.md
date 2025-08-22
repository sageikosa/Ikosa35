# Uzi.Core
"Uzi" is the phonetic pronounciation of my last name, so I latched onto it in my namespaces quite some time ago while I was using binary (de-)serialization. 
I thought about trying to rename it before publishing this, but then the saved game packages I built would fail to load.

Anyway "Uzi.Workshop", "Uzi.Host" and "Uzi.Client" are the three main projects.
- Uzi.Workshop: for building and manipulating resource packages and map packages
- Uzi.Host: for setting up and running a game session
- Uzi.Client: for actually playing or game-mastering (depending on the user's login capabilities as adjustable in workshop)

The files for feeding the game system are in [this repository](https://github.com/sageikosa/Ikosa35ResourcesRoot) under the "Root" folder.
config.json in the project (and ultimately runtime) folder for Workshop and Host needs to be set to match the location of "Root" from the resources repository.

It all runs on .NET Framework 4.8 and uses WPF (ie, DirectX 9) as well as WCF for C/S communication, so setting it up can be a bit of a fun ride and exploration
into what are now legacy and seldom used features outside of older enterprise applications.

## More Stuff
[SageIkosa: the Blog](https://sageikosa.guildsmanship.com/)
