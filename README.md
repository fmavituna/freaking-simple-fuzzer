# trunk

FSF is a plug-in based freakin' simple fuzzer for fuzzing web applications and scraping data.

## Platform
* Windows(.NET Framework 3.5)
* OSX (Mono).
* Linux (Mono).

##Quick Info
It supports some basic stuff and missing some features however it has got some advanced RegEx capturing features for scraping data out of web applications.

It's still in early stage of development. It's not well tested and I developed it when I need it, so don't keep your hopes high.

Although you are always welcome to do feature requests and report some bugs.

##Why bring yet another fuzzer into this cruel world?
Yeah, I know there are so many of them hanging around. Basically I was trying to fuzz something and after spending about 2-3 hours about 3-4 different terribly designed fuzzers I thought knocking up mine would be better.

No offence to other fuzzers and by no means I claim that this fuzzer design is user-friendly or so much better than others. It's more like I designed it so I know my own shit sort of usable.

##Don't use if you....
* want a fuzzer where you can control the raw HTTP request
* need some crazy features such as fuzzing multiple locations at a time (actually latest version support fuzzing 2 points simultaneously, still that's the limit)
##Use if you need a fuzzer...
* that allows to take advantage of RegEx with the full power for scraping data (this is quite useful while exploiting SQL Injections, gathering data, looking for some hidden resource or trying to enumerate all valid "user id"s)
* easy to run
* that makes it easy to write your own fuzzing modules
* with a simple and compact .NET code


```
                                |  _|___|  _|
                                |  _|_ -|  _|
                                |_| |___|_|
                        Freaking Simple Fuzzer v7.1.0.0

FSF.exe -u http://example.com/?id=[FUZZ] -m fuzzingmodule -o moduleoptions [options]

= Available Fuzzing Modules =
  Integer                       Opt: StartNumber-EndNumber[-Increaser] i.e. (1,1000)
  Wordlist                      Opt: File Path i.e. (c:\wordlist\list.txt)

= Parameters =

  u, url                Required Target URL. http://example.com/?param=[FUZZ]
  m, module             Fuzzing Module
  o, fuzzing-options    Fuzzing Module Options
  method                HTTP Method (default:GET)
  addheader             Add Custom Headers. i.e. "Referrer=http://example.com";Header2=Value2

  h, hide-status        Hide Status Code (seperate status codes, i.e. 404;301)
  c, capture            Regex Capture (i.e : (.*.)) to output a file
  capture-output        Regex capture output file (create, append)
  g, capture-template   Capture template (uses String.Format. {0}=attack param,{1}=capture, {2}=New Line, default:{0} : {1}{2})

  capture-group         Capture Group Index (only captures the specified group,
default:none)
  match-template        Uses String.Format. Defines output format when you got more than 1 matches.({0}=Match,{1}=New Line, default:"{0}," )

  p, proxy              Proxy URL
  d, use-default-proxy  Default Proxy
  q, timeout            Timeout as Seconds (default:60)
  t, thread             Thread Count (default:10)
  p, postdata           Raw Post Data
  print-responses       Prints HTTP repsonses to the screen
  help                  Display this help screen.

= Examples =
SQL Injection fuzzing, hides HTTP status code 200
FSF -u http://example.com/?id=[FUZZ] -m wordlist -o "c:\wordlists\sqli.txt" -h 200

Find directories
FSF -h 404 -o c:\Wordlist\directorynames.txt -u http://example.com/[FUZZ]/
FSF -h 404,302 -f c:\Wordlist\filenames.txt -u http://example.com/[FUZZ].aspx
```
