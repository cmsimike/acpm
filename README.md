### acpm - Assetto Corsa Package Manager (0.0.1)

A (hopefully) simple way to help you manage your Assetto Corsa mods.

acpm is a little application that keeps track of which version of a package you have installed (where a package is a plugin/mod/app) and notifies you when the author releases an update.

Please exit Assetto Corsa before installing/updating any packages.

#### THIS IS A PROTYPE!
As of right now, it does not install packages.
Don't judge me too harshly on my code - again PROTOTYPE. I do hope on cleaning this up over time. This is my first C# project in a very long time.

#### Known Limitations
Since this is a prototype, it doesn't do a lot of things it probably should. Known limitations:
- This application will only assume the game is installed in the default c:\program files(blah blah bla)\assettocoresa. 
- No uninstall
- Updating packages overwrites any file on your hard drive that is in the zip. Not sure how that will play out.
- If you click on "install" for any packages that may show up, it won't install anything but will create a package.json in the same directory as the executable. Final location will change.

### Issue Tracker
Please submit any and all bugs and questions about acpm and any requests for additional data for [acpmr](https://github.com/cmsimike/acpmr/).


## To Download
Click into the releases tab and download the one that most closely matches your config.
