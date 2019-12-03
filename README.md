# Unity Analytics for Snowplow

[![Build Status][travis-image]][travis]
[![Release][release-image]][releases]
[![License][license-image]][license]

## Overview

Add analytics to your Unity games and apps with the **[Snowplow][snowplow]** event tracker for **[Unity][unity]**.

With this tracker you can collect event data from your Unity-based applications, games or frameworks.

## Quickstart

### Requirements

* SnowplowTracker requires Unity 2018.1+ and .NET Standard 2.0 Api Compatibility Level. 
* SnowplowTracker supports both Mono and IL2CPP scripting backends.
* SnowplowTracker.Demo and SnowplowTracker.Tests are Unity 2018.4.13f1 (LTS) projects.

### Building

Assuming git, **[Vagrant][vagrant-install]** and **[VirtualBox][virtualbox-install]** installed:

```bash
 host$ git clone https://github.com/snowplow/snowplow-unity-tracker.git
 host$ cd snowplow-unity-tracker
```

### Development

To work on the Tracker:

* Open the following file in your IDE of choice (Visual Studio, Visual Studio for Mac or JetBrains Rider): `snowplow-unity-tracker/SnowplowTracker/SnowplowTracker.sln`
* This solution file will open the `SnowplowTracker` and `UnityJSON` libraries in your editor.
* Build files will output to bin/Debug or bin/Release depending on your configuration.
* Built DLLs will also be copied on successful builds to the Demo and Test projects Assets folders.

### Setting up a Test Endpoint

```bash
host$ vagrant up && vagrant ssh
guest$ cd /vagrant
guest$ mb &
guest$ curl -X POST -d @/vagrant/Resources/imposter.json http://localhost:2525/imposters
```

The test collector to use: `http://localhost:4545`

Now open Mountebank in your browser (on host is fine):
* **[http://localhost:2525](http://localhost:2525)**

### Testing Framework

Requires: Unity 2018.4.13f1 (LTS)

* Open `snowplow-unity-tracker/SnowplowTracker.Tests` in the Unity Editor
* Build the SnowplowTracker solution (desribed above) if you have made any changes to the Tracker. This will copy the DLLs to Demo and Tests asset folders.
* Ensure the Mountebank Test Collector is up and running (desribed above)
* Open the Test Runner `Window` -> `General` -> `Test Runner` and open `EditMode`
* Click `Run All` to run all tests

Please note that all Unit Tests are written with **[NUnit][nunit]**.

### Running the Snowplow Demo Game

Requires: Unity 2018.4.13f1 (LTS)

To open the Demo Game simply add the following folder within Unity Hub, `snowplow-unity-tracker/SnowplowTracker.Demo`, you can then build and run it yourself.

Tested Platforms:

* Windows
* Mac OSX
* Linux
* iOS
* Android

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

[release-image]: http://img.shields.io/badge/release-0.4.0-blue.svg?style=flat
[releases]: https://github.com/snowplow/snowplow-unity-tracker/releases

[license-image]: http://img.shields.io/badge/license-Apache--2-blue.svg?style=flat
[license]: http://www.apache.org/licenses/LICENSE-2.0

[travis-image]: https://travis-ci.org/snowplow/snowplow-unity-tracker.svg?branch=master
[travis]: https://travis-ci.org/snowplow/snowplow-unity-tracker

[techdocs-image]: https://d3i6fms1cm1j0i.cloudfront.net/github/images/techdocs.png
[setup-image]: https://d3i6fms1cm1j0i.cloudfront.net/github/images/setup.png
[roadmap-image]: https://d3i6fms1cm1j0i.cloudfront.net/github/images/roadmap.png
[contributing-image]: https://d3i6fms1cm1j0i.cloudfront.net/github/images/contributing.png

[techdocs]: https://github.com/snowplow/snowplow/wiki/Unity-Tracker
[setup]: https://github.com/snowplow/snowplow/wiki/Unity-Tracker-Setup
[roadmap]: https://github.com/snowplow/snowplow/wiki/Product-roadmap
[contributing]: https://github.com/snowplow/snowplow/wiki/Contributing
