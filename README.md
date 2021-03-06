## Introduction

This is the README file for MSM, a simple multi server manager written in C#.

MSM is an opensource tool aimed at easing the maintenance of multiple ssh connected servers.

## Features (some not fully complete)
* Fully X64 compatible, fast with simple sourcecode (and properly balanced DllImports)
* Fully portable by design, old style settings are also honoured
* Optimized for very large server/node/variables/keyword lists and combinations
* Cron scheduling for automated commands
* Keywords to only run commands on certain servers
* Variables configurable per server usable in commands
* Works with most (terminal) clients
* Built in updater

## Building

* Should be straight forward using Visual Studio 2017 (or newer), don't know about compatibility with mono and other .NET alternatives.

## Acknowledgements

Following components are being used by MSM:

* [Quartz.net] (https://github.com/quartznet/quartznet) (Apache License, Version 2.0) as a scheduling agent for performing interval/scheduled events
* [Newtonsoft.Json] (https://github.com/JamesNK/Newtonsoft.Json) (MIT License) for (de)serializing objects
* [DockPanelSuite] (https://github.com/dockpanelsuite/dockpanelsuite) (MIT License) for UI elements and theming
* [Icons] (https://www.flaticon.com/packs/essential-collection, Designed by SmashIcons) (Flaticon Basic License)
* [log4net] (https://logging.apache.org/log4net/) (Apache License, Version 2.0)

## License

MSM (Multi Server Manager) is released as open source software under the [GPL v3](https://opensource.org/licenses/gpl-3.0.html) 
license, see the [LICENSE](./LICENSE.md) file in the project root for the full license text.