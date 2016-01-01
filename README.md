# url2wiz.NET
a tool to transfer url(s) to wiz note

Usage:
-D:	the root directory of your internate shortcuts, 
	default as the same directory of the tool
-U:	your wiz email ID, mandatory
-S:	interval of each request to wiz API, default as 20s, unit as seconds
-L:	log those failed to request, save to the same directory as -D specified
	default as true
-I:	if ignore the repeat urls, default as true
-W:	single web url to add
-F:	a file contains the url to add, format as [path(\t)url].
	this will be usefull when adding the failed ones
-H/-?	Help

example:
	url2wiz.NET.exe -D C:\Links -U YourUserId -S 10 -L -I false
			-W http://www.wiz.cn -F C:\yourfaillist.txt