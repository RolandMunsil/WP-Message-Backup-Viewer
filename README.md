# WP-Message-Backup-Extractor
Microsoft was nice enough to provide an app to back up messages and contacts from your Windows Phone. 
However, the intended purpose of this is so that you can put them back on another Windows Phone, so
there's no way (as far as I can tell) of viewing the messages on your computer. So I created this program
to read the XML files created by the app and allow them to be viewed. At the moment there's a very simple
console application for viewing, and a GUI that supports viewing conversations and extracting images from
them (although at the moment it just assumes where your backup folder is, so I need to fix that). There
are definitely more features I'd like to add to them, but at the moment they are at a sort of "very minimum
viable product" stage.

Also: all of this code was written based solely on my personal message backups. So I have no idea if it works
for, say, backups of non-US messages, or if there are features of MMS and SMS that will cause my code to crash.
So if something isn't working for you, let me know!
