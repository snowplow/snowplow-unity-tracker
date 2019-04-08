# Unity Analytics for Snowplow

[![Release][release-image]][releases] [![License][license-image]][license]

## Overview

Add analytics to your Unity games and apps with the **[Snowplow][snowplow]** event tracker for **[Unity][unity]**.

With this tracker you can collect event data from your Unity-based applications, games or frameworks.

## Quickstart

### Building

Assuming git, **[Vagrant][vagrant-install]** and **[VirtualBox][virtualbox-install]** installed:

```bash
 host$ git clone https://github.com/snowplow/snowplow-unity-tracker.git
 host$ cd snowplow-unity-tracker
 host$ vagrant up && vagrant ssh
guest$ cd /vagrant
```

### Setting up a Test Endpoint

```bash
guest$ mb &
guest$ curl -X POST -d @/vagrant/Resources/imposter.json http://localhost:2525/imposters
```

The test collector to use: `http://localhost:4545`

Now open Mountebank in your browser (on host is fine):
* **[http://localhost:2525](http://localhost:2525)**

### Development

Currently we have only tested developing from within `MonoDevelop-Unity`.  The IDE that is bundled with the Unity installer for Mac OSX.  However the project *should* open with any C# IDE.

To work on the Tracker:

* Open the following file in your IDE of choice: `snowplow-unity-tracker/SnowplowTracker/SnowplowTracker.sln`
* This solution file will open the `SnowplowTracker`, `SnowplowTrackerTests`, `UnityHTTP` and `UnityJSON` libraries in your editor.

Please note that all Unit Tests are written with **[NUnit][nunit]**.

### Running the Snowpong Demo Game

To open the Demo Game simply select the following file solution from Unity, `snowplow-unity-tracker/DemoGame/DemoGame.sln`, you can then build and run it yourself.
Currently the Demo will only play on your desktop as the layout has not been configured for mobile platforms yet.

Tested Platforms:

* Windows
* Mac OSX
* Linux
* iOS (Opens but not playable as of yet)

## Find out more

| Technical Docs                 | Setup Guide               | Roadmap                | Contributing                     |
|--------------------------------|---------------------------|------------------------|----------------------------------|
| ![i1][techdocs-image]          | ![i2][setup-image]        | ![i3][roadmap-image]   | ![i4][contributing-image]        |
| **[Technical Docs][techdocs]** | **[Setup Guide][setup]**  | **[Roadmap][roadmap]** | **[Contributing][contributing]** |

## Copyright and license

The Snowplow Unity Tracker is copyright 2015-2019 Snowplow Analytics Ltd.

Licensed under the **[Apache License, Version 2.0][license]** (the "License");
you may not use this software except in compliance with the License.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

[snowplow]: http://snowplowanalytics.com
[unity]: https://unity3d.com/
[nunit]: http://www.nunit.org/

[vagrant-install]: http://docs.vagrantup.com/v2/installation/index.html
[virtualbox-install]: https://www.virtualbox.org/wiki/Downloads

[release-image]: http://img.shields.io/badge/release-0.3.0-blue.svg?style=flat
[releases]: https://github.com/snowplow/snowplow-unity-tracker/releases

[license-image]: http://img.shields.io/badge/license-Apache--2-blue.svg?style=flat
[license]: http://www.apache.org/licenses/LICENSE-2.0

[techdocs-image]: https://d3i6fms1cm1j0i.cloudfront.net/github/images/techdocs.png
[setup-image]: https://d3i6fms1cm1j0i.cloudfront.net/github/images/setup.png
[roadmap-image]: https://d3i6fms1cm1j0i.cloudfront.net/github/images/roadmap.png
[contributing-image]: https://d3i6fms1cm1j0i.cloudfront.net/github/images/contributing.png

[techdocs]: https://github.com/snowplow/snowplow/wiki/Unity-Tracker
[setup]: https://github.com/snowplow/snowplow/wiki/Unity-Tracker-Setup
[roadmap]: https://github.com/snowplow/snowplow/wiki/Product-roadmap
[contributing]: https://github.com/snowplow/snowplow/wiki/Contributing
