# WP-Message-Backup-Extractor
Microsoft was nice enough to provide an app to back up messages and contacts from your Windows Phone. 
However, the intended purpose of this is so that you can put them back on another Windows Phone, so
there's no way (as far as I can tell) of viewing the messages on your computer. So I created this program
to read the XML files created by the app and allow them to be viewed. At the moment it's just a console
application, but I want to eventually turn it into a library for turning the backup files into objects, 
and then a command-line and a GUI viewing/extracting program.
