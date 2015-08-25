SipekSDK helps developers building VoIP based applications (currently SIP only). It's based on [pjsip.org](http://pjsip.org) SIP protocol stack.

The SDK includes three modules:
  * **pjsipDll** is a C++ (unmanaged) project integrated in pjsip.org solution. The output is Dll library which can be used in .Net applications.
  * **pjsipWrapper** is a set of C# wrapper classes dynamically linked to pjsipDll. It provides low level API for SIP applications
  * **CallControl** is a higher level C# API designed upon pjsipWrapper. It offers easy to use and intuitive API, makes extremely easy to develop advanced VoIP applications. It's possible to connect CallControl to any other VoIP wrapper based upon AbstractWrapper interface (ie. H.323).

For more details visit the new Sipek home page http://sites.google.com/site/sipekvoip/.

Sipek SDK accelerates the development of VoIP based applications with your own GUI and brand name. It's really easy to incorporate SipekSDK controls into your applications. Sample source codes are available in repository.


&lt;wiki:gadget url="http://www.ohloh.net/projects/23904/widgets/project\_basic\_stats.xml" height="220"  border="1" /&gt;

&lt;wiki:gadget url="http://www.ohloh.net/projects/23904/widgets/project\_thin\_badge.xml" height="36"  border="0" /&gt;

&lt;wiki:gadget url="http://www.ohloh.net/projects/23904/widgets/project\_users\_logo.xml" height="43"  border="0" /&gt;